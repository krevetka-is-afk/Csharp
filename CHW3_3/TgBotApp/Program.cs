using System.Collections.Concurrent;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Exceptions;
using System.Reflection;
using Lib;

// bot id ---> @electrocar_power_bot
// bot token > 7006621448:AAG9fhbGoKxM3ApCb4Vf-eb_C8eAuUDhEag
// Растворов Сергей 236
class Program
{
    private static string token = "7006621448:AAG9fhbGoKxM3ApCb4Vf-eb_C8eAuUDhEag";
    static readonly ConcurrentDictionary<long, string[]> Answers = new ConcurrentDictionary<long, string[]>(); // словарь -- хранилище ответов пользователя
    static void Main(string[] args)
    {
        var client = new TelegramBotClient(token);
        client.StartReceiving(Update, Error);
        Console.WriteLine("Hello world");
        _ = Console.ReadLine();
    }

    async static Task Update(ITelegramBotClient client, Update update, CancellationToken token)
    {
        Message message = update.Message;
        if (message.Text != null)
        {
            await OnMessageHandler(client, message);
        }
        if (message.Document != null)
        {
            await OnDocumentHandler(client, message);
        }
    }

    async static Task OnMessageHandler(ITelegramBotClient client, Message message)
    {
        string? sms = message.Text;
        if (Answers.TryGetValue(message.From.Id, out string[] answers) &&
            answers[0] != null &&
            answers[1] != null)
        {
            if (answers[2] == null)
            {
                answers[2] = sms;
                string action = answers[1];
                switch (action)
                {
                    case "Выбрать по AdmArea":
                        {
                            string field = "AdmArea";
                            await SelectByOneField(client, message, field);
                            break;
                        }
                    case "Выбрать по District":
                        {
                            string field = "District";
                            await SelectByOneField(client, message, field);
                            break;
                        }
                    case "Выбрать по AdmArea и Longitude и Latitude":
                        {
                            await MakeRequestForMode(client, message);
                            break;
                        }
                }
            }
            else
            {
                answers[3] = sms;
                if (answers[1] == "Выбрать по AdmArea и Longitude и Latitude")
                {
                    await SelectByAdmLL(client, message);
                }
                else
                {
                    _ = await client.SendTextMessageAsync(message.Chat.Id, "Что-то пошло не так. Начните сначала, введя команду /start");
                }
            }
        }
        else
        {
            switch (sms)
            {
                case "/start":
                    {
                        string txt = "Отправьте мне файл для обработки.\nЯ поддерживаю только CSV и JSON форматы !";
                        _ = Answers.TryAdd(message.From.Id, new string[4]);
                        _ = await client.SendTextMessageAsync(message.Chat.Id, txt);
                        break;
                    }
                case "Сортировать по AdmArea по возрастанию":
                    {
                        await SortByAdmArea(client, message);
                        break;
                    }
                case "Сортировать по AdmArea по убыванию":
                    {
                        await SortByAdmArea(client, message, true);
                        break;
                    }
                case "Выбрать по AdmArea":
                    {
                        string field = "AdmArea";
                        await MakeRequestForFirstValue(client, message, field);
                        break;
                    }
                case "Выбрать по District":
                    {
                        string field = "District";
                        await MakeRequestForFirstValue(client, message, field);
                        break;
                    }
                case "Выбрать по AdmArea и Longitude и Latitude":
                    {
                        string field = "AdmArea и Longitude и Latitude";
                        await MakeRequestForFirstValue(client, message, field);
                        break;
                    }
                default:
                    {
                        _ = await client.SendTextMessageAsync(message.Chat.Id, "Приветствую !\nДля начала взаимодействия со мной введите /start");
                        break;
                    }
            }
        }
        return;
    }

    async static Task OnDocumentHandler(ITelegramBotClient client, Message message)
    {
        string ext = message.Document.FileName.Split('.')[^1];
        if (ext == "json" || ext == "csv")
        {
            var fileId = message.Document.FileId;
            var fileInfo = await client.GetFileAsync(fileId);
            var filePath = fileInfo.FilePath;

            string destinationFilePath = GetDestinationFilePath(message);

            await using Stream fileStream = System.IO.File.Create(destinationFilePath);
            await client.DownloadFileAsync(filePath, fileStream);
            fileStream.Close();

            string fileName = message.Document.FileName;
            long userId = message.From.Id;

            if (Answers.TryGetValue(userId, out string[] answers))
            {
                if (answers[0] == null)
                {
                    answers[0] = fileName;
                    _ = await client.SendTextMessageAsync(message.Chat.Id, "Для взаимодействия используйте меню", replyMarkup: GetChoseOptionButtons());
                }
                else
                {
                    _ = await client.SendTextMessageAsync(message.Chat.Id, "Что-то пошло не так. Начните сначала, введя команду /start");
                    _ = Answers.TryRemove(message.From.Id, out _);
                }
            }
            else
            {
                _ = await client.SendTextMessageAsync(message.Chat.Id, "Что-то пошло не так. Начните сначала, введя команду /start");
            }
        }
        else
        {
            _ = await client.SendTextMessageAsync(message.Chat.Id, "Неправильное расширение файла. Начните сначала, введя команду /start");
        }
    }

    async static Task SortByAdmArea(ITelegramBotClient client, Message message, bool ord = false)
    {
        if (Answers.TryGetValue(message.From.Id, out string[] answers) &&
            answers[0] != null &&
            answers[1] == null)
        {
            string fileName = answers[0];
            string order = ord ? "убывани" : "возрастани";
            answers[1] = $"Сортировать по AdmArea по {order}ю";
            _ = await client.SendTextMessageAsync(message.Chat.Id, $"Сортирую по полю AdmArea в порядке {order}я данные в {fileName}");
            string path = GetDestinationFilePathByName(message, fileName);
            string ext = fileName.Split('.')[^1];
            // ассинхронное чтение
            try
            {
                CsvRecord[] data = await ReadData(path, ext);

                CsvRecord[] result = DataProcessing.SortByAdmArea(data, ord);
                string newPath = GetDestinationFileResultPathByName(message, fileName);

                await WriteResult(newPath, ext, result);

                await using Stream stream = System.IO.File.OpenRead(newPath);
                _ = await client.SendDocumentAsync(
                    chatId: message.Chat.Id,
                    document: InputFile.FromStream(stream: stream, fileName: GetNewFileName(fileName)));
                stream.Close();
            }
            catch (Exception)
            {
                _ = await client.SendTextMessageAsync(message.Chat.Id, "Похоже что-то не так с вашим файлом.\n Попробуйте предоставить корректные данные или изменить название файла");
            }

            _ = await client.SendTextMessageAsync(message.Chat.Id, "Чтобы повторить процесс введите /start", replyMarkup: GetStartButton());
            _ = Answers.TryRemove(message.From.Id, out _);
        }
        else
        {
            _ = await client.SendTextMessageAsync(message.Chat.Id, "Что-то пошло не так. Начните сначала, введя команду /start");
        }
    }

    async static Task MakeRequestForFirstValue(ITelegramBotClient client, Message message, string field)
    {
        if (Answers.TryGetValue(message.From.Id, out string[] answers) &&
            answers[0] != null &&
            answers[1] == null)
        {
            string fileName = answers[0];
            answers[1] = $"Выбрать по {field}";

            string[] fields = field.Split(' ');
            _ = await client.SendTextMessageAsync(message.Chat.Id, $"Давайте выберем по полю {field} в {fileName}");
            _ = await client.SendTextMessageAsync(message.Chat.Id, $"Пришлите мне значение для {fields[0]}");

        }
        else
        {
            _ = await client.SendTextMessageAsync(message.Chat.Id, "Что-то пошло не так. Начните сначала, введя команду /start");
        }
    }

    async static Task SelectByOneField(ITelegramBotClient client, Message message, string field)
    {
        if (Answers.TryGetValue(message.From.Id, out string[] answers) &&
            answers[0] != null &&
            answers[1] != null &&
            answers[2] != null)
        {
            string fileName = answers[0];
            string value = answers[2];
            _ = await client.SendTextMessageAsync(message.Chat.Id, $"Ищу в {fileName} объекты, у которых в поле {field} значение {value}");
            string path = GetDestinationFilePathByName(message, fileName);
            string ext = fileName.Split('.')[^1];

            try
            {
                CsvRecord[] data = await ReadData(path, ext);

                int mode = field == "AdmArea" ? 1 : 2;
                CsvRecord[] result = DataProcessing.SelectBy(value, mode, data);
                string newPath = GetDestinationFileResultPathByName(message, fileName);


                await WriteResult(newPath, ext, result);


                await using Stream stream = System.IO.File.OpenRead(newPath);
                _ = await client.SendDocumentAsync(
                    chatId: message.Chat.Id,
                    document: InputFile.FromStream(stream: stream, fileName: GetNewFileName(fileName)));
                stream.Close();
            }
            catch (Exception)
            {
                _ = await client.SendTextMessageAsync(message.Chat.Id, "Похоже что-то не так с вашим файлом.\n Попробуйте предоставить корректные данные или изменить название файла");
            }

            _ = await client.SendTextMessageAsync(message.Chat.Id, "Чтобы повторить процесс введите /start", replyMarkup: GetStartButton());
            _ = Answers.TryRemove(message.From.Id, out _);
        }
        else
        {
            _ = await client.SendTextMessageAsync(message.Chat.Id, "Что-то пошло не так. Начните сначала, введя команду /start", replyMarkup: GetStartButton());
        }
    }

    async static Task MakeRequestForMode(ITelegramBotClient client, Message message)
    {
        if (Answers.TryGetValue(message.From.Id, out string[] answers) &&
            answers[0] != null &&
            answers[1] != null &&
            answers[2] != null &&
            answers[3] == null /*&&
            answers[4] == null*/)
        {
            _ = await client.SendTextMessageAsync(message.Chat.Id, $"А теперь пришлите мне значение для Longitude_WGS84 и Latitude_WGS84 через пробел");

        }
        else
        {
            _ = await client.SendTextMessageAsync(message.Chat.Id, "Что-то пошло не так. Начните сначала, введя команду /start");
        }
    }

    async static Task SelectByAdmLL(ITelegramBotClient client, Message message)
    {
        if (Answers.TryGetValue(message.From.Id, out string[] answers) &&
            answers[0] != null &&
            answers[1] != null &&
            answers[2] != null &&
            answers[3] != null /*&& answers[4] != null*/)
        {
            string fileName = answers[0];
            string value = answers[2];
            string secondValue = answers[3];
            // string thirdValue = answers[4];
            _ = await client.SendTextMessageAsync(message.Chat.Id, $"Ищу в {fileName} объекты, у которых в поля AdmArea и Longitude и Latitude имеют значения {value} и {secondValue} соответственно");
            string path = GetDestinationFilePathByName(message, fileName);
            string ext = fileName.Split('.')[^1];

            try
            {
                CsvRecord[] data = await ReadData(path, ext);

                CsvRecord[] result = DataProcessing.SelectBy(value, 3, data, secondValue);
                string newPath = GetDestinationFileResultPathByName(message, fileName);

                await WriteResult(newPath, ext, result);

                await using Stream stream = System.IO.File.OpenRead(newPath);
                _ = await client.SendDocumentAsync(
                    chatId: message.Chat.Id,
                    document: InputFile.FromStream(stream: stream, fileName: GetNewFileName(fileName)));
                stream.Close();
            }
            catch (Exception)
            {
                _ = await client.SendTextMessageAsync(message.Chat.Id, "Похоже что-то не так с вашим файлом.\n Попробуйте предоставить корректные данные или изменить название файла");
            }

            _ = await client.SendTextMessageAsync(message.Chat.Id, "Чтобы повторить процесс введите /start", replyMarkup: GetStartButton());
            _ = Answers.TryRemove(message.From.Id, out _);
        }
        else
        {
            _ = await client.SendTextMessageAsync(message.Chat.Id, "Что-то пошло не так. Начните сначала, введя команду /start");
        }
    }

    static async Task<CsvRecord[]> ReadData(string path, string ext)
    {
        CsvRecord[] data;
        if (ext == "csv")
        {
            data = await OutProcessing.ReadCsv(path);
        }
        else
        {
            data = await OutProcessing.ReadJson(path);
        }
        return data;
    }

    static async Task WriteResult(string newPath, string ext, CsvRecord[] result)
    {
        if (ext == "csv")
        {
            await OutProcessing.WriteCsv(newPath, result);
        }
        else
        {
            await OutProcessing.WriteJson(newPath, result);
        }
    }

    static string GetDestinationFilePathByName(Message message, string fileName)
    {
        try
        {
            string binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            char sep = Path.DirectorySeparatorChar;
            long id = message.Chat.Id;
            string dirPath = Path.Combine(binPath, "temp", id.ToString());
            string dstPath = Path.Combine(dirPath, fileName);

            if (!Directory.Exists(dirPath))
            {
                _ = Directory.CreateDirectory(dirPath);
            }

            return dstPath;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            return null;
        }
    }

    static string GetDestinationFilePath(Message message)
    {
        try
        {
            string binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            long id = message.Chat.Id;
            string dirPath = Path.Combine(binPath, "temp", id.ToString());

            if (!Directory.Exists(dirPath))
            {
                _ = Directory.CreateDirectory(dirPath);
            }

            string fileName = message.Document.FileName;
            string dstPath = Path.Combine(dirPath, fileName);

            return dstPath;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            return null;
        }
    }

    static string GetDestinationFileResultPathByName(Message message, string fileName)
    {
        try
        {
            string fileName1 = GetNewFileName(fileName);
            string binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            long id = message.Chat.Id;
            string dirPath = Path.Combine(binPath, "temp", id.ToString());

            if (!Directory.Exists(dirPath))
            {
                _ = Directory.CreateDirectory(dirPath);
            }

            string dstPath = Path.Combine(dirPath, fileName1);

            return dstPath;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            return null;
        }
    }

    static string GetNewFileName(string fileName)
    {
        string[] parts = fileName.Split('.');
        if (parts.Length < 2)
        {
            throw new ArgumentException("Invalid file name format");
        }

        string name = string.Join(".", parts.Take(parts.Length - 1));
        string ext = parts[^1];
        string newFileName = $"{name}_modified.{ext}";
        return newFileName;
    }

    private static Task Error(ITelegramBotClient client, Exception exception, CancellationToken token)
    {
        // код ошибки и её сообщение 
        var ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        Console.WriteLine(ErrorMessage);
        return Task.CompletedTask;
    }
    // клавиатура
    private static IReplyMarkup GetStartButton()
    {
        return new ReplyKeyboardMarkup(new KeyboardButton("/start"));
    }
    // клавиатура
    private static IReplyMarkup GetChoseOptionButtons()
    {
        List<List<KeyboardButton>> keyboard = new List<List<KeyboardButton>>
            {
                new List<KeyboardButton> {new KeyboardButton("Сортировать по AdmArea по возрастанию"), new KeyboardButton("Сортировать по AdmArea по убыванию") },
                new List<KeyboardButton> {new KeyboardButton("Выбрать по AdmArea"), new KeyboardButton("Выбрать по District"), new KeyboardButton("Выбрать по AdmArea и Longitude и Latitude") }
            };
        return new ReplyKeyboardMarkup(keyboard);
    }

}
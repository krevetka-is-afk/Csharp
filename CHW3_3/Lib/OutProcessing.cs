namespace Lib;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;


public class OutProcessing
{
    static string? firstRow;
    static string? secondRow;

    public static async Task<CsvRecord[]> ReadJson(string path)
    {
        if (File.Exists(path))
        {
            string jsonString;
            jsonString = await File.ReadAllTextAsync(path);
            CsvRecord[] data = JsonSerializer.Deserialize<CsvRecord[]>(jsonString);
            return data;
        }
        else
        {
            Console.WriteLine("Something went wrong while reading json");
            throw new Exception();
        }
    }

    public static async Task WriteJson(string path, CsvRecord[] data)
    {
        if (data != null && path != null)
        {
            var options = new JsonSerializerOptions { Encoder = JavaScriptEncoder.Create(UnicodeRanges.All), WriteIndented = true };
            string jsonString = JsonSerializer.Serialize<CsvRecord[]>(data, options);
            jsonString += "\n";

            await File.WriteAllTextAsync(path, jsonString);
        }
        else
        {
            Console.WriteLine("Something went wrong while writing json");
            throw new Exception();
        }
    }

    public static async Task<CsvRecord[]> ReadCsv(string path)
    {
        if (File.Exists(path))
        {
            string bigString;
            try
            {
                bigString = await File.ReadAllTextAsync(path);
                string[] result = bigString.Split('\n');
                firstRow = result[0] + '\n';
                secondRow = result[1] + '\n';
                int c = result[result.Length - 1].Length == 0 ? result.Length - 1 : result.Length;
                string[] data = result[2..c];
                if (data == null)
                {
                    throw new ArgumentNullException();
                }
                CsvRecord[] result1 = new CsvRecord[data.Length];
                int i = 0;
                foreach (string s in data)
                {
                    result1[i++] = new CsvRecord(s);
                }
                return result1;

            }
            catch (Exception)
            {
                Console.WriteLine("file is open in another program or bad data in file or something's else wrong");
                throw new Exception();
            }
        }
        else
        {
            Console.WriteLine("Sorry we have a problems with path, try to change path");
            throw new Exception();
        }
    }

    public static async Task WriteCsv(string path, CsvRecord[] data)
    {
        if (path == null || data == null || firstRow == null || secondRow == null)
        {
            throw new Exception();
        }
        try
        {
            string[] txt = new string[data.Length + 2];
            txt[0] = firstRow;
            txt[1] = secondRow;
            int i = 2;
            foreach (CsvRecord rec in data)
            {
                txt[i++] = rec.GetCsvRow();
            }
            string text = String.Join("", txt);

            await File.WriteAllTextAsync(path, text);
        }
        catch (Exception)
        {
            Console.WriteLine("Something went wrong while writing csv");
            throw new Exception();
        }
    }
}

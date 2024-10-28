namespace Lib;
using System.Collections;
using System.Text.Json;
using System.Text.Json.Serialization;

public class CsvRecord
{
    string objectCategoryId = "";
    int id;
    string name;
    string admArea;
    string district;
    string address;
    double longitude;
    double latitude;
    string geodataCenter;
    string geoarea;
    uint globalId;

    public CsvRecord() { }

    public CsvRecord(string row)
    {
        if (row == null)
        {
            Console.WriteLine("row == null");
            throw new Exception();
        }
        string[] parsed = row.Split(';');

        if (parsed.Length != 12)
        {
            Console.WriteLine("parsed.Length != 12");
            throw new Exception();
        }

        int c1 = parsed[1].Length - 1;
        bool isNoNone1 = int.TryParse(parsed[1][1..c1], out int p1);

        int c6 = parsed[6].Length - 1;
        bool isNoNone6 = double.TryParse(parsed[6][1..c6].Replace('.', ','), out double p6);

        int c7 = parsed[7].Length - 1;
        bool isNoNone7 = double.TryParse(parsed[7][1..c7].Replace('.', ','), out double p7);

        int c8 = parsed[8].Length - 1;
        bool isNoNone8 = uint.TryParse(parsed[8][1..c8].Replace('.', ','), out uint p8);

        ObjectCategoryId = parsed[0] ?? "";
        Id = isNoNone1 ? p1 : -1;
        Name = parsed[2];
        AdmArea = parsed[3];
        District = parsed[4];
        Address = parsed[5];
        Longitude = isNoNone6 ? p6 : -1;
        Latitude = isNoNone7 ? p7 : -1;
        GlobalId = isNoNone8 ? p8 : 0;
        GeodataCenter = parsed[9];
        Geoarea = parsed[10];
    }

    public CsvRecord(string objectCategoryId, int id, string name, string admArea, string district,
        string address, double longitude, double latitude, uint globalId,
        string geodataCenter, string geoarea)
    {
        ObjectCategoryId = objectCategoryId;
        Id = id;
        Name = name;
        AdmArea = admArea;
        District = district;
        Address = address;
        Longitude = longitude;
        Latitude = latitude;
        GlobalId = globalId;
        GeodataCenter = geodataCenter;
        Geoarea = geoarea;
    }

    public string GetCsvRow()
    {
        string result = objectCategoryId + ';';
        result += id == -1 ? "\"\"" : '\"' + id.ToString() + '\"';
        result += $";\"{name}\";";
        result += $"\"{admArea}\";\"{district}\";\"{address}\";";
        result += longitude == -1 ? "\"\"" : '\"' + longitude.ToString().Replace(',', '.') + '\"' + ';';
        result += latitude == -1 ? "\"\"" : '\"' + latitude.ToString().Replace(',', '.') + '\"' + ';';
        result += globalId == 0 ? "\"\"" : '\"' + globalId.ToString() + '\"' + ';';
        result += $"\"{geodataCenter}\";\"{geoarea}\";\n";
        return result;
    }

    [JsonPropertyName("object_category_Id")]
    public string ObjectCategoryId 
    { 
        get { return objectCategoryId; } 
        set 
        { 
            if (value.Length > 0 && value[0] == '\"' && value[^1] == '\"')
            {
                int c = value.Length - 1;
                objectCategoryId = value[1..c];
            }
            else
            {
                objectCategoryId = value;;
            }
        }
    }

    [JsonPropertyName("ID")]
    public int Id { get { return id; } set { id = value; } }

    [JsonPropertyName("Name")]
    public string Name
    {
        get { return name; }
        set
        {
            if (value.Length > 0 && value[0] == '\"' && value[^1] == '\"')
            {
                int c = value.Length - 1;
                name = value[1..c];
            }
            else
            {
                name = value;
            }
        }
    }

    [JsonPropertyName("AdmArea")]
    public string AdmArea
    {
        get { return admArea; }
        set
        {
            if (value.Length > 0 && value[0] == '\"' && value[^1] == '\"')
            {
                int c = value.Length - 1;
                admArea = value[1..c];
            }
            else
            {
                admArea = value;
            }
        }
    }

    [JsonPropertyName("District")]
    public string District
    {
        get { return district; }
        set
        {
            if (value.Length > 0 && value[0] == '\"' && value[^1] == '\"')
            {
                int c = value.Length - 1;
                district = value[1..c];
            }
            else
            {
                district = value;
            }
        }
    }

    [JsonPropertyName("Address")]
    public string Address
    {
        get { return address; }
        set
        {
            if (value.Length > 0 && value[0] == '\"' && value[^1] == '\"')
            {
                int c = value.Length - 1;
                address = value[1..c];
            }
            else
            {
                address = value;
            }
        }
    }

    [JsonPropertyName("Longitude_WGS84")]
    public double Longitude { get { return longitude; } set { longitude = value; } }

    [JsonPropertyName("Latitude_WGS84")]
    public double Latitude { get { return latitude; } set { latitude = value; } }

    [JsonPropertyName("global_id")]
    public uint GlobalId { get { return globalId; } set { globalId = value; } } 

    [JsonPropertyName("geodata_center")]
    public string GeodataCenter
    {
        get { return geodataCenter; }
        set
        {
            if (value.Length > 0 && value[0] == '\"' && value[^1] == '\"')
            {
                int c = value.Length - 1;
                geodataCenter = value[1..c];
            }
            else
            {
                geodataCenter = value;
            }
        }
    }

    [JsonPropertyName("geoarea")]
    public string Geoarea
    {
        get { return geoarea; }
        set
        {
            if (value.Length > 0 && value[0] == '\"' && value[^1] == '\"')
            {
                int c = value.Length - 1;
                geoarea = value[1..c];
            }
            else
            {
                geoarea = value;
            }
        }
    }

}
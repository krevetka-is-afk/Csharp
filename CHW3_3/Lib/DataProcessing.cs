namespace Lib;
using System;
using System.Collections;
using System.Linq;

public static class DataProcessing
{
    public static CsvRecord[] SelectBy(string value, int mode, in CsvRecord[] data, string? second_value = null/*, string? third_value = null*/)
    {
        // Выборка по соответствующему полю по заданному значению
        // mode = 1: AdmArea
        // mode = 2: District
        // mode = 3: AdmArea и Longitude и Latitude
        if (value == null || data == null || (!(mode == 1 || mode == 2 || mode == 3)))
        {
            throw new Exception();
        }
        CsvRecord[] result = new CsvRecord[data.Length];
        int i = 0;
        switch (mode)
        {
            case 1:

                IEnumerable<CsvRecord> recs = from n in data
                                              where n.AdmArea == value
                                              select n;
                int len = recs.Count();
                result = new CsvRecord[len];
                foreach (CsvRecord rec in recs)
                {
                    result[i++] = rec;
                }
                break;

            case 2:
                
                recs = from n in data
                       where n.District == value
                       select n;
                len = recs.Count();
                result = new CsvRecord[len];
                foreach (CsvRecord rec in recs)
                {
                    result[i++] = rec;
                }
                break;

            case 3:
                if (second_value == null /*&& third_value == null*/)
                {
                    Console.WriteLine("second_value == null");
                    throw new Exception();
                }
                string[] m = second_value.Split();
                recs = from n in data
                       where n.AdmArea == value && n.Longitude.ToString().Replace(',', '.') == m[0] && n.Latitude.ToString().Replace(',', '.') == m[1]
                       select n;
                len = recs.Count();
                result = new CsvRecord[len];
                foreach (CsvRecord rec in recs)
                {
                    result[i++] = rec;
                }
                break;

        }
        CsvRecord[] result1 = new CsvRecord[i];
        Array.Copy(result, result1, i);
        return result1;
    }

    // Сортировка Linq
    public static CsvRecord[] SortByAdmArea(in CsvRecord[] data, bool ord = false)
    {
        if (data == null)
        {
            throw new Exception();
        }
        int len = data.Length;
        CsvRecord[] sortedData = new CsvRecord[len];
        IEnumerable<CsvRecord> query;
        if (ord)
        {
            query = from n in data
                    orderby n.AdmArea descending
                    select n;
        }
        else
        {
            query = from n in data
                    orderby n.AdmArea ascending
                    select n;
        }
        int i = 0;
        foreach (CsvRecord rec in query)
        {
            sortedData[i++] = rec;
        }
        return sortedData;
    }
}

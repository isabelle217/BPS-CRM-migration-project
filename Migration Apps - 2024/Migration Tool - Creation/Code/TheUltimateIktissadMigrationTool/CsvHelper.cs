using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using System.IO;

using System.Data;
using System.Data.SqlClient;

namespace TheUltimateIktissadMigrationTool
{
    public class CsvHelper
    {

        public DataTable ConvertCSVtoDataTable(string strFilePath)
        {
            DataTable dt = new DataTable();

            try
            {
                using (StreamReader sr = new StreamReader(strFilePath))
                {
                    string[] headers = sr.ReadLine().Split(',');
                    foreach (string header in headers)
                    {
                        dt.Columns.Add(header);
                    }
                    while (!sr.EndOfStream)
                    {
                        string[] rows = sr.ReadLine().Split(',');
                        DataRow dr = dt.NewRow();
                        for (int i = 0; i < headers.Length; i++)
                        {
                                dr[i] = rows[i];
                        }
                        dt.Rows.Add(dr);
                    }

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return dt;
        }

        public void WriteToCsv<T>(List<T> list, string Filename)
        {
            string CsvPath = "C:\\Users\\karimbz\\Desktop\\UpdateWarnings\\" + Filename;

            try
            {
                using (StreamWriter textWriter = File.CreateText(CsvPath))
                {
                    var csv = new CsvWriter(textWriter);
                    csv.Configuration.HasHeaderRecord = false;
                    csv.WriteRecords(list);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

    }
}

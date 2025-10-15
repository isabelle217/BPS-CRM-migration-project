using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using System.Data;
using System.Data.SqlClient;

namespace TheUltimateIktissadMigrationTool
{
    public class SQLHelper
    {

        public DataTable GetCRM4Records(DataTable DTCSV, string Filtration)
        {
            DataTable DT = new DataTable();
            SqlConnection myConnection = new SqlConnection(TheUltimateTool.SqlConnectionString);
            string EntityType = TheUltimateTool.EntityType;

            try
            {
                myConnection.Open();

                string fieldsToquery = "";
                foreach (DataRow row in DTCSV.Rows)
                {
                    fieldsToquery += row["sourcename"].ToString() + ",";

                }
                fieldsToquery = fieldsToquery.Substring(0, (fieldsToquery.Length - 1));

                SqlCommand myCommand;
                if (TheUltimateTool.NoStateCodeEntites.Contains(EntityType.ToLower()))
                    myCommand = new SqlCommand("SELECT " + EntityType + "id,"
                    + fieldsToquery + "  FROM dbo." + EntityType + " ORDER BY " + EntityType + "id  " + Filtration, myConnection);
                else
                    myCommand = new SqlCommand("SELECT " + EntityType + "id,statecode,statuscode,createdon,"
                    + fieldsToquery + "  FROM dbo." + EntityType + " ORDER BY " + EntityType + "id  " + Filtration, myConnection);

                using (SqlDataReader myReader = myCommand.ExecuteReader())
                {
                    DT.Load(myReader);
                }
                myConnection.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return DT;
        }
    }
}

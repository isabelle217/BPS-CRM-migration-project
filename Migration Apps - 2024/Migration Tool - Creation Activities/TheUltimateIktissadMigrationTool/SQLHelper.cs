using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using System.Data;
using System.Data.SqlClient;
using System.Threading;

namespace TheUltimateIktissadMigrationTool
{
    public  class SQLHelper
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
                string typeOfCustomerFields = "";
                foreach (DataRow row in DTCSV.Rows)
                {
                    fieldsToquery += row["sourcename"].ToString() + ",";
                    if(row["targetentitytype"].ToString().ToLower().Trim() == "customer")
                       typeOfCustomerFields += row["sourcename"].ToString()+"Type,";
                }
                fieldsToquery += typeOfCustomerFields;
               fieldsToquery = fieldsToquery + "RegardingObjectTypeCode ";

                //SqlCommand myCommand = new SqlCommand("SELECT " + EntityType + "id,statecode,statuscode,createdon,"
                // + fieldsToquery + "  FROM dbo." + EntityType + " ORDER BY " + EntityType + "id  " + Filtration, myConnection);

                SqlCommand myCommand;

                if (TheUltimateTool.NoStateCodeEntites.Contains(EntityType.ToLower()))
                   myCommand = new SqlCommand("SELECT " + "activity" + "id,"
                 + fieldsToquery + "  FROM dbo." + EntityType + " " +  Filtration+ " ORDER BY " + "activity" + "id  ", myConnection );
                 
                else
                   myCommand = new SqlCommand("SELECT " + "activity" + "id,"
                   + fieldsToquery + ",statecode,statuscode,createdon FROM dbo." + EntityType + " " +  Filtration +" ORDER BY " + "activity" + "id  ", myConnection);
              //      myCommand = new SqlCommand("SELECT " + "campaignid" + ","
               //  + fieldsToquery + "statecode,statuscode,createdon FROM dbo." + EntityType + " " + Filtration + " ORDER BY " + "campaignid " , myConnection);

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

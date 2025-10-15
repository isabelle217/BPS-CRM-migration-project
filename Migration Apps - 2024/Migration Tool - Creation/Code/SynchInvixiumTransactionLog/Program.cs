using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel.Description;
using System.Configuration;
using System.Net;
using System.Data;
using System.Data.SqlClient;

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;


namespace SynchInvixiumTransactionLog
{
    class Program
    {
        static CrmServiceClient crmService;
        static int Year;
        static Log log;
        static SqlConnection conn = null;
        
         static void Main(string[] args)
        {
            try
            {
                Program main = new Program();
                log = new Log();
                log.Logfile = Convert.ToBoolean(ConfigurationManager.AppSettings["LogFile"]);
                log.InitializeLog("SynchInvixiumTransactionOnline");
                log.Info("Getting start");
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                //string ConnectionString = string.Format("RequireNewInstance=True;AuthType=ClientSecret;url=https://la.crm4.dynamics.com/;ClientId=9b8dc583-a1fe-4b71-a8bd-efe4e4bd8c99;ClientSecret=EAQ8Q~uuWjW~PfMS4M6IalbOh~~C7zVqCWvX_cUa");

                string ConnectionString = string.Format("RequireNewInstance=True;AuthType=ClientSecret;url=https://la.crm4.dynamics.com/;ClientId=55383c21-9232-427b-9871-e636e5ea7f73;ClientSecret=_uS8Q~a1rnjldwlrf69_CP4iq6JWALWXubchJcDh");

                crmService = new CrmServiceClient(ConnectionString);

                log.Info("Getting Last Successful Synch.");
                var InvixiumConfigEntity = main.GetLastSuccessfulSynch();
                var LastSuccessfulSynch = InvixiumConfigEntity.GetAttributeValue<DateTime>("new_lastsuccessfulsynch").ToLocalTime();
                log.Info("Last Successful Synch: " + LastSuccessfulSynch);
                log.Info("Get Users.");
                var Users = main.GetUsers();
                log.Info("User Retrieved");
                log.Info("Get Devices.");
                var Devices = main.GetDevices();
                log.Info("User Devices");
                log.Info("Get New Transaction Logs");
                var TransactionLogs = main.GetNewTransactionLogs(LastSuccessfulSynch);
                log.Info("New Transaction Logs retrieved. Count = " + TransactionLogs.Rows.Count);
                log.Info("Create new Logs in CRM");
                main.CreateTransactionLogs(TransactionLogs, Users, Devices);
                log.Info("Logs Creation Completed.");
                log.Info("Update Invicium Configuration");
                main.UpdateInvixiumConfiguration(InvixiumConfigEntity);
                log.Info("Invixium Configuration Updated");
            }
            catch(Exception ex)
            {
                log.Error(ex);
            }

        }

         public Entity GetLastSuccessfulSynch()
         {
             QueryExpression query = new QueryExpression("new_invixiumconfiguration");
             query.ColumnSet = new ColumnSet("new_lastsuccessfulsynch");
             query.Criteria.AddCondition("new_lastsuccessfulsynch", ConditionOperator.NotNull);

             var Results = crmService.RetrieveMultiple(query).Entities;

             if (Results.Count == 0)
                 throw new Exception("There are no Invixium Configuration Record with the Synch Date Filled.");
             else
                 return Results.First();

         }

         public Dictionary<string, EntityReference> GetUsers()
         {
             var map = new Dictionary<string, EntityReference>();

             QueryExpression query = new QueryExpression("systemuser");
             query.ColumnSet = new ColumnSet("systemuserid", "fullname", "new_userid");
             query.Criteria.AddCondition("isdisabled", ConditionOperator.Equal, false);

             var Users = crmService.RetrieveMultiple(query).Entities;
             foreach (var user in Users)
             {
                 if (user.Contains("new_userid"))
                     map.Add(user.GetAttributeValue<string>("new_userid"), user.ToEntityReference());
             }

             return map;
         }

         public Dictionary<string, EntityReference> GetDevices()
         {
             var map = new Dictionary<string, EntityReference>();

             QueryExpression query = new QueryExpression("new_invixiumdevice");
             query.ColumnSet = new ColumnSet("new_invixiumdeviceid", "new_serialnumber");
             query.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);

             var Devices = crmService.RetrieveMultiple(query).Entities;
             foreach (var device in Devices)
             {
                 if (device.Contains("new_serialnumber"))
                     map.Add(device.GetAttributeValue<string>("new_serialnumber"), device.ToEntityReference());
             }

             return map;
         }

         public DataTable GetNewTransactionLogs(DateTime LastSuccessfulSynch)
         {
             SqlCommand cmd = new SqlCommand();
             cmd.CommandText = @"  SELECT TransactionId, TransactionDate, UserRecordId, DeviceSerialNo, Event, EventType, dev.Name, FirstName, LastName, TemplateIndex
                  FROM TransactionLog as trans
                  INNER JOIN Device AS dev ON dev.SerialNumber = trans.DeviceSerialNo 
                  WHERE UserRecordId is not null
                  --AND Event = 15
                  AND EventType = '2'
                  AND TransactionDate > '" + LastSuccessfulSynch.ToString("yyyy-MM-dd HH:mm:ss.fff") + "'";

             //Use to import for specific users
             //SqlCommand cmd = new SqlCommand();
             //cmd.CommandText = @"  SELECT TransactionId, TransactionDate, UserRecordId, DeviceSerialNo, Event, EventType, dev.Name, FirstName, LastName, TemplateIndex
             //      FROM TransactionLog as trans
             //      INNER JOIN Device AS dev ON dev.SerialNumber = trans.DeviceSerialNo 
             //      WHERE UserRecordId is not null
             //      --AND Event = 15
             //      AND EventType = '2'
             //      --and TransactionDate>='2020-6-1 00:00:00.000'
             //       --and TransactionDate<'2020-11-19 00:00:00.000'
             //      and (UserRecordId='30188')";

             cmd.Connection = getConnection();
             SqlDataReader dr = cmd.ExecuteReader();
             DataTable dt = new DataTable();
             dt.CaseSensitive = false;
             dt.Load(dr);
             return dt;
         }

         public void CreateTransactionLogs(DataTable DT, Dictionary<string, EntityReference> Users, Dictionary<string, EntityReference> Devices)
         {
             foreach (DataRow row in DT.Rows)
             {
                 if (Users.ContainsKey(row.Field<string>("UserRecordId")))
                 {
                     Entity TransactionLog = new Entity("new_invixiumtransactionlog");
                     TransactionLog["new_transactionid"] = row["transactionid"];
                     TransactionLog["new_transactiondate"] = row["transactiondate"];
                     TransactionLog["new_userid"] = row["UserRecordId"];
                     TransactionLog["new_deviceserialno"] = row["DeviceSerialNo"];
                     TransactionLog["new_event"] = new OptionSetValue(15);//new OptionSetValue(row.Field<int>("event"));
                     TransactionLog["new_eventtype"] = new OptionSetValue(Convert.ToInt32(row.Field<string>("eventtype")));
                     TransactionLog["new_devicename"] = row["name"];
                     TransactionLog["new_name"] = row["transactionid"].ToString();
                     if (row["TemplateIndex"] != DBNull.Value && row.Field<int>("TemplateIndex") > 0 && row.Field<int>("TemplateIndex") <= 5)
                         TransactionLog["new_templateindex"] = new OptionSetValue(row.Field<int>("TemplateIndex"));

                     if (Users.ContainsKey(row.Field<string>("UserRecordId")))
                         TransactionLog["new_systemuserid"] = Users[row.Field<string>("UserRecordId")];
                     else
                         log.Info(string.Format("Transaction with ID '{0}' has a User '{1} {2}' not found in CRM", row["transactionid"], row["FirstName"], row["LastName"]));

                     if (Devices.ContainsKey(row.Field<string>("DeviceSerialNo")))
                         TransactionLog["new_deviceid"] = Devices[row.Field<string>("DeviceSerialNo")];

                     try
                     {
                         crmService.Create(TransactionLog);
                     }
                     catch (Exception ex)
                     {
                         if (row["transactionid"] != DBNull.Value)
                             log.Info("Creation failed for Transaction ID = " + row["transactionid"]);

                         log.Error(ex);
                     }
                 }
                 else
                     log.Info(string.Format("Transaction with ID '{0}' has a User '{1} {2}' not found in CRM", row["transactionid"], row["FirstName"], row["LastName"]));
             }
         }

         public void UpdateInvixiumConfiguration(Entity Config)
         {
             Entity UpdateDate = new Entity(Config.LogicalName, Config.Id);
             UpdateDate["new_lastsuccessfulsynch"] = DateTime.Now;
             crmService.Update(UpdateDate);
         }

         private SqlConnection getConnection()
         {
             if (conn == null)
             {
                 conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["SQLConnection"].ConnectionString);
             }
             if (conn.State != System.Data.ConnectionState.Open)
             {
                 conn.Open();
             }
             return conn;
         }
       
        
    }
}

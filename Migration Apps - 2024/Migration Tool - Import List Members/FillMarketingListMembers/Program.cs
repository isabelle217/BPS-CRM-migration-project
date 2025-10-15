using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;

using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Client;

using System.ServiceModel.Description;
using System.Net;
using System.Threading;

using Microsoft.Crm.Sdk.Messages;

namespace FillMarketingListMembers
{
    class Program
    {
        static SqlConnection conn = null;
       
        static void Main(string[] args)
        {
            try
            {
                Program main = new Program();

                List<Guid> ListMembers = main.GetIDList();

                string scan = Console.ReadLine();
                int start = Convert.ToInt32(scan);

                scan = Console.ReadLine();
                int end = Convert.ToInt32(scan);

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
               // string ConnectionString = string.Format("AuthType=ClientSecret;url=https://bps.api.crm4.dynamics.com/;ClientId=4f4ad400-fd37-4047-961e-ddeb47aecaa5;ClientSecret=DFe-8P.-Ch_1tYD88pQglYczso3po5Wu_9");
                string ConnectionString = string.Format("RequireNewInstance=True;AuthType=ClientSecret;url=https://la.crm4.dynamics.com/;ClientId=9b8dc583-a1fe-4b71-a8bd-efe4e4bd8c99;ClientSecret=L7p1N9Vnh2VXhF5Q1ZY6J-~O7J~Dau_4R5");
         
                CrmServiceClient _crmService = new CrmServiceClient(ConnectionString);
                if (_crmService.IsReady)
                {
                    for (int i = start; i < Math.Min(end, ListMembers.Count); i++)
                    {
                        DataTable dt = main.getListMemberRow(ListMembers[i]);
                        DataRow row = dt.Rows[0];



                        try
                        {

                            AddMemberListRequest req = new AddMemberListRequest();
                            req.EntityId = row.Field<Guid>("entityid");
                            req.ListId = row.Field<Guid>("listid");
                            AddMemberListResponse resp = (AddMemberListResponse)_crmService.Execute(req);

                            Console.WriteLine("{0}/{1} has been created successfully", i, ListMembers.Count);

                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                Console.WriteLine("Press <enter> to exit.");
                Console.Read();
            }
        
        }

        public DataTable getListMemberRow(Guid listmemberid)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = string.Format("select * from dbo.listmember where listmemberid = '" + listmemberid + "'");
            cmd.Connection = getConnection();
            SqlDataReader dr = cmd.ExecuteReader();
            DataTable dt = new DataTable();
            dt.CaseSensitive = false;
            dt.Load(dr);
            return dt;
        }

        public List<Guid> GetIDList()
        {
            SqlConnection sqlconn = getConnection();
            SqlCommand command = new SqlCommand("SELECT listmemberid FROM dbo.listmember ORDER BY listmemberid", sqlconn);

            SqlDataReader dr = command.ExecuteReader();
            List<Guid> IDList = new List<Guid>();
            while (dr.Read())
            {
                IDList.Add(dr.GetGuid(0));
            }
            dr.Close();
            return IDList;
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

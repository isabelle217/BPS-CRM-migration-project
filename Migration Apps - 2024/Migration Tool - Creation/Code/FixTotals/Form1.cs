using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.ServiceModel;
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
using System.Data.SqlClient;
using System.Data;
using System.IO;

namespace FixTotals
{
    public partial class Form1 : Form
    {
        public Dictionary<Guid, Guid> UserMap;

        private static IOrganizationService serviceOnPremises;
        public Form1()
        {
            InitializeComponent();
           
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            string ConnectionString = string.Format("AuthType=ClientSecret;url=https://bps.api.crm4.dynamics.com/;ClientId=4f4ad400-fd37-4047-961e-ddeb47aecaa5;ClientSecret=DFe-8P.-Ch_1tYD88pQglYczso3po5Wu_9");

            CrmServiceClient _crmService = new CrmServiceClient(ConnectionString);
            int queryCount = 5000;
            int pageNumber = 1;
            int counter = 0;
            if (_crmService.IsReady)
            {

                FilterExpression Filter = new FilterExpression(LogicalOperator.Or);
                //Filter.AddCondition("statuscode", ConditionOperator.Equal, 3);
                //Filter.AddCondition("statuscode", ConditionOperator.Equal, 4);
                Filter.AddCondition("opportunityid", ConditionOperator.Equal, new Guid("ff9745e0-9513-e611-8bb8-00155d000f04"));
                QueryExpression closedOpp = new QueryExpression();
                closedOpp.ColumnSet = new ColumnSet(true);
                closedOpp.EntityName = "opportunity";
                closedOpp.Criteria = Filter;
                closedOpp.PageInfo = new PagingInfo();
                closedOpp.PageInfo.Count = queryCount;
                closedOpp.PageInfo.PageNumber = pageNumber;
                closedOpp.PageInfo.PagingCookie = null;
                while (true)
                {
                    // Retrieve the page.
                    EntityCollection closedOppCollection = _crmService.RetrieveMultiple(closedOpp);
                    if (closedOppCollection.Entities != null)
                    {
                        foreach (var opp in closedOppCollection.Entities)
                        {
                            

                            if (opp.GetAttributeValue<OptionSetValue>("statuscode").Value == 3)
                            {

                                Entity updateTarget = new Entity("opportunity", opp.Id);
                                updateTarget["statecode"] = new OptionSetValue(0); // 0 = Open
                                updateTarget["statuscode"] = new OptionSetValue(1); // 1 = In Progress
                                _crmService.Update(updateTarget);

                                WinOpportunityRequest cancelRequest = new WinOpportunityRequest();
                                cancelRequest.OpportunityClose = new Entity("opportunityclose");
                                cancelRequest.OpportunityClose["opportunityid"] = new EntityReference("opportunity", opp.Id);
                                cancelRequest.Status = new OptionSetValue(3);

                                _crmService.Execute(cancelRequest);

                                



                            }
                            else if (opp.GetAttributeValue<OptionSetValue>("statuscode").Value == 4)
                            {
                                Entity updateTarget = new Entity("opportunity", opp.Id);
                                updateTarget["statecode"] = new OptionSetValue(0); // 0 = Open
                                updateTarget["statuscode"] = new OptionSetValue(1); // 1 = In Progress
                                _crmService.Update(updateTarget);


                                LoseOpportunityRequest fulfillRequest = new LoseOpportunityRequest();
                                fulfillRequest.OpportunityClose = new Entity("opportunityclose");
                                fulfillRequest.OpportunityClose["opportunityid"] = new EntityReference("opportunity", opp.Id);
                                fulfillRequest.Status = new OptionSetValue(4);

                                _crmService.Execute(fulfillRequest);
                             

                            }
                            counter++;
                            if (counter % 500 == 0)
                            {
                                textBox1.AppendText("Group Updated");
                            }
                         
                        }

                    }
                    if (closedOppCollection.MoreRecords)
                    {
                        closedOpp.PageInfo.PageNumber++;
                        closedOpp.PageInfo.PagingCookie = closedOppCollection.PagingCookie;
                    }
                    else
                    {
                        break;
                    }
                }

            }

            textBox1.AppendText("done!!");
        }
     
        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private SqlConnection TryConnect(int maxreconnectretries)
        {
            SqlConnection connection = null;
            string SqlConnectionString = "Data Source=sql-crm2016-9;Initial Catalog=BPSCRMLive_MSCRM;Integrated Security=False; User ID=sa; Password=P@ssw0rd;MultipleActiveResultSets=True";



            int reconnectretries = 0;
            do
            {
                try
                {
                    reconnectretries++;
                    connection = new SqlConnection(SqlConnectionString);
                    connection.Open();
                }
                catch (SqlException exception)
                {
                    throw exception;
                }
            }
            while (reconnectretries < maxreconnectretries && (connection == null || connection.State != ConnectionState.Open));

            return connection;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Console.WriteLine("start");
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            //  string ConnectionString = string.Format("RequireNewInstance=True;AuthType=ClientSecret;url=https://la.crm4.dynamics.com/;ClientId=9b8dc583-a1fe-4b71-a8bd-efe4e4bd8c99;ClientSecret=L7p1N9Vnh2VXhF5Q1ZY6J-~O7J~Dau_4R5");
            Console.WriteLine("connection");
            string ConnectionString = string.Format("RequireNewInstance=True;AuthType=ClientSecret;url=https://bpscrm.crm4.dynamics.com/;ClientId=59a6052e-29ec-4dbb-8b0c-7e233586deeb;ClientSecret=oNm8Q~fFGS5pPgK~HTkYKg0beibWp5yBe1gDzatJ");

            Console.WriteLine("ConnectionString");

            CrmServiceClient _crmService = new CrmServiceClient(ConnectionString);
            Console.WriteLine("_crmService" + _crmService);
           // UserMap = GetUserMap();
            SqlConnection con = TryConnect(3);
            if (con.State != ConnectionState.Open)
            {
                con = TryConnect(3);
            }


            if (con.State == ConnectionState.Open)
            {
                Console.WriteLine("State" + ConnectionState.Open);

                string query = "SELECT bps_countryid , new_programid FROM [BPSCRMLive_MSCRM].[dbo].[new_ServiceProvidersEndCustomersCountry]";
              //  Console.WriteLine("query" + query);

                SqlCommand command = new SqlCommand(query, con);

                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    
                   Console.WriteLine("reader");
                   Guid programId = reader.GetGuid(reader.GetOrdinal("new_programid"));
                  Console.WriteLine("new_programid" + programId);


                    // Guid countryId = reader.GetGuid(reader.GetOrdinal("systemuserid"));
                        Guid countryId =reader.GetGuid(reader.GetOrdinal("bps_countryid")); ;
                        Console.WriteLine("bps_countryid" + countryId);
                      

                        try
                        {
                            // Create entity references
                            EntityReference programRef = new EntityReference("new_program", programId);
                            EntityReference countryRef = new EntityReference("bps_country", countryId);


                            // Create relationship reference (N:N schema name)
                            Relationship relationship = new Relationship("new_ServiceProvidersEndCustomersCountry");

                            // Create the association request
                            AssociateRequest associateRequest = new AssociateRequest
                            {
                                Target = programRef,
                                RelatedEntities = new EntityReferenceCollection { countryRef },
                                Relationship = relationship
                            };


                            // Execute the association
                            _crmService.Execute(associateRequest);

                            textBox1.AppendText($"Associated Country {countryId} to Program {programId}");
                        }


                        catch (FaultException<OrganizationServiceFault> ex)
                        {
                            Console.WriteLine("CRM Fault: " + ex.Message);
                            Console.WriteLine("Detail: " + ex.Detail?.Message);
                            Console.WriteLine("TraceText: " + ex.Detail?.TraceText);
                        }
                    
                }

                    reader.Close();
                
            }

            Console.WriteLine("Done.");





        }

        private int GetML(string id, CrmServiceClient service)
        {
            var count = 0;

            var MLQ = new QueryExpression("list");
            MLQ.Criteria.AddCondition("listid", ConditionOperator.Equal, new Guid(id));

            var ML = service.RetrieveMultiple(MLQ);

            count = ML.Entities.Count;

            return count;

        }

        private string GetEntityName(int code)
        {

            SqlConnection con = TryConnect(3); string entityname = string.Empty;
            if (con.State != ConnectionState.Open)
            {
                con = TryConnect(3);
            }


            if (con.State == ConnectionState.Open)
            {
                SqlDataReader rdr = null;
                var cmd = new SqlCommand(
                                            @"SELECT 
                            DISPLAYNAME.LABEL 'Display Name', 
                            EV.NAME 'Logical Name',
                            ObjectTypeCode 'Object Type Code'
                            FROM 
                            ENTITYVIEW EV INNER JOIN 
                            LOCALIZEDLABELLOGICALVIEW DISPLAYNAME
                            ON (EV.ENTITYID = DISPLAYNAME.OBJECTID) AND (DISPLAYNAME.OBJECTCOLUMNNAME = 'LOCALIZEDNAME')
                            WHERE 
                            objecttypecode =" + code, con);

                try
                {
                    rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        entityname = rdr["Logical Name"] != DBNull.Value ? (string)rdr["Logical Name"] : "";
                    }
                    con.Close();
                }
                catch (Exception e)
                {
                    throw e;
                }
                finally
                {
                    if (rdr != null) rdr.Close();
                }
            }
            return entityname;

        }

        private void button3_Click(object sender, EventArgs e)
        {

            Console.WriteLine("button3");
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
             string ConnectionString = string.Format("AuthType=ClientSecret;url=https://la.crm4.dynamics.com/;ClientId=9b8dc583-a1fe-4b71-a8bd-efe4e4bd8c99;ClientSecret=L7p1N9Vnh2VXhF5Q1ZY6J-~O7J~Dau_4R5");

            //string ConnectionString = string.Format("AuthType=ClientSecret;url=https://bps.api.crm4.dynamics.com/;ClientId=4f4ad400-fd37-4047-961e-ddeb47aecaa5;ClientSecret=DFe-8P.-Ch_1tYD88pQglYczso3po5Wu_9");
            CrmServiceClient _crmService = new CrmServiceClient(ConnectionString);
            if(_crmService.IsReady)
            {
                    QueryExpression accounts = new QueryExpression("account");
                    accounts.Criteria.AddCondition("name",ConditionOperator.NotNull);

                    var coll = _crmService.RetrieveMultiple(accounts);
                    var count = coll.Entities.Count;

            }
        }


        private Dictionary<Guid, Guid> GetUserMap()
        {
            //// BPS
            var UserMap = new Dictionary<Guid, Guid>();
         
            UserMap.Add(new Guid("16616E36-8B08-F011-B82E-000D3AB832BD"), new Guid("d62f6b16-9a08-f011-bae3-000d3a68b8b4"));// ADHWA ALHUSSAN
            UserMap.Add(new Guid("1127E172-2BA4-DD11-9F6E-001E0BD83A6E"), new Guid("74e7f86e-117f-ef11-ac21-000d3ab79fa7"));// Caroline Haddad
            UserMap.Add(new Guid("92300FD7-06D6-EE11-B820-00155D000F4D"), new Guid("893e6f7e-117f-ef11-ac21-000d3ab04806"));//boulous mahfouz
            UserMap.Add(new Guid("FF450333-F726-EE11-B817-00155D000F4D"), new Guid("cb6e1c03-8980-ef11-ac21-002248a12a5f"));//carole
            UserMap.Add(new Guid("2018DC1D-AB7B-E511-BDE6-00155D001305"), new Guid("983e6f7e-117f-ef11-ac21-000d3ab04806"));//christa Abdel Karim
            UserMap.Add(new Guid("692F1CA6-04C4-E111-92B5-001E0BD83A70"), new Guid("cdbddf70-117f-ef11-ac21-000d3ab04806"));// Diana Sidani
            UserMap.Add(new Guid("1237F12E-68C4-DE11-98D6-001E0BD83A70"), new Guid("a73e6f7e-117f-ef11-ac21-000d3ab04806"));// Eliana Iskandar
            UserMap.Add(new Guid("C493D7B2-6480-ED11-B809-00155D000F4D"), new Guid("d0c32b4f-9180-ef11-ac21-000d3ab04806"));//eliane najem
            UserMap.Add(new Guid("0A95E751-2209-EE11-B815-00155D000F4D"), new Guid("3d3e6f7e-117f-ef11-ac21-000d3ab04806"));//Elias Ghaleb
            UserMap.Add(new Guid("973CA593-C56A-EF11-B82A-000D3AB832BD"), new Guid("463e6f7e-117f-ef11-ac21-000d3ab04806"));//Elias tawk
            UserMap.Add(new Guid("98672CE4-1C86-EF11-B82C-000D3AB832BD"), new Guid("82e5118a-afa1-ef11-a72d-0022489d7972"));//Eman A
            UserMap.Add(new Guid("5779B8BD-7379-EE11-B81B-00155D000F4D"), new Guid("413e6f7e-117f-ef11-ac21-000d3ab04806"));//Faten Salhieh
            UserMap.Add(new Guid("345BC7F9-6530-EE11-B817-00155D000F4D"), new Guid("4dbff07a-117f-ef11-ac21-000d3ab79fa7"));//Firas Abdel Dayem
            UserMap.Add(new Guid("217CB9A6-9678-EE11-B81B-00155D000F4D"), new Guid("7a3e6f7e-117f-ef11-ac21-000d3ab04806"));//gaya akiki
            UserMap.Add(new Guid("E8ABB468-1898-EF11-B82C-000D3AB832BD"), new Guid("7a3e6f7e-117f-ef11-ac21-000d3ab04806"));//Hajar AlMulaik
            UserMap.Add(new Guid("D9C650F7-C56A-EF11-B82A-000D3AB832BD"), new Guid("49bef308-bba1-ef11-a72d-0022489d7972"));//Isabelle Francis
            UserMap.Add(new Guid("DF9E510D-4EB9-EE11-B81C-00155D000F4D"), new Guid("5853fd77-117f-ef11-ac21-000d3ab04806"));//julia salameh
            UserMap.Add(new Guid("42D03B70-FF72-EE11-B81A-00155D000F4D"), new Guid("5d53fd77-117f-ef11-ac21-000d3ab04806"));//Kareem Fouly
            UserMap.Add(new Guid("7D15EDC4-3E1E-E511-9864-00155D001305"), new Guid("3f8f557e-afa1-ef11-a72d-000d3add7547"));//Karima Fawaz  - sales
            UserMap.Add(new Guid("209EA46F-314F-EA11-80F0-00155D000F1F"), new Guid("7abff07a-117f-ef11-ac21-000d3ab79fa7"));//lie-marie
            UserMap.Add(new Guid("89A54879-2BA4-DD11-9F6E-001E0BD83A6E"), new Guid("be9f5438-120a-f011-bae2-000d3a68b8b4"));//Leila Neaimeh
            UserMap.Add(new Guid("5F611701-05E5-E111-8878-001E0BD83A70"), new Guid("76e7f86e-117f-ef11-ac21-000d3ab79fa7"));// Lina Agha
            UserMap.Add(new Guid("AA06B2B2-8974-E311-A954-00155D001306"), new Guid("64bff07a-117f-ef11-ac21-000d3ab79fa7"));//Linda
            UserMap.Add(new Guid("5D825613-9352-E311-A954-00155D001306"), new Guid("f3bff07a-117f-ef11-ac21-000d3ab79fa7"));//Majd
            UserMap.Add(new Guid("C2C27F3B-BC8B-E211-AB79-001E0BD83A70"), new Guid("7ebff07a-117f-ef11-ac21-000d3ab79fa7"));//Michel Mikhael*
            UserMap.Add(new Guid("BFDE33BA-D11A-E811-80D7-00155D000F1F"), new Guid("b4bff07a-117f-ef11-ac21-000d3ab79fa7"));//Mohammad Mooti
            UserMap.Add(new Guid("E92FFC20-6938-EB11-8102-00155D000F1F"), new Guid("03d4ed80-117f-ef11-ac21-000d3ab79fa7"));//Monika Kain
            UserMap.Add(new Guid("3698B0B0-2F4E-EA11-80F0-00155D000F1F"), new Guid("153e6f7e-117f-ef11-ac21-000d3ab04806"));//Nadeem Khan
            UserMap.Add(new Guid("FED66351-D9B7-EB11-8103-00155D000F1F"), new Guid("b1e7468d-9580-ef11-ac21-000d3ab04806"));//Nadine Akl
            UserMap.Add(new Guid("1927E172-2BA4-DD11-9F6E-001E0BD83A6E"), new Guid("fcbddf70-117f-ef11-ac21-000d3ab04806"));//Negib abouhabib
            UserMap.Add(new Guid("FE2E822D-BDC2-E711-80D3-00155D000F1F"), new Guid("493e6f7e-117f-ef11-ac21-000d3ab04806"));//Nicole
            UserMap.Add(new Guid("C1C450B2-915E-ED11-B808-00155D000F4D"), new Guid("d7bff07a-117f-ef11-ac21-000d3ab79fa7"));//Nour K
            UserMap.Add(new Guid("01952D9C-2409-EE11-B815-00155D000F4D"), new Guid("1bc0f07a-117f-ef11-ac21-000d3ab79fa7"));//Orilka Al Selfany
            UserMap.Add(new Guid("DE521661-B1B0-EF11-B82C-000D3AB832BD"), new Guid("de24045d-7305-f011-bae2-7c1e5288140d"));//Paresh
            UserMap.Add(new Guid("6108C71C-2409-EE11-B815-00155D000F4D"), new Guid("93bff07a-117f-ef11-ac21-000d3ab79fa7"));//Peter A
            UserMap.Add(new Guid("4B9481A9-D159-ED11-B808-00155D000F4D"), new Guid("c5cb40c5-9580-ef11-ac21-000d3ab04806"));//petra nassar
            UserMap.Add(new Guid("9C39AE4B-A27B-E511-BDE6-00155D001305"), new Guid("00bedf70-117f-ef11-ac21-000d3ab04806"));//Rafca Andari
            UserMap.Add(new Guid("1527E172-2BA4-DD11-9F6E-001E0BD83A6E"), new Guid("0cf7d11e-65a2-ef11-a72d-0022489d7972"));//Rasha 
            UserMap.Add(new Guid("4704DEEB-47E8-E611-80C2-00155D000F1F"), new Guid("2aab02ba-7f79-ef11-ac21-0022489b6da8"));//Rayan Fakhro
            UserMap.Add(new Guid("39089D14-23D2-EE11-B820-00155D000F4D"), new Guid("f7bff07a-117f-ef11-ac21-000d3ab79fa7"));//Refaat Mansour
            UserMap.Add(new Guid("406EED15-8ADF-EE11-B820-00155D000F4D"), new Guid("c0bff07a-117f-ef11-ac21-000d3ab79fa7"));//Riddhi Anilkumar
            UserMap.Add(new Guid("5E5FE01C-3FD2-EF11-B82D-000D3AB832BD"), new Guid("3c90c658-7305-f011-bae3-000d3ad7e052"));//sharmila
            UserMap.Add(new Guid("E4894606-4C8F-EE11-B81C-00155D000F4D"), new Guid("e8bff07a-117f-ef11-ac21-000d3ab79fa7"));//soumia
            UserMap.Add(new Guid("53A44432-706B-EB11-8103-00155D000F1F"), new Guid("cebddf70-117f-ef11-ac21-000d3ab04806"));//Sahar Kahawani
            UserMap.Add(new Guid("2B0EFE4A-D777-EE11-B81B-00155D000F4D"), new Guid("8261a4c4-9780-ef11-ac21-000d3ab79fa7"));//sawsan saadeh
            UserMap.Add(new Guid("2C10BBDD-E0C0-EE11-B81D-00155D000F4D"), new Guid("c70dde89-9880-ef11-ac21-000d3ab04806"));//Taif
            UserMap.Add(new Guid("0DC71FEA-61DA-EB11-8106-00155D000F1F"), new Guid("d0eeae79-9880-ef11-ac21-000d3ab04806"));//Tony
            UserMap.Add(new Guid("BC4813EE-3BA2-ED11-B80D-00155D000F4D"), new Guid("58e0f1c7-9880-ef11-ac21-000d3ab04806"));//Vidhi
            UserMap.Add(new Guid("C1FAA4C8-2928-ED11-B806-00155D000F4D"), new Guid("1d3e6f7e-117f-ef11-ac21-000d3ab04806"));//Youssef Kassab
            UserMap.Add(new Guid("A87C5F6A-2BA4-DD11-9F6E-001E0BD83A6E"), new Guid("4953fd77-117f-ef11-ac21-000d3ab04806"));//CRM Admin

            /* disabled users */
            //UserMap.Add(new Guid("13B8CC65-2AA7-EB11-8103-00155D000F1F"), new Guid("13b8cc65-2aa7-eb11-8103-00155d000f1f"));//ayaz khan
            //UserMap.Add(new Guid("061FA80F-A707-E011-BCF8-001E0BD83A70"), new Guid("061fa80f-a707-e011-bcf8-001e0bd83a70"));//hadi khalaf
            //UserMap.Add(new Guid("E8EB6B58-1807-DE11-9F42-001E0BD83A70"), new Guid("36b909b0-bd9e-ec11-b400-000d3ad7b252"));//bps office1 to services
            //UserMap.Add(new Guid("E815DE00-3DC3-E411-B94C-00155D001305"), new Guid("e815de00-3dc3-e411-b94c-00155d001305"));//ange
            //UserMap.Add(new Guid("A87C5F6A-2BA4-DD11-9F6E-001E0BD83A6E"), new Guid("4953fd77-117f-ef11-ac21-000d3ab04806"));//CRM Admin
            //UserMap.Add(new Guid("FC902350-D745-E111-A428-001E0BD83A70"), new Guid("fc902350-d745-e111-a428-001e0bd83a70"));//joseph abouatmeh
            //UserMap.Add(new Guid("21E50D26-2A55-ED11-B808-00155D000F4D"), new Guid("841293db-7048-ed11-bba2-0022489e24bb"));//ghassan
            //UserMap.Add(new Guid("9196009D-74A2-E511-BDE6-00155D001305"), new Guid("36b909b0-bd9e-ec11-b400-000d3ad7b252"));//bps host to services
            //UserMap.Add(new Guid("A2C39ED3-583E-E411-A402-00155D001305"), new Guid("a2c39ed3-583e-e411-a402-00155d001305"));//serge
            //UserMap.Add(new Guid("05BCB5DB-F347-E611-9DB4-00155D000F04"), new Guid("05bcb5db-f347-e611-9db4-00155d000f04"));//adxadmin
            //UserMap.Add(new Guid("AB722326-ED8F-ED11-B809-00155D000F4D"), new Guid("12dd0936-8671-ed11-9561-0022489e2596"));//ali
            //UserMap.Add(new Guid("53F33A19-026E-E711-80C4-00155D000F1F"), new Guid("a2ea53ae-e665-ef11-bfe2-000d3ab01665"));//gisele
            //UserMap.Add(new Guid("89ED72D7-CB5A-E511-8CA3-00155D001305"), new Guid("89ed72d7-cb5a-e511-8ca3-00155d001305"));//sarah
            //UserMap.Add(new Guid("753D0730-117A-E411-9C30-00155D001305"), new Guid("36b909b0-bd9e-ec11-b400-000d3ad7b252"));//layal to services
            //UserMap.Add(new Guid("25D3D2BA-583E-E411-A402-00155D001305"), new Guid("25d3d2ba-583e-e411-a402-00155d001305"));//christopher
            //UserMap.Add(new Guid("2BE0FAB5-5346-E411-A402-00155D001305"), new Guid("2be0fab5-5346-e411-a402-00155d001305"));//joy hayek
            //UserMap.Add(new Guid("B3FF4D6F-20C3-E411-B94C-00155D001305"), new Guid("b3ff4d6f-20c3-e411-b94c-00155d001305"));//maria
            //UserMap.Add(new Guid("FF33A1E0-2B41-E311-A954-00155D001306"), new Guid("ff33a1e0-2b41-e311-a954-00155d001306"));//stephanie
            //UserMap.Add(new Guid("15692AEE-2366-E311-A954-00155D001306"), new Guid("15692aee-2366-e311-a954-00155d001306"));//rana
            //UserMap.Add(new Guid("B2BA0CD0-449D-E311-A954-00155D001306"), new Guid("b2ba0cd0-449d-e311-a954-00155d001306"));//rihani
            //UserMap.Add(new Guid("85A54879-2BA4-DD11-9F6E-001E0BD83A6E"), new Guid("85a54879-2ba4-dd11-9f6e-001e0bd83a6e"));//ibrahim farah
            //UserMap.Add(new Guid("8DA54879-2BA4-DD11-9F6E-001E0BD83A6E"), new Guid("8da54879-2ba4-dd11-9f6e-001e0bd83a6e"));//Michele Halaby
            //UserMap.Add(new Guid("99A54879-2BA4-DD11-9F6E-001E0BD83A6E"), new Guid("99a54879-2ba4-dd11-9f6e-001e0bd83a6e"));//cynethia
            //UserMap.Add(new Guid("C922A0E6-B251-DE11-844A-001E0BD83A70"), new Guid("c922a0e6-b251-de11-844a-001e0bd83a70"));//jack
            //UserMap.Add(new Guid("3F3F4915-4170-E111-92B4-001E0BD83A70"), new Guid("3f3f4915-4170-e111-92b4-001e0bd83a70"));//lama yamout
            //UserMap.Add(new Guid("2BBDF35A-D31C-E111-976D-001E0BD83A70"), new Guid("2bbdf35a-d31c-e111-976d-001e0bd83a70"));//demo
            //UserMap.Add(new Guid("703BA76B-AA36-E111-A428-001E0BD83A70"), new Guid("703ba76b-aa36-e111-a428-001e0bd83a70"));//ali yassine
            //UserMap.Add(new Guid("0ECDC2EB-D745-E111-A428-001E0BD83A70"), new Guid("0ECDC2EB-D745-E111-A428-001E0BD83A70"));//carl
            //UserMap.Add(new Guid("8625038C-7247-E111-A428-001E0BD83A70"), new Guid("996f8443-50e8-e111-8878-001e0bd83a70"));//bcc3
            //UserMap.Add(new Guid("EBD85A93-BA81-E211-AB79-001E0BD83A70"), new Guid("ebd85a93-ba81-e211-ab79-001e0bd83a70"));//rabah
            //UserMap.Add(new Guid("3422F2E9-B3DF-E211-B2C9-001E0BD83A70"), new Guid("3422f2e9-b3df-e211-b2c9-001e0bd83a70"));//joseph sabbagh
            //UserMap.Add(new Guid("7E6C6F14-6B4E-E111-BA9C-001E0BD83A70"), new Guid("7e6c6f14-6b4e-e111-ba9c-001e0bd83a70"));//bps spla
            //UserMap.Add(new Guid("A9FBB8EC-A607-E011-BCF8-001E0BD83A70"), new Guid("a9fbb8ec-a607-e011-bcf8-001e0bd83a70"));//sanaa

            return UserMap;
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }
    }
}

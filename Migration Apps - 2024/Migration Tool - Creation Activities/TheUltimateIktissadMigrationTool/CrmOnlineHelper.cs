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
using Microsoft.Xrm.Sdk.Metadata.Query;
using System.Threading;



namespace TheUltimateIktissadMigrationTool
{
    public class CrmOnlineHelper
    {
        public CrmServiceClient _crmService;
        public TheUltimateTool thisForm;
        public CrmOnlineMetadataHelper Metadata;
        int gcounter = 0;

        public CrmOnlineHelper(TheUltimateTool tool)
        {


            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            string ConnectionString = string.Format("RequireNewInstance=True;AuthType=ClientSecret;url=https://bpscrm.crm4.dynamics.com/;ClientId=59a6052e-29ec-4dbb-8b0c-7e233586deeb;ClientSecret=oNm8Q~fFGS5pPgK~HTkYKg0beibWp5yBe1gDzatJ");

            _crmService = new CrmServiceClient(ConnectionString);
            thisForm = tool;
            Metadata = null;

        }

        //Fetch all the online Guids and status by looping through all the pages with 5000records/page
        public DataTable GetCRMOnlineRecords()
        {
            try
            {
                //Create Target DataTable
                string EntityType = TheUltimateTool.EntityType;
                DataTable DT = new DataTable();
                string EntityTypeId = "activity" + "id";
                DT.Columns.Add(EntityTypeId, typeof(Guid));

                if (!TheUltimateTool.NoStateCodeEntites.Contains(EntityType.ToLower()))
                {
                    DT.Columns.Add("statecode", typeof(int));
                    DT.Columns.Add("statuscode", typeof(int));
                }

                //Create Query
                int queryCount = 5000;

                int pageNumber = 1;

                OrderExpression order = new OrderExpression();
                order.AttributeName = EntityTypeId;
                order.OrderType = OrderType.Ascending;

                QueryExpression pagequery = new QueryExpression();
                pagequery.EntityName = EntityType;
                pagequery.Orders.Add(order);
                pagequery.ColumnSet.AddColumns(EntityTypeId);

                if (!TheUltimateTool.NoStateCodeEntites.Contains(EntityType.ToLower()))
                    pagequery.ColumnSet.AddColumns("statecode", "statuscode");

                FilterExpression filter = new FilterExpression(LogicalOperator.Or);

                pagequery.Criteria.AddFilter(filter);

                pagequery.PageInfo = new PagingInfo();
                pagequery.PageInfo.Count = queryCount;
                pagequery.PageInfo.PageNumber = pageNumber;

                pagequery.PageInfo.PagingCookie = null;
                if (_crmService.IsReady)
                {
                while (true)
                {

                    EntityCollection results = _crmService.RetrieveMultiple(pagequery);
                    if (results.Entities != null)
                    {
                        //For Each record found fill a datatable row
                        foreach (Entity Entity in results.Entities)
                        {
                            DataRow dRow = DT.NewRow();
                            dRow[EntityTypeId] = Entity.GetAttributeValue<Guid>(EntityTypeId);

                            if (!TheUltimateTool.NoStateCodeEntites.Contains(EntityType.ToLower()))
                            {
                                dRow["statecode"] = Entity.GetAttributeValue<OptionSetValue>("statecode").Value;
                                dRow["statuscode"] = Entity.GetAttributeValue<OptionSetValue>("statuscode").Value;
                            }

                            DT.Rows.Add(dRow);
                        }
                    }

                    // Go to the next page
                    if (results.MoreRecords)
                    {
                        pagequery.PageInfo.PageNumber++;
                        pagequery.PageInfo.PagingCookie = results.PagingCookie;
                    }
                    else
                    {
                        // No more Records. Exit loop
                        break;
                    }
                }
            }

                return DT;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void CreateMissingRecordsOld(IEnumerable<DataRow> Rows, DataTable DTCsv)
        {
            if (_crmService.IsReady)
            {
                //Fetch Metadata (Attribute Types, Lookup Targets, Option Set Values...)
                if (Metadata == null || Metadata.EntityType != TheUltimateTool.EntityType)
                    Metadata = new CrmOnlineMetadataHelper(_crmService);

                //Get starting and ending index
                int updateStart = thisForm.GetStart();
                int updateEnd = thisForm.GetEnd();

                int counter = updateStart;

                ExecuteMultipleRequest MultipleRequest = null;

                for (int i = updateStart; i < Math.Min(Rows.Count(), updateEnd); i++)
                {
                    DataRow row = Rows.ElementAt(i);

                    if (MultipleRequest == null)
                        MultipleRequest = InitializeRequest();

                    try
                    {
                        // Create the Entity, Map the values from the SQL, Create
                        Entity entity;

                        entity = PrepareEntity(row, DTCsv, Metadata);
                        counter++;

                        CreateRequest updateRequest = new CreateRequest { Target = entity };
                        MultipleRequest.Requests.Add(updateRequest);

                        if (counter % 999 == 0)
                        {
                            ExecuteBulk(MultipleRequest);
                            MultipleRequest = null;
                            thisForm.RefreshCounterText(counter);

                           
                        }
                    }
                    catch (Exception ex)
                    {
                        thisForm.AppendText(ex.Message);
                    }
                }
                ExecuteBulk(MultipleRequest);
            }

        }
        public void CreateMissingRecords(IEnumerable<DataRow> Rows, DataTable DTCsv)
        {
            int x = 0;
            x = 1;
            if (_crmService.IsReady)
            {
                //CrmOnlineMetadataHelper Metadata=null;
                //Fetch Metadata (Attribute Types, Lookup Targets, Option Set Values...)
                if (Metadata == null || Metadata.EntityType != TheUltimateTool.EntityType)
                    Metadata = new CrmOnlineMetadataHelper(_crmService);


                //Get starting and ending index
                int updateStart = thisForm.GetStart();
                int updateEnd = thisForm.GetEnd();
                int counter = updateStart;

                ExecuteMultipleRequest MultipleRequest = null;

                for (int i = updateStart; i < Math.Min(Rows.Count(), updateEnd); i++)
                {
                    DataRow row = Rows.ElementAt(i);

                    if (MultipleRequest == null)
                        MultipleRequest = InitializeRequest();

                    try
                    {
                        // Create the Entity, Map the values from the SQL, Create
                        Entity entity;
                        string threadName = Thread.CurrentThread.Name;
                        entity = PrepareEntity(row, DTCsv, Metadata);
                        counter++;

                        CreateRequest updateRequest = new CreateRequest { Target = entity };
                        MultipleRequest.Requests.Add(updateRequest);
                        if (counter % 100 == 0)
                        {

                            var MultipleRequestEx = MultipleRequest;
                            ExecuteBulk(MultipleRequestEx);
                            MultipleRequest = null;
                            gcounter += 100;
                            thisForm.RefreshCounterText(counter);

                        }
                      

                    }
                    catch (Exception ex)
                    {
                        thisForm.AppendText(ex.Message);
                    }
                }

                ExecuteBulk(MultipleRequest);
            }
            else
            {
                int y = 0;
            }
        }
      
        public async void CreateMissingRecords2(IEnumerable<DataRow> Rows, DataTable DTCsv)
        {
            var splitedList = Rows
                                .Select((x, i) => new { Index = i, Value = x })
                                .GroupBy(x => x.Index / 60000)
                                .Select(x => x.Select(v => v.Value).ToList())
                                .ToList();

            List<Task> lsttask = new List<Task>();
            foreach (var item in splitedList)
            {
                lsttask.Add(Task.Factory.StartNew(() => CreateMissingRecords(item, DTCsv)));
                if (lsttask.Count() > 3)
                {
                    await Task.WhenAll(lsttask.ToArray());
                    lsttask.Clear();
                }

            }

            await Task.WhenAll(lsttask.ToArray());

        }

        public void UpdateRecords(IEnumerable<DataRow> Rows, DataTable DTCsv)
        {
            if (_crmService.IsReady)
            {
                if (Metadata == null || Metadata.EntityType != TheUltimateTool.EntityType)
                    Metadata = new CrmOnlineMetadataHelper(_crmService);

                int updateStart = thisForm.GetStart();
                int updateEnd = thisForm.GetEnd();

                int counter = updateStart;

                ExecuteMultipleRequest MultipleRequest = null;

                for (int i = updateStart; i < Math.Min(Rows.Count(), updateEnd); i++)
                {
                    DataRow row = Rows.ElementAt(i);

                    if (MultipleRequest == null)
                        MultipleRequest = InitializeRequest();

                    try
                    {
                        Entity entity;

                        entity = PrepareEntity(row, DTCsv,Metadata);
                        counter++;

                        UpdateRequest updateRequest = new UpdateRequest { Target = entity };
                        MultipleRequest.Requests.Add(updateRequest);

                        if (counter % 999 == 0)
                        {
                            gcounter += 999;
                            thisForm.RefreshCounterText(gcounter);
                            ExecuteBulk(MultipleRequest);
                            MultipleRequest = null;
                        }
                    }
                    catch (Exception ex)
                    {
                        thisForm.AppendText(ex.Message);
                    }
                }
                ExecuteBulk(MultipleRequest);
            }
        }
        public void AssignRecords(IEnumerable<DataRow> Rows, DataTable DTCsv)
        {
            if (_crmService.IsReady)
            {
                string EntityType = TheUltimateTool.EntityType;

                if (Metadata == null || Metadata.EntityType != TheUltimateTool.EntityType)
                    Metadata = new CrmOnlineMetadataHelper(_crmService);

                int assignStart = thisForm.GetStart();
                int assignEnd = thisForm.GetEnd();

                int counter = assignStart;

                ExecuteMultipleRequest MultipleRequest = null;

                for (int i = assignStart; i < Math.Min(Rows.Count(), assignEnd); i++)
                {
                    DataRow row = Rows.ElementAt(i);

                    if (MultipleRequest == null)
                        MultipleRequest = InitializeRequest();

                    try
                    {
                        var guid = row.Field<Guid>("ownerid");
                        if (Metadata.UserMap.ContainsKey(guid))
                        {
                            guid = Metadata.UserMap[guid];
                            EntityReference Assignee = new EntityReference("systemuser", guid);


                            AssignRequest assignRequest = new AssignRequest
                            {
                                Assignee = Assignee,
                                Target = new EntityReference(EntityType, row.Field<Guid>(0))
                            };

                            MultipleRequest.Requests.Add(assignRequest);
                            counter++;
                        }

                        if (counter > 0 && counter % 50 == 0)
                        {
                            ExecuteBulk(MultipleRequest);
                            MultipleRequest = null;
                        }
                    }
                    catch (Exception ex)
                    {
                        thisForm.AppendText(ex.Message);
                    }
                }
                ExecuteBulk(MultipleRequest);
            }
        }

        public void DeleteDifference(IEnumerable<DataRow> Rows)
        {
            string EntityType = TheUltimateTool.EntityType;
            int counter = 0;
            if (_crmService.IsReady)
            {
                foreach (DataRow row in Rows)
                {
                    _crmService.Delete(EntityType, row.Field<Guid>("activityid"));
                    thisForm.AppendText(++counter + " of " + Rows.Count() + " record id = " + row[0].ToString() + " was deleted successfully. \r\n");
                }
            }
        }


        public void SetRecordsStatus(IEnumerable<DataRow> Rows)
        {
            int updateStart = thisForm.GetStart();
            int updateEnd = thisForm.GetEnd();

            string EntityType = TheUltimateTool.EntityType;

            int counter = updateStart;

            ExecuteMultipleRequest MultipleRequest = null;

            for (int i = updateStart; i < Math.Min(Rows.Count(), updateEnd); i++)
            {
                DataRow row = Rows.ElementAt(i);

                try
                {

                    if (MultipleRequest == null)
                        MultipleRequest = InitializeRequest();

                    Entity ent = new Entity(EntityType);

                    if (row["statecode"] == DBNull.Value)
                        continue;

                    if (EntityType != "salesorder" ||
                        EntityType == "salesorder" && row.Field<int>("statecode") != 2 && row.Field<int>("statecode") != 3)
                    {
                        ent["activity" + "id"] = row.Field<Guid>("activity" + "id");
                        ent["statecode"] = new OptionSetValue(row.Field<int>("statecode"));

                        //In case the CRM contains non matching status and status reason values. I got this case in CRM.
                        if (row.Field<int>("statecode") == 1 && row.Field<int>("statuscode") == 1)
                            ent["statuscode"] = new OptionSetValue(2);
                        else
                            ent["statuscode"] = new OptionSetValue(row.Field<int>("statuscode"));

                        UpdateRequest updateRequest = new UpdateRequest { Target = ent };
                        MultipleRequest.Requests.Add(updateRequest);
                    }
                    else //In the case of order. For the Order fulfillment and cancellation the standard update does not work
                    {
                        if (row.Field<int>("statecode") == 2)
                        {
                            CancelSalesOrderRequest cancelRequest = new CancelSalesOrderRequest();
                            cancelRequest.OrderClose = new Entity("orderclose");
                            cancelRequest.OrderClose["salesorderid"] = new EntityReference("salesorder", row.Field<Guid>(EntityType + "id"));
                            cancelRequest.Status = new OptionSetValue(row.Field<int>("statuscode"));

                            MultipleRequest.Requests.Add(cancelRequest);
                        }
                        else if (row.Field<int>("statecode") == 3)
                        {
                            FulfillSalesOrderRequest fulfillRequest = new FulfillSalesOrderRequest();
                            fulfillRequest.OrderClose = new Entity("orderclose");
                            fulfillRequest.OrderClose["salesorderid"] = new EntityReference("salesorder", row.Field<Guid>(EntityType + "id"));
                            fulfillRequest.Status = new OptionSetValue(row.Field<int>("statuscode"));

                            MultipleRequest.Requests.Add(fulfillRequest);

                        }


                    }


                    counter++;

                    if (counter % 500 == 0)
                    {
                        ExecuteBulk(MultipleRequest);
                        MultipleRequest = null;
                    }
                }
                catch (Exception ex)
                {
                    thisForm.AppendText(ex.Message);
                }
            }

            ExecuteBulk(MultipleRequest);
        }

        public ExecuteMultipleRequest InitializeRequest()
        {
            ExecuteMultipleRequest MultipleRequest = new ExecuteMultipleRequest()
            {
                // Set the execution behavior to not continue after the first error is received
                // and to not return responses.
                Settings = new ExecuteMultipleSettings()
                {
                    ContinueOnError = true,
                    ReturnResponses = false
                },
                Requests = new OrganizationRequestCollection()
            };
            return MultipleRequest;
        }


        public void ExecuteBulk(ExecuteMultipleRequest MultipleRequest)
        {
            if (MultipleRequest == null || MultipleRequest.Requests.Count == 0)
                return;
            int retry = 0;
            ExecuteMultipleResponse response = null;

            while (retry < 3)
            {
                try
                {
                    if (_crmService.IsReady)
                    {
                        response =
                                      (ExecuteMultipleResponse)_crmService.Execute(MultipleRequest);
                        retry = 3;
                    }
                }
                catch (Exception ex)
                {
                    retry++;
                }
            }



            // There should be no responses except for those that contain an error. 
            if (response.Responses.Count > 0)
            {
                if (response.Responses.Count < MultipleRequest.Requests.Count)
                {
                    thisForm.AppendText("Response collection contain a mix of successful response objects and errors.");
                }
                var errorBuilder = new StringBuilder();

                //UNCOMMENT THESE!!!!
                //THESE SHOULD BE UNCOMMENTED TO GET THE ERRORS
                //foreach (var responseItem in response.Responses)
                //{
                    //if (responseItem.Fault != null)
                        //errorBuilder.AppendLine(responseItem.Fault.Message);
                //}

                //if (errorBuilder.Length > 0)
                    //thisForm.AppendText(errorBuilder.ToString());
            }
            else
            {
                // No errors means all transactions are successful.
                thisForm.AppendText("All records have been updated successfully.");
            }
        }
        // Read each column, fill the data from the SQL type according to its type
        // Consider special lookup cases, like currency or unit...
        public Entity PrepareEntity(DataRow row, DataTable DTCsv, CrmOnlineMetadataHelper Metadata)
        {
            string EntityType = TheUltimateTool.EntityType;
            Entity entity = new Entity(EntityType);

            entity.Id = (Guid)row[0];

            if (!TheUltimateTool.NoStateCodeEntites.Contains(EntityType.ToLower()))
                entity["overriddencreatedon"] = row["createdon"];

            for (int i = 1; i < DTCsv.Rows.Count + 1; i++)
            {

                // Get Field Name
                string targetName = DTCsv.Rows[i - 1].Field<string>("targetname").ToLower().Trim();

                // Skip statecode and status code because they have no effect in updates and creates.
                if (targetName == "statecode" || targetName == "statuscode")
                    continue;

                if (row[i] != DBNull.Value)
                {
                    if (row[i] is Guid)
                    {
                        EntityReference ER;

                        if (Metadata.LookupTargets[targetName] == "transactioncurrency")
                        {
                            var guid = GetTransactionNewGuid(row.Field<Guid>(i));
                            ER = new EntityReference(Metadata.LookupTargets[targetName], guid);
                        }
                        else if (Metadata.LookupTargets[targetName] == "uom")
                        {
                            var guid = row.Field<Guid>(i);
                            if (Metadata.UomMap.ContainsKey(guid))
                                guid = Metadata.UomMap[row.Field<Guid>(i)];
                            ER = new EntityReference(Metadata.LookupTargets[targetName], guid);
                        }
                        else if (Metadata.LookupTargets[targetName] == "systemuser")
                        {
                            var guid = row.Field<Guid>(i);
                            if (Metadata.UserMap.ContainsKey(guid))
                            {
                                guid = Metadata.UserMap[row.Field<Guid>(i)];
                                ER = new EntityReference(Metadata.LookupTargets[targetName], guid);
                            }
                            else
                            {
                                continue;
                            }
                        }
                        else if (Metadata.LookupTargets[targetName] == "uomschedule")
                        {
                            var guid = row.Field<Guid>(i);
                            guid = GetUserScheduleMap(guid);
                            ER = new EntityReference(Metadata.LookupTargets[targetName], guid);
                        }
                        else if (Metadata.LookupTargets[targetName] == "customer")
                        {
                            string entityTargetType = DTCsv.Rows[i - 1].Field<string>("targetentitytype").ToLower().Trim();
                            ER = new EntityReference(entityTargetType, row.Field<Guid>(i));

                            if (entityTargetType == "customer")
                            {
                                ER = new EntityReference(Metadata.CustomerLookups[row.Field<int>(targetName + "Type")], row.Field<Guid>(i));
                            }
                        }
                        else if (Metadata.LookupTargets[targetName] == "regarding")
                        {
                            int typecode = row.Field<int>("RegardingObjectTypeCode");
                            //string logicalname = Metadata.ObjectTypeCodeToEntityName[row.Field<int>("RegardingObjectTypeCode")].ToLower();
                            string logicalname = GetEntityName(typecode).ToLower();
                           

                            ER = new EntityReference(logicalname, row.Field<Guid>(i));
                        }
                        else
                        {
                            ER = new EntityReference(Metadata.LookupTargets[targetName], row.Field<Guid>(i));
                        }

                        entity[targetName] = ER;
                    }
                    else if (row[i] is Decimal)
                    {
                        if (Metadata.Attributes[targetName] == typeof(MoneyAttributeMetadata))
                            entity[targetName] = new Money(row.Field<decimal>(i));
                        else
                            entity[targetName] = row.Field<decimal>(i);
                    }
                    //else if (row[i] is bool || row[i] is string)
                    //{
                    //    entity[targetName] = row[i];
                    //}
                    else if (row[i] is bool || row[i] is string || row[i] is DateTime)
                    {
                        if (targetName == "subject")
                        {
                            var name = row[i];
                        }
                        
                         entity[targetName] = row[i];

                    }
                    //else if (row[i] is DateTime)
                    //{
                    //    TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
                    //    DateTime cstTime = TimeZoneInfo.ConvertTimeFromUtc(row.Field<DateTime>(i), cstZone);
                    //    entity[targetName] = cstTime;
                    //}
                    else if (row[i] is int)
                    {
                        if (Metadata.Attributes[targetName] == typeof(PicklistAttributeMetadata))
                        {
                            int optionValue = row.Field<int>(i);
                            if (Metadata.OptionSetValues[targetName].Contains(optionValue))
                                entity[targetName] = new OptionSetValue(row.Field<int>(i));
                        }
                        else
                        {
                            entity[targetName] = row[i];
                        }
                    }
                }
                else
                {
                    /////////////////////////// USED FROM CRM4 TO MAKE SURE ACCOUNT & CONTACT ARE NULL BEFORE CUSTOMER CLEAR/////////////////
                    //// Clear Customer field given that the same field depends on two others.
                    //if (row.Table.Columns[i].DataType == typeof(Guid) && Metadata.LookupTargets[targetName] == "customer")
                    //{
                    //    var AccConRes = from r in DTCsv.AsEnumerable()
                    //                    where r.Field<string>("targetname").ToLower().Trim() == targetName
                    //                    && r.Field<string>("sourcename") != DTCsv.Rows[i - 1].Field<string>("sourcename")
                    //                    select r.Field<string>("sourcename");

                    //    if (AccConRes.Count() == 0 || AccConRes.Count() > 0 && row[AccConRes.ElementAt(0)] == DBNull.Value)
                    //        entity[targetName] = null;
                    //}
                    //else
                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                    entity[targetName] = null;
                }

            }
            return entity;
        }

          
            public Guid GetTransactionNewGuid(Guid OldGuid)
            {
                //if (OldGuid == new Guid("84289EA9-36DF-E711-A2CF-00155D84DA80"))
                //    return new Guid("3D2A4F31-CEB6-EC11-983F-000D3A2ED6E2");
                if (OldGuid == new Guid("246ae36f-c1e9-dc11-96ff-000476f463c4"))
                    return new Guid("124AF334-E379-EF11-AC21-0022489B6DA8");
                else
                    return OldGuid;

            }

        private Guid GetUserScheduleMap(Guid OldGuid)
        {
           
            //Dolar
            if (OldGuid == new Guid("36B5EB2C-117F-491F-9C7D-E7003544057D"))
                return new Guid("4508341c-9838-4cf9-9683-af3bc2827836");
            else
                return OldGuid;
        }
    
        public string GetEntityName(int code)
        {

            SqlConnection con = new SqlConnection(TheUltimateTool.SqlConnectionString);
            string entityname = string.Empty;
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

            private SqlConnection TryConnect(int maxreconnectretries)
        {
            SqlConnection connection = null;
            int reconnectretries = 0;
            do
            {
                try
                {
                    reconnectretries++;
                    connection = new SqlConnection(TheUltimateTool.SqlConnectionString);
                    connection.Open();
                }
                catch (SqlException exception)
                {
                }
            }
            while (reconnectretries < maxreconnectretries && (connection == null || connection.State != ConnectionState.Open));

            return connection;
        }
      
       
    }
}


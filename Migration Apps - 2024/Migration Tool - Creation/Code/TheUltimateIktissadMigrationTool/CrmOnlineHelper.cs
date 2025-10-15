using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
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


namespace TheUltimateIktissadMigrationTool
{
    public class CrmOnlineHelper
    {
     

        public CrmServiceClient _crmService;
        public TheUltimateTool thisForm;
        public static CrmOnlineMetadataHelper Metadata;
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
                string EntityTypeId = EntityType + "id";
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

                while (true)
                {
                    EntityCollection results = null;
                    if (_crmService.IsReady)
                    {
                        results = _crmService.RetrieveMultiple(pagequery);
                                        
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

                return DT;

            }
            catch (Exception ex)
            {
                throw ex;
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


                ////Get starting and ending index
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

                            thisForm.RefreshCounterText(gcounter);

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
        public async void UpdateRecords2(IEnumerable<DataRow> Rows, DataTable DTCsv)
        {
            var splitedList = Rows
                                .Select((x, i) => new { Index = i, Value = x })
                                .GroupBy(x => x.Index / 60000)
                                .Select(x => x.Select(v => v.Value).ToList())
                                .ToList();

            List<Task> lsttask = new List<Task>();
            foreach (var item in splitedList)
            {
                lsttask.Add(Task.Factory.StartNew(() => UpdateRecords(item, DTCsv)));
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

                    entity = PrepareEntity(row, DTCsv, Metadata);
                    counter++;

                    UpdateRequest updateRequest = new UpdateRequest { Target = entity };
                    MultipleRequest.Requests.Add(updateRequest);
                    if (counter % 100 == 0)
                    {

                        var MultipleRequestEx = MultipleRequest;
                        ExecuteBulk(MultipleRequestEx);
                        MultipleRequest = null;
                        gcounter += 100;

                        thisForm.RefreshCounterText(gcounter);

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

            string EntityType = TheUltimateTool.EntityType;
            if (_crmService.IsReady)
            {
                if (Metadata == null || Metadata.EntityType != TheUltimateTool.EntityType)
                    Metadata = new CrmOnlineMetadataHelper(_crmService);


            int assignStart = thisForm.GetStart();
            int assignEnd = thisForm.GetEnd();

            int counter = assignStart;

            ExecuteMultipleRequest MultipleRequest = null;

            for (int i = assignStart; i < Math.Min(Rows.Count(), assignEnd); i++)
            {
                DataRow row = Rows.ElementAt(i);
                //Logger.Log(DateTime.Now.ToString() + " Prepare Entity " + i + "/60000, " + gcounter);

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
                        //Logger.Log(DateTime.Now.ToString() + " Start updating on CRM, " + gcounter);
                        gcounter += 50;
                      

                        thisForm.RefreshCounterText(gcounter);
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
        public async void AssignRecords2(IEnumerable<DataRow> Rows, DataTable DTCsv)
        {
            gcounter = 0;
            var splitedList = Rows
                                .Select((x, i) => new { Index = i, Value = x })
                                .GroupBy(x => x.Index / 60000)
                                .Select(x => x.Select(v => v.Value).ToList())
                                .ToList();

            List<Task> lsttask = new List<Task>();
            foreach (var item in splitedList)
            {
                lsttask.Add(Task.Factory.StartNew(() => AssignRecords(item, DTCsv)));

                if (lsttask.Count() > 3)
                {
                    await Task.WhenAll(lsttask.ToArray());
                    lsttask.Clear();
                }

            }
            await Task.WhenAll(lsttask.ToArray());

        }
        public async void DeleteDifference2(IEnumerable<DataRow> Rows)
        {
            gcounter = 0;
            var splitedList = Rows
                                .Select((x, i) => new { Index = i, Value = x })
                                .GroupBy(x => x.Index / 60000)
                                .Select(x => x.Select(v => v.Value).ToList())
                                .ToList();

            List<Task> lsttask = new List<Task>();
            foreach (var item in splitedList)
            {
                lsttask.Add(Task.Factory.StartNew(() => DeleteDifference(item)));
                //Logger.Log("New Task Initiated");

                if (lsttask.Count() > 3)
                {
                    await Task.WhenAll(lsttask.ToArray());
                    lsttask.Clear();
                }

            }
            //Logger.Log("Start Wait");
            await Task.WhenAll(lsttask.ToArray());
           // Logger.Log("End Wait");

        }
        public void DeleteDifference(IEnumerable<DataRow> Rows)
        {
            string EntityType = TheUltimateTool.EntityType;
            int counter = 0;

            foreach (DataRow row in Rows)
            {
                if (_crmService.IsReady)
                {
                    _crmService.Delete(EntityType, row.Field<Guid>(EntityType + "id"));
                    thisForm.AppendText(++counter + " of " + Rows.Count() + " record id = " + row[0].ToString() + " was deleted successfully. \r\n");
                    //Logger.Log(DateTime.Now.ToString() + " Delete From CRM, " + gcounter);
                    gcounter++;
                }

            }
        }
        public async void SetRecordsStatus2(IEnumerable<DataRow> Rows)
        {
            gcounter = 0;
            var splitedList = Rows
                                .Select((x, i) => new { Index = i, Value = x })
                                .GroupBy(x => x.Index / 60000)
                                .Select(x => x.Select(v => v.Value).ToList())
                                .ToList();

            List<Task> lsttask = new List<Task>();
            foreach (var item in splitedList)
            {
                lsttask.Add(Task.Factory.StartNew(() => SetRecordsStatus(item)));
                //Logger.Log("New Task Initiated");

                if (lsttask.Count() > 3)
                {
                    await Task.WhenAll(lsttask.ToArray());
                    lsttask.Clear();
                }

            }
            //Logger.Log("Start Wait");
            await Task.WhenAll(lsttask.ToArray());
            //Logger.Log("End Wait");

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
                //Logger.Log(DateTime.Now.ToString() + " Prepare Entity " + i + "/60000, " + gcounter);

                try
                {

                    if (MultipleRequest == null)
                        MultipleRequest = InitializeRequest();

                    Entity ent = new Entity(EntityType);

                    var statecodeopp = row.Field<int>("statuscode");
                    if (EntityType == "opportunity")  //In the case of order. For the Order fulfillment and cancellation the standard update does not work
                    {
                      
                        if (row.Field<int>("statuscode") == 3)
                        {
                            WinOpportunityRequest cancelRequest = new WinOpportunityRequest();
                            cancelRequest.OpportunityClose = new Entity("opportunityclose");
                            cancelRequest.OpportunityClose["opportunityid"] = new EntityReference("opportunity", row.Field<Guid>(EntityType + "id"));
                            cancelRequest.Status = new OptionSetValue(row.Field<int>("statuscode"));

                            MultipleRequest.Requests.Add(cancelRequest);

                          

                        }
                        else if (row.Field<int>("statuscode") == 4 || row.Field<int>("statuscode") == 5
                            )
                        {
                            LoseOpportunityRequest fulfillRequest = new LoseOpportunityRequest();
                            fulfillRequest.OpportunityClose = new Entity("opportunityclose");
                            fulfillRequest.OpportunityClose["opportunityid"] = new EntityReference("opportunity", row.Field<Guid>(EntityType + "id"));
                            fulfillRequest.Status = new OptionSetValue(row.Field<int>("statuscode"));

                            MultipleRequest.Requests.Add(fulfillRequest);

                        }
                        //else  if(
                        //{ /* Reopen the Opportunity record */
                        //    int id=row.Field<int>("opportunityid");
                        //    Entity updateTarget = new Entity("opportunity");
                        //    updateTarget["statecode"] = new OptionSetValue(0); // 0 = Open
                        //    updateTarget["statuscode"] = new OptionSetValue(1); // 1 = In Progress
                        //    MultipleRequest.Requests.Add(fulfillRequest);
                        //}

                    }
                    else if (EntityType == "quote")
                    {
                        if (row.Field<int>("statuscode") == 2 || row.Field<int>("statuscode") == 3)
                        {
                            Entity party = new Entity("quote", row.Field<Guid>(EntityType + "id"));
                            party["statecode"] = new OptionSetValue(1); // status - Active
                            party["statuscode"] = new OptionSetValue(2); // status Reason - inprogress
                            UpdateRequest updateRequest = new UpdateRequest { Target = party };
                            MultipleRequest.Requests.Add(updateRequest);

                        }
                        else if (row.Field<int>("statuscode") == 5 || row.Field<int>("statuscode") == 6 || row.Field<int>("statuscode") == 7)
                        {
                            // First activate quote
                            Entity party = new Entity("quote", row.Field<Guid>(EntityType + "id"));
                            party["statecode"] = new OptionSetValue(1); // status - Active
                            party["statuscode"] = new OptionSetValue(2); // status Reason - inprogress
                            UpdateRequest updateRequest = new UpdateRequest { Target = party };
                            _crmService.Execute(updateRequest);
                            // then close the quote 
                            CloseQuoteRequest req = new CloseQuoteRequest();
                            Entity quoteClose = new Entity("quoteclose");
                            quoteClose.Attributes.Add("quoteid", new EntityReference("quote", row.Field<Guid>(EntityType + "id")));
                            quoteClose.Attributes.Add("subject", "Quote Closed.");
                            req.QuoteClose = quoteClose;
                            req.RequestName = "CloseQuote";
                            OptionSetValue o = new OptionSetValue();
                            o.Value = row.Field<int>("statuscode");
                            req.Status = o;
                            //CloseQuoteResponse resp = (CloseQuoteResponse)service.Execute(req);
                            MultipleRequest.Requests.Add(req);
                        }
                        else if (row.Field<int>("statuscode") == 4)
                        {
                            // First activate quote
                            Entity party = new Entity("quote", row.Field<Guid>(EntityType + "id"));
                            party["statecode"] = new OptionSetValue(1); // status - Active
                            party["statuscode"] = new OptionSetValue(2); // status Reason - inprogress
                            UpdateRequest updateRequest = new UpdateRequest { Target = party };
                            _crmService.Execute(updateRequest);
                            // then close the quote 
                            WinQuoteRequest winQuoteRequest = new WinQuoteRequest();
                            Entity quoteClose = new Entity("quoteclose");
                            quoteClose.Attributes["quoteid"] = new EntityReference("quote", row.Field<Guid>(EntityType + "id"));
                            quoteClose.Attributes["subject"] = "Quote Close" + DateTime.Now.ToString();
                            winQuoteRequest.QuoteClose = quoteClose;
                            winQuoteRequest.Status = new OptionSetValue(-1);
                            MultipleRequest.Requests.Add(winQuoteRequest);
                        }

                    }
                    else if (EntityType == "salesorder")
                    {
                        if (row.Field<int>("statuscode") == 100001 || row.Field<int>("statuscode") == 100002)
                        {
                            FulfillSalesOrderRequest req = new FulfillSalesOrderRequest();
                            req.OrderClose = new Entity("orderclose");
                            req.OrderClose["salesorderid"] = new EntityReference("salesorder", row.Field<Guid>(EntityType + "id"));
                            req.Status = new OptionSetValue(row.Field<int>("statuscode"));
                            // FulfillSalesOrderResponse resp = (FulfillSalesOrderResponse)localContext.OrganizationService.Execute(req);
                            MultipleRequest.Requests.Add(req);
                           
                        }
                        else if (row.Field<int>("statuscode") == 4)
                        {
                            CancelSalesOrderRequest req = new CancelSalesOrderRequest();
                            req.OrderClose = new Entity("orderclose");
                            req.OrderClose["salesorderid"] = new EntityReference("salesorder", row.Field<Guid>(EntityType + "id"));
                            req.Status = new OptionSetValue(row.Field<int>("statuscode"));
                            // FulfillSalesOrderResponse resp = (FulfillSalesOrderResponse)localContext.OrganizationService.Execute(req);
                            MultipleRequest.Requests.Add(req);

                        }
                    }

                    else if (EntityType == "salesorderdetail")
                    {
                        var salesorder = _crmService.Retrieve("salesorder", row.Field<EntityReference>("salesorderid").Id, new ColumnSet(true));

                        var orderState = salesorder.GetAttributeValue<OptionSetValue>("statuscode").Value;


                        if (orderState == 2)
                        {
                            //open order
                            var setStateRequest = new SetStateRequest
                            {
                                EntityMoniker = new EntityReference("salesorder", salesorder.Id),
                                State = new OptionSetValue(0),         // 0 = Active (Open)
                                Status = new OptionSetValue(1)         // 1 = New (Open status reason)
                            };
                            _crmService.Execute(setStateRequest);

                           

                        }
                        
                    }
                    else if (EntityType == "incident")
                    {
                        if (row.Field<int>("statuscode") == 1000 || row.Field<int>("statuscode") == 5)
                        {
                            Entity caseResolution = new Entity("incidentresolution");
                            caseResolution.Attributes.Add("incidentid", new EntityReference("incident", row.Field<Guid>(EntityType + "id")));
                            caseResolution.Attributes.Add("subject", "Case Resolved");
                            // Close the incident with the resolution.
                            CloseIncidentRequest req = new CloseIncidentRequest();
                            req.IncidentResolution = caseResolution;
                            req.RequestName = "CloseIncident";
                            OptionSetValue o = new OptionSetValue();
                            o.Value = row.Field<int>("statuscode");
                            req.Status = o;

                            MultipleRequest.Requests.Add(req);

                        }
                        else
                        {
                            ent[EntityType + "id"] = row.Field<Guid>(EntityType + "id");
                            ent["statecode"] = new OptionSetValue(row.Field<int>("statecode"));
                            ent["statuscode"] = new OptionSetValue(row.Field<int>("statuscode"));
                            UpdateRequest updateRequest = new UpdateRequest { Target = ent };
                            MultipleRequest.Requests.Add(updateRequest);

                        }

                    }
                    else
                    {
                        ent[EntityType + "id"] = row.Field<Guid>(EntityType + "id");
                        ent["statecode"] = new OptionSetValue(row.Field<int>("statecode"));

                        //In case the CRM contains non matching status and status reason values. I got this case in CRM.
                        //if (row.Field<int>("statecode") == 1 && row.Field<int>("statuscode") == 1)
                        //    ent["statuscode"] = new OptionSetValue(1);
                        
                        ent["statuscode"] = new OptionSetValue(row.Field<int>("statuscode"));

                        UpdateRequest updateRequest = new UpdateRequest { Target = ent };
                        MultipleRequest.Requests.Add(updateRequest);
                    }


                    counter++;

                    if (counter % 100 == 0)
                    {
                        ExecuteBulk(MultipleRequest);
                        MultipleRequest = null;
                        //Logger.Log(DateTime.Now.ToString() + " Start updating on CRM, " + gcounter);
                        gcounter += 100;
                       thisForm.RefreshCounterText(gcounter);
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

                //THIS SHOULD BE UNCOMMENTED FOR MORE ERRORS
                foreach (var responseItem in response.Responses)
                {
                    if (responseItem.Fault != null)
                        thisForm.AppendText(responseItem.Fault.Message);
                }
            }
            else
            {
                // No errors means all transactions are successful.
                thisForm.AppendText("All records have been updated successfully.");
            }
        }
        string test = "";
        // Read each column, fill the data from the SQL type according to its type
        // Consider special lookup cases, like currency or unit...
        public Entity PrepareEntity(DataRow row, DataTable DTCsv, CrmOnlineMetadataHelper Metadata)
        {
            
            string EntityType = TheUltimateTool.EntityType;
            Entity entity = new Entity(EntityType);

            entity[EntityType + "id"] = row[0];
           
           if (!TheUltimateTool.NoStateCodeEntites.Contains(EntityType.ToLower()))
             entity["overriddencreatedon"] = row["createdon"];

            Guid createdbyID = new Guid(row["createdby"].ToString());

            //entity["new_requestedby"] = new EntityReference ("systemuser",Metadata.UserMap[createdbyID]);

      // entity["new_holdenddate"] = row["expireson"];
          


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

                        //thisForm.AppendText(Metadata.LookupTargets[targetName].ToString());
                        var m = row.Field<Guid>(i);
                        //thisForm.AppendText(m.ToString());
                        

                        if (Metadata.LookupTargets[targetName] == "transactioncurrency")
                              {
                                //thisForm.AppendText("currency");
                                var d = row.Field<Guid>(i);
                                  var guid = GetTransactionNewGuid(row.Field<Guid>(i));
                                  ER = new EntityReference(Metadata.LookupTargets[targetName], guid);
                              }
                              else if (Metadata.LookupTargets[targetName] == "team")
                              {
                                  var guid = Metadata.teamMap[row.Field<Guid>(i)];
                                  ER = new EntityReference(Metadata.LookupTargets[targetName], guid);
                              }

                        else if (Metadata.LookupTargets[targetName] == "systemuser")
                              {
                                
                                   try
                                  {

                                    //thisForm.AppendText("user");
                                    var guid = Metadata.UserMap[row.Field<Guid>(i)];
                                      ER = new EntityReference(Metadata.LookupTargets[targetName], guid);
                                  }
                                  catch (Exception ex)
                                  {
                                      ER = null;
                                  }
                              }

                      
                              else if (Metadata.LookupTargets[targetName] == "uom")
                              {
                            //thisForm.AppendText("unit");
                            var guid = row.Field<Guid>(i);
                                  if (Metadata.UomMap.ContainsKey(guid))
                                      guid = Metadata.UomMap[row.Field<Guid>(i)];
                                  ER = new EntityReference(Metadata.LookupTargets[targetName], guid);
                              }

                          else if (Metadata.LookupTargets[targetName] == "uomschedule")
                                 {
                                    //thisForm.AppendText("unit group");
                                    var guid = row.Field<Guid>(i);
                                     guid = GetUserScheduleMap(guid);
                                     ER = new EntityReference(Metadata.LookupTargets[targetName], guid);
                                 }


                   
                          
                              else if (Metadata.LookupTargets[targetName] == "customer")
                              {
                                  string entityTargetType = DTCsv.Rows[i - 1].Field<string>("targetentitytype").ToLower().Trim();
                                  ER = new EntityReference(entityTargetType, row.Field<Guid>(i));
                                  //ER = new EntityReference("contact", row.Field<Guid>(i));
                            
                              }
                                
                                 else if (Metadata.LookupTargets[targetName] == "objectid")
                                 {
                                     ER = new EntityReference(row.Field<string>("logicalname"), row.Field<Guid>(i));
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
                        {


                            if (EntityType == "opportunityproduct" || EntityType == "quotedetail" || EntityType == "salesorderdetail")
                            {
                                entity["ispriceoverridden"] = true;
                            }
                            else if (EntityType == "salesorderdetail")
                            {
                                var salesorder = _crmService.Retrieve("salesorder", row.Field<EntityReference>("salesorderid").Id, new ColumnSet(true));

                                var orderState = salesorder.GetAttributeValue<OptionSetValue>("statuscode").Value;


                                if (orderState == 3)
                                {
                                    //open order
                                    var setStateRequest = new SetStateRequest
                                    {
                                        EntityMoniker = new EntityReference("salesorder", salesorder.Id),
                                        State = new OptionSetValue(0),         // 0 = Active (Open)
                                        Status = new OptionSetValue(1)         // 1 = New (Open status reason)
                                    };
                                    _crmService.Execute(setStateRequest);



                                }
                            }
                                var m=  new Money(row.Field<decimal>(i));
                            entity[targetName] = new Money(row.Field<decimal>(i));
                        }
                        else
                        {
                            //if (targetName == "remainingterms")
                            //{
                            //    entity[targetName] = Convert.ToDecimal(" 50.00");
                            //}
                            //else if (targetName == "totalterms")
                            //{
                            //    entity[targetName] = Convert.ToDecimal(" 100.00");
                            //}
                            //else { entity[targetName] = row.Field<decimal>(i); }
                           entity[targetName] = row.Field<decimal>(i);
                        }
                    }
                    else if (row[i] is bool || row[i] is string || row[i] is DateTime)
                    {
                        if (targetName == "title") {
                            var name = row[i];
                            entity[targetName] = row[i];
                        }
                     //else if (targetName == "enddate") { entity[targetName] = new DateTime(2021, 10, 10); }else if (targetName == "enddate") { entity[targetName] = new DateTime(2021, 10, 10); }

              //     else if (targetName == "billingendon") { entity[targetName] = new DateTime(2021,12,30); }
                        ////else if (targetName == "billingstarton") { entity[targetName] = new DateTime(2021, 9, 5); }

                        //else if (targetName == "activeon") { entity[targetName] = new DateTime(2012, 3, 11); }
                   //   else if (targetName == "expireson") { entity[targetName] = new DateTime(2021, 12, 30); }
                        //else if (targetName == "cancelon")
                        //{ 
                        //    var date = row[i];
                        //    entity[targetName] = new DateTime(2021, 12, 30);

                        //}enddate startdate
                    
                        else
                        {
                            entity[targetName] = row[i];
                        }
                       
              //   entity[targetName] = row[i]; 
                    }
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
                    test +=" , "+ targetName;
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
            if (OldGuid == new Guid("246ae36f-c1e9-dc11-96ff-000476f463c4"))
                return new Guid("124AF334-E379-EF11-AC21-0022489B6DA8");
            
                return OldGuid;
        }
        private Guid GetUserScheduleMap(Guid OldGuid)
        {
           
            //Dolar
            if (OldGuid == new Guid("36B5EB2C-117F-491F-9C7D-E7003544057D"))
                return new Guid("3119506c-610c-475f-a584-4dca6115a2d4");
            else
                return OldGuid;
        }
    }
}

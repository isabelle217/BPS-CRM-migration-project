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

namespace TheUltimateIktissadMigrationTool
{
    public class CrmOnlineHelper
    {
        public CrmServiceClient _crmService;
        public TheUltimateTool thisForm;
        public CrmOnlineMetadataHelper Metadata;

        public CrmOnlineHelper(TheUltimateTool tool)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
           // string ConnectionString = string.Format("AuthType=ClientSecret;url=https://bps.api.crm4.dynamics.com/;ClientId=4f4ad400-fd37-4047-961e-ddeb47aecaa5;ClientSecret=DFe-8P.-Ch_1tYD88pQglYczso3po5Wu_9");
            string ConnectionString = string.Format("RequireNewInstance=True;AuthType=ClientSecret;url=https://la.crm4.dynamics.com/;ClientId=9b8dc583-a1fe-4b71-a8bd-efe4e4bd8c99;ClientSecret=L7p1N9Vnh2VXhF5Q1ZY6J-~O7J~Dau_4R5");
        
            _crmService = new CrmServiceClient(ConnectionString);
            thisForm = tool;
            Metadata = null;

            //Get CRM Online Service
            //CrmServiceClient conn = new CrmServiceClient(TheUltimateTool.CrmOnlineConnectionString);
            //_crmService = (IOrganizationService)conn.OrganizationWebProxyClient != null ? (IOrganizationService)conn.OrganizationWebProxyClient : (IOrganizationService)conn.OrganizationServiceProxy;
            thisForm = tool;
            Metadata = null;
        }

        public DataTable GetCRMOnlineRecords(DataTable DTCsv)
        {
            try
            {
                string EntityType = TheUltimateTool.EntityType;
                string EntityTypeId = EntityType + "id";

                DataTable dTable = new DataTable();

                List<string> targetfields = new List<string>();
                targetfields.Add(EntityTypeId);
                dTable.Columns.Add(EntityTypeId);

                if (!TheUltimateTool.NoStateCodeEntites.Contains(EntityType.ToLower()))
                {
                    targetfields.Add("statecode");
                    dTable.Columns.Add("statecode");
                }

                foreach (DataRow row in DTCsv.Rows)
                {
                    if (!targetfields.Contains(row.Field<string>("targetname")))
                    {
                        targetfields.Add(row.Field<string>("targetname"));
                        dTable.Columns.Add(row.Field<string>("targetname"));
                    }
                }

                string[] targetfieldsAr = targetfields.ToArray();

                ColumnSet cs = new ColumnSet(targetfieldsAr);

                int queryCount = 5000;

                int pageNumber = 1;

                OrderExpression order = new OrderExpression();
                order.AttributeName = EntityTypeId;
                order.OrderType = OrderType.Ascending;

                QueryExpression pagequery = new QueryExpression();
                pagequery.EntityName = EntityType;
                pagequery.Orders.Add(order);
                pagequery.ColumnSet = cs;

                FilterExpression filter = new FilterExpression(LogicalOperator.And);

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

                        foreach (Entity entity in results.Entities)
                        {
                            DataRow dRow = dTable.NewRow();
                            for (int i = 0; i <= entity.Attributes.Count - 1; i++)
                            {
                                string colName = entity.Attributes.Keys.ElementAt(i);
                                //dRow[colName] = entity.Attributes.Values.ElementAt(i);
                                if (entity.Attributes.Values.ElementAt(i) is EntityReference)
                                {
                                    dRow[colName] = entity.GetAttributeValue<EntityReference>(colName).Id;
                                }
                                else if (entity.Attributes.Values.ElementAt(i) is OptionSetValue)
                                {
                                    dRow[colName] = entity.GetAttributeValue<OptionSetValue>(colName).Value;
                                }
                                else if (entity.Attributes.Values.ElementAt(i) is Money)
                                {
                                    dRow[colName] = entity.GetAttributeValue<Money>(colName).Value;
                                }
                                else
                                {
                                    if (dTable.Columns.Contains(colName))
                                        dRow[colName] = entity.Attributes.Values.ElementAt(i);
                                }
                            }
                            dTable.Rows.Add(dRow);
                        }

                        thisForm.AppendText("Finished Page " + pagequery.PageInfo.PageNumber);

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
                return dTable;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void UpdateField(string sourcename, string targetname, IEnumerable<DataRow> Rows)
        {
            if (_crmService.IsReady)
            {
                if (Metadata == null || Metadata.EntityType != TheUltimateTool.EntityType)
                    Metadata = new CrmOnlineMetadataHelper(_crmService);

                int counter = 0;

                ExecuteMultipleRequest MultipleRequest = null;

                foreach (DataRow row in Rows)
                {
                    if (MultipleRequest == null)
                        MultipleRequest = InitializeRequest();

                    try
                    {

                        Entity entity;
                        entity = PrepareEntity(row, sourcename, targetname);

                        counter++;

                        UpdateRequest updateRequest = new UpdateRequest { Target = entity };
                        MultipleRequest.Requests.Add(updateRequest);

                        if (counter % 999 == 0)
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

        public void AddToMarketingList(IEnumerable<DataRow> Rows)
        {
            ExecuteMultipleRequest MultipleRequest = null;

            int counter = 0;

            foreach (DataRow row in Rows)
            {
                if (MultipleRequest == null)
                    MultipleRequest = InitializeRequest();


                try
                {
                    counter++;

                    AddMemberListRequest req = new AddMemberListRequest();
                    req.EntityId = row.Field<Guid>("entityid");
                    req.ListId = row.Field<Guid>("listid");

                    MultipleRequest.Requests.Add(req);

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

        public void RemoveFromMarketingList(IEnumerable<DataRow> Rows)
        {
            ExecuteMultipleRequest MultipleRequest = null;

            int counter = 0;

            foreach (DataRow row in Rows)
            {
                if (MultipleRequest == null)
                    MultipleRequest = InitializeRequest();


                try
                {
                    counter++;

                    RemoveMemberListRequest req = new RemoveMemberListRequest();
                    req.EntityId = new Guid(row.Field<string>("entityid"));
                    req.ListId = new Guid(row.Field<string>("listid"));

                    MultipleRequest.Requests.Add(req);

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
            if (_crmService.IsReady)
            {
                if (MultipleRequest == null || MultipleRequest.Requests.Count == 0)
                    return;

                ExecuteMultipleResponse response =
                               (ExecuteMultipleResponse)_crmService.Execute(MultipleRequest);

                // There should be no responses except for those that contain an error. 
                if (response.Responses.Count > 0)
                {
                    if (response.Responses.Count < MultipleRequest.Requests.Count)
                    {
                        thisForm.AppendText("Response collection contain a mix of successful response objects and errors.");
                    }
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
        }

        public Entity PrepareEntity(DataRow row, string sourcename, string targetName)
        {
            string EntityType = TheUltimateTool.EntityType;
            Entity entity = new Entity(EntityType);

            entity[EntityType + "id"] = row[EntityType + "id"];

            if (row[sourcename] != DBNull.Value)
            {
                if (row[sourcename] is Guid)
                {
                    EntityReference ER;

                    if (Metadata.LookupTargets[targetName] == "transactioncurrency")
                    {
                        var guid = GetTransactionNewGuid(row.Field<Guid>(sourcename));
                        ER = new EntityReference(Metadata.LookupTargets[targetName], guid);
                    }
                    else if (Metadata.LookupTargets[targetName] == "uom")
                    {
                        var guid = row.Field<Guid>(sourcename);
                        if (Metadata.UomMap.ContainsKey(guid))
                            guid = Metadata.UomMap[row.Field<Guid>(sourcename)];
                        ER = new EntityReference(Metadata.LookupTargets[targetName], guid);
                    }
                    //else if (Metadata.LookupTargets[targetName] == "customer")
                    //{
                    //    string entityTargetType = DTCsv.Rows[i - 4].Field<string>("targetentitytype").ToLower().Trim();
                    //    ER = new EntityReference(entityTargetType, row.Field<Guid>(i));

                    //    if (entityTargetType == "customer")
                    //    {
                    //        ER = new EntityReference("contact", row.Field<Guid>(i));
                    //        Entity e = _crmService.Retrieve("contact", row.Field<Guid>(i), new ColumnSet("contactid"));
                    //        if (e == null)
                    //        {
                    //            ER = new EntityReference("account", row.Field<Guid>(i));
                    //        }
                    //    }
                    //}
                    else
                    {
                        ER = new EntityReference(Metadata.LookupTargets[targetName], row.Field<Guid>(sourcename));
                    }

                    entity[targetName] = ER;
                }
                else if (row[sourcename] is Decimal)
                {
                    if (Metadata.Attributes[targetName] == typeof(MoneyAttributeMetadata))
                        entity[targetName] = new Money(row.Field<decimal>(sourcename));
                    else
                        entity[targetName] = row.Field<decimal>(sourcename);
                }
                else if (row[sourcename] is bool || row[sourcename] is string || row[sourcename] is DateTime)
                {
                    entity[targetName] = row[sourcename];
                }
                else if (row[sourcename] is int)
                {
                    if (targetName == "statuscode")
                    {
                        entity[targetName] = new OptionSetValue(row.Field<int>(sourcename));
                    }
                    else if (Metadata.Attributes[targetName] == typeof(PicklistAttributeMetadata))
                    {
                        int optionValue = row.Field<int>(sourcename);
                        if (Metadata.OptionSetValues[targetName].Contains(optionValue))
                            entity[targetName] = new OptionSetValue(row.Field<int>(sourcename));
                    }
                    else
                    {
                        entity[targetName] = row[sourcename];
                    }
                }
            }
            else
            {
                // Clear Customer field given that the same field depends on two others.
                if (row.Table.Columns[sourcename].DataType == typeof(Guid) && Metadata.LookupTargets[targetName] == "customer")
                {
                    //var AccConRes = from r in DTCsv.AsEnumerable()
                    //                where r.Field<string>("targetname").ToLower().Trim() == targetName
                    //                && r.Field<string>("sourcename") != DTCsv.Rows[i - 4].Field<string>("sourcename")
                    //                select r.Field<string>("sourcename");

                    //if (AccConRes.Count() == 0 || AccConRes.Count() > 0 && row[AccConRes.ElementAt(0)] == DBNull.Value)
                    //    entity[targetName] = null;
                }
                else
                    entity[targetName] = null;
            }

            return entity;
        }

        public Guid GetTransactionNewGuid(Guid OldGuid)
        {
          
            if (OldGuid == new Guid("246AE36F-C1E9-DC11-96FF-000476F463C4"))
                return new Guid("031A4D60-E215-E811-A95E-000D3A2CD0DA");
            //ريال سعودي
            else if (OldGuid == new Guid("F36EBEF3-12AD-E411-B94B-00155D001305"))
                return new Guid("5315FFAB-FD00-E911-A97C-000D3A296DB9");



            else
                return OldGuid;
        }

    }
}
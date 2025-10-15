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
            string ConnectionString = string.Format("RequireNewInstance=True;AuthType=ClientSecret;url=https://bpscrm.crm4.dynamics.com/;ClientId=59a6052e-29ec-4dbb-8b0c-7e233586deeb;ClientSecret=oNm8Q~fFGS5pPgK~HTkYKg0beibWp5yBe1gDzatJ");

            _crmService = new CrmServiceClient(ConnectionString);
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

                while (true)
                {
                    EntityCollection results = null;
              
                    if (_crmService.IsReady)
                    {
                        results = _crmService.RetrieveMultiple(pagequery);
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
                                    dRow[colName] = entity.Attributes.Values.ElementAt(i);
                                }
                            }
                            dTable.Rows.Add(dRow);
                        }
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

                        if (counter % 100 == 0)
                        {

                            var MultipleRequestEx = MultipleRequest;
                            ExecuteBulk(MultipleRequestEx);
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

            if (row.Table.Columns.Contains(EntityType + "id"))
            {
                entity[EntityType + "id"] = row[EntityType + "id"];
            }

            if (row[sourcename] != DBNull.Value)
            {
                if (row[sourcename] is Guid)
                {
                    if (Metadata.LookupTargets.ContainsKey(targetName))
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
                                guid = Metadata.UomMap[guid];
                            ER = new EntityReference(Metadata.LookupTargets[targetName], guid);
                        }
                        else if (Metadata.LookupTargets[targetName] == "systemuser")
                        {
                            var guid = row.Field<Guid>(sourcename);
                            if (Metadata.UserMap.ContainsKey(guid))
                                guid = Metadata.UserMap[guid];
                            ER = new EntityReference(Metadata.LookupTargets[targetName], guid);
                        }
                        else if (Metadata.LookupTargets[targetName] == "customer")
                        {
                            ER = new EntityReference("account", row.Field<Guid>(sourcename));
                        }
                        else
                        {
                            ER = new EntityReference(Metadata.LookupTargets[targetName], row.Field<Guid>(sourcename));
                        }

                        entity[targetName] = ER;
                    }
                }
                else if (row[sourcename] is decimal)
                {
                    if (Metadata.Attributes.ContainsKey(targetName) && Metadata.Attributes[targetName] == typeof(MoneyAttributeMetadata))
                    {
                        if (EntityType == "opportunityproduct" || EntityType == "quotedetail" || EntityType == "salesorderdetail")
                        {
                            entity["ispriceoverridden"] = true;
                        }
                        if (EntityType == "quote" || EntityType == "salesorder" || EntityType == "entitlement")
                        {
                            entity["statecode"] = new OptionSetValue(0);
                        }
                        entity[targetName] = new Money(row.Field<decimal>(sourcename));
                    }
                    else
                    {
                        entity[targetName] = row.Field<decimal>(sourcename);
                    }
                }
                else if (row[sourcename] is bool || row[sourcename] is string)
                {
                    if (sourcename == "name")
                    {
                        string text = row[sourcename].ToString();
                        string text1 = text.Length > 102 ? text.Substring(0, 102) : text;
                        entity[targetName] = text1;
                    }
                    else
                    {
                        entity[targetName] = row[sourcename];
                    }
                }
                else if (row[sourcename] is DateTime)
                {
                    entity[targetName] = TimeZone.CurrentTimeZone.ToLocalTime(row.Field<DateTime>(sourcename));
                }
                else if (row[sourcename] is int)
                {
                    if (targetName == "statuscode")
                    {
                        entity[targetName] = new OptionSetValue(row.Field<int>(sourcename));
                    }
                    else if (Metadata.Attributes.ContainsKey(targetName) && Metadata.Attributes[targetName] == typeof(PicklistAttributeMetadata))
                    {
                        int optionValue = row.Field<int>(sourcename);
                        if (Metadata.OptionSetValues.ContainsKey(targetName) && Metadata.OptionSetValues[targetName].Contains(optionValue))
                        {
                            entity[targetName] = new OptionSetValue(optionValue);
                        }
                    }
                    else
                    {
                        if (EntityType == "entitlement")
                        {
                            entity["statecode"] = new OptionSetValue(0);
                        }
                        entity[targetName] = row[sourcename];
                    }
                }
            }
            else
            {
                entity[targetName] = null;
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

    }
}
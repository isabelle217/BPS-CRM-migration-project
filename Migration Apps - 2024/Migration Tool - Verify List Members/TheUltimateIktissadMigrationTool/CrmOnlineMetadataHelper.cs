using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Tooling.Connector;

using Microsoft.Crm.Sdk.Messages;

namespace TheUltimateIktissadMigrationTool
{
    public class CrmOnlineMetadataHelper
    {
        public Dictionary<string, Type> Attributes;
        public Dictionary<string, List<int>> OptionSetValues;
        public Dictionary<string, string> LookupTargets;
        public Dictionary<string, int> CustomerLookups;
        public Dictionary<Guid, Guid> UomMap;
        public string EntityType;

        public CrmOnlineMetadataHelper(IOrganizationService crmService)
        {
            Attributes = new Dictionary<string, Type>();
            OptionSetValues = new Dictionary<string, List<int>>();
            LookupTargets = new Dictionary<string, string>();
            CustomerLookups = new Dictionary<string, int>();
            UomMap = GetUomMap();

            RetrieveAllEntitiesRequest request = new RetrieveAllEntitiesRequest()
            {
                EntityFilters = EntityFilters.Attributes,
                RetrieveAsIfPublished = true
            };

            // Retrieve the MetaData.
            RetrieveAllEntitiesResponse response = (RetrieveAllEntitiesResponse)crmService.Execute(request);

            EntityType = TheUltimateTool.EntityType;
            EntityMetadata currentEntity = response.EntityMetadata.Where(M => M.SchemaName.ToLower() == EntityType).FirstOrDefault();
            foreach (AttributeMetadata currentAttribute in currentEntity.Attributes)
            {
                // Only write out main attributes.
                if (currentAttribute.AttributeOf == null)
                {
                    string AttributeName = currentAttribute.SchemaName.ToLower();
                    // Add the Attribute and its Type to the Map
                    Attributes.Add(AttributeName, currentAttribute.GetType());

                    if (currentAttribute.GetType() == typeof(PicklistAttributeMetadata))
                    {
                        PicklistAttributeMetadata optionMetadata = (PicklistAttributeMetadata)currentAttribute;
                        List<int> CurrentOsValues = new List<int>();
                        for (int c = 0; c < optionMetadata.OptionSet.Options.Count; c++)
                        {
                            CurrentOsValues.Add(optionMetadata.OptionSet.Options[c].Value.Value);
                        }
                        OptionSetValues.Add(AttributeName, CurrentOsValues);
                    }
                    else if (currentAttribute.GetType() == typeof(LookupAttributeMetadata))
                    {
                        LookupAttributeMetadata lookupMetadata = (LookupAttributeMetadata)currentAttribute;
                        if (lookupMetadata.Targets.Length > 1)
                        {
                            LookupTargets.Add(AttributeName, "customer");
                        }
                        else
                        {
                            LookupTargets.Add(AttributeName, lookupMetadata.Targets[0]);
                        }
                    }
                }
            }
        }

        private Dictionary<Guid,Guid> GetUomMap ()
        {
            //var map = new Dictionary<Guid, Guid>();

            //map.Add(new Guid("66DA84AA-275F-E211-8E19-000423B19F4F"), new Guid("adb86d33-7632-e811-80f5-5065f38b5381"));
            //map.Add(new Guid("6CBB8294-ED0C-E011-9957-002264108D71"), new Guid("aeb86d33-7632-e811-80f5-5065f38b5381"));
            //map.Add(new Guid("DD2EF430-F20C-E011-9957-002264108D71"), new Guid("afb86d33-7632-e811-80f5-5065f38b5381"));
            //map.Add(new Guid("01336939-FE52-E111-AEB0-002264108D71"), new Guid("b0b86d33-7632-e811-80f5-5065f38b5381"));
            //map.Add(new Guid("64A599E1-350F-E011-B43F-002264108D71"), new Guid("b1b86d33-7632-e811-80f5-5065f38b5381"));
            //map.Add(new Guid("D31CA0E2-9368-DF11-BD54-002264108D71"), new Guid("b2b86d33-7632-e811-80f5-5065f38b5381"));
            //map.Add(new Guid("7B2FC8C2-9568-DF11-BD54-002264108D71"), new Guid("b3b86d33-7632-e811-80f5-5065f38b5381"));




            var UomMap = new Dictionary<Guid, Guid>();
            // //

            //1
            UomMap.Add(new Guid("D64B7C1E-6E5F-E011-8618-001E0BD83A70"), new Guid("de8c85c0-a305-ec11-94ef-002248830db2"));//1 1
            //bolts
            UomMap.Add(new Guid("2B6DDBEF-EA08-E011-BCF8-001E0BD83A70"), new Guid("2b6ddbef-ea08-e011-bcf8-001e0bd83a70"));//package
            UomMap.Add(new Guid("CECC6BC1-EA08-E011-BCF8-001E0BD83A70"), new Guid("e68c85c0-a305-ec11-94ef-002248830db2"));//bolt
            //CD
            UomMap.Add(new Guid("D9B56519-7D04-DD11-919C-000476F463C4"), new Guid("d18c85c0-a305-ec11-94ef-002248830db2"));//CD1
            //CPU
            UomMap.Add(new Guid("F22C61AF-E212-E711-80C3-00155D000F1F"), new Guid("d68c85c0-a305-ec11-94ef-002248830db2"));//CPU 1
            //Day
            UomMap.Add(new Guid("26807C1C-D300-DD11-AA9E-000476F463C4"), new Guid("d38c85c0-a305-ec11-94ef-002248830db2"));// 1
            //Default Unit
            UomMap.Add(new Guid("5232912F-AE67-4B92-99F0-02C0370EB400"), new Guid("4b99a4a2-df67-4a34-b9df-a5f95ffc3663"));//Primary Unit
            //Framework
            UomMap.Add(new Guid("3556FF00-BD3B-DE11-844A-001E0BD83A70"), new Guid("dc8c85c0-a305-ec11-94ef-002248830db2"));//Framework1

            //Hour
            //
            UomMap.Add(new Guid("4DF5E931-9B3B-DE11-844A-001E0BD83A70"), new Guid("4df5e931-9b3b-de11-844a-001e0bd83a70"));//50 Hours
            UomMap.Add(new Guid("7F6918C5-5B1A-E011-9C5B-001E0BD83A70"), new Guid("7f6918c5-5b1a-e011-9c5b-001e0bd83a70"));//35 Hours
            UomMap.Add(new Guid("35574EBE-5B1A-E011-9C5B-001E0BD83A70"), new Guid("35574ebe-5b1a-e011-9c5b-001e0bd83a70"));//25 Hours
            UomMap.Add(new Guid("E8B1FCFC-AB72-E111-92B4-001E0BD83A70"), new Guid("e8b1fcfc-ab72-e111-92b4-001e0bd83a70"));//20 Hours
            UomMap.Add(new Guid("856B3FB4-5B1A-E011-9C5B-001E0BD83A70"), new Guid("856b3fb4-5b1a-e011-9c5b-001e0bd83a70"));//15 Hours
            UomMap.Add(new Guid("5C425F0A-D300-DD11-AA9E-000476F463C4"), new Guid("d28c85c0-a305-ec11-94ef-002248830db2"));//120
            UomMap.Add(new Guid("643E144C-5F50-E311-A954-00155D001306"), new Guid("643e144c-5f50-e311-a954-00155d001306"));//105 hours
            UomMap.Add(new Guid("4EF5E931-9B3B-DE11-844A-001E0BD83A70"), new Guid("4ef5e931-9b3b-de11-844a-001e0bd83a70"));//100 Hours


            //Incidents
            UomMap.Add(new Guid("A8CE47D9-9A3B-DE11-844A-001E0BD83A70"), new Guid("a8ce47d9-9a3b-de11-844a-001e0bd83a70"));//50 Incidents
            UomMap.Add(new Guid("677399AB-9A3B-DE11-844A-001E0BD83A70"), new Guid("677399ab-9a3b-de11-844a-001e0bd83a70"));//5 Incidents
            UomMap.Add(new Guid("49B3A2CE-9A3B-DE11-844A-001E0BD83A70"), new Guid("49b3a2ce-9a3b-de11-844a-001e0bd83a70"));//40 Incidents
            UomMap.Add(new Guid("F2990BC7-9A3B-DE11-844A-001E0BD83A70"), new Guid("f2990bc7-9a3b-de11-844a-001e0bd83a70"));//30 Incidents
            UomMap.Add(new Guid("8FDF02D2-0323-E011-B380-001E0BD83A70"), new Guid("8fdf02d2-0323-e011-b380-001e0bd83a70"));//25 Incidents
            UomMap.Add(new Guid("F48914BD-9A3B-DE11-844A-001E0BD83A70"), new Guid("f48914bd-9a3b-de11-844a-001e0bd83a70"));//20 Incidents
            UomMap.Add(new Guid("2AF7D2A0-9A3B-DE11-844A-001E0BD83A70"), new Guid("2af7d2a0-9a3b-de11-844a-001e0bd83a70"));//2 Incidents
            UomMap.Add(new Guid("5398CE90-591A-E011-9C5B-001E0BD83A70"), new Guid("5398ce90-591a-e011-9c5b-001e0bd83a70"));//15 Incidents
            UomMap.Add(new Guid("8BBB80C6-CE50-E111-A7F4-001E0BD83A70"), new Guid("8bbb80c6-ce50-e111-a7f4-001e0bd83a70"));//12 Incidents
            UomMap.Add(new Guid("3EB961B4-9A3B-DE11-844A-001E0BD83A70"), new Guid("3eb961b4-9a3b-de11-844a-001e0bd83a70"));//10 Incidents
            UomMap.Add(new Guid("7643B48D-9A3B-DE11-844A-001E0BD83A70"), new Guid("d98c85c0-a305-ec11-94ef-002248830db2"));//1 Incidents

            // job
            UomMap.Add(new Guid("B9FFEDF6-BC3B-DE11-844A-001E0BD83A70"), new Guid("db8c85c0-a305-ec11-94ef-002248830db2"));//1 job

            //License
            UomMap.Add(new Guid("1AB6AC7E-9BA9-DD11-9F6E-001E0BD83A6E"), new Guid("d78c85c0-a305-ec11-94ef-002248830db2"));//1 License

            //Maintenance
            UomMap.Add(new Guid("D8649AF0-FC97-E011-A005-001E0BD83A70"), new Guid("e18c85c0-a305-ec11-94ef-002248830db2"));//1

            //Manday
            UomMap.Add(new Guid("107FCD03-403B-DE11-844A-001E0BD83A70"), new Guid("d88c85c0-a305-ec11-94ef-002248830db2"));//1 Manday

            //Month
            UomMap.Add(new Guid("74FA7C25-558C-E011-8AE0-001E0BD83A70"), new Guid("df8c85c0-a305-ec11-94ef-002248830db2"));//1 Month

            //Pack of hours
            UomMap.Add(new Guid("BB27AA12-BD3B-DE11-844A-001E0BD83A70"), new Guid("dd8c85c0-a305-ec11-94ef-002248830db2"));//1 Pack of hours

            //Package
            UomMap.Add(new Guid("13C08799-DCD8-E211-AB79-001E0BD83A70"), new Guid("e58c85c0-a305-ec11-94ef-002248830db2"));//1 Package

            //Person 
            UomMap.Add(new Guid("FA854A46-9B3B-DE11-844A-001E0BD83A70"), new Guid("da8c85c0-a305-ec11-94ef-002248830db2"));//
            //Piece

            UomMap.Add(new Guid("C2343630-D300-DD11-AA9E-000476F463C4"), new Guid("d58c85c0-a305-ec11-94ef-002248830db2"));//
            // SAL
            UomMap.Add(new Guid("CBD9C642-CBF1-DD11-A3B8-001E0BD83A70"), new Guid("e28c85c0-a305-ec11-94ef-002248830db2"));//1 SAL
            // Scope update
            UomMap.Add(new Guid("C1BAE963-CBF1-DD11-A3B8-001E0BD83A70"), new Guid("e38c85c0-a305-ec11-94ef-002248830db2"));//1 Scope

            //Session
            UomMap.Add(new Guid("51F009A4-591A-E011-9C5B-001E0BD83A70"), new Guid("51f009a4-591a-e011-9c5b-001e0bd83a70"));//Session 6
            UomMap.Add(new Guid("5F5FD034-20E3-E211-A30A-001E0BD83A70"), new Guid("5f5fd034-20e3-e211-a30a-001e0bd83a70"));//Session 5
            UomMap.Add(new Guid("62906D37-591A-E011-9C5B-001E0BD83A70"), new Guid("e08c85c0-a305-ec11-94ef-002248830db2"));//Session 4
            UomMap.Add(new Guid("4DF009A4-591A-E011-9C5B-001E0BD83A70"), new Guid("4df009a4-591a-e011-9c5b-001e0bd83a70"));//Session 2
            //unit
            UomMap.Add(new Guid("26F3BA82-CBF1-DD11-A3B8-001E0BD83A70"), new Guid("e48c85c0-a305-ec11-94ef-002248830db2"));//unit 1
            //user
            UomMap.Add(new Guid("8D6721C1-CFC2-DD11-8553-001E0BD83A6E"), new Guid("8d6721c1-cfc2-dd11-8553-001e0bd83a6e"));//user 5
            UomMap.Add(new Guid("E84C73B0-CFC2-DD11-8553-001E0BD83A6E"), new Guid("e84c73b0-cfc2-dd11-8553-001e0bd83a6e"));//2 user
            UomMap.Add(new Guid("40FFBB25-D300-DD11-AA9E-000476F463C4"), new Guid("d48c85c0-a305-ec11-94ef-002248830db2"));//1 user
            return UomMap;
        }
    }
}

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
using System.Net;

namespace TheUltimateIktissadMigrationTool
{
    public class CrmOnlineMetadataHelper
    {
        public Dictionary<string, Type> Attributes;
        public Dictionary<string, List<int>> OptionSetValues;
        public Dictionary<string, string> LookupTargets;
        public Dictionary<int, string> CustomerLookups;
        public Dictionary<Guid, Guid> UomMap;
        public Dictionary<Guid, Guid> UomScheduleMap;
        public Dictionary<Guid, Guid> UserMap;
        public Dictionary<Guid, Guid> buMap;
        public Dictionary<Guid, Guid> teamMap;
        public string EntityType;

        public CrmOnlineMetadataHelper(CrmServiceClient crmService)
        {

            Attributes = new Dictionary<string, Type>();
            OptionSetValues = new Dictionary<string, List<int>>();
            LookupTargets = new Dictionary<string, string>();
            CustomerLookups = GetCustomerMap();
            UomMap = GetUomMap();
            UomScheduleMap = GetUserScheduleMap();
            UserMap = GetUserMap();
            buMap = GetBUMap();
            teamMap = GetteamMap();


            RetrieveEntityRequest Request = new RetrieveEntityRequest
            {
                EntityFilters = EntityFilters.Attributes,
                LogicalName = TheUltimateTool.EntityType,
                RetrieveAsIfPublished = true
            };
            RetrieveEntityResponse entityResponse = (RetrieveEntityResponse)crmService.Execute(Request);



            EntityMetadata currentEntity = entityResponse.EntityMetadata;

            foreach (AttributeMetadata currentAttribute in currentEntity.Attributes)
            {
                // Only write out main attributes.
                if (currentAttribute.AttributeOf == null)
                {
                    string AttributeName = currentAttribute.SchemaName.ToLower();
                    // Add the Attribute and its Type to the Map
                    Attributes.Add(AttributeName, currentAttribute.GetType());

                    //If it is an option set, retrieve the possible values
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
                    //If it is a lookup get the name of the target entity
                    else if (currentAttribute.GetType() == typeof(LookupAttributeMetadata))
                    {
                        LookupAttributeMetadata lookupMetadata = (LookupAttributeMetadata)currentAttribute;
                        if (lookupMetadata.Targets.Length > 1)
                        {
                            if (lookupMetadata.Targets[0] == "systemuser" || lookupMetadata.Targets[0] == "team")
                            {
                                LookupTargets.Add(AttributeName, "systemuser");
                            }
                            else
                            {
                                LookupTargets.Add(AttributeName, "customer");
                            }
                        }
                        else if (lookupMetadata.Targets.Count() == 1)
                        {
                            LookupTargets.Add(AttributeName, lookupMetadata.Targets[0]);
                        }
                    }
                }
            }
        }
        private Dictionary<Guid, Guid> GetUomMap()
        {

            var UomMap = new Dictionary<Guid, Guid>();
            // //

            //1
            UomMap.Add(new Guid("CF857C01-450C-EE11-B815-00155D000F4D"), new Guid("cf857c01-450c-ee11-b815-00155d000f4d"));//1 1
            UomMap.Add(new Guid("D64B7C1E-6E5F-E011-8618-001E0BD83A70"), new Guid("02d20e0f-370a-f011-bae2-0022489c7949"));//1  basic

            //bolts
            UomMap.Add(new Guid("2B6DDBEF-EA08-E011-BCF8-001E0BD83A70"), new Guid("2b6ddbef-ea08-e011-bcf8-001e0bd83a70"));//package
            UomMap.Add(new Guid("CECC6BC1-EA08-E011-BCF8-001E0BD83A70"), new Guid("0ad20e0f-370a-f011-bae2-0022489c7949"));//bolt

            //App Service Plan
            UomMap.Add(new Guid("5A7C2834-450C-EE11-B815-00155D000F4D"), new Guid("5a7c2834-450c-ee11-b815-00155d000f4d"));//App Service Plan
            UomMap.Add(new Guid("881D8251-6BC2-ED11-B80F-00155D000F4D"), new Guid("f8d10e0f-370a-f011-bae2-0022489c7949"));//App Service Plan basic


            //CD
            UomMap.Add(new Guid("F5C6764D-450C-EE11-B815-00155D000F4D"), new Guid("f5c6764d-450c-ee11-b815-00155d000f4d"));//CD1
            UomMap.Add(new Guid("D9B56519-7D04-DD11-919C-000476F463C4"), new Guid("f2d10e0f-370a-f011-bae2-0022489c7949"));//CD1


            //CPU
            UomMap.Add(new Guid("A871F95A-450C-EE11-B815-00155D000F4D"), new Guid("a871f95a-450c-ee11-b815-00155d000f4d"));//CPU 1
            UomMap.Add(new Guid("F22C61AF-E212-E711-80C3-00155D000F1F"), new Guid("f7d10e0f-370a-f011-bae2-0022489c7949"));//CPU 1 base


            //Day
            UomMap.Add(new Guid("26807C1C-D300-DD11-AA9E-000476F463C4"), new Guid("f4d10e0f-370a-f011-bae2-0022489c7949"));// 1 basic
            UomMap.Add(new Guid("E1E30972-450C-EE11-B815-00155D000F4D"), new Guid("e1e30972-450c-ee11-b815-00155d000f4d"));// 1
            //Default Unit
            UomMap.Add(new Guid("5232912F-AE67-4B92-99F0-02C0370EB400"), new Guid("956de362-59a3-499e-aa5b-4208d78c06a5"));//Primary Unit basic
            UomMap.Add(new Guid("5515E17E-450C-EE11-B815-00155D000F4D"), new Guid("956de362-59a3-499e-aa5b-4208d78c06a5"));//Primary Unit
            //Framework
            UomMap.Add(new Guid("3556FF00-BD3B-DE11-844A-001E0BD83A70"), new Guid("00d20e0f-370a-f011-bae2-0022489c7949"));//Framework1 basic
            UomMap.Add(new Guid("CF719BA0-450C-EE11-B815-00155D000F4D"), new Guid("cf719ba0-450c-ee11-b815-00155d000f4d"));//Framework1
            //Hour
            //
            UomMap.Add(new Guid("4DF5E931-9B3B-DE11-844A-001E0BD83A70"), new Guid("4df5e931-9b3b-de11-844a-001e0bd83a70"));//50 Hours
            UomMap.Add(new Guid("7F6918C5-5B1A-E011-9C5B-001E0BD83A70"), new Guid("7f6918c5-5b1a-e011-9c5b-001e0bd83a70"));//35 Hours
            UomMap.Add(new Guid("35574EBE-5B1A-E011-9C5B-001E0BD83A70"), new Guid("35574ebe-5b1a-e011-9c5b-001e0bd83a70"));//25 Hours
            UomMap.Add(new Guid("E8B1FCFC-AB72-E111-92B4-001E0BD83A70"), new Guid("e8b1fcfc-ab72-e111-92b4-001e0bd83a70"));//20 Hours
            UomMap.Add(new Guid("856B3FB4-5B1A-E011-9C5B-001E0BD83A70"), new Guid("856b3fb4-5b1a-e011-9c5b-001e0bd83a70"));//15 Hours
            UomMap.Add(new Guid("5C425F0A-D300-DD11-AA9E-000476F463C4"), new Guid("f3d10e0f-370a-f011-bae2-0022489c7949"));//120 basic
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
            UomMap.Add(new Guid("7643B48D-9A3B-DE11-844A-001E0BD83A70"), new Guid("fcd10e0f-370a-f011-bae2-0022489c7949"));//1 Incidents

            // job
            UomMap.Add(new Guid("7B9BBBC7-450C-EE11-B815-00155D000F4D"), new Guid("7b9bbbc7-450c-ee11-b815-00155d000f4d"));//1 job
            UomMap.Add(new Guid("B9FFEDF6-BC3B-DE11-844A-001E0BD83A70"), new Guid("fed10e0f-370a-f011-bae2-0022489c7949"));//1 job basic


            //License
            //   UomMap.Add(new Guid("46BD8DDD-450C-EE11-B815-00155D000F4D"), new Guid("fad10e0f-370a-f011-bae2-0022489c7949"));//1 License
            UomMap.Add(new Guid("1AB6AC7E-9BA9-DD11-9F6E-001E0BD83A6E"), new Guid("fad10e0f-370a-f011-bae2-0022489c7949"));// Office 365 E3(Cycle fee) <<P1Y Purchase >>
            UomMap.Add(new Guid("46BD8DDD-450C-EE11-B815-00155D000F4D"), new Guid("46bd8ddd-450c-ee11-b815-00155d000f4d"));//1 License

            //Maintenance
            UomMap.Add(new Guid("D8649AF0-FC97-E011-A005-001E0BD83A70"), new Guid("05d20e0f-370a-f011-bae2-0022489c7949"));//1 basic
            UomMap.Add(new Guid("14143BEB-450C-EE11-B815-00155D000F4D"), new Guid("14143beb-450c-ee11-b815-00155d000f4d"));//1

            //Manday
            UomMap.Add(new Guid("107FCD03-403B-DE11-844A-001E0BD83A70"), new Guid("fbd10e0f-370a-f011-bae2-0022489c7949"));//1 Manday basic
            UomMap.Add(new Guid("0ECDB83B-AD3F-EF11-B826-00155D000F4D"), new Guid("0ecdb83b-ad3f-ef11-b826-00155d000f4d"));//3 Mandays
            UomMap.Add(new Guid("385B84FE-450C-EE11-B815-00155D000F4D"), new Guid("385b84fe-450c-ee11-b815-00155d000f4d"));//1 Manday

            //Month
            UomMap.Add(new Guid("74FA7C25-558C-E011-8AE0-001E0BD83A70"), new Guid("03d20e0f-370a-f011-bae2-0022489c7949"));//1 Month basic
            UomMap.Add(new Guid("55F95415-460C-EE11-B815-00155D000F4D"), new Guid("55f95415-460c-ee11-b815-00155d000f4d"));//1 Month

            //Pack of hours
            UomMap.Add(new Guid("BB27AA12-BD3B-DE11-844A-001E0BD83A70"), new Guid("01d20e0f-370a-f011-bae2-0022489c7949"));//1 Pack of hours basic
            UomMap.Add(new Guid("DC27F52C-460C-EE11-B815-00155D000F4D"), new Guid("dc27f52c-460c-ee11-b815-00155d000f4d"));//1 Pack of hours

            //Package
            UomMap.Add(new Guid("13C08799-DCD8-E211-AB79-001E0BD83A70"), new Guid("09d20e0f-370a-f011-bae2-0022489c7949"));//1 Package  basic
            UomMap.Add(new Guid("E3AAFC45-460C-EE11-B815-00155D000F4D"), new Guid("e3aafc45-460c-ee11-b815-00155d000f4d"));//1 Package

            //Person 
            UomMap.Add(new Guid("FA854A46-9B3B-DE11-844A-001E0BD83A70"), new Guid("fdd10e0f-370a-f011-bae2-0022489c7949"));// Person basic
            UomMap.Add(new Guid("6BFAC054-460C-EE11-B815-00155D000F4D"), new Guid("6bfac054-460c-ee11-b815-00155d000f4d"));// Person 


            //Piece
            UomMap.Add(new Guid("C2343630-D300-DD11-AA9E-000476F463C4"), new Guid("f6d10e0f-370a-f011-bae2-0022489c7949"));//Piece  basic
            UomMap.Add(new Guid("03D24C71-460C-EE11-B815-00155D000F4D"), new Guid("03d24c71-460c-ee11-b815-00155d000f4d"));//Piece

            // SAL
            UomMap.Add(new Guid("CBD9C642-CBF1-DD11-A3B8-001E0BD83A70"), new Guid("06d20e0f-370a-f011-bae2-0022489c7949"));//1 SAL basic
            UomMap.Add(new Guid("0D03A487-460C-EE11-B815-00155D000F4D"), new Guid("0d03a487-460c-ee11-b815-00155d000f4d"));//1 SAL
            // Scope update
            UomMap.Add(new Guid("C1BAE963-CBF1-DD11-A3B8-001E0BD83A70"), new Guid("07d20e0f-370a-f011-bae2-0022489c7949"));//1 Scope basic
            UomMap.Add(new Guid("D0F39890-460C-EE11-B815-00155D000F4D"), new Guid("d0f39890-460c-ee11-b815-00155d000f4d"));//1 Scope

            //Session
            UomMap.Add(new Guid("51F009A4-591A-E011-9C5B-001E0BD83A70"), new Guid("51f009a4-591a-e011-9c5b-001e0bd83a70"));//Session 6
            UomMap.Add(new Guid("5F5FD034-20E3-E211-A30A-001E0BD83A70"), new Guid("5f5fd034-20e3-e211-a30a-001e0bd83a70"));//Session 5
            UomMap.Add(new Guid("62906D37-591A-E011-9C5B-001E0BD83A70"), new Guid("04d20e0f-370a-f011-bae2-0022489c7949"));//Session 4
            UomMap.Add(new Guid("4DF009A4-591A-E011-9C5B-001E0BD83A70"), new Guid("4df009a4-591a-e011-9c5b-001e0bd83a70"));//Session 2
            //unit
            UomMap.Add(new Guid("26F3BA82-CBF1-DD11-A3B8-001E0BD83A70"), new Guid("08d20e0f-370a-f011-bae2-0022489c7949"));//unit 1 basic
            UomMap.Add(new Guid("F83FFDA6-460C-EE11-B815-00155D000F4D"), new Guid("f83ffda6-460c-ee11-b815-00155d000f4d"));//unit 1
            //user
            UomMap.Add(new Guid("EA87F118-9BC2-ED11-B80F-00155D000F4D"), new Guid("ea87f118-9bc2-ed11-b80f-00155d000f4d"));//Add-on 50 Users
            UomMap.Add(new Guid("6E102A07-9BC2-ED11-B80F-00155D000F4D"), new Guid("6e102a07-9bc2-ed11-b80f-00155d000f4d"));//user 50
            UomMap.Add(new Guid("8D6721C1-CFC2-DD11-8553-001E0BD83A6E"), new Guid("8d6721c1-cfc2-dd11-8553-001e0bd83a6e"));//5 user
            UomMap.Add(new Guid("E84C73B0-CFC2-DD11-8553-001E0BD83A6E"), new Guid("e84c73b0-cfc2-dd11-8553-001e0bd83a6e"));//2 user
            UomMap.Add(new Guid("40FFBB25-D300-DD11-AA9E-000476F463C4"), new Guid("f5d10e0f-370a-f011-bae2-0022489c7949"));//1 user
            UomMap.Add(new Guid("719040B4-9AC2-ED11-B80F-00155D000F4D"), new Guid("719040b4-9ac2-ed11-b80f-00155d000f4d"));//8 VMs
            UomMap.Add(new Guid("78B0FD84-9AC2-ED11-B80F-00155D000F4D"), new Guid("78b0fd84-9ac2-ed11-b80f-00155d000f4d"));//4 VMs
            UomMap.Add(new Guid("DED07C66-9AC2-ED11-B80F-00155D000F4D"), new Guid("f9d10e0f-370a-f011-bae2-0022489c7949"));//1 VM (Base)

            return UomMap;

        }

        private Dictionary<Guid, Guid> GetUserScheduleMap()
        {
            var UomMap = new Dictionary<Guid, Guid>();
            UomMap.Add(new Guid("36B5EB2C-117F-491F-9C7D-E7003544057D"), new Guid("3119506c-610c-475f-a584-4dca6115a2d4"));//default unit

            return UomMap;
        }
        //private Dictionary<Guid, Guid> GetUserMap()
        //{
        //    var UserMap = new Dictionary<Guid, Guid>();
        //    UserMap.Add(new Guid("16616E36-8B08-F011-B82E-000D3AB832BD"), new Guid("d62f6b16-9a08-f011-bae3-000d3a68b8b4"));// ADHWA ALHUSSAN
        //    UserMap.Add(new Guid("1127E172-2BA4-DD11-9F6E-001E0BD83A6E"), new Guid("74e7f86e-117f-ef11-ac21-000d3ab79fa7"));// Caroline Haddad
        //    UserMap.Add(new Guid("92300FD7-06D6-EE11-B820-00155D000F4D"), new Guid("893e6f7e-117f-ef11-ac21-000d3ab04806"));//boulous mahfouz
        //    UserMap.Add(new Guid("FF450333-F726-EE11-B817-00155D000F4D"), new Guid("cb6e1c03-8980-ef11-ac21-002248a12a5f"));//carole K
        //    UserMap.Add(new Guid("2018DC1D-AB7B-E511-BDE6-00155D001305"), new Guid("983e6f7e-117f-ef11-ac21-000d3ab04806"));//christa Abdel Karim
        //    UserMap.Add(new Guid("692F1CA6-04C4-E111-92B5-001E0BD83A70"), new Guid("cdbddf70-117f-ef11-ac21-000d3ab04806"));// Diana Sidani
        //    UserMap.Add(new Guid("1237F12E-68C4-DE11-98D6-001E0BD83A70"), new Guid("a73e6f7e-117f-ef11-ac21-000d3ab04806"));// Eliana Iskandar
        //    UserMap.Add(new Guid("C493D7B2-6480-ED11-B809-00155D000F4D"), new Guid("d0c32b4f-9180-ef11-ac21-000d3ab04806"));//eliane najem
        //    UserMap.Add(new Guid("0A95E751-2209-EE11-B815-00155D000F4D"), new Guid("3d3e6f7e-117f-ef11-ac21-000d3ab04806"));//Elias Ghaleb
        //    UserMap.Add(new Guid("973CA593-C56A-EF11-B82A-000D3AB832BD"), new Guid("463e6f7e-117f-ef11-ac21-000d3ab04806"));//Elias tawk
        //    UserMap.Add(new Guid("98672CE4-1C86-EF11-B82C-000D3AB832BD"), new Guid("82e5118a-afa1-ef11-a72d-0022489d7972"));//Eman A
        //    UserMap.Add(new Guid("5779B8BD-7379-EE11-B81B-00155D000F4D"), new Guid("413e6f7e-117f-ef11-ac21-000d3ab04806"));//Faten Salhieh
        //    UserMap.Add(new Guid("345BC7F9-6530-EE11-B817-00155D000F4D"), new Guid("4dbff07a-117f-ef11-ac21-000d3ab79fa7"));//Firas Abdel Dayem
        //    UserMap.Add(new Guid("217CB9A6-9678-EE11-B81B-00155D000F4D"), new Guid("7a3e6f7e-117f-ef11-ac21-000d3ab04806"));//gaya akiki
        //    UserMap.Add(new Guid("E8ABB468-1898-EF11-B82C-000D3AB832BD"), new Guid("7a3e6f7e-117f-ef11-ac21-000d3ab04806"));//Hajar AlMulaik
        //    UserMap.Add(new Guid("D9C650F7-C56A-EF11-B82A-000D3AB832BD"), new Guid("49bef308-bba1-ef11-a72d-0022489d7972"));//Isabelle Francis
        //    UserMap.Add(new Guid("DF9E510D-4EB9-EE11-B81C-00155D000F4D"), new Guid("5853fd77-117f-ef11-ac21-000d3ab04806"));//julia salameh
        //    UserMap.Add(new Guid("42D03B70-FF72-EE11-B81A-00155D000F4D"), new Guid("5d53fd77-117f-ef11-ac21-000d3ab04806"));//Kareem Fouly
        //    UserMap.Add(new Guid("7D15EDC4-3E1E-E511-9864-00155D001305"), new Guid("3f8f557e-afa1-ef11-a72d-000d3add7547"));//Karima Fawaz  - sales
        //    UserMap.Add(new Guid("209EA46F-314F-EA11-80F0-00155D000F1F"), new Guid("7abff07a-117f-ef11-ac21-000d3ab79fa7"));//lie-marie
        //    UserMap.Add(new Guid("89A54879-2BA4-DD11-9F6E-001E0BD83A6E"), new Guid("be9f5438-120a-f011-bae2-000d3a68b8b4"));//Leila Neaimeh
        //    UserMap.Add(new Guid("5F611701-05E5-E111-8878-001E0BD83A70"), new Guid("76e7f86e-117f-ef11-ac21-000d3ab79fa7"));// Lina Agha
        //    UserMap.Add(new Guid("AA06B2B2-8974-E311-A954-00155D001306"), new Guid("64bff07a-117f-ef11-ac21-000d3ab79fa7"));//Linda
        //    UserMap.Add(new Guid("5D825613-9352-E311-A954-00155D001306"), new Guid("f3bff07a-117f-ef11-ac21-000d3ab79fa7"));//Majd
        //    UserMap.Add(new Guid("C2C27F3B-BC8B-E211-AB79-001E0BD83A70"), new Guid("7ebff07a-117f-ef11-ac21-000d3ab79fa7"));//Michel Mikhael*
        //    UserMap.Add(new Guid("BFDE33BA-D11A-E811-80D7-00155D000F1F"), new Guid("b4bff07a-117f-ef11-ac21-000d3ab79fa7"));//Mohammad Mooti
        //    UserMap.Add(new Guid("E92FFC20-6938-EB11-8102-00155D000F1F"), new Guid("03d4ed80-117f-ef11-ac21-000d3ab79fa7"));//Monika Kain
        //    UserMap.Add(new Guid("3698B0B0-2F4E-EA11-80F0-00155D000F1F"), new Guid("153e6f7e-117f-ef11-ac21-000d3ab04806"));//Nadeem Khan
        //    UserMap.Add(new Guid("FED66351-D9B7-EB11-8103-00155D000F1F"), new Guid("b1e7468d-9580-ef11-ac21-000d3ab04806"));//Nadine Akl
        //    UserMap.Add(new Guid("1927E172-2BA4-DD11-9F6E-001E0BD83A6E"), new Guid("fcbddf70-117f-ef11-ac21-000d3ab04806"));//Negib abouhabib
        //    UserMap.Add(new Guid("FE2E822D-BDC2-E711-80D3-00155D000F1F"), new Guid("493e6f7e-117f-ef11-ac21-000d3ab04806"));//Nicole
        //    UserMap.Add(new Guid("C1C450B2-915E-ED11-B808-00155D000F4D"), new Guid("d7bff07a-117f-ef11-ac21-000d3ab79fa7"));//Nour K
        //    UserMap.Add(new Guid("01952D9C-2409-EE11-B815-00155D000F4D"), new Guid("1bc0f07a-117f-ef11-ac21-000d3ab79fa7"));//Orilka Al Selfany
        //    UserMap.Add(new Guid("DE521661-B1B0-EF11-B82C-000D3AB832BD"), new Guid("de24045d-7305-f011-bae2-7c1e5288140d"));//Paresh
        //    UserMap.Add(new Guid("6108C71C-2409-EE11-B815-00155D000F4D"), new Guid("93bff07a-117f-ef11-ac21-000d3ab79fa7"));//Peter A
        //    UserMap.Add(new Guid("4B9481A9-D159-ED11-B808-00155D000F4D"), new Guid("c5cb40c5-9580-ef11-ac21-000d3ab04806"));//petra nassar
        //    UserMap.Add(new Guid("9C39AE4B-A27B-E511-BDE6-00155D001305"), new Guid("00bedf70-117f-ef11-ac21-000d3ab04806"));//Rafca Andari
        //    UserMap.Add(new Guid("1527E172-2BA4-DD11-9F6E-001E0BD83A6E"), new Guid("0cf7d11e-65a2-ef11-a72d-0022489d7972"));//Rasha 
        //    UserMap.Add(new Guid("4704DEEB-47E8-E611-80C2-00155D000F1F"), new Guid("2aab02ba-7f79-ef11-ac21-0022489b6da8"));//Rayan Fakhro
        //    UserMap.Add(new Guid("39089D14-23D2-EE11-B820-00155D000F4D"), new Guid("f7bff07a-117f-ef11-ac21-000d3ab79fa7"));//Refaat Mansour
        //    UserMap.Add(new Guid("406EED15-8ADF-EE11-B820-00155D000F4D"), new Guid("c0bff07a-117f-ef11-ac21-000d3ab79fa7"));//Riddhi Anilkumar
        //    UserMap.Add(new Guid("5E5FE01C-3FD2-EF11-B82D-000D3AB832BD"), new Guid("3c90c658-7305-f011-bae3-000d3ad7e052"));//sharmila
        //    UserMap.Add(new Guid("E4894606-4C8F-EE11-B81C-00155D000F4D"), new Guid("e8bff07a-117f-ef11-ac21-000d3ab79fa7"));//soumia
        //    UserMap.Add(new Guid("53A44432-706B-EB11-8103-00155D000F1F"), new Guid("cebddf70-117f-ef11-ac21-000d3ab04806"));//Sahar Kahawani
        //    UserMap.Add(new Guid("2B0EFE4A-D777-EE11-B81B-00155D000F4D"), new Guid("8261a4c4-9780-ef11-ac21-000d3ab79fa7"));//sawsan saadeh
        //    UserMap.Add(new Guid("2C10BBDD-E0C0-EE11-B81D-00155D000F4D"), new Guid("c70dde89-9880-ef11-ac21-000d3ab04806"));//Taif
        //    UserMap.Add(new Guid("0DC71FEA-61DA-EB11-8106-00155D000F1F"), new Guid("d0eeae79-9880-ef11-ac21-000d3ab04806"));//Tony
        //    UserMap.Add(new Guid("BC4813EE-3BA2-ED11-B80D-00155D000F4D"), new Guid("58e0f1c7-9880-ef11-ac21-000d3ab04806"));//Vidhi
        //    UserMap.Add(new Guid("C1FAA4C8-2928-ED11-B806-00155D000F4D"), new Guid("1d3e6f7e-117f-ef11-ac21-000d3ab04806"));//Youssef Kassab
        //    UserMap.Add(new Guid("A87C5F6A-2BA4-DD11-9F6E-001E0BD83A6E"), new Guid("4953fd77-117f-ef11-ac21-000d3ab04806"));//CRM Admin

        //    /* disabled users */
        //    //UserMap.Add(new Guid("13B8CC65-2AA7-EB11-8103-00155D000F1F"), new Guid("13b8cc65-2aa7-eb11-8103-00155d000f1f"));//ayaz khan
        //    //UserMap.Add(new Guid("061FA80F-A707-E011-BCF8-001E0BD83A70"), new Guid("061fa80f-a707-e011-bcf8-001e0bd83a70"));//hadi khalaf
        //    //UserMap.Add(new Guid("E8EB6B58-1807-DE11-9F42-001E0BD83A70"), new Guid("36b909b0-bd9e-ec11-b400-000d3ad7b252"));//bps office1 to services
        //    //UserMap.Add(new Guid("E815DE00-3DC3-E411-B94C-00155D001305"), new Guid("e815de00-3dc3-e411-b94c-00155d001305"));//ange
        //    //UserMap.Add(new Guid("A87C5F6A-2BA4-DD11-9F6E-001E0BD83A6E"), new Guid("4953fd77-117f-ef11-ac21-000d3ab04806"));//CRM Admin
        //    //UserMap.Add(new Guid("FC902350-D745-E111-A428-001E0BD83A70"), new Guid("fc902350-d745-e111-a428-001e0bd83a70"));//joseph abouatmeh
        //    //UserMap.Add(new Guid("21E50D26-2A55-ED11-B808-00155D000F4D"), new Guid("841293db-7048-ed11-bba2-0022489e24bb"));//ghassan
        //    //UserMap.Add(new Guid("9196009D-74A2-E511-BDE6-00155D001305"), new Guid("36b909b0-bd9e-ec11-b400-000d3ad7b252"));//bps host to services
        //    //UserMap.Add(new Guid("A2C39ED3-583E-E411-A402-00155D001305"), new Guid("a2c39ed3-583e-e411-a402-00155d001305"));//serge
        //    //UserMap.Add(new Guid("05BCB5DB-F347-E611-9DB4-00155D000F04"), new Guid("05bcb5db-f347-e611-9db4-00155d000f04"));//adxadmin
        //    //UserMap.Add(new Guid("AB722326-ED8F-ED11-B809-00155D000F4D"), new Guid("12dd0936-8671-ed11-9561-0022489e2596"));//ali
        //    //UserMap.Add(new Guid("53F33A19-026E-E711-80C4-00155D000F1F"), new Guid("a2ea53ae-e665-ef11-bfe2-000d3ab01665"));//gisele
        //    //UserMap.Add(new Guid("89ED72D7-CB5A-E511-8CA3-00155D001305"), new Guid("89ed72d7-cb5a-e511-8ca3-00155d001305"));//sarah
        //    //UserMap.Add(new Guid("753D0730-117A-E411-9C30-00155D001305"), new Guid("36b909b0-bd9e-ec11-b400-000d3ad7b252"));//layal to services
        //    //UserMap.Add(new Guid("25D3D2BA-583E-E411-A402-00155D001305"), new Guid("25d3d2ba-583e-e411-a402-00155d001305"));//christopher
        //    //UserMap.Add(new Guid("2BE0FAB5-5346-E411-A402-00155D001305"), new Guid("2be0fab5-5346-e411-a402-00155d001305"));//joy hayek
        //    //UserMap.Add(new Guid("B3FF4D6F-20C3-E411-B94C-00155D001305"), new Guid("b3ff4d6f-20c3-e411-b94c-00155d001305"));//maria
        //    //UserMap.Add(new Guid("FF33A1E0-2B41-E311-A954-00155D001306"), new Guid("ff33a1e0-2b41-e311-a954-00155d001306"));//stephanie
        //    //UserMap.Add(new Guid("15692AEE-2366-E311-A954-00155D001306"), new Guid("15692aee-2366-e311-a954-00155d001306"));//rana
        //    //UserMap.Add(new Guid("B2BA0CD0-449D-E311-A954-00155D001306"), new Guid("b2ba0cd0-449d-e311-a954-00155d001306"));//rihani
        //    //UserMap.Add(new Guid("85A54879-2BA4-DD11-9F6E-001E0BD83A6E"), new Guid("85a54879-2ba4-dd11-9f6e-001e0bd83a6e"));//ibrahim farah
        //    //UserMap.Add(new Guid("8DA54879-2BA4-DD11-9F6E-001E0BD83A6E"), new Guid("8da54879-2ba4-dd11-9f6e-001e0bd83a6e"));//Michele Halaby
        //    //UserMap.Add(new Guid("99A54879-2BA4-DD11-9F6E-001E0BD83A6E"), new Guid("99a54879-2ba4-dd11-9f6e-001e0bd83a6e"));//cynethia
        //    //UserMap.Add(new Guid("C922A0E6-B251-DE11-844A-001E0BD83A70"), new Guid("c922a0e6-b251-de11-844a-001e0bd83a70"));//jack
        //    //UserMap.Add(new Guid("3F3F4915-4170-E111-92B4-001E0BD83A70"), new Guid("3f3f4915-4170-e111-92b4-001e0bd83a70"));//lama yamout
        //    //UserMap.Add(new Guid("2BBDF35A-D31C-E111-976D-001E0BD83A70"), new Guid("2bbdf35a-d31c-e111-976d-001e0bd83a70"));//demo
        //    //UserMap.Add(new Guid("703BA76B-AA36-E111-A428-001E0BD83A70"), new Guid("703ba76b-aa36-e111-a428-001e0bd83a70"));//ali yassine
        //    //UserMap.Add(new Guid("0ECDC2EB-D745-E111-A428-001E0BD83A70"), new Guid("0ECDC2EB-D745-E111-A428-001E0BD83A70"));//carl
        //    //UserMap.Add(new Guid("8625038C-7247-E111-A428-001E0BD83A70"), new Guid("996f8443-50e8-e111-8878-001e0bd83a70"));//bcc3
        //    //UserMap.Add(new Guid("EBD85A93-BA81-E211-AB79-001E0BD83A70"), new Guid("ebd85a93-ba81-e211-ab79-001e0bd83a70"));//rabah
        //    //UserMap.Add(new Guid("3422F2E9-B3DF-E211-B2C9-001E0BD83A70"), new Guid("3422f2e9-b3df-e211-b2c9-001e0bd83a70"));//joseph sabbagh
        //    //UserMap.Add(new Guid("7E6C6F14-6B4E-E111-BA9C-001E0BD83A70"), new Guid("7e6c6f14-6b4e-e111-ba9c-001e0bd83a70"));//bps spla
        //    //UserMap.Add(new Guid("A9FBB8EC-A607-E011-BCF8-001E0BD83A70"), new Guid("a9fbb8ec-a607-e011-bcf8-001e0bd83a70"));//sanaa

        //    return UserMap;
        //}

        private Dictionary<Guid, Guid> GetUserMap()
        {
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
            UserMap.Add(new Guid("D9C650F7-C56A-EF11-B82A-000D3AB832BD"), new Guid("6abff07a-117f-ef11-ac21-000d3ab79fa7"));//Isabelle Francis
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
            UserMap.Add(new Guid("C52372B6-960F-F011-B82E-000D3AB832BD"), new Guid("6c228118-ec0b-f011-bae2-0022489c7949"));//Ghinwa Kaddouha
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

        private Dictionary<int, string> GetCustomerMap()
        {
            var map = new Dictionary<int, string>();

            map.Add(1, "account");
            map.Add(2, "contact");


            return map;
        }
        private Dictionary<Guid, Guid> GetBUMap()
        {


            var UserMap = new Dictionary<Guid, Guid>();
            UserMap.Add(new Guid("BD55AE4D-F007-E411-9400-00155DFA2067"), new Guid("D2324001-5AA9-EA11-A812-000D3A23C7A8"));
            UserMap.Add(new Guid("99E1C850-7816-E511-9410-00155DFA1F8C"), new Guid("D0FE1957-5AA9-EA11-A812-000D3A23C7A8"));
            UserMap.Add(new Guid("F09AAD2A-B4A6-E411-9405-00155DFA1F8C"), new Guid("C485C4D0-59A9-EA11-A812-000D3A23C7A8"));
            UserMap.Add(new Guid("F616BDB2-D5B8-E411-9409-00155DFA1F8C"), new Guid("D4CBC5B9-5CA9-EA11-A812-000D3A23C7A8"));
            UserMap.Add(new Guid("CACF1510-3D12-E411-9400-00155DFA2067"), new Guid("79DC887C-5CA9-EA11-A812-000D3A23C7A8"));
            UserMap.Add(new Guid("252BF7A4-F442-E411-9402-00155DFA2067"), new Guid("C7A778A1-5CA9-EA11-A812-000D3A23C7A8"));
            UserMap.Add(new Guid("E0104F43-D5B8-E411-9409-00155DFA1F8C"), new Guid("048F103A-5CA9-EA11-A812-000D3A23C7A8"));
            UserMap.Add(new Guid("05ADC740-3FF2-E411-940B-00155DFA1F8C"), new Guid("D5FC81FD-5BA9-EA11-A812-000D3A23C7A8"));
            UserMap.Add(new Guid("361FE27E-F542-E411-9402-00155DFA2067"), new Guid("6A5BCD90-5BA9-EA11-A812-000D3A23C7A8"));
            UserMap.Add(new Guid("CC80674E-6479-E511-9414-00155DFA1F8C"), new Guid("561B09A9-5BA9-EA11-A812-000D3A23C7A8"));
            UserMap.Add(new Guid("61036B8B-21BE-E311-93FA-00155DFA2037"), new Guid("1ADC1F7C-5AA9-EA11-A812-000D3A23C7A8"));
            UserMap.Add(new Guid("5D036B8B-21BE-E311-93FA-00155DFA2037"), new Guid("066BFF2F-5BA9-EA11-A812-000D3A23C7A8"));
            UserMap.Add(new Guid("8B5A8712-9AEF-E311-9400-00155DFA2067"), new Guid("69117F5A-5BA9-EA11-A812-000D3A23C7A8"));
            UserMap.Add(new Guid("C3F0B786-86AA-E311-93F7-00155DFA2037"), new Guid("EEDCDC80-B770-EA11-A811-000D3A4B2C6E"));
            UserMap.Add(new Guid("29AA52CA-B4F7-E311-9400-00155DFA2067"), new Guid("A19832BB-5BA9-EA11-A812-000D3A23C7A8"));
            return UserMap;
        }
        private Dictionary<Guid, Guid> GetteamMap()
        {

            var TeamMap = new Dictionary<Guid, Guid>();
            TeamMap.Add(new Guid("5660B1B2-268A-DD11-B226-000476F463C4"), new Guid("5660b1b2-268a-dd11-b226-000476f463c4"));//PS Team
            TeamMap.Add(new Guid("E26B752D-ED31-DF11-9556-001E0BD83A70"), new Guid("e26b752d-ed31-df11-9556-001e0bd83a70"));//admin team
            TeamMap.Add(new Guid("08A77641-ED31-DF11-9556-001E0BD83A70"), new Guid("08a77641-ed31-df11-9556-001e0bd83a70"));//bps
            TeamMap.Add(new Guid("4B94B5F9-1B0D-EB11-80FF-00155D000F1F"), new Guid("4b94b5f9-1b0d-eb11-80ff-00155d000f1f"));//BPS Azure Team
            TeamMap.Add(new Guid("DB468757-D652-E411-A402-00155D001305"), new Guid("109f7d33-ed7f-ef11-ac21-000d3ab04806"));//bps me
            TeamMap.Add(new Guid("4671BD95-3F91-E611-80C0-00155D000F1F"), new Guid("4671bd95-3f91-e611-80c0-00155d000f1f"));//BPS Support Team
            TeamMap.Add(new Guid("CA5D0894-3B76-E511-BDE6-00155D001305"), new Guid("d04e6058-ed7f-ef11-ac21-000d3ab04806"));//Diana's BU
            TeamMap.Add(new Guid("6DFA7CA7-F80B-E311-A47C-00155D001306"), new Guid("6dfa7ca7-f80b-e311-a47c-00155d001306"));//bpscrm
            TeamMap.Add(new Guid("DEC3257D-F81E-E911-80DE-00155D000F1F"), new Guid("7967dbd7-950f-f011-998a-7c1e5288140d"));//BS team
            TeamMap.Add(new Guid("4F43E441-CB83-E611-80C0-00155D000F1F"), new Guid("cdcfa773-4d06-ec11-94ef-002248830db2"));//Daad's Team
            TeamMap.Add(new Guid("15FADA24-F97E-DF11-A55F-001E0BD83A70"), new Guid("15fada24-f97e-df11-a55f-001e0bd83a70"));//dev team
            TeamMap.Add(new Guid("C29F54CC-0727-E511-9864-00155D001305"), new Guid("63870976-ed7f-ef11-ac21-000d3ab79fa7"));//Enterprise Sales
            TeamMap.Add(new Guid("A1EDEC2B-8C4B-DF11-9A03-001E0BD83A70"), new Guid("a1edec2b-8c4b-df11-9a03-001e0bd83a70"));//infra team
            TeamMap.Add(new Guid("CF12D817-8CE8-E611-80C2-00155D000F1F"), new Guid("ed620b7c-ed7f-ef11-ac21-000d3ab79fa7"));//Lina's Team
            TeamMap.Add(new Guid("333AC5E4-796F-E611-80C0-00155D000F1F"), new Guid("333ac5e4-796f-e611-80c0-00155d000f1f"));//Machine Creation/Update Team
            TeamMap.Add(new Guid("9EC059D9-7CF9-EA11-80FC-00155D000F1F"), new Guid("9ec059d9-7cf9-ea11-80fc-00155d000f1f"));//Outsourcee Team
            TeamMap.Add(new Guid("E72D5DB1-661A-E911-80DE-00155D000F1F"), new Guid("e72d5db1-661a-e911-80de-00155d000f1f"));//Presales Team
            TeamMap.Add(new Guid("CFF03FD7-EF05-E611-B4BE-00155D001305"), new Guid("50abdb85-ed7f-ef11-ac21-000d3ab04806"));//Presales
            TeamMap.Add(new Guid("6EE94D98-DFCB-EA11-80FB-00155D000F1F"), new Guid("871cb325-cf0f-f011-998a-7c1e5288140d"));//Sales & Payments (BPS) ///
            TeamMap.Add(new Guid("49588D31-1ECB-EA11-80FB-00155D000F1F"), new Guid("6f7cb043-cf0f-f011-998a-7c1e5288140d"));//Sales & Payments (LA)
            TeamMap.Add(new Guid("6FFA7CA7-F80B-E311-A47C-00155D001306"), new Guid("99ef944d-ed7f-ef11-ac21-000d3ab04806"));//Professional Services
            TeamMap.Add(new Guid("71FA7CA7-F80B-E311-A47C-00155D001306"), new Guid("cc38b58f-ed7f-ef11-ac21-000d3ab79fa7"));//Sales Dept.
            TeamMap.Add(new Guid("18239C1F-ED31-DF11-9556-001E0BD83A70"), new Guid("87d1080d-f00a-f011-bae1-7c1e5288140d"));//sales team
            TeamMap.Add(new Guid("7E248C55-EA11-E511-9864-00155D001305"), new Guid("a060ce9a-ed7f-ef11-ac21-000d3ab04806"));//Support Team
            TeamMap.Add(new Guid("700D8F6A-E0DA-EE11-904B-002248825D83"), new Guid("3cbfaec8-1bb4-e311-8b2f-00155d001306"));//SPLA Team
            TeamMap.Add(new Guid("73FA7CA7-F80B-E311-A47C-00155D001306"), new Guid("3907ac79-4d06-ec11-94ef-002248830db2"));//Team A
            TeamMap.Add(new Guid("75FA7CA7-F80B-E311-A47C-00155D001306"), new Guid("4007ac79-4d06-ec11-94ef-002248830db2"));//Team b


            return TeamMap;
        }
    }
}

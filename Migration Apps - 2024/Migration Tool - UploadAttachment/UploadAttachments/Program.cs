using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

using System.Data;
using System.Data.SqlClient;

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Tooling.Connector;
using Microsoft.Xrm.Sdk.Client;

using System.ServiceModel.Description;
using System.Net;

namespace UploadAttachments
{
    class Program
    {
        static SqlConnection conn = null;
        static void Main(string[] args)
        {
            try
            {
                Program main = new Program();
             
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



                //UserMap.Add(new Guid("E815DE00-3DC3-E411-B94C-00155D001305"), new Guid("e815de00-3dc3-e411-b94c-00155d001305"));//ange
                //UserMap.Add(new Guid("BA89D799-E365-E011-85A9-001E0BD83A70"), new Guid("d81424ba-f7b9-eb11-8236-000d3ab4912e"));//admin
                //UserMap.Add(new Guid("A87C5F6A-2BA4-DD11-9F6E-001E0BD83A6E"), new Guid("ba89d799-e365-e011-85a9-001e0bd83a70"));//admin
                //UserMap.Add(new Guid("05BCB5DB-F347-E611-9DB4-00155D000F04"), new Guid("d81424ba-f7b9-eb11-8236-000d3ab4912e"));//adxadmin -admin
                //UserMap.Add(new Guid("0A6CE0BA-1A8E-DD11-A528-000476F463C4"), new Guid("d81424ba-f7b9-eb11-8236-000d3ab4912e"));//Rana N  disable to admin
                //UserMap.Add(new Guid("753D0730-117A-E411-9C30-00155D001305"), new Guid("d81424ba-f7b9-eb11-8236-000d3ab4912e"));//Layal Sahyoun  disable to admin
                //UserMap.Add(new Guid("B2BA0CD0-449D-E311-A954-00155D001306"), new Guid("d81424ba-f7b9-eb11-8236-000d3ab4912e"));//Joseph  Rihani disable to admin
                //UserMap.Add(new Guid("FC902350-D745-E111-A428-001E0BD83A70"), new Guid("d81424ba-f7b9-eb11-8236-000d3ab4912e"));//Joseph Abouatmeh  disable to admin
                //UserMap.Add(new Guid("0ECDC2EB-D745-E111-A428-001E0BD83A70"), new Guid("d81424ba-f7b9-eb11-8236-000d3ab4912e"));//Carl disable to admin
                //UserMap.Add(new Guid("8625038C-7247-E111-A428-001E0BD83A70"), new Guid("d81424ba-f7b9-eb11-8236-000d3ab4912e"));//BPS Suppliers enable but not exist in bps online to admin
                //UserMap.Add(new Guid("A2C39ED3-583E-E411-A402-00155D001305"), new Guid("d81424ba-f7b9-eb11-8236-000d3ab4912e"));//Serge Zamora disable to admin
                //UserMap.Add(new Guid("0DC71FEA-61DA-EB11-8106-00155D000F1F"), new Guid("d81424ba-f7b9-eb11-8236-000d3ab4912e"));//Toni enable but not exist in bps online to admin
                ////89ED72D7-CB5A-E511-8CA3-00155D001305
                //UserMap.Add(new Guid("FF33A1E0-2B41-E311-A954-00155D001306"), new Guid("d81424ba-f7b9-eb11-8236-000d3ab4912e"));//Stephanie Mitri disable to admin
                //UserMap.Add(new Guid("89ED72D7-CB5A-E511-8CA3-00155D001305"), new Guid("d81424ba-f7b9-eb11-8236-000d3ab4912e"));//sara Moussa disable to admin
                //UserMap.Add(new Guid("15692AEE-2366-E311-A954-00155D001306"), new Guid("d81424ba-f7b9-eb11-8236-000d3ab4912e"));//Rana Dansh disable to admin
                //UserMap.Add(new Guid("1927E172-2BA4-DD11-9F6E-001E0BD83A6E"), new Guid("d91424ba-f7b9-eb11-8236-000d3ab4912e"));//Negib abouhabib disable to admin
                ////5D825613-9352-E311-A954-00155D001306
                //UserMap.Add(new Guid("5D825613-9352-E311-A954-00155D001306"), new Guid("5d825613-9352-e311-a954-00155d001306"));// Majd Allam  abouhabib disable to admin
                /////3f3f4915-4170-e111-92b4-001e0bd83a70
                //UserMap.Add(new Guid("3F3F4915-4170-E111-92B4-001E0BD83A70"), new Guid("3f3f4915-4170-e111-92b4-001e0bd83a70"));// Lama Yamout abouhabib disable to admin
                ////2be0fab5-5346-e411-a402-00155d001305
                //UserMap.Add(new Guid("2BE0FAB5-5346-E411-A402-00155D001305"), new Guid("2be0fab5-5346-e411-a402-00155d001305"));// Joy   Hayek disable 
                ////FC902350-D745-E111-A428-001E0BD83A70
                //UserMap.Add(new Guid("85A54879-2BA4-DD11-9F6E-001E0BD83A6E"), new Guid("85a54879-2ba4-dd11-9f6e-001e0bd83a6e"));// ibrahim farah 
                ////7b6c1cc0-f7b9-eb11-8236-000d3ab4912e
                //UserMap.Add(new Guid("88EEC473-EA11-E511-9864-00155D001305"), new Guid("7b6c1cc0-f7b9-eb11-8236-000d3ab4912e"));// George Younan  
                ////A34411B4-450C-E611-8BB8-00155D000F04
                //UserMap.Add(new Guid("A34411B4-450C-E611-8BB8-00155D000F04"), new Guid("a34411b4-450c-e611-8bb8-00155d000f04"));// Elie Asswad  
                ////1237F12E-68C4-DE11-98D6-001E0BD83A70
                //UserMap.Add(new Guid("1237F12E-68C4-DE11-98D6-001E0BD83A70"), new Guid("1237f12e-68c4-de11-98d6-001e0bd83a70"));// Eliana Iskandar  
                ////25d3d2ba-583e-e411-a402-00155d001305
                //UserMap.Add(new Guid("25D3D2BA-583E-E411-A402-00155D001305"), new Guid("25d3d2ba-583e-e411-a402-00155d001305"));// Christopher
                ////2018DC1D-AB7B-E511-BDE6-00155D001305
                //UserMap.Add(new Guid("2018DC1D-AB7B-E511-BDE6-00155D001305"), new Guid("2018dc1d-ab7b-e511-bde6-00155d001305"));// Christa Abdel Karim
                ////7E6C6F14-6B4E-E111-BA9C-001E0BD83A70
                //UserMap.Add(new Guid("7E6C6F14-6B4E-E111-BA9C-001E0BD83A70"), new Guid("9fa16d96-e1ed-eb11-bacb-000d3ade794a"));// Bps SPLA
                ////40B87DF5-1507-DE11-9F42-001E0BD83A70
                //UserMap.Add(new Guid("40B87DF5-1507-DE11-9F42-001E0BD83A70"), new Guid("9fa16d96-e1ed-eb11-bacb-000d3ade794a"));// Bps office 1
                ////3e6e8bd2-fe98-e511-bde6-00155d001305
                //UserMap.Add(new Guid("3E6E8BD2-FE98-E511-BDE6-00155D001305"), new Guid("9fa16d96-e1ed-eb11-bacb-000d3ade794a"));//Corporate telesales to admin 
                ////C55E2159-4EE6-E811-80DE-00155D000F1F
                //UserMap.Add(new Guid("C55E2159-4EE6-E811-80DE-00155D000F1F"), new Guid("9fa16d96-e1ed-eb11-bacb-000d3ade794a"));//Abed jaber
                ////ebd85a93-ba81-e211-ab79-001e0bd83a70
                //UserMap.Add(new Guid("EBD85A93-BA81-E211-AB79-001E0BD83A70"), new Guid("ebd85a93-ba81-e211-ab79-001e0bd83a70"));//Rabih
                ////3422F2E9-B3DF-E211-B2C9-001E0BD83A70
                //UserMap.Add(new Guid("3422F2E9-B3DF-E211-B2C9-001E0BD83A70"), new Guid("3422f2e9-b3df-e211-b2c9-001e0bd83a70"));//Joseph Sabbagh
                ////9196009d-74a2-e511-bde6-00155d001305
                //UserMap.Add(new Guid("9196009D-74A2-E511-BDE6-00155D001305"), new Guid("9fa16d96-e1ed-eb11-bacb-000d3ade794a"));//BPS ME | Hosting Info to admin
                ////b9077CBD6-C038-E411-8806-00155D001305
                //UserMap.Add(new Guid("9077CBD6-C038-E411-8806-00155D001305"), new Guid("9077cbd6-c038-e411-8806-00155d001305"));//Ingrid Bechara
                //UserMap.Add(new Guid("1127E172-2BA4-DD11-9F6E-001E0BD83A6E"), new Guid("385445e1-48b6-eb11-8236-0022489aee3a"));// Caroline Haddad

                //UserMap.Add(new Guid("B5443F98-3402-EA11-80EC-00155D000F1F"), new Guid("206d1cc0-f7b9-eb11-8236-000d3ab4912e"));//Celine Mansour
                ////322AD64A-FDF9-E611-80C3-00155D000F1Ff
                //UserMap.Add(new Guid("322AD64A-FDF9-E611-80C3-00155D000F1F"), new Guid("322ad64a-fdf9-e611-80c3-00155d000f1f"));//Bilal hamad
                //UserMap.Add(new Guid("AA896A69-5931-E711-80C3-00155D000F1F"), new Guid("aa896a69-5931-e711-80c3-00155d000f1f"));//Marc Marc Abi Saad
                //UserMap.Add(new Guid("D900E811-46BD-E711-80D2-00155D000F1F"), new Guid("d900e811-46bd-e711-80d2-00155d000f1f"));//karim
                //UserMap.Add(new Guid("37594d3f-02d8-de11-b8f2-001e0bd83a70"), new Guid("37594d3f-02d8-de11-b8f2-001e0bd83a70"));//Richard
                //UserMap.Add(new Guid("BBB7B937-01CF-E111-92B5-001E0BD83A70"), new Guid("bbb7b937-01cf-e111-92b5-001e0bd83a70"));// Christa Abdel Karim

                //UserMap.Add(new Guid("692F1CA6-04C4-E111-92B5-001E0BD83A70"), new Guid("a86d1cc0-f7b9-eb11-8236-000d3ab4912e"));// Diana

                //UserMap.Add(new Guid("68101299-5BC9-E311-A60E-00155D001305"), new Guid("9fa16d96-e1ed-eb11-bacb-000d3ade794a"));// Roger to admin app


                //UserMap.Add(new Guid("F423D187-D11A-E811-80D7-00155D000F1F"), new Guid("f423d187-d11a-e811-80d7-00155d000f1f"));// Elias Khoury Disable

                //UserMap.Add(new Guid("53F33A19-026E-E711-80C4-00155D000F1F"), new Guid("53f33a19-026e-e711-80c4-00155d000f1f"));// Giselle Mdawar -accounts

                //UserMap.Add(new Guid("C6FE71AE-ECAD-E311-8B2F-00155D001306"), new Guid("c6fe71ae-ecad-e311-8b2f-00155d001306"));// Ibrahim Atallah

                //UserMap.Add(new Guid("54303720-F57C-E711-80C4-00155D000F1F"), new Guid("54303720-f57c-e711-80c4-00155d000f1f"));// Ibrahim Badawi


                //UserMap.Add(new Guid("3F263578-47E8-E611-80C2-00155D000F1F"), new Guid("2f88e093-64b9-eb11-8236-000d3ab49951"));// Iman El Cheikh

                //UserMap.Add(new Guid("7D15EDC4-3E1E-E511-9864-00155D001305"), new Guid("306d1cc0-f7b9-eb11-8236-000d3ab4912e"));//Karima Fawaz  - sales

                //UserMap.Add(new Guid("209EA46F-314F-EA11-80F0-00155D000F1F"), new Guid("cf6c1cc0-f7b9-eb11-8236-000d3ab4912e"));// Lee-Marie El Wadi

                //UserMap.Add(new Guid("5F611701-05E5-E111-8878-001E0BD83A70"), new Guid("e9ba14c6-f7b9-eb11-8236-000d3ab4912e"));// Lina Agha

                //UserMap.Add(new Guid("89A54879-2BA4-DD11-9F6E-001E0BD83A6E"), new Guid("db1424ba-f7b9-eb11-8236-000d3ab4912e"));// Leila Neaimeh

                //UserMap.Add(new Guid("AA06B2B2-8974-E311-A954-00155D001306"), new Guid("aa06b2b2-8974-e311-a954-00155d001306"));// Linda Mrad

                //UserMap.Add(new Guid("C2C27F3B-BC8B-E211-AB79-001E0BD83A70"), new Guid("c2c27f3b-bc8b-e211-ab79-001e0bd83a70"));// Michel Mikhael

                //UserMap.Add(new Guid("BFDE33BA-D11A-E811-80D7-00155D000F1F"), new Guid("fb6c1cc0-f7b9-eb11-8236-000d3ab4912e"));// Mohammad Mooti

                //UserMap.Add(new Guid("E92FFC20-6938-EB11-8102-00155D000F1F"), new Guid("e92ffc20-6938-eb11-8102-00155d000f1f"));// Monika Kain

                //UserMap.Add(new Guid("3698B0B0-2F4E-EA11-80F0-00155D000F1F"), new Guid("3698b0b0-2f4e-ea11-80f0-00155d000f1f"));// Nadeem Khan

                //UserMap.Add(new Guid("FE2E822D-BDC2-E711-80D3-00155D000F1F"), new Guid("ce6d1cc0-f7b9-eb11-8236-000d3ab4912e"));//Nicole Maalouf

                //UserMap.Add(new Guid("9C39AE4B-A27B-E511-BDE6-00155D001305"), new Guid("c26c1cc0-f7b9-eb11-8236-000d3ab4912e"));//Rafca Andari

                //UserMap.Add(new Guid("1527E172-2BA4-DD11-9F6E-001E0BD83A6E"), new Guid("476c1cc0-f7b9-eb11-8236-000d3ab4912e"));//Rasha Moghrabi


                //UserMap.Add(new Guid("4704DEEB-47E8-E611-80C2-00155D000F1F"), new Guid("866c1cc0-f7b9-eb11-8236-000d3ab4912e"));//Rayan Fakhro

                //UserMap.Add(new Guid("67D4298B-51EB-EA11-80FB-00155D000F1F"), new Guid("67d4298b-51eb-ea11-80fb-00155d000f1f"));//Rita Samia

                //UserMap.Add(new Guid("53A44432-706B-EB11-8103-00155D000F1F"), new Guid("a86c1cc0-f7b9-eb11-8236-000d3ab4912e"));//Sahar Kahawani

                //UserMap.Add(new Guid("6067D118-4382-E711-80C4-00155D000F1F"), new Guid("f46d1cc0-f7b9-eb11-8236-000d3ab4912e"));//Yara ElKoussa
                //UserMap.Add(new Guid("3EE860DF-DAE7-DF11-9E4A-001E0BD83A70"), new Guid("9fa16d96-e1ed-eb11-bacb-000d3ade794a"));//Job BPS

                //UserMap.Add(new Guid("E815DE00-3DC3-E411-B94C-00155D001305"), new Guid("8ff1ed4b-afab-ea11-a812-000d3a23c9f4"));//ange
                //UserMap.Add(new Guid("53F33A19-026E-E711-80C4-00155D000F1F"), new Guid("0e19ddac-88bc-ea11-a812-000d3a23c2ba"));// Giselle Mdawar -accounts
                //UserMap.Add(new Guid("1127E172-2BA4-DD11-9F6E-001E0BD83A6E"), new Guid("fb057f57-c66a-4407-9608-8e8e82f5c0c7"));// Caroline Haddad
                //UserMap.Add(new Guid("B5443F98-3402-EA11-80EC-00155D000F1F"), new Guid("6b0d6ef2-3002-ea11-a811-000d3a23cd1d"));//Celine Mansour
                //UserMap.Add(new Guid("2018DC1D-AB7B-E511-BDE6-00155D001305"), new Guid("a7d7e4a6-88bc-ea11-a812-000d3a23c2ba"));// Christa Abdel Karim
                //UserMap.Add(new Guid("692F1CA6-04C4-E111-92B5-001E0BD83A70"), new Guid("e3c0dd55-ff1e-e811-a959-000d3a2acf58"));// Diana
                //UserMap.Add(new Guid("1237F12E-68C4-DE11-98D6-001E0BD83A70"), new Guid("ebd8e4a6-88bc-ea11-a812-000d3a23c2ba"));// Eliana Iskandar  
                //UserMap.Add(new Guid("88EEC473-EA11-E511-9864-00155D001305"), new Guid("c918ddac-88bc-ea11-a812-000d3a23c2ba"));// George Younan  
                //UserMap.Add(new Guid("54303720-F57C-E711-80C4-00155D000F1F"), new Guid("b0d8e4a6-88bc-ea11-a812-000d3a23c2ba"));// Ibrahim Badawi
                //UserMap.Add(new Guid("3F263578-47E8-E611-80C2-00155D000F1F"), new Guid("ffc0dd55-ff1e-e811-a959-000d3a2acf58"));// Iman El Cheikh
                //UserMap.Add(new Guid("209EA46F-314F-EA11-80F0-00155D000F1F"), new Guid("23d8e4a6-88bc-ea11-a812-000d3a23c2ba"));// Lee-Marie El Wadi
                //UserMap.Add(new Guid("89A54879-2BA4-DD11-9F6E-001E0BD83A6E"), new Guid("910149c8-1d17-e811-a957-000d3a2acf58"));// Leila Neaimeh
                //UserMap.Add(new Guid("5F611701-05E5-E111-8878-001E0BD83A70"), new Guid("f1c0dd55-ff1e-e811-a959-000d3a2acf58"));// Lina Agha
                //UserMap.Add(new Guid("AA06B2B2-8974-E311-A954-00155D001306"), new Guid("091addac-88bc-ea11-a812-000d3a23c2ba"));// Linda Mrad
                //UserMap.Add(new Guid("C2C27F3B-BC8B-E211-AB79-001E0BD83A70"), new Guid("371addac-88bc-ea11-a812-000d3a23c2ba"));// Michel Mikhael
                //UserMap.Add(new Guid("BFDE33BA-D11A-E811-80D7-00155D000F1F"), new Guid("81d7306a-5673-ea11-a811-000d3a23c2ba"));// Mohammad Mooti
                //UserMap.Add(new Guid("3698B0B0-2F4E-EA11-80F0-00155D000F1F"), new Guid("f081b84c-e2ab-ea11-a812-000d3a23c23e"));// Nadeem Khan
                //UserMap.Add(new Guid("FED66351-D9B7-EB11-8103-00155D000F1F"), new Guid("409aed30-19b3-eb11-8236-000d3a448cb5"));// Nadine Akl
                //UserMap.Add(new Guid("1927E172-2BA4-DD11-9F6E-001E0BD83A6E"), new Guid("196527c2-1d17-e811-a957-000d3a2acf58"));//Negib abouhabib 
                //UserMap.Add(new Guid("FE2E822D-BDC2-E711-80D3-00155D000F1F"), new Guid("b7a489a2-6eac-ea11-a812-000d3a23c639"));//Nicole Maalouf
                //UserMap.Add(new Guid("9C39AE4B-A27B-E511-BDE6-00155D001305"), new Guid("d90149c8-1d17-e811-a957-000d3a2acf58"));//Rafca Andari
                //UserMap.Add(new Guid("1527E172-2BA4-DD11-9F6E-001E0BD83A6E"), new Guid("27fa3951-bacc-e811-a992-000d3a2ac06d"));//Rasha Moghrabi
                //UserMap.Add(new Guid("4704DEEB-47E8-E611-80C2-00155D000F1F"), new Guid("0dc1dd55-ff1e-e811-a959-000d3a2acf58"));//Rayan Fakhro
                //UserMap.Add(new Guid("67D4298B-51EB-EA11-80FB-00155D000F1F"), new Guid("5094423d-b5db-ea11-a816-000d3a23ca40"));//Rita Samia
                //UserMap.Add(new Guid("53A44432-706B-EB11-8103-00155D000F1F"), new Guid("0f57aecd-d951-eb11-bb23-000d3a23c9f4"));//Sahar Kahawani
                //UserMap.Add(new Guid("6067D118-4382-E711-80C4-00155D000F1F"), new Guid("036527c2-1d17-e811-a957-000d3a2acf58"));//Yara ElKoussa
                //UserMap.Add(new Guid("7D15EDC4-3E1E-E511-9864-00155D001305"), new Guid("227163ac-d88e-ea11-a811-000d3a23c2ba"));//Karima Fawaz  - sales
                /////306d1cc0-f7b9-eb11-8236-000d3ab4912e
                //UserMap.Add(new Guid("753D0730-117A-E411-9C30-00155D001305"), new Guid("abd4da08-d504-ec11-94ef-002248830301"));//Layal Sahyoun  disable to admin
                //UserMap.Add(new Guid("B2BA0CD0-449D-E311-A954-00155D001306"), new Guid("abd4da08-d504-ec11-94ef-002248830301"));//Joseph  Rihani disable to admin
                //UserMap.Add(new Guid("FC902350-D745-E111-A428-001E0BD83A70"), new Guid("abd4da08-d504-ec11-94ef-002248830301"));//Joseph Abouatmeh  disable to admin
                //UserMap.Add(new Guid("0A6CE0BA-1A8E-DD11-A528-000476F463C4"), new Guid("abd4da08-d504-ec11-94ef-002248830301"));//Rana N  disable to admin
                //UserMap.Add(new Guid("0ECDC2EB-D745-E111-A428-001E0BD83A70"), new Guid("abd4da08-d504-ec11-94ef-002248830301"));//Carl disable to admin
                //UserMap.Add(new Guid("A2C39ED3-583E-E411-A402-00155D001305"), new Guid("abd4da08-d504-ec11-94ef-002248830301"));//Serge Zamora disable to admin
                //UserMap.Add(new Guid("0DC71FEA-61DA-EB11-8106-00155D000F1F"), new Guid("abd4da08-d504-ec11-94ef-002248830301"));//Toni enable but not exist in bps online to admin
                //UserMap.Add(new Guid("FF33A1E0-2B41-E311-A954-00155D001306"), new Guid("abd4da08-d504-ec11-94ef-002248830301"));//Stephanie Mitri disable to admin
                //UserMap.Add(new Guid("89ED72D7-CB5A-E511-8CA3-00155D001305"), new Guid("abd4da08-d504-ec11-94ef-002248830301"));//sara Moussa disable to admin
                //UserMap.Add(new Guid("15692AEE-2366-E311-A954-00155D001306"), new Guid("abd4da08-d504-ec11-94ef-002248830301"));//Rana Dansh disable to admin
                //UserMap.Add(new Guid("5D825613-9352-E311-A954-00155D001306"), new Guid("abd4da08-d504-ec11-94ef-002248830301"));// Majd Allam  abouhabib disable to admin
                //UserMap.Add(new Guid("3F3F4915-4170-E111-92B4-001E0BD83A70"), new Guid("abd4da08-d504-ec11-94ef-002248830301"));// Lama Yamout abouhabib disable to admin
                //UserMap.Add(new Guid("8625038C-7247-E111-A428-001E0BD83A70"), new Guid("abd4da08-d504-ec11-94ef-002248830301"));//BPS Suppliers enable but not exist in bps online to admin
                //UserMap.Add(new Guid("13B8CC65-2AA7-EB11-8103-00155D000F1F"), new Guid("abd4da08-d504-ec11-94ef-002248830301"));//ayaz to admin

                ////Disable user//
                //UserMap.Add(new Guid("C55E2159-4EE6-E811-80DE-00155D000F1F"), new Guid("061addac-88bc-ea11-a812-000d3a23c2ba"));//Abed jaber
                //UserMap.Add(new Guid("322AD64A-FDF9-E611-80C3-00155D000F1F"), new Guid("09850333-78b0-ea11-a812-000d3a23c9f4"));//Bilal hamad
                //UserMap.Add(new Guid("F423D187-D11A-E811-80D7-00155D000F1F"), new Guid("ded8e4a6-88bc-ea11-a812-000d3a23c2ba"));// Elias Khoury Disable
                //UserMap.Add(new Guid("D900E811-46BD-E711-80D2-00155D000F1F"), new Guid("80e352ce-1d17-e811-a957-000d3a2acf58"));//karim
                //UserMap.Add(new Guid("132E27FF-EC07-E711-80C3-00155D000F1F"), new Guid("9f74357f-537e-ea11-a811-000d3a23c23e"));//marc hajjar
                //UserMap.Add(new Guid("A87C5F6A-2BA4-DD11-9F6E-001E0BD83A6E"), new Guid("abd4da08-d504-ec11-94ef-002248830301"));//bps crm admin to admin
                //UserMap.Add(new Guid("2BE0FAB5-5346-E411-A402-00155D001305"), new Guid("abd4da08-d504-ec11-94ef-002248830301"));// Joy   Hayek disable 
                //UserMap.Add(new Guid("85A54879-2BA4-DD11-9F6E-001E0BD83A6E"), new Guid("abd4da08-d504-ec11-94ef-002248830301"));// ibrahim farah 
                //UserMap.Add(new Guid("A34411B4-450C-E611-8BB8-00155D000F04"), new Guid("abd4da08-d504-ec11-94ef-002248830301"));// Elie Asswad  
                //UserMap.Add(new Guid("25D3D2BA-583E-E411-A402-00155D001305"), new Guid("abd4da08-d504-ec11-94ef-002248830301"));// Christopher
                //UserMap.Add(new Guid("7E6C6F14-6B4E-E111-BA9C-001E0BD83A70"), new Guid("abd4da08-d504-ec11-94ef-002248830301"));// Bps SPLA
                //UserMap.Add(new Guid("E8EB6B58-1807-DE11-9F42-001E0BD83A70"), new Guid("abd4da08-d504-ec11-94ef-002248830301"));// Bps office 1
                //UserMap.Add(new Guid("3E6E8BD2-FE98-E511-BDE6-00155D001305"), new Guid("abd4da08-d504-ec11-94ef-002248830301"));//Corporate telesales to admin 
                //UserMap.Add(new Guid("EBD85A93-BA81-E211-AB79-001E0BD83A70"), new Guid("abd4da08-d504-ec11-94ef-002248830301"));//Rabih
                //UserMap.Add(new Guid("3422F2E9-B3DF-E211-B2C9-001E0BD83A70"), new Guid("abd4da08-d504-ec11-94ef-002248830301"));//Joseph Sabbagh
                //UserMap.Add(new Guid("9196009D-74A2-E511-BDE6-00155D001305"), new Guid("abd4da08-d504-ec11-94ef-002248830301"));//BPS ME | Hostine9cc148b-343d-e611-9db4-00155d000f04g Info to admin
                //UserMap.Add(new Guid("9077CBD6-C038-E411-8806-00155D001305"), new Guid("abd4da08-d504-ec11-94ef-002248830301"));//Ingrid Bechara
                //UserMap.Add(new Guid("37594d3f-02d8-de11-b8f2-001e0bd83a70"), new Guid("abd4da08-d504-ec11-94ef-002248830301"));//Richard
                //////////user not exist //

                //// not exists//0e19ddac-88bc-ea11-a812-000d3a23c2ba
                //UserMap.Add(new Guid("9C0C1D3B-97D0-E511-BDE6-00155D001305"), new Guid("abd4da08-d504-ec11-94ef-002248830301"));//alaa Traboulsi  to admin
                //UserMap.Add(new Guid("8DE39958-54A2-E511-BDE6-00155D001305"), new Guid("abd4da08-d504-ec11-94ef-002248830301"));//Anthony sroor  to admin
                //UserMap.Add(new Guid("1C165147-9D81-E311-A954-00155D001306"), new Guid("abd4da08-d504-ec11-94ef-002248830301"));// Daad Seifedine to admin
                //UserMap.Add(new Guid("9C0CA583-10B6-E511-BDE6-00155D001305"), new Guid("abd4da08-d504-ec11-94ef-002248830301"));// issa Itani to admin
                //UserMap.Add(new Guid("A1A54879-2BA4-DD11-9F6E-001E0BD83A6E"), new Guid("abd4da08-d504-ec11-94ef-002248830301"));// ibrahim ashkar  to admin
                //UserMap.Add(new Guid("C93589C5-FD0B-E011-BCF8-001E0BD83A70"), new Guid("abd4da08-d504-ec11-94ef-002248830301"));//Joseph haddad  to admin
                //UserMap.Add(new Guid("BBB7B937-01CF-E111-92B5-001E0BD83A70"), new Guid("abd4da08-d504-ec11-94ef-002248830301"));// sania  to admin

                //UserMap.Add(new Guid("4A87802F-36A6-EA11-80FB-00155D000F1F"), new Guid("abd4da08-d504-ec11-94ef-002248830301"));// Dev user   to admin

                //UserMap.Add(new Guid("68101299-5BC9-E311-A60E-00155D001305"), new Guid("abd4da08-d504-ec11-94ef-002248830301"));// Roger to admin app

                //UserMap.Add(new Guid("BA89D799-E365-E011-85A9-001E0BD83A70"), new Guid("abd4da08-d504-ec11-94ef-002248830301"));//admin

                //UserMap.Add(new Guid("05BCB5DB-F347-E611-9DB4-00155D000F04"), new Guid("abd4da08-d504-ec11-94ef-002248830301"));//adxadmin -admin

                //UserMap.Add(new Guid("3EE860DF-DAE7-DF11-9E4A-001E0BD83A70"), new Guid("abd4da08-d504-ec11-94ef-002248830301"));//Job BPS
                //UserMap.Add(new Guid("061FA80F-A707-E011-BCF8-001E0BD83A70"), new Guid("abd4da08-d504-ec11-94ef-002248830301")); //Hadi Khalaf
                ////
                //UserMap.Add(new Guid("5094F0D1-AAC4-DF11-9BBC-001E0BD83A70"), new Guid("abd4da08-d504-ec11-94ef-002248830301")); //Ahmad Ramadan
                //UserMap.Add(new Guid("E9CC148B-343D-E611-9DB4-00155D000F04"), new Guid("abd4da08-d504-ec11-94ef-002248830301")); //Rachel Assaf
                //UserMap.Add(new Guid("AA896A69-5931-E711-80C3-00155D000F1F"), new Guid("abd4da08-d504-ec11-94ef-002248830301"));//marc abi saad
                //UserMap.Add(new Guid("38869781-5BC9-E311-A60E-00155D001305"), new Guid("abd4da08-d504-ec11-94ef-002248830301"));//Mario Karam
                ////c6fe71ae-ecad-e311-8b2f-00155d001306
                //UserMap.Add(new Guid("c6fe71ae-ecad-e311-8b2f-00155d001306"), new Guid("abd4da08-d504-ec11-94ef-002248830301"));//Ibrahim Atallah
                ////
                //UserMap.Add(new Guid("94F0164C-2761-E511-8CA3-00155D001305"), new Guid("abd4da08-d504-ec11-94ef-002248830301"));//Ibrahim Atallah
                //UserMap.Add(new Guid("99A54879-2BA4-DD11-9F6E-001E0BD83A6E"), new Guid("abd4da08-d504-ec11-94ef-002248830301"));//Cynthia Bejjani
                //UserMap.Add(new Guid("91A54879-2BA4-DD11-9F6E-001E0BD83A6E"), new Guid("abd4da08-d504-ec11-94ef-002248830301"));//Dania El Ashi
                //UserMap.Add(new Guid("44AC7F92-38A4-DF11-ADDE-001E0BD83A70"), new Guid("abd4da08-d504-ec11-94ef-002248830301"));//Sandra Hallal

                //UserMap.Add(new Guid("8DA54879-2BA4-DD11-9F6E-001E0BD83A6E"), new Guid("abd4da08-d504-ec11-94ef-002248830301"));// Michele halaby


                //UserMap.Add(new Guid("703BA76B-AA36-E111-A428-001E0BD83A70"), new Guid("abd4da08-d504-ec11-94ef-002248830301"));//Ali yassin
                //UserMap.Add(new Guid("F89D713F-1807-DE11-9F42-001E0BD83A70"), new Guid("abd4da08-d504-ec11-94ef-002248830301"));//bps office 2

                //UserMap.Add(new Guid("83445B60-2BA4-DD11-9F6E-001E0BD83A6E"), new Guid("abd4da08-d504-ec11-94ef-002248830301"));//samer

                //UserMap.Add(new Guid("A47C5F6A-2BA4-DD11-9F6E-001E0BD83A6E"), new Guid("abd4da08-d504-ec11-94ef-002248830301"));//nader
                //UserMap.Add(new Guid("95A54879-2BA4-DD11-9F6E-001E0BD83A6E"), new Guid("abd4da08-d504-ec11-94ef-002248830301"));//sabine

                //UserMap.Add(new Guid("9DA54879-2BA4-DD11-9F6E-001E0BD83A6E"), new Guid("abd4da08-d504-ec11-94ef-002248830301"));//Roland Abou Younes

                //UserMap.Add(new Guid("F221B72B-D0CF-EA11-A30D-00155D009F98"), new Guid("0ca54051-17d8-ec11-a7b5-000d3abb61a3"));//Abubakr_Ali@people365.com
                //UserMap.Add(new Guid("667C1B68-3C10-E911-A2F2-00155D009FA4"), new Guid("e4a0c89a-17d8-ec11-a7b5-000d3abc4d5c"));//Antoinette_Tayar@people365.com
                //UserMap.Add(new Guid("CF3BA05A-3C10-E911-A2F2-00155D009FA4"), new Guid("0e22f3ac-78b6-ec11-983f-000d3a2ed6e2"));//Anthony_Ghaziri to crm365admin@people365.com
                //UserMap.Add(new Guid("C646BE78-3C10-E911-A2F2-00155D009FA4"), new Guid("6f012643-18d8-ec11-a7b5-000d3abb61a3"));//Charbel_Chahine@people365.com
                //UserMap.Add(new Guid("2A9E26C8-BA25-EB11-A314-00155D84DA66"), new Guid("490b589c-18d8-ec11-a7b5-000d3abc4d5c"));//Chirine_Nakhoul@people365.com
                //UserMap.Add(new Guid("EB1FDC7A-0AE3-EB11-A326-00155D84DACE"), new Guid("9a5672d2-fed1-ec11-a7b5-000d3a3a57ec"));//Christelle_Helayel@people365.com
                //UserMap.Add(new Guid("BE79BA70-35DF-E711-A2CF-00155D84DA80"), new Guid("0e22f3ac-78b6-ec11-983f-000d3a2ed6e2"));//crm365admin@people365.com
                //UserMap.Add(new Guid("81434A5A-2452-E911-A2F5-00155D84DA6F"), new Guid("0e22f3ac-78b6-ec11-983f-000d3a2ed6e2"));//Caesar_Chehwan to crm365admin@people365.com
                //UserMap.Add(new Guid("98A6B082-3C10-E911-A2F2-00155D009FA4"), new Guid("0e22f3ac-78b6-ec11-983f-000d3a2ed6e2"));//Charbel_Sara to crm365admin@people365.com
                //UserMap.Add(new Guid("5B20950B-6FA3-EB11-A322-00155D84DAC0"), new Guid("d49ff223-cebb-ec11-983f-000d3abe9bab"));//Diana_Tawil@people365.com
                //UserMap.Add(new Guid("9134AA95-3C10-E911-A2F2-00155D009FA4"), new Guid("0e22f3ac-78b6-ec11-983f-000d3a2ed6e2"));//Dina_ibdah to crm365admin@people365.com
                //UserMap.Add(new Guid("75230EA1-3C10-E911-A2F2-00155D009FA4"), new Guid("0e22f3ac-78b6-ec11-983f-000d3a2ed6e2"));//Eddy_Daou to crm365admin@people365.com
                //UserMap.Add(new Guid("F6B74D09-4399-EC11-A349-00155D009F97"), new Guid("c8cb220f-94b9-ec11-983f-000d3abe9bab"));//Elia_AbouJaoude@people365.com
                //UserMap.Add(new Guid("DB132BAC-43F0-E711-A2DB-00155D84DA96"), new Guid("bbcb220f-94b9-ec11-983f-000d3abe9bab"));//Elie_Karam@people365.com
                //UserMap.Add(new Guid("BEDF3971-32F1-E711-A2DB-00155D84DA96"), new Guid("6b757486-19d8-ec11-a7b5-000d3abc4d5c"));//Evelyne_Ghosn@people365.com
                //UserMap.Add(new Guid("419649D5-14FD-E711-A2DC-00155D84DA96"), new Guid("0e22f3ac-78b6-ec11-983f-000d3a2ed6e2"));//fadytest to crm365admin@people365.com
                //UserMap.Add(new Guid("F8F3A806-A4EB-E711-A2DB-00155D84DA96"), new Guid("abcb220f-94b9-ec11-983f-000d3abe9bab"));//Fadi_abiaad@people365.com
                //UserMap.Add(new Guid("8E825EB0-3C10-E911-A2F2-00155D009FA4"), new Guid("5dcb220f-94b9-ec11-983f-000d3abe9bab"));//Fady_Chammas@people365.com
                //UserMap.Add(new Guid("CD803CC6-3C10-E911-A2F2-00155D009FA4"), new Guid("10a50b06-48d0-ec11-a7b5-000d3a3a57ec"));//Georges_Akl@people365.com
                //UserMap.Add(new Guid("30750397-43F0-E711-A2DB-00155D84DA96"), new Guid("2cdba507-1ad8-ec11-a7b5-000d3abc4d5c"));//Gilberte_Hakim@people365.com
                //UserMap.Add(new Guid("A3B0A504-F152-EA11-A303-00155D84DAE0"), new Guid("0e22f3ac-78b6-ec11-983f-000d3a2ed6e2"));//Gilberte_ESS@sets.com.lb to crm365admin@people365.com
                //UserMap.Add(new Guid("23BE46CE-3C10-E911-A2F2-00155D009FA4"), new Guid("0e22f3ac-78b6-ec11-983f-000d3a2ed6e2"));//hadeel_salameh to crm365admin@people365.com
                //UserMap.Add(new Guid("C1CCA419-7F0E-E911-A2F2-00155D009FA4"), new Guid("1e41ce54-1ad8-ec11-a7b5-000d3abc4d5c"));//Imkiyas_abduljabbar@people365.com
                //UserMap.Add(new Guid("1AE0E00B-3D10-E911-A2F2-00155D009FA4"), new Guid("b5ffd47a-1ad8-ec11-a7b5-000d3abb61a3"));//Jacqueline_Eid@people365.com
                //UserMap.Add(new Guid("D37E4A87-5409-EC11-A330-00155D009F36"), new Guid("e9cb220f-94b9-ec11-983f-000d3abe9bab"));//Jad_Bouchebl@people365.com
                //UserMap.Add(new Guid("18D94C44-54F1-E711-A2DB-00155D84DA96"), new Guid("0e22f3ac-78b6-ec11-983f-000d3a2ed6e2"));//Jhonny_Lebanon to crm365admin@people365.com
                //UserMap.Add(new Guid("46D04E1B-52F0-E711-A2DB-00155D84DA96"), new Guid("0e22f3ac-78b6-ec11-983f-000d3a2ed6e2"));//jhonny_merhi to crm365admin@people365.com
                //UserMap.Add(new Guid("D597F125-3D10-E911-A2F2-00155D009FA4"), new Guid("184655ec-1ad8-ec11-a7b5-000d3abb61a3"));//Jihane_AbouJaoude@people365.com
                //UserMap.Add(new Guid("CDD7C73A-3D10-E911-A2F2-00155D009FA4"), new Guid("0e22f3ac-78b6-ec11-983f-000d3a2ed6e2"));//Joelle_Azzi to crm365admin@people365.com
                //UserMap.Add(new Guid("33927BDF-5572-E911-A2F5-00155D84DA6F"), new Guid("a6a23a22-1bd8-ec11-a7b5-000d3abb61a3"));//Johnny_Bassili@people365.com
                //UserMap.Add(new Guid("C8F96D43-C3EF-E711-A2DB-00155D84DA96"), new Guid("a1cb220f-94b9-ec11-983f-000d3abe9bab"));//Josette_Obeid@people365.com
                //UserMap.Add(new Guid("845C0346-3D10-E911-A2F2-00155D009FA4"), new Guid("0e22f3ac-78b6-ec11-983f-000d3a2ed6e2"));//Julien_Mansour to crm365admin@people365.com
                //UserMap.Add(new Guid("AF3E104F-3D10-E911-A2F2-00155D009FA4"), new Guid("0e22f3ac-78b6-ec11-983f-000d3a2ed6e2"));//mahdi_nehme to crm365admin@people365.com
                //UserMap.Add(new Guid("750FE730-A4EB-E711-A2DB-00155D84DA96"), new Guid("d29ff223-cebb-ec11-983f-000d3abe9bab"));//MC@people365.com
                //UserMap.Add(new Guid("E9022B60-3D10-E911-A2F2-00155D009FA4"), new Guid("6a1106e4-4ad0-ec11-a7b5-000d3a3a57ec"));//MarieMarthe_Haddad@people365.com
                //UserMap.Add(new Guid("0E0BB9EF-3AD1-E811-A2F1-00155D009FA4"), new Guid("4276840e-1cd8-ec11-a7b5-000d3abb61a3"));//Marlene_Harfouche@people365.com
                //UserMap.Add(new Guid("F1C296C5-B717-E811-A2DD-00155D009F4A"), new Guid("d69ff223-cebb-ec11-983f-000d3abe9bab"));//Marwan.Chahlawi@people365.com
                //UserMap.Add(new Guid("3F9952DB-7C9D-E911-A2F5-00155D84DA6F"), new Guid("0e22f3ac-78b6-ec11-983f-000d3a2ed6e2"));//Maya_Naous to crm365admin@people365.com
                //UserMap.Add(new Guid("8E20ED8C-E768-EA11-A304-00155D009F3B"), new Guid("d3cb220f-94b9-ec11-983f-000d3abe9bab"));//Michel_Abdo@people365.com
                //UserMap.Add(new Guid("52DFA378-3D10-E911-A2F2-00155D009FA4"), new Guid("0e22f3ac-78b6-ec11-983f-000d3a2ed6e2"));//nahwa_elhajjar to crm365admin@people365.com
                //UserMap.Add(new Guid("B1EE8982-43F0-E711-A2DB-00155D84DA96"), new Guid("0e22f3ac-78b6-ec11-983f-000d3a2ed6e2"));//Naji_hajjar to crm365admin@people365.com
                //UserMap.Add(new Guid("CA802625-593B-E911-A2F4-00155D84DA6F"), new Guid("0e22f3ac-78b6-ec11-983f-000d3a2ed6e2"));//nareg_karamanoukian to crm365admin@people365.com
                //UserMap.Add(new Guid("FC429D80-3D10-E911-A2F2-00155D009FA4"), new Guid("0e22f3ac-78b6-ec11-983f-000d3a2ed6e2"));//Nayef_Mamlouk to crm365admin@people365.com
                //UserMap.Add(new Guid("AA51F38C-3D10-E911-A2F2-00155D009FA4"), new Guid("0e22f3ac-78b6-ec11-983f-000d3a2ed6e2"));//noor_daas to crm365admin@people365.com
                //UserMap.Add(new Guid("8001F395-3D10-E911-A2F2-00155D009FA4"), new Guid("de4499b0-1cd8-ec11-a7b5-000d3abc4d5c"));//Pierre_Matar@people365.com
                //UserMap.Add(new Guid("8D01278B-9AEA-E911-A2F8-00155D84DA98"), new Guid("0e22f3ac-78b6-ec11-983f-000d3a2ed6e2"));//Rabih_Hamad to crm365admin@people365.com
                //UserMap.Add(new Guid("DDADA8A8-3D10-E911-A2F2-00155D009FA4"), new Guid("0e22f3ac-78b6-ec11-983f-000d3a2ed6e2"));//Rabih_Kraidli to crm365admin@people365.com
                //UserMap.Add(new Guid("ED92F9B4-3D10-E911-A2F2-00155D009FA4"), new Guid("104100eb-1cd8-ec11-a7b5-000d3abb61a3"));//Racha_Younes@people365.com
                //UserMap.Add(new Guid("0CB498BC-3D10-E911-A2F2-00155D009FA4"), new Guid("9e6ce50c-1dd8-ec11-a7b5-000d3abc4d5c"));//Raed_Mansour@people365.com
                //UserMap.Add(new Guid("F0AB5BEE-DEC6-E811-A2F1-00155D009FA4"), new Guid("0fa56f20-1dd8-ec11-a7b5-000d3abb61a3"));//Rakan_Arafah@people365.com
                //UserMap.Add(new Guid("5C98A334-86F0-E711-A2DB-00155D84DA96"), new Guid("0e22f3ac-78b6-ec11-983f-000d3a2ed6e2"));//Randa to crm365admin@people365.com
                //UserMap.Add(new Guid("3A8C7F04-7F0E-E911-A2F2-00155D009FA4"), new Guid("0e22f3ac-78b6-ec11-983f-000d3a2ed6e2"));//renil_rajan to crm365admin@people365.com
                //UserMap.Add(new Guid("C5BEB1A4-CFD1-E811-A2F1-00155D009FA4"), new Guid("4ccb220f-94b9-ec11-983f-000d3abe9bab"));//Rita_Chammas@people365.com
                //UserMap.Add(new Guid("F3CE5DEE-3D10-E911-A2F2-00155D009FA4"), new Guid("0e22f3ac-78b6-ec11-983f-000d3a2ed6e2"));//Rita_Geha to crm365admin@people365.com
                //UserMap.Add(new Guid("1EA5EF48-06FD-E711-A2DC-00155D84DA96"), new Guid("4f206a7a-fed1-ec11-a7b5-000d3a3a57ec"));//Safaa_Aboufarah@people365.com
                //UserMap.Add(new Guid("A3BC8485-BA88-E811-A2E6-00155D009F78"), new Guid("1a61f784-1dd8-ec11-a7b5-000d3abb61a3"));//Samir_Saafan@people365.com
                //UserMap.Add(new Guid("D2E2FA46-BE5C-EC11-A33F-00155D84DA34"), new Guid("1c99a74f-0dbb-ec11-983f-000d3abe9bab"));//Sawsan_Madhoun@people365.com
                //UserMap.Add(new Guid("E025A015-06FD-E711-A2DC-00155D84DA96"), new Guid("6d6ef345-fed1-ec11-a7b5-000d3a448b39"));//Stephanie_Hayek@people365.com
                //UserMap.Add(new Guid("7542B7DF-BD5C-EC11-A33F-00155D84DA34"), new Guid("4a31cdba-1dd8-ec11-a7b5-000d3abc4d5c"));//Walid_Farah@people365.com
                //UserMap.Add(new Guid("6426EA30-A025-EC11-A331-00155D009F36"), new Guid("0e22f3ac-78b6-ec11-983f-000d3a2ed6e2"));//Ziad_Francis to crm365admin@people365.com
                
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
               
                string ConnectionString = string.Format("RequireNewInstance=True;AuthType=ClientSecret;url=https://bpscrm.crm4.dynamics.com/;ClientId=59a6052e-29ec-4dbb-8b0c-7e233586deeb;ClientSecret=oNm8Q~fFGS5pPgK~HTkYKg0beibWp5yBe1gDzatJ");
                 SqlCommand cmd = null;  
                 string sql = null;
                 SqlDataReader reader = null;   
                 SqlConnection conn = null;

                List<Guid> AnnotationsOnlineID =new List<Guid>();


                CrmServiceClient crmService = new CrmServiceClient(ConnectionString);

                


                List<Guid> Annotations = main.GetIDList();

                string scan = Console.ReadLine();
                int start = Convert.ToInt32(scan);

                scan = Console.ReadLine();
                int end = Convert.ToInt32(scan);

                


                string fields = "annotationid,objectid,subject,notetext,mimetype,documentbody,filesize,filename,createdon,ownerid";
                string[] Fields = fields.Split(',');
                for (int i = start; i < Math.Min(end, Annotations.Count); i++)
                {
                    DataTable dt = main.getAnnotationRow(Annotations[i]);
                    DataRow row = dt.Rows[0];

                    Entity an = new Entity("annotation");

                    foreach (string field in Fields)
                    {
                        if (row[field] != DBNull.Value)
                        {
                            if (field == "createdon")
                                an["overriddencreatedon"] = row[field];
                            else if (field == "objectid")
                                an["objectid"] = new EntityReference(row.Field<string>("logicalname"), row.Field<Guid>(field));
                            else if (field == "ownerid")
                            {
                                if (UserMap.ContainsKey((Guid)row[field]))
                                    an["ownerid"] = new EntityReference("systemuser", UserMap[(Guid)row[field]]);
                            }
                            else
                                an[field] = row[field];
                        }
                    }

                    try
                    {
                        if (crmService.IsReady)
                        {
                            
                            
                                crmService.Create(an);
                                Console.WriteLine("{0}/{1} has been created successfully", i, Annotations.Count);
                            
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
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
       
        public DataTable getAnnotationRow(Guid annotationid)
        {
            string fields = "an.annotationid,an.objectid,an.subject,an.notetext,an.mimetype,an.documentbody,an.filesize,an.filename,an.createdon,an.ownerid";
             //string fields = "annotationid,objectid,subject,notetext,mimetype,documentbody,filesize,filename,createdon,ownerid";
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = "select Top (100) " + fields + @", ent.logicalname 
                                from annotation as an 
                                inner join Entity as ent 
                                on an.ObjectTypeCode = ent.ObjectTypeCode 
                                where annotationid = '" + annotationid + "'";


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
            SqlCommand command = new SqlCommand(@"SELECT annotationid FROM dbo.annotation
                   where  objecttypecode = 10457 ORDER BY annotationid", sqlconn);

            SqlDataReader dr = command.ExecuteReader();
            List<Guid> IDList = new List<Guid>();
            while (dr.Read())
            {
                IDList.Add(dr.GetGuid(0));
            }
            dr.Close();
            return IDList;
        }

        public SqlConnection getConnection()
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

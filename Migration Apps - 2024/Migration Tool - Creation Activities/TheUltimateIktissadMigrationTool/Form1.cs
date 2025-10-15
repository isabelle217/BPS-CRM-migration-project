using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;

namespace TheUltimateIktissadMigrationTool
{
    public partial class TheUltimateTool : Form
    {
        public static string EntityType;
        public static string SqlConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["SQLConnection"].ConnectionString;
        public static string CrmOnlineConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["CRMOnline"].ConnectionString;
        private delegate void CounterDelegate(int count);
        public CrmOnlineHelper CrmHelper;
        public CsvHelper CsvHelper;
        public SQLHelper SqlHelper;

        public DataTable DTOnline;
        public DataTable DTSql;
        public DataTable DTCsv;

        public static List<string> NoStateCodeEntites;


        #region Initialize Form
        public TheUltimateTool()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //fill all tables from SQL 
            comboBox1.DataSource = GetTables(SqlConnectionString);
            //Get the name of the entity with no Status Field
            FillNoStateCode();
        }

        private void FillNoStateCode()
        {
            // Add Entity With No Status Here
            NoStateCodeEntites = new List<string>();
            NoStateCodeEntites.Add("salesorderdetail");
            NoStateCodeEntites.Add("invoicedetail");
            NoStateCodeEntites.Add("quotedetail");
            NoStateCodeEntites.Add("opportunityproduct");
            NoStateCodeEntites.Add("uom");
            NoStateCodeEntites.Add("productpricelevel");
            NoStateCodeEntites.Add("listmember");
            NoStateCodeEntites.Add("systemuser");
        }

        // Function used outside this class to output text.
        public void AppendText(string text)
        {
            textBox.Text += text + "\r\n";
            System.Windows.Forms.Application.DoEvents();
            textBox.ScrollToCaret();
        }
        public void SetIndex(string text)
        {
            label7.Text = text;
            System.Windows.Forms.Application.DoEvents();
        }

        //Get Starting Index
        public int GetStart()
        {
            if (textBox4.Text != "" && textBox4 != null)
                return Convert.ToInt32(textBox4.Text);
            else
                return 0;
        }

        //Get Ending Index
        public int GetEnd()
        {
            if (textBox5.Text != "" && textBox5 != null)
                return Convert.ToInt32(textBox5.Text);
            else
                return 999999;
        }

        // Get the Table and View Names to fill the comboBox.
        public static List<string> GetTables(string connectionString)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                DataTable schema = connection.GetSchema("Tables");
                List<string> TableNames = new List<string>();

                foreach (DataRow row in schema.Rows)
                {
                    TableNames.Add(row[2].ToString());
                }

                TableNames.Sort();
                return TableNames;
            }
        }

        //Pick the CSV File
        private void btnPath_Click(object sender, EventArgs e)
        {
            // Create Open File Dialog with the correct filter
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "csv-file (*.csv) | *.csv";

                string fileNameAndFolder = "";
                string fileName = "";

                // Get file plus folder.
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    fileNameAndFolder = ofd.FileName;

                    // Split folder and filename
                    fileName = Path.GetFileName(fileNameAndFolder);
                }

                // Return the fileName;
                textBox2.Text = fileNameAndFolder;
            }
        }

        //Pick a CSV file containing the Guids of the records to be updated.
        private void btnPath2_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "csv-file (*.csv) | *.csv";

                string fileNameAndFolder = "";
                string fileName = "";

                // Get file plus folder.
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    fileNameAndFolder = ofd.FileName;

                    // Split folder and filename
                    fileName = Path.GetFileName(fileNameAndFolder);
                }

                // Return the fileName;
                textBox3.Text = fileNameAndFolder;
            }
        }

        #endregion

        #region Button Events

        private void btnCreate_Click(object sender, EventArgs e)
        {
            InitializeTables();
            CreateDifference();
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            InitializeTables();
            UpdateRecords();
        }

        private void btnDeactivate_Click(object sender, EventArgs e)
        {
            InitializeTables();
            ManageStatusDifference();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            InitializeTables();
            DeleteDifference();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            InitializeTables();
            AssignRecords();
        }

        // Prepare Data. 
        // Set All the Columns of the FromData in a DataTable and the Already Migrated Guids of the target org in another DataTable
        private void InitializeTables()
        {
            // From Table Chosen
            if (comboBox1.Text != "" && textBox2.Text != "")
            {
                try
                {
                    if (comboBox1.Text.StartsWith("Filtered"))
                        EntityType = comboBox1.Text.ToLower().Substring(8);
                    else
                        EntityType = comboBox1.Text.ToLower();

                    //Initialize Helper Classes
                    CrmHelper = new CrmOnlineHelper(this);
                    CsvHelper = new CsvHelper();
                    SqlHelper = new SQLHelper();

                    //Get csv File, SQL Data, Online Existing Guids.
                    DTCsv = CsvHelper.ConvertCSVtoDataTable(textBox2.Text);
                    DTSql = SqlHelper.GetCRM4Records(DTCsv, TxtFiltration.Text);
                    DTOnline = CrmHelper.GetCRMOnlineRecords();

                    textBox.Text += DTCsv.Rows.Count + " Fields will be Migrated" + "\r\n";
                    textBox.Text += " CRM 4 has " + DTSql.Rows.Count + " records \r\n";
                    textBox.Text += " CRM Online has " + DTOnline.Rows.Count + " records \r\n\r\n";
                }
                catch (OutOfMemoryException ex)
                {
                    textBox.Text += ex.ToString() + "\r\n";
                    throw ex;
                }
                catch (Exception ex)
                {
                    textBox.Text += ex.ToString() + "\r\n";
                    throw ex;
                }
            }
            else
            {
                textBox.Text = "Please fill in the Entity Type and the CSV file path.";
            }
        }


        #endregion

        #region Execute Request

        public void CreateDifference()
        {
            // Find Difference
            var EntityDiff = from entityFrom in DTSql.AsEnumerable()
                             join entityTo in DTOnline.AsEnumerable()
                             on entityFrom.Field<Guid>("activity" + "id") equals entityTo.Field<Guid>("activity" + "id") into JoinedEntities
                             from joinedElem in JoinedEntities.DefaultIfEmpty()
                             where joinedElem == null
                             select entityFrom;
            textBox.Text += " CRM Difference of records is  " + EntityDiff.Count() + " records \r\n\r\n";

            // Create missing records
            CrmHelper.CreateMissingRecords2(EntityDiff, DTCsv);
        }

        public void ManageStatusDifference()
        {
            // Get Records with Same ID but different Status
            var EntityDiff = from entityFrom in DTSql.AsEnumerable()
                             join entityTo in DTOnline.AsEnumerable()
                             on entityFrom.Field<Guid>("activity" + "id") equals entityTo.Field<Guid>("activity" + "id")
                             where entityFrom["statecode"] != DBNull.Value && entityFrom["statuscode"] != DBNull.Value && 
                             (entityTo.Field<int>("statecode") != entityFrom.Field<int>("statecode")
                             || entityTo.Field<int>("statuscode") != entityFrom.Field<int>("statuscode"))
                             select entityFrom;

            textBox.Text += " CRM Difference of records is  " + EntityDiff.Count() + " records \r\n\r\n";

            // Update Their Status
            CrmHelper.SetRecordsStatus(EntityDiff.ToList());
        }

        public void DeleteDifference()
        {
            // Find Difference
            var EntityDiff = from entityFrom in DTOnline.AsEnumerable()
                             join entityTo in DTSql.AsEnumerable()
                             on entityFrom.Field<Guid>("activity" + "id") equals entityTo.Field<Guid>("activity" + "id") into JoinedEntities
                             from joinedElem in JoinedEntities.DefaultIfEmpty()
                             where joinedElem == null
                             select entityFrom;

            textBox.Text += " CRM Difference of records is  " + EntityDiff.Count() + " records \r\n\r\n";

            // Delete Difference
            CrmHelper.DeleteDifference(EntityDiff);
        }

        public void UpdateRecords()
        {
            if (textBox3.Text != "" && textBox3.Text != null)
            {
                //If a Guid file has been added, get their SQL associated rows 
                DataTable UpdateFailures = ReadUpdateFailures(textBox3.Text);
                var EntityDiff = from entityFrom in DTSql.AsEnumerable()
                                 join entityTo in UpdateFailures.AsEnumerable()
                                 on entityFrom["activity" + "id"].ToString() equals entityTo["guid"].ToString().ToLower()
                                 select entityFrom;
                textBox.Text += " The Update will start on " + EntityDiff.Count() + " records \r\n\r\n";

                // Update Online Records
                CrmHelper.UpdateRecords(EntityDiff, DTCsv);
            }
            else
            {
                textBox.Text += " The Update will start on " + DTSql.Rows.Count + " records \r\n\r\n";

                //Update All the Records
                CrmHelper.UpdateRecords(DTSql.Rows.OfType<DataRow>(), DTCsv);
            }
        }

        //Insert the Guid from the csv file in a DataTable
        public DataTable ReadUpdateFailures(string path)
        {
            DataTable UpdateFailures;

            CsvHelper csvReader = new CsvHelper();
            UpdateFailures = csvReader.ConvertCSVtoDataTable(path);

            return UpdateFailures;
        }

        //Assign Records
        public void AssignRecords()
        {

            textBox.Text += " The Assign will start on " + DTSql.Rows.Count + " records \r\n\r\n";

            CrmHelper.AssignRecords(DTSql.Rows.OfType<DataRow>(), DTCsv);
        }



        #endregion

        public void RefreshCounterText(int count)
        {
            if (lblCounter.InvokeRequired)
            {
                var d = new CounterDelegate(RefreshCounterText);
                lblCounter.Invoke(d, new object[] { count });
            }
            else
            {
                lblCounter.Text = count.ToString() + "/" + DTSql.Rows.Count.ToString();
                textBox.Text += DateTime.Now.ToString() + "  : " + count.ToString() + "/" + DTSql.Rows.Count.ToString() + "\r\n";
                //Logger.Log( DateTime.Now.ToString() + "  : " + count.ToString() + "/" + DTSql.Rows.Count.ToString());
                textBox.ScrollToCaret();
                System.Windows.Forms.Application.DoEvents();
            }

        }

        private void lblCounter_Click(object sender, EventArgs e)
        {

        }

        private void textBox_TextChanged(object sender, EventArgs e)
        {

        }
    }
}

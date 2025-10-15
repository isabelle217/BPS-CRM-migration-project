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

        public CrmOnlineHelper CrmHelper;
        public CsvHelper CsvHelper;
        public SQLHelper SqlHelper;

        public DataTable DTOnline;
        public DataTable DTSql;
        public DataTable DTCsv;

        public Dictionary<string, IEnumerable<DataRow>> Delta;
        public List<Guid> GuidDiff;
        public List<string> GuidDifffromnew;

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
            FillNoStateCode();
        }

        private void FillNoStateCode()
        {
            NoStateCodeEntites = new List<string>();
            NoStateCodeEntites.Add("salesorderdetail");
            NoStateCodeEntites.Add("invoicedetail");
            NoStateCodeEntites.Add("uom");
            NoStateCodeEntites.Add("productpricelevel");
            NoStateCodeEntites.Add("listmember");
        }

        private void FrmMigrate_Load(object sender, EventArgs e)
        {
            //fill all tables from SQL 
            comboBox1.DataSource = GetTables(SqlConnectionString);
        }

        public void AppendText(string text)
        {
            textBox.Text += text + "\r\n";
            System.Windows.Forms.Application.DoEvents();
            textBox.ScrollToCaret();
        }


        // Get the Entity Names to fill the comboBox.
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

        #endregion

        #region Button Events

        private void InitializeTables()
        {
            if (comboBox1.Text != "" && textBox2.Text != "")
            {
                try
                {
                    if (comboBox1.Text.StartsWith("Filtered"))
                        EntityType = comboBox1.Text.ToLower().Substring(8);
                    else
                        EntityType = comboBox1.Text.ToLower();

                    CrmHelper = new CrmOnlineHelper(this);
                    CsvHelper = new CsvHelper();
                    SqlHelper = new SQLHelper();

                    DTCsv = CsvHelper.ConvertCSVtoDataTable(textBox2.Text);
                    DTSql = SqlHelper.GetCRM4Records(DTCsv, TxtFiltration.Text);
                    DTOnline = CrmHelper.GetCRMOnlineRecords(DTCsv);

                    textBox.Text += DTCsv.Rows.Count + " Fields will be Migrated" + "\r\n";
                    textBox.Text += " CRM 4 has " + DTSql.Rows.Count + " records \r\n";
                    textBox.Text += " CRM Online has " + DTOnline.Rows.Count + " records \r\n\r\n";

                    Delta = new Dictionary<string, IEnumerable<DataRow>>();
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

        private void button1_Click(object sender, EventArgs e)
        {
            InitializeTables();
            CalculateDifferences();
        }


        #endregion

        #region Execute Request

        public void CalculateDifferences()
        {
            DataTable DifferenceResults = new DataTable();
            DifferenceResults.Columns.Add("Source Name", typeof(string));
            DifferenceResults.Columns.Add("Target Name", typeof(string));
            DifferenceResults.Columns.Add("Count", typeof(int));
            foreach (DataRow row in DTCsv.Rows)
            {
                var EntityDiff = from entityFrom in DTSql.AsEnumerable()
                                 join entityTo in DTOnline.AsEnumerable()
                                 on entityFrom.Field<Guid>(EntityType + "id").ToString() equals entityTo.Field<string>(EntityType + "id")
                                 where entityFrom[row.Field<string>("sourcename")].ToString() != entityTo[row.Field<string>("targetname")].ToString()
                                 select entityFrom;

                int EntityDiffCount = EntityDiff.Count();
                if (EntityDiffCount > 0)
                {
                    Delta[row.Field<string>("sourcename")] = EntityDiff;
                }

                DifferenceResults.Rows.Add(row.Field<string>("sourcename"), row.Field<string>("targetname"), EntityDiffCount);

                textBox.Text += row.Field<string>("sourcename") + " - " + row.Field<string>("targetname") + ": Difference of " + EntityDiffCount + "\r\n";
                System.Windows.Forms.Application.DoEvents();
            }


            dataGridView1.DataSource = DifferenceResults;
            comboBox2.DataSource = Delta.Keys.ToList();

            
            


            
        }



        #endregion

        private void button2_Click(object sender, EventArgs e)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Guid");
            dt.Columns.Add("Value");
            foreach (var r in Delta[comboBox2.Text])
                dt.Rows.Add(r.Field<Guid>(EntityType + "id").ToString(), r[comboBox2.Text].ToString());
            dataGridView2.DataSource = dt;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string targetname = (from r in DTCsv.AsEnumerable()
                                                where r.Field<string>("sourcename") == comboBox2.Text 
                                                select r.Field<string>("targetname")).ElementAt(0);
            CrmHelper.UpdateField(comboBox2.Text, targetname, Delta[comboBox2.Text]);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //////////////////////////////////////////////////////
            // Get missing ids - useful for non easily creatable entities like listmember
            var RecordDiff = from entityFrom in DTSql.AsEnumerable()
                             join entityTo in DTOnline.AsEnumerable()
                             on new Tuple<string,string>( entityFrom.Field<Guid>("listid").ToString(), entityFrom.Field<Guid>("entityid").ToString() )
                             equals new Tuple<string,string> ( entityTo.Field<string>("listid"), entityTo.Field<string>("entityid") )
                             into JoinedEntities
                             from joinedElem in JoinedEntities.DefaultIfEmpty()
                             where joinedElem == null
                             select entityFrom;
                            // select entityFrom.Field<Guid>(EntityType + "id");

            ///////////////////////////////////////////////////////
            dataGridView2.DataSource = RecordDiff.CopyToDataTable();

            CrmHelper.AddToMarketingList(RecordDiff);

        }

        private void button5_Click(object sender, EventArgs e)
        {
            //////////////////////////////////////////////////////
            // Get Additional Wrong Tuples
            var RecordDiff2 = from entityTo in DTOnline.AsEnumerable()
                             join entityFrom in DTSql.AsEnumerable()
                             on new Tuple<string,string> ( entityTo.Field<string>("listid"), entityTo.Field<string>("entityid") )
                             equals new Tuple<string,string>( entityFrom.Field<Guid>("listid").ToString(), entityFrom.Field<Guid>("entityid").ToString() )
                             into JoinedEntities
                             from joinedElem in JoinedEntities.DefaultIfEmpty()
                             where joinedElem == null
                             select entityTo;
                             //select entityTo.Field<string>(EntityType + "id");
            ///////////////////////////////////////////////////////

            dataGridView2.DataSource = RecordDiff2.CopyToDataTable();

            CrmHelper.RemoveFromMarketingList(RecordDiff2);
        }
    }
}



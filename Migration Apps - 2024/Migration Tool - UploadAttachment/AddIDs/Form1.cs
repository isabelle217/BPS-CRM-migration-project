using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Windows.Forms;

using System.ServiceModel.Description;

namespace AddIDs
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        static SqlConnection conn = null;

        private void Form1_Load(object sender, EventArgs e)
        {

            SqlConnection sqlconn = getConnection();
            SqlCommand command = new SqlCommand(@"SELECT annotationid FROM dbo.annotation
                  ORDER BY annotationid", sqlconn);

            SqlDataReader dr = command.ExecuteReader();
            List<Guid> IDList = new List<Guid>();
            while (dr.Read())
            {
                IDList.Add(dr.GetGuid(0));
            }
            dr.Close();
           

        }
        private SqlConnection TryConnect(int maxreconnectretries)
        {
            SqlConnection connection = null;
            string SqlConnectionString = "Data Source=crm2016-lab;Initial Catalog=SETSCRM;Integrated Security=False; User ID=sa; Password=P@ssw0rd;MultipleActiveResultSets=True";

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
      
    }
}

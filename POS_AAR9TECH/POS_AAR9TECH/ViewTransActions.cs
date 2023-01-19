using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Relational;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace POS_AAR9TECH
{
    public partial class ViewTransActions : Form
    {
        string connectionString = "server=localhost;uid=root;pwd=\"\";database=projectmotor";

        public ViewTransActions()
        {
            InitializeComponent();

        }
        public ViewTransActions(string connectionstring)
        {
            InitializeComponent();
            this.connectionString = connectionstring;
        }

        //NEEDED ON POPUPS

        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (Form.ModifierKeys == Keys.None && keyData == Keys.Escape)
            {
                this.Close();
                return true;
            }
            return base.ProcessDialogKey(keyData);
        }

        private void getTransactions()
        {
            try
            {
                MySqlConnection con = new MySqlConnection(connectionString);
                MySqlDataAdapter adapter = new MySqlDataAdapter("SELECT trans_code as \"TRANSACTION CODE\", SUM(TRANS_QUANTITY) as QUANTITY,SUM(TRANS_PRICE) as TOTAL FROM TRANSACTIONS GROUP BY trans_code", con);
                DataTable table = new DataTable();
                adapter.Fill(table);

                dataGridView1.DataSource = table;
            }
            catch (SqlException ex)
            {
                MessageBox.Show("An error occurred while retrieving data from the database: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ViewTransActions_Load(object sender, EventArgs e)
        {
            getTransactions();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(dataGridView1.SelectedRows.Count >  0)
            {
                using(INVOICE form = new INVOICE(dataGridView1.SelectedRows[0].Cells[0].Value.ToString(), connectionString))
                {
                    var result = form.ShowDialog();
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}

using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace POS_AAR9TECH
{
    public partial class CustomerSelect : Form
    {
        string connectionString = "server=localhost;uid=root;pwd=\"\";database=projectmotor";
        public int id { get; set; }
        public CustomerSelect()
        {
            InitializeComponent();

            dataGridView1.DataSource = getCustomer();
        }
        public CustomerSelect(string connectionString)
        {
            InitializeComponent();
            this.connectionString = connectionString;
            dataGridView1.DataSource = getCustomer();
        }

        //NEEDED ON POPUPS

        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (Form.ModifierKeys == Keys.None && keyData == Keys.Escape)
            {
                this.Close();
                return true;
            }
            if (Form.ModifierKeys == Keys.None && keyData == Keys.Enter)
            {
                button1.PerformClick();
                return true;
            }
            return base.ProcessDialogKey(keyData);
        }



        private DataTable getCustomer()
        {
            DataTable dt = new DataTable();
            MySqlConnection con = new MySqlConnection(connectionString); //open connection
            con.Open();
            MySqlDataAdapter adapter = new MySqlDataAdapter("SELECT CUS_ID as ID, CUS_fNAME as \"FIRST NAME\", CUS_lNAME as \"LAST NAME\", CUS_EMAIL as \"EMAIL\", CUS_PHONENUM as \"PHONE NUMBER\" FROM CUSTOMER", con);
            adapter.Fill(dt);
            con.Close();
            return dt;
        }
        private DataTable searchByName(string name)
        {
            DataTable dt = new DataTable();

            MySqlConnection con = new MySqlConnection(connectionString); //open connection
            con.Open();
            MySqlDataAdapter adapter = new MySqlDataAdapter("SELECT CUS_ID as ID, CUS_fNAME as \"FIRST NAME\", CUS_lNAME as \"LAST NAME\", CUS_EMAIL as \"EMAIL\", CUS_PHONENUM as \"PHONE NUMBER\" FROM CUSTOMER WHERE CUS_LNAME LIKE \"%" + name+"%\" OR CUS_FNAME LIKE \"%"+name+ "%\" OR CUS_EMAIL LIKE \"%" + name + "%\" OR CUS_ID LIKE \"%"+name+"%\"", con);
            adapter.Fill(dt);
            con.Close();
            return dt;
        }

        private void CustomerSelect_Deactivate(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(searchTxtBox.Text != null)
            {
                try
                {
                    dataGridView1.DataSource = searchByName(searchTxtBox.Text);
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message, "ERROR",MessageBoxButtons.OK,MessageBoxIcon.Error);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                dataGridView1.DataSource = getCustomer();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            searchTxtBox.Text = null;
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                id = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells[0].Value);
                textBox2.Text = id.ToString() + " - " + dataGridView1.SelectedRows[0].Cells[1].Value.ToString() + " " + dataGridView1.SelectedRows[0].Cells[2].Value.ToString();
            }
            catch
            {

            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            try
            {
                this.id = Int32.Parse(dataGridView1.SelectedRows[0].Cells[0].Value.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView1.IsCurrentCellInEditMode == true) dataGridView1.CancelEdit();
        }
    }
}

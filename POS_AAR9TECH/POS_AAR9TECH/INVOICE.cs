using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace POS_AAR9TECH
{
    public partial class INVOICE : Form
    {
        MySqlConnection conn;
        MySqlCommand cmd;
        MySqlDataAdapter adapter;
        DataTable dt;
        double total = 0;
        double payment = 0;
        double change = 0;

        string connectionString = "server=localhost;uid=root;pwd='';database=projectmotor";
        string transCode = "";
        public INVOICE()
        {
            InitializeComponent();
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

        public INVOICE(string transCode, string connectionString)
        {
            InitializeComponent();
            this.connectionString = connectionString;
            this.transCode = transCode;
        }
        public void PrintScreen()
        {
            // Create a new bitmap with the same size as the form
            Bitmap bmp = new Bitmap(this.Width, this.Height);
            // Create a new graphics object from the bitmap
            Graphics g = Graphics.FromImage(bmp);
            // Draw all the controls on the form onto the bitmap
            this.DrawControls(g, bmp);
            // Create a new PrintDocument
            PrintDocument doc = new PrintDocument();
            // Set the document to print the bitmap
            doc.PrintPage += (sender, e) => {
                e.Graphics.DrawImage(bmp, 0, 0);
            };
            // Show the Print dialog
            PrintDialog dlg = new PrintDialog();
            dlg.Document = doc;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                // Print the document
                doc.Print();
            }
        }

        private void DrawControls(Graphics g, Bitmap bmp)
        {
            // Draw all the labels on the form
            foreach (Control control in this.Controls)
            {
                if (control is Label)
                {
                    control.DrawToBitmap(bmp, control.Bounds);
                }
                if (control is DataGridView)
                {
                    DataGridView dgv = (DataGridView)control;
                    dgv.ScrollBars = ScrollBars.Both;
                    control.DrawToBitmap(bmp, control.Bounds);
                }
            }
        }

        private void getPayment()
        {
            try
            {
                DataTable dt2 = new DataTable();
                // Connect to MySQL
                conn = new MySqlConnection(connectionString);
                conn.Open();

                // Populate DataGridView
                cmd = new MySqlCommand("SELECT trans_payment, customer.cus_fname, customer.cus_lname, staff.staff_fname, staff.staff_lname,trans_created_at FROM transactions JOIN customer ON transactions.cus_id = customer.cus_id JOIN staff ON transactions.staff_id = staff.staff_id WHERE transactions.trans_code = " + transCode, conn);
                adapter = new MySqlDataAdapter(cmd);
                adapter.Fill(dt2);
                payment = Convert.ToDouble(dt2.Rows[0]["trans_payment"].ToString());
                txtCustomer.Text = dt2.Rows[0]["cus_fname"].ToString() + " " + dt2.Rows[0]["cus_lname"].ToString();
                txtStaff.Text = dt2.Rows[0]["staff_fname"].ToString() + " " + dt2.Rows[0]["staff_lname"].ToString();
                txtDate.Text = dt2.Rows[0]["trans_created_at"].ToString() ;
                label7.Text += transCode;

            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                conn.Close();
            }
        }

        private void INVOICE_Load(object sender, EventArgs e)
        {
            getPayment();
            try
            {
                // Connect to MySQL
                conn = new MySqlConnection(connectionString);
                conn.Open();

                // Populate DataGridView
                cmd = new MySqlCommand("SELECT item.item_name, transactions.trans_quantity, item.item_price, transactions.trans_price FROM transactions JOIN item ON transactions.item_id = item.item_id WHERE transactions.trans_code = "+transCode, conn);
                adapter = new MySqlDataAdapter(cmd);
                dt = new DataTable();
                adapter.Fill(dt);



                // Calculate total amount
                double total = 0;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    total += Convert.ToDouble(dt.Rows[i]["trans_price"]);
                }

                DataTable dtClone = dt.Clone(); 
                for (int i = 0; i < dtClone.Columns.Count; i++)
                {
                    if (dtClone.Columns[i].DataType != typeof(string))
                        dtClone.Columns[i].DataType = typeof(string);
                }

                foreach (DataRow dr in dt.Rows)
                {
                    dtClone.ImportRow(dr);
                }
                dtClone.Rows.Add("", "", "PAYMENT:", this.payment.ToString("0,00.00"));
                dtClone.Rows.Add("", "", "TOTAL:", total.ToString("0,00.00"));
                dtClone.Rows.Add("", "", "CHANGE:", (this.payment- total).ToString("0,00.00"));
                dataGridView1.DataSource = dtClone;
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                conn.Close();
            }
        }
    }
}

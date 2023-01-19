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
using K4os.Compression.LZ4.Encoders;
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Relational;
using ZXing;

namespace POS_AAR9TECH
{
    public partial class Form1 : Form
    {
        static string server = "";
        static string uid = "";
        static string pwd = "";
        static string database = "";

        string connectionString = $"server={server};uid={uid};pwd={pwd};database={database}";
        bool validated = false;
        double totalPrice = 0;
        int customerId = 1;
        int staffId = 0;
        double payment = 0;
        string transCode = GenerateTransactionCode();
        bool checkingOut = false;

        public Form1()
        {
            InitializeComponent();
        }

        public bool clearing = false;
        private void resetAll()
        {
            ChangeCustomer(1);
            payment = 0;
            transCode = GenerateTransactionCode();
            lblTransCode.Text = transCode;
            checkingOut = false;
            clearing = true;
            dataGridView1.Rows.Clear();
            dataGridView1.Refresh();
            clearing = false;
            EnableAll();
            btnEditQuantity.Enabled = false;
            btnRmvItem.Enabled = false;
            label1.Visible = false; label2.Visible = false;
            label3.Visible = false; lblTotal.Visible = false;
            label4.Visible = false; label8.Visible = false;
            lblPayment.Visible = false;
            lblChange.Visible = false;
            btnCheckOut.Text = "CHECKOUT";
            checkingOut = false;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            openForm("AddItem");
        }

        //For Showing FORMS
        public bool isFormOpen(string formName)
        {

            bool isOpen = false;
            FormCollection fc = Application.OpenForms;

            foreach (Form frm in fc)
            {
                //iterate through
                if (frm.Name == formName)
                {
                    isOpen = true;
                }
            }
            return isOpen;
        }

        public static string GenerateTransactionCode()
        {
            var currentDate = DateTime.Now;
            var transactionCode = $"{currentDate.Year - 2020}{currentDate.Month.ToString("00")}{currentDate.Day.ToString("00")}{currentDate.Hour.ToString("00")}{currentDate.Minute.ToString("00")}{currentDate.Second.ToString("00")}";
            return transactionCode;
        }

        //CODE FOR GETTING ITEM FROM DATABASE
        private DataTable getItem()
        {
            DataTable dt = new DataTable();
            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    MySqlDataAdapter adapter = new MySqlDataAdapter("select * from item where item_status = \"ACTIVE\"", con);
                    adapter.Fill(dt);
                }
                if (dt.Rows.Count == 0)
                {
                    MessageBox.Show("No items found. Items might not be active or is empty.","ERROR", MessageBoxButtons.OK,MessageBoxIcon.Error);
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            finally
            {
                
            }
            return dt;
        }

        //Gets Item DATA from DATABASE USING ID
        private DataTable getItem(int id)
        {
            DataTable dt = new DataTable();
            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();
                string query = "select * from item where item_id = @id AND item_status = \"ACTIVE\"";
                MySqlCommand cmd = new MySqlCommand(query, con);
                cmd.Parameters.AddWithValue("@id", id);
                MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                adapter.Fill(dt);
            }
            return dt;
        }

        //Gets Item DATA from DATABASE USING ID
        private DataTable getItem(int id, string column)
        {
            DataTable dt = new DataTable();
            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();
                string query = "select ITEM_QUANTITY from item where item_id = @id AND item_status = \"ACTIVE\"";
                MySqlCommand cmd = new MySqlCommand(query, con);
                cmd.Parameters.AddWithValue("@id", id);
                MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                adapter.Fill(dt);
            }
            return dt;
        }

        //CODE FOR THE SHORTCUTS
        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (Form.ModifierKeys == Keys.None)
            {
                switch (keyData)
                {
                    case Keys.F1:
                        openForm("AddItem");
                        break;
                }
            }
            return base.ProcessDialogKey(keyData);
        }

        private void button3_Click(object sender, EventArgs e)
        {

            if (dataGridView1.CurrentCell != null)
                dataGridView1.BeginEdit(true);
        }

        private void openForm(string s)
        {
            if (isFormOpen(s))
            {
                return;
            }

            Form formBackground = new Form();
            formBackground.StartPosition = FormStartPosition.Manual;
            formBackground.FormBorderStyle = FormBorderStyle.None;
            formBackground.Opacity = .50d;
            formBackground.BackColor = Color.Black;
            //formBackground.WindowState = FormWindowState.Maximized;
            formBackground.Size = this.Size;
            formBackground.Location = this.Location;
            formBackground.ShowInTaskbar = false;

            try
            {
                switch (s)
                {
                    case "AddItem":
                        formBackground.Show();
                        if (getItem().Rows.Count < 1) break;
                        using (var form = new AddItem(getItem()))
                        {
                            var result = form.ShowDialog();
                            if (result == DialogResult.OK)
                            {
                                string id = form.IDSelected;
                                int quantity = form.quantity;
                                AddToCart(id, quantity);
                            }
                        }
                        break;

                    case "Login":
                        formBackground.Show();
                        using (var loginForm = new Login())
                        {
                            var result = loginForm.ShowDialog();
                            if (result == DialogResult.OK)
                            {
                                server = loginForm.server;
                                uid = loginForm.uid;
                                pwd = loginForm.pwd;
                                database= loginForm.database;
                                staffId = loginForm.staffId;
                                connectionString = $"server={loginForm.server};uid={loginForm.uid};pwd={loginForm.pwd};database={loginForm.database}";
                                customerId = getFirstCustomer();
                            }
                            else
                            {
                                this.Dispose();
                            }
                        }
                        break;

                    case "CustomerSelect":
                        formBackground.Show();
                        using (var form = new CustomerSelect(connectionString))
                        {
                            form.Owner = formBackground;
                            var result = form.ShowDialog();
                            if (result == DialogResult.OK)
                            {
                                ChangeCustomer(form.id);
                            }
                        }
                        break;

                    case "CheckOut":
                        formBackground.Show();
                        using (var form = new CheckOut(GetTotalPrice()))
                        {
                            form.Owner = formBackground;
                            var result = form.ShowDialog();
                            if (result == DialogResult.OK)
                            {
                                payment = form.payment;
                                formBackground.Dispose();
                                CheckOutProceed();
                            }
                        }
                        break;

                    case "ViewTransActions":
                        formBackground.Show();
                        using (var form = new ViewTransActions(connectionString))
                        {
                            form.Owner = formBackground;
                            form.ShowDialog();
                        }
                        break;

                    case "INVOICE":
                        using (var form = new INVOICE(transCode, connectionString))
                        {
                            formBackground.TopMost = false;
                            formBackground.Show();
                            var result = form.ShowDialog();
                            formBackground.Dispose();
                            form.PrintScreen();
                        }
                        break;

                    default:
                        throw new ArgumentException("Invalid form name: " + s);
                }
            }
            catch (Exception ex)
            {
                // Log or handle the exception as appropriate
                Console.WriteLine("Error: " + ex.ToString());
            }
            finally
            {
                formBackground.Dispose();
            }
        }

        private int getFirstCustomer()
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT CUS_ID FROM Customer ORDER BY CUS_ID ASC LIMIT 1";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    var customer_id = command.ExecuteScalar();
                    if (customer_id != null)
                    {
                        return Convert.ToInt32(customer_id);
                    }
                    else
                    {
                        MessageBox.Show("No customer in the table");
                        return 1;
                    }
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show("An error occurred while trying to retrieve the first Customer_id: " + ex.Message);
                }
                finally
                {
                    connection.Close();
                }
            }

            return 1;
        }

        private void DisableAll()
        {

            button2.Enabled = false;
            btnEditQuantity.Enabled = false;
            btnRmvItem.Enabled = false;
            dataGridView1.Enabled = false;
            button8.Enabled = false;
            button10.Enabled = false;
            button9.Enabled = false;
            button11.Enabled = false;
        }
        private void EnableAll()
        {
            button2.Enabled = true;
            dataGridView1.Enabled = true;
            button8.Enabled = true;
            button10.Enabled = true;
            button9.Enabled = true;
            button11.Enabled = true;
        }

        private void CheckOutProceed()
        {
            try
            {
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    int itemId = Convert.ToInt32(row.Cells[0].Value);
                    int quantity = Convert.ToInt32(row.Cells[3].Value);
                    reduceQuantity(itemId, quantity);
                    recordTrans(row);
                }
            }
            catch (Exception ex)
            {
                // Log or handle the exception as appropriate
                Console.WriteLine("Error: " + ex.ToString());
                // Show a message to the user
                MessageBox.Show("An error occurred while processing the checkout. Please try again or contact support.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DisableAll();
            btnCheckOut.Text = "DONE";
            lblPayment.Text = payment.ToString("0,0.00");
            lblChange.Text = (payment - GetTotalPrice()).ToString("0,0.00");
            lblPayment.Visible = true; lblChange.Visible = true; label4.Visible = true; label8.Visible = true;
            checkingOut = true;

            openForm("INVOICE");
        }
        private void reduceQuantity(int id, int quantity)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();

                    string updateSql = "UPDATE item SET ITEM_QUANTITY = ITEM_QUANTITY - @quantity WHERE ITEM_ID = @itemId";

                    using (MySqlCommand cmd = new MySqlCommand(updateSql, conn))
                    {
                        cmd.Parameters.AddWithValue("@quantity", quantity);
                        cmd.Parameters.AddWithValue("@itemId", id);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected == 0)
                        {
                            // Show a message to the user
                            MessageBox.Show("An error occurred while reducing the quantity of the item. Please try again or contact support.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                // Log or handle the exception as appropriate
                Console.WriteLine("Error: " + ex.ToString());
                // Show a message to the user
                MessageBox.Show("An error occurred while reducing the quantity of the item. Please try again or contact support.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void recordTrans(DataGridViewRow row)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string insertSql = "INSERT INTO transactions (ITEM_ID, CUS_ID, STAFF_ID, TRANS_CODE, TRANS_QUANTITY, TRANS_CREATED_AT, TRANS_UPDATED_AT, TRANS_PRICE, TRANS_PAYMENT) VALUES (@item_id, @cus_id, @staff_id, @trans_code, @trans_quantity, current_timestamp(), current_timestamp(), @trans_price, @trans_payment)";
                    using (MySqlCommand cmd = new MySqlCommand(insertSql, conn))
                    {
                        cmd.Parameters.AddWithValue("@item_id", Convert.ToInt32(row.Cells[0].Value));
                        cmd.Parameters.AddWithValue("@cus_id", customerId);
                        cmd.Parameters.AddWithValue("@staff_id", staffId);
                        cmd.Parameters.AddWithValue("@trans_code", transCode);
                        cmd.Parameters.AddWithValue("@trans_quantity", Convert.ToInt32(row.Cells[3].Value));
                        cmd.Parameters.AddWithValue("@trans_price", Convert.ToDecimal(row.Cells[4].Value));
                        cmd.Parameters.AddWithValue("@trans_payment", payment);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected == 0)
                        {
                            // Show a message to the user
                            MessageBox.Show("An error occurred while recording the transaction. Please try again or contact support.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    conn.Close();
                }
            }
            catch (MySqlException ex)
            {
                // Log or handle the exception as appropriate
                Console.WriteLine("Error: " + ex.ToString());
                // Show a message to the user
                MessageBox.Show("An error occurred while recording the transaction. Please try again or contact support.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private DataTable getCustomer(int id)
        {
            DataTable dt = new DataTable();
            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    string query = "SELECT CUS_fNAME as \"FIRST NAME\", CUS_lNAME as \"LAST NAME\", CUS_EMAIL as \"EMAIL\", CUS_PHONENUM as \"PHONE NUMBER\" FROM CUSTOMER WHERE CUS_ID = @id";
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(query, con))
                    {
                        adapter.SelectCommand.Parameters.AddWithValue("@id", id);
                        adapter.Fill(dt);
                    }
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error: " + ex.ToString());
                MessageBox.Show("An error occurred while retrieving customer data. Please try again or contact support.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return dt;
        }

        private void ChangeCustomer(int id)
        {
            // Create a DataTable to store the customer data
            DataTable dt = new DataTable();

            try
            {
                // Retrieve the customer data using the provided id
                dt = getCustomer(id);
            }
            catch (Exception ex)
            {
                // Show an error message if an exception is thrown
                MessageBox.Show(ex.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // Exit the method to prevent further execution
                return;
            }
            // Update the label to display the customer's name
            lblCustomer.Text = dt.Rows[0][0].ToString() + " " + dt.Rows[0][1].ToString();
            // Update the customerId variable to the provided id
            this.customerId = id;
        }

        private void AddToCart(string id, int quantity)
        {
            validated = true;
            if (quantity < 1)
            {
                MessageBox.Show("Quantity must be at least 1.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DataTable dt = new DataTable();

            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter("SELECT ITEM_ID,ITEM_NAME,ITEM_PRICE,ITEM_QUANTITY FROM item WHERE item_id = @itemId", con))
                    {
                        adapter.SelectCommand.Parameters.AddWithValue("@itemId", id);
                        adapter.Fill(dt);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error has occurred: " + ex.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (dt.Rows.Count < 1)
            {
                MessageBox.Show("Item not found.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            bool found = false;

            //GO THROUGH EACH ROW IN DATAGRIDVIEW
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                //IF THE ID IS ON DATAGRIDVIEW ALREADY
                if (Convert.ToString(row.Cells[0].Value) == dt.Rows[0][0].ToString())
                {
                    //THE DATA IS FOUND
                    found = true;
                    //ADD BOTH QUANTITY FROM DATAGRIDVIEW AND ADDED QUANTITY
                    int newQuantity = quantity + int.Parse(row.Cells[3].Value.ToString());

                    //IF STOCK IS LESS THAN QUANTITY
                    if (int.Parse(dt.Rows[0][3].ToString()) < newQuantity)
                    {
                        //USE THE DATABASE STOCK LEFT AS QUANTITY
                        MessageBox.Show("Stock is lower than quantity, using the max stock instead.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        row.Cells[3].Value = int.Parse(dt.Rows[0][3].ToString());
                    }
                    else
                    {
                        //USE NEW QUANTITY
                        row.Cells[3].Value = newQuantity;
                    }

                }
            }
            //IF NOT FOUND, ADD TO GRIDVIEW
            if (!found)
            {
                dt.Rows[0][3] = quantity;
                dataGridView1.Rows.Add(dt.Rows[0].ItemArray);
            }

            dataGridView1.CurrentCell = dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[3];

            TotalPriceCalculate();
        }

        private void UpdateDisplay()
        {
            try
            {
                if (dataGridView1.Rows.Count < 1)
                {
                    label1.Visible = false;
                    label2.Visible = false;
                    label3.Visible = false;
                    lblTotal.Visible = false;
                    btnEditQuantity.Enabled = false;
                    btnRmvItem.Enabled = false;
                    btnCheckOut.Enabled = false;
                    return;
                }
                else
                {
                    btnEditQuantity.Enabled = true;
                    btnRmvItem.Enabled = true;
                    btnCheckOut.Enabled = true;

                    lblTransCode.Text = transCode.ToString();
                    label1.Text =
                        dataGridView1.SelectedRows[0].Cells[1].Value.ToString() + "     $"
                        + Double.Parse(dataGridView1.SelectedRows[0].Cells[2].Value.ToString()).ToString("0,0.00")
                        + " X " + dataGridView1.SelectedRows[0].Cells[3].Value.ToString();
                    label1.Visible = true;
                    if (dataGridView1.SelectedRows.Count != 0 && dataGridView1.SelectedRows[0].Cells[4].Value != null)
                    {
                        label2.Text = "$" + Double.Parse(dataGridView1.SelectedRows[0].Cells[4].Value.ToString()).ToString("0,0.00");
                    }
                    label2.Visible = true;
                    label3.Visible = true;
                    lblTotal.Visible = true;
                    dataGridView1.CurrentCell = dataGridView1.SelectedRows[0].Cells[3];
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public bool calculating = false;
        private void TotalPriceCalculate()
        {
            calculating = true;
            
            try
            {
                totalPrice = 0;
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    double itemPrice = double.Parse(row.Cells[2].Value.ToString());
                    int itemQuantity = int.Parse(row.Cells[3].Value.ToString());
                    row.Cells[4].Value = itemPrice * itemQuantity;
                    totalPrice += Convert.ToDouble(row.Cells[4].Value);
                }
                lblTotal.Text = "$" + totalPrice.ToString("0,0.00");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                calculating = false;
                return;
            }
            calculating = false;
        }
        private double GetTotalPrice()
        {
            double totalPrice = 0;
            try
            {
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    double price = double.Parse(row.Cells[2].Value.ToString());
                    int quantity = int.Parse(row.Cells[3].Value.ToString());
                    row.Cells[4].Value = price * quantity;

                    totalPrice += Convert.ToDouble(row.Cells[4].Value);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return totalPrice;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            openForm("Login");
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (clearing) return;
            TotalPriceCalculate();
            UpdateDisplay();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (dataGridView1.Rows.Count == 0) return;
            validated = true;
            dataGridView1.Rows.RemoveAt(dataGridView1.CurrentCell.RowIndex);
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if(!calculating && !editing)
                TotalPriceCalculate();
            UpdateDisplay();
        }
        public bool editing = false;
        private void dataGridView1_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if(clearing) return;
            if (validated)
            {
                validated = false;
                return;
            }

            if (e.ColumnIndex != 3)
            {
                return;
            }

            int i;

            if (!int.TryParse(Convert.ToString(e.FormattedValue), out i))
            {
                e.Cancel = true;
                MessageBox.Show("PLEASE ENTER NUMERIC VALUE!", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                int id = int.Parse(dataGridView1.CurrentRow.Cells[0].Value.ToString());
                DataTable data = getItem(id, "ITEM_QUANTITY");

                int stock = int.Parse(data.Rows[0][0].ToString());

                if (stock < Convert.ToInt32(e.FormattedValue))
                {
                    if (checkingOut) return;
                    var result = MessageBox.Show("NOT ENOUGH STOCK! (STOCK LEFT: " + stock + "). USE "+stock + "?", "ERROR", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                    if(result == DialogResult.OK)
                    {
                        editing = true;
                        dataGridView1.EndEdit();
                        editing = false;
                        dataGridView1.CurrentCell.Value = stock;
                    }
                    e.Cancel = true;
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void dataGridView1_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.KeyCode == Keys.F2)
            {

                e.Handled = true;
                if (dataGridView1.CurrentCell != null)
                    dataGridView1.BeginEdit(true);
            }
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            UpdateDisplay();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            openForm("CustomerSelect");
        }

        private void btnCheckOut_Click(object sender, EventArgs e)
        {
            if (!checkingOut)
            {
                openForm("CheckOut");
            }
            else
            {
                checkingOut = true;
                resetAll();
                btnCheckOut.Enabled = false;
                checkingOut = false;
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            openForm("ViewTransActions");
        }




        private void button11_Click(object sender, EventArgs e)
        {

            // Create a barcode reader instance
            var barcodeReader = new BarcodeReader();

            // Open a file dialog to select the image of the barcode
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png, *.gif) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png; *.gif";
            openFileDialog.InitialDirectory = @"C:\";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                // Load the image of the barcode
                var image = (Bitmap)Image.FromFile(openFileDialog.FileName);
                // Decode the barcode from the image
                var result = barcodeReader.Decode(image);
                // Print the decoded text
                if (result != null)

                    AddToCart(result.Text, 1);

                else
                    MessageBox.Show("No Barcode Found", "Error");
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            resetAll();
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dataGridView1_DoubleClick(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {

        }
    }

}

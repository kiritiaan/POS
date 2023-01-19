using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace POS_AAR9TECH
{
    public partial class Login : Form
    {
        private Label usernameLabel;
        private TextBox usernameTextBox;
        private Label passwordLabel;
        private TextBox passwordTextBox;
        private Button loginButton;


        static string server1 = "";
        static string uid1 = "";
        static string pwd1 = "";
        static string database1 = "";


        public int staffId { get; set; }
        public string server { get; set; }
        public string uid { get; set; }
        public string pwd { get; set; }
        public string database { get; set; }

        string connectionString = $"server={server1};uid={uid1};pwd={pwd1};database={database1}";
        public Login()
        {

            InitializeComponent();

        }

        private string ReadDataFromTextFile()
        {
            // Get the path of the %appdata% folder
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            // Create the path for the text file
            string textFilePath = Path.Combine(appDataPath, "MyAppData.txt");

            // Check if the file exists
            if (File.Exists(textFilePath))
            {
                // Read the data from the text file
                string data = File.ReadAllText(textFilePath);
                return data;
            }
            else
            {
                return "File not found";
            }
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text;
            string password = txtPassword.Text;

            if (username == "" || password == "")
            {
                lblError.Text = "Enter value to all input box.";
                return;
            }

            lblError.Text = "Logging In...";

            // Create a connection to the MySQL database
            MySqlConnection connection = new MySqlConnection(connectionString);

            // Create a command to query the STAFF table for the entered username and password, and retrieve the STAFF_ID
            string query = "SELECT STAFF_ID FROM STAFF WHERE STAFF_USER = @username AND BINARY STAFF_PASS = @password";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@username", username);
            command.Parameters.AddWithValue("@password", password);

            try
            {
                // Open the connection and execute the command
                connection.Open();
                var staff = command.ExecuteScalar();
                if (staff != null)
                {
                    this.staffId = (int)staff;

                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    // Invalid username or password
                    lblError.Text = "Invalid username or password.";
                }
            }
            catch (MySqlException ex)
            {
                // An error occurred while connecting to the database
                lblError.Text = ex.Message;
            }
            finally
            {
                connection.Close();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (panel1.Visible)
            {
                panel1.Visible = false;
                panel2.Visible = true;
                panel1.Enabled = false;
                panel2.Enabled = true;
                panel2.BringToFront();
            }
            else
            {
                panel1.Visible = true;
                panel2.Visible = false;
                panel1.Enabled = true;
                panel2.Enabled = false;
                panel1.BringToFront();
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {

            server1 = txtServer.Text;
            uid1 = txtDBUser.Text;
            pwd1 = txtDBPass.Text;
            database1 = txtDBName.Text;

            server = txtServer.Text;
            uid = txtDBUser.Text;
            pwd = txtDBPass.Text;
            database = txtDBName.Text;

            connectionString = $"server={server1};uid={uid1};pwd={pwd1};database={database1}";

            // Get the path of the %appdata% folder
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            // Create the path for the text file
            string textFilePath = Path.Combine(appDataPath, "MyAppData.txt");

            // Write the values to the text file
            File.WriteAllText(textFilePath, server1 + Environment.NewLine + uid1 + Environment.NewLine + pwd1 + Environment.NewLine + database1);

            MessageBox.Show("Data Successfully Saved!","SUCCESS", MessageBoxButtons.OK, MessageBoxIcon.Information);

        }

        private void btnTestConnection_Click(object sender, EventArgs e)
        {

            // Create a connection string using the variables
            string connectionString = "server=" + txtServer.Text + ";user id=" + txtDBUser.Text + ";password=" + txtDBPass.Text + ";database=" + txtDBName.Text + ";";

            // Create a new MySqlConnection object
            MySqlConnection connection = new MySqlConnection(connectionString);

            try
            {
                // Open the connection
                connection.Open();

                // If the connection is successful, display a message
                MessageBox.Show("Connection successful!","SUCCESS!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (MySqlException ex)
            {
                // If the connection is not successful, display an error message
                MessageBox.Show("Error: " + ex.Message,"CONNECTION FAILED", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Close the connection
                connection.Close();
            }
        }

        private void Login_Load(object sender, EventArgs e)
        {
            string[] data = ReadDataFromTextFile().Replace("\r", string.Empty).Split("\n".ToCharArray());
            server1 = data[0];
            uid1 = data[1];
            pwd1 = data[2];
            database1 = data[3];

            server = data[0];
            uid = data[1];
            pwd = data[2];
            database = data[3];


            txtServer.Text = server1;
            txtDBUser.Text = uid1;
            txtDBPass.Text = pwd1;
            txtDBName.Text = database1;

            connectionString = $"server={server1};uid={uid1};pwd={pwd1};database={database1}";
        }

        private void txtUsername_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
            {
                btnSubmit.PerformClick();
            }
        }
    }
}

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
    public partial class AddItem : Form
    {
        public DataTable data;
        public string IDSelected { get; set; }
        public int quantity { get; set; }
        public AddItem()
        {
            InitializeComponent();
        }
        public AddItem(DataTable data)
        {
            this.data = data;
            InitializeComponent();
            data.Columns.Remove("ITEM_DESC");
            data.Columns.Remove("CATEGORY_ID");
            data.Columns.Remove("ITEM_CREATED_AT");
            data.Columns.Remove("ITEM_UPDATED_AT");
            data.Columns.Remove("ITEM_CODE");
            data.Columns.Remove("ITEM_IMAGE");
            data.Columns.Remove("ITEM_STATUS");
            data.Columns[0].ColumnName = "ITEM ID";
            data.Columns[1].ColumnName = "ITEM NAME";

            data.Columns[2].ColumnName = "PRICE";
            data.Columns[3].ColumnName = "STOCK";
            dataGridView1.TabStop = false;
            dataGridView1.DataSource = data;
            dataGridView1.RowsDefaultCellStyle.ForeColor = Color.Black;
            dataGridView1.AlternatingRowsDefaultCellStyle.BackColor = Color.DarkGray;
        }

        private void AddItem_Load(object sender, EventArgs e)
        {

        }

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

        private void AddItem_Deactivate(object sender, EventArgs e)
        {
            this.Close();
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                itemSelected.Text = dataGridView1.SelectedRows[0].Cells[0].Value.ToString();
                itemSelectedName.Text = dataGridView1.SelectedRows[0].Cells[1].Value.ToString();
                quantityTxt.Text = "1";
                quantityTxt.Maximum = int.Parse(dataGridView1.SelectedRows[0].Cells[3].Value.ToString());
                quantityTxt.Focus();
            }
            catch(Exception ex)
            {

            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var found = false;
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells[0].Value.ToString().Equals(itemSelected.Text)) 
                    found = true;
            }

            if (!found)
            {
                MessageBox.Show("ITEM ID NOT FOUND!", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            else
            {
                this.IDSelected = itemSelected.Text;
                this.quantity = Int32.Parse(quantityTxt.Value.ToString()); //example
                this.DialogResult = DialogResult.OK;
                this.Close();
            }

        }

        private void quantityTxt_Enter(object sender, EventArgs e)
        {
            quantityTxt.Select(0, quantityTxt.Text.Length);
        }

        private void itemSelected_Leave(object sender, EventArgs e)
        {
            var found = false;
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells[0].Value.ToString().Equals(itemSelected.Text))
                {
                    found = true;
                    dataGridView1.CurrentCell = row.Cells[0];
                }
            }

            if (!found)
            {
                MessageBox.Show("ITEM ID NOT FOUND!", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }


        }
    }
}

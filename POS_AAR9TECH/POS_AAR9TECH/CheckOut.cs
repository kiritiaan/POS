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
    public partial class CheckOut : Form
    {
        public double payment { get; set; }
        double min = 0;
        public CheckOut()
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
            if (Form.ModifierKeys == Keys.None && keyData == Keys.Enter)
            {
                button1.PerformClick();
                return true;
            }
            return base.ProcessDialogKey(keyData);
        }


        public CheckOut(double total)
        {
            InitializeComponent();
            min = total; 
            this.ActiveControl = inputPayment;
            inputPayment.Focus();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (inputPayment.Value < Convert.ToDecimal(min))
            {
                MessageBox.Show("NOT ENOUGH PAYMENT!","ERROR", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            payment = Convert.ToDouble(inputPayment.Value);
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}

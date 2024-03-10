using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProPlan2250QC
{
    public partial class pass : Form
    {
        public pass()
        {
            InitializeComponent();
        }

        private void pass_Load(object sender, EventArgs e)
        {
            textBox1.Select();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == "1453")
            {
                var dbViewer = new DatabaseViewer();
                dbViewer.ShowDialog();
                this.Close();
            }
            else
            {
                MessageBox.Show("Şifre Yanlış");
                textBox1.Clear();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == Convert.ToChar(Keys.Enter))
            {
                button1_Click(sender, e);
            }
            if (e.KeyChar == Convert.ToChar(Keys.Escape))
            {
                this.Close();
            }
        }
    }
}

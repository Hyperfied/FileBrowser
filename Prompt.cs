using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileBrowser
{
    public partial class Prompt : Form
    {
        private string input = "";
        private string caption;

        public Prompt(string caption)
        {
            InitializeComponent();
            this.caption = caption;
        }

        private void Prompt_Load(object sender, EventArgs e)
        {
            Text = caption;
        }

        private void OKButton_Click(object sender, EventArgs e)
        {
            input = textBox1.Text;
            Close();
        }

        private void CancelPrompt_Click(object sender, EventArgs e)
        {
            input = "";
            Close();
        }

        public string Response
        { get { return input; } }
    }
}

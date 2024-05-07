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
        private string hint;

        public Prompt(string caption)
        {
            InitializeComponent();
            this.caption = caption;
            hint = "";
        }

        public Prompt(string caption, string hint)
        {
            InitializeComponent();
            this.caption = caption;
            this.hint = hint;
        }

        private void Prompt_Load(object sender, EventArgs e)
        {
            Text = caption;
            textBox1.Text = hint;
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

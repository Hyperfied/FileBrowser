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
    public partial class PropertiesForm : Form
    {
        public DirectoryInfo? directory;
        private long totalSize;
        private int numOfDirectories;
        private int numOfFiles;
        private int numOfUnauths;
        private CancellationTokenSource cancellationTokenSource;

        public PropertiesForm()
        {
            cancellationTokenSource = new();
            InitializeComponent();
        }

        private async void PropertiesForm_Load(object sender, EventArgs e)
        {
            await StartSizeSearch();
            label1.Text = $"Total Size: {FileBrowser.BytesToString(totalSize)}";
        }

        private async Task StartSizeSearch()
        {
            totalSize = 0;

            if (directory == null) { return; }
            await Task.Run(() =>
            {
                CalculateDirectorySize(directory);
            });
        }

        private void CalculateDirectorySize(DirectoryInfo directory)
        {
            DirectoryInfo[] directories = [];
            FileInfo[] files = [];

            try { files = directory.GetFiles(); } catch (UnauthorizedAccessException) { }
            try { directories = directory.GetDirectories(); } catch (UnauthorizedAccessException) { numOfUnauths++; }
            foreach (DirectoryInfo nestedDir in directories)
            {
                numOfDirectories++;
                CalculateDirectorySize(nestedDir);
            }

            foreach (FileInfo file in files)
            {
                if (cancellationTokenSource.IsCancellationRequested) return;

                totalSize += file.Length;
                numOfFiles++;

                try
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        label1.Text = $"Size: {FileBrowser.BytesToString(totalSize)}";
                        label2.Text = $"Folders: {numOfDirectories}, Files: {numOfFiles}";
                        if (numOfUnauths > 0)
                        {
                            label3.Text = $"Unauthorized Folders: {numOfUnauths}";
                        }
                    });
                } catch (ObjectDisposedException) { return; }
                
            }
        }

        private void PropertiesForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            cancellationTokenSource.Cancel();
        }
    }
}

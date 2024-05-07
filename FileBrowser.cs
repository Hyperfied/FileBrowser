using System.Diagnostics;
using System.Numerics;
using System.Reflection.Metadata;

namespace FileBrowser
{
    public partial class FileBrowser : Form
    {
        private TreeNode? rightClickedNode;
        private ListViewColumnSorter columnSorter;

        public FileBrowser()
        {
            InitializeComponent();
            columnSorter = new();
            listView1.ListViewItemSorter = columnSorter;
        }

        private void FileBrowser_Load(object sender, EventArgs e)
        {
            UpdateDrives();
            ShowFirstDirs();
            ExpandTo(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            UpdateFileNames();
        }
        private void UpdateDrives()
        {
            treeView1.BeginUpdate();
            string[] drives = Directory.GetLogicalDrives();
            foreach (string drive in drives)
            {
                TreeNode node = new TreeNode(drive);
                treeView1.Nodes.Add(node);
            }
            treeView1.EndUpdate();
        }

        private void UpdateFileNames()
        {
            DirectoryInfo di = new(treeView1.SelectedNode.FullPath);
            FileInfo[] files;
            try
            {
                files = di.GetFiles();
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Program does not have the authorization to read this Directory.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            catch (DirectoryNotFoundException)
            {
                MessageBox.Show("Directory does not exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            listView1.Items.Clear();

            listView1.BeginUpdate();
            foreach (FileInfo file in files)
            {
                ListViewItem item = new(file.Name); // File name
                item.SubItems.Add(BytesToString(file.Length)); // File Size 
                item.SubItems.Add(file.LastWriteTime.ToString()); // Date modified
                listView1.Items.Add(item);
            }
            listView1.EndUpdate();
        }

        private void treeView1_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            if (e.Node == null)
            {
                return;
            }
            treeView1.SelectedNode = e.Node;
            ShowNestedDirs(e.Node);
        }

        private void ShowNestedDirs(TreeNode node)
        {
            var nodes = node.Nodes;

            foreach (TreeNode n in nodes)
            {
                string path = n.FullPath;
                DirectoryInfo dInfo = new(path);
                DirectoryInfo[] dirs;
                try { dirs = dInfo.GetDirectories(); } catch { continue; }

                foreach (DirectoryInfo dir in dirs)
                {
                    TreeNode treeNode = new(dir.Name);
                    n.Nodes.Add(treeNode);
                }
            }
        }

        private void ShowFirstDirs()
        {
            var firstNodes = treeView1.Nodes;

            treeView1.BeginUpdate();
            foreach (TreeNode node in firstNodes)
            {
                string path = node.FullPath;
                DirectoryInfo dInfo = new(path);
                DirectoryInfo[] dirs = dInfo.GetDirectories();

                foreach (DirectoryInfo dir in dirs)
                {
                    TreeNode treeNode = new(dir.Name);
                    node.Nodes.Add(treeNode);
                }
            }
            treeView1.EndUpdate();
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                ExpandTo(textBox1.Text);
            }
        }

        private void ExpandTo(string path)
        {
            List<string> split = [.. path.Split('\\')];
            split.RemoveAll(s => s.Length == 0);


            TreeNode? baseNode = null;
            foreach (TreeNode node in treeView1.Nodes)
            {
                if (node.Text == $"{split[0]}\\")
                {
                    baseNode = node;
                }
            }

            if (baseNode == null)
            {
                MessageBox.Show("Cannot find path.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            baseNode.Expand();
            for (int i = 1; i < split.Count; i++)
            {
                TreeNode? nextNode = null;
                foreach (TreeNode node in baseNode.Nodes)
                {
                    if (node.Text == split[i])
                    {
                        nextNode = node;
                    }
                }

                if (nextNode == null)
                {
                    MessageBox.Show("Cannot find path.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                nextNode.Expand();
                baseNode = nextNode;
            }
            treeView1.SelectedNode = baseNode;

        }

        public static string BytesToString(long bytes)
        {
            int indices = (int)MathF.Floor(MathF.Log(bytes, 1024));
            string extension = string.Empty;
            switch (indices)
            {
                case 0:
                    extension = "B";
                    break;
                case 1:
                    extension = "KB";
                    break;
                case 2:
                    extension = "MB";
                    break;
                case 3:
                    extension = "GB";
                    break;
                case 4:
                    extension = "TB";
                    break;
                case 5:
                    extension = "PB";
                    break;
            }
            float shortBytes = MathF.Round(bytes / MathF.Pow(1024, indices), 1);
            return $"{shortBytes} {extension}";

        }

        public static long StringToBytes(string bytes)
        {
            string[] splitString = bytes.Split(' ');
            float num = float.Parse(splitString[0]);
            int indices = 0;

            switch (splitString[1])
            {
                case "KB":
                    indices = 1;
                    break;
                case "MB":
                    indices = 2;
                    break;
                case "GB":
                    indices = 3;
                    break;
                case "TB":
                    indices = 4;
                    break;
                case "PB":
                    indices = 5;
                    break;
            }

            float actualBytes = (MathF.Pow(1024, indices) * num);
            return (long)actualBytes;

        }

        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                rightClickedNode = e.Node;
                contextMenuStrip1.Show(this, new Point(e.X, e.Y));
            }
        }

        private void propertiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (rightClickedNode == null) return;
            DirectoryInfo dInfo = new DirectoryInfo(rightClickedNode.FullPath);

            PropertiesForm propertiesForm = new PropertiesForm();
            propertiesForm.directory = dInfo;
            propertiesForm.ShowDialog();

        }

        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (e.Column == columnSorter.SortColumn)
            {
                if (columnSorter.Order == SortOrder.Ascending)
                {
                    columnSorter.Order = SortOrder.Descending;
                }
                else

                {
                    columnSorter.Order = SortOrder.Ascending;
                }
            }
            else
            {
                columnSorter.SortColumn = e.Column;
                columnSorter.Order = SortOrder.Ascending;
            }

            listView1.Sort();
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (rightClickedNode == null) return;
            Prompt prompt = new("Enter folder name");
            prompt.ShowDialog();
            string response = prompt.Response;

            if (response != "")
            {
                try
                {
                    if (Directory.Exists($"{rightClickedNode.FullPath}\\{response}"))
                    {
                        MessageBox.Show("Folder already exists.", "Duplicate Folder", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        return;
                    }
                    Directory.CreateDirectory($"{rightClickedNode.FullPath}\\{response}");
                    string[] split = response.Split('\\');
                    TreeNode root = rightClickedNode;
                    TreeNode tn = new();
                    foreach (string s in split)
                    {
                        tn = new TreeNode(s);
                        root.Nodes.Add(tn);
                        root = tn;
                    }
                    ExpandTo(tn.FullPath);
                }
                catch (IOException)
                {
                    MessageBox.Show("Invalid character in folder name.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }
        }

        private void renameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (rightClickedNode == null) return;
            Prompt prompt = new("Enter new folder name", rightClickedNode.Text);
            prompt.ShowDialog();
            string response = prompt.Response;

            if (response != "")
            {
                string newPath = $"{rightClickedNode.Parent.FullPath}\\{response}";
                try
                {
                    Directory.Move(rightClickedNode.FullPath, newPath);
                    rightClickedNode.Text = response;
                    ExpandTo(rightClickedNode.FullPath);
                }
                catch (IOException)
                {
                    MessageBox.Show("Invalid character.");
                }
            }
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (rightClickedNode == null) return;
            DialogResult result = MessageBox.Show($"Are you sure you want to delete {rightClickedNode.Name}", "Delete Folder", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                Directory.Delete(rightClickedNode.FullPath);
                TreeNode prevNode = rightClickedNode.Parent;
                rightClickedNode.Remove();
                ExpandTo(prevNode.FullPath); 
            }
        }
    }
}

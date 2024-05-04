using System.Diagnostics;
using System.Reflection.Metadata;

namespace FileBrowser
{
    public partial class FileBrowser : Form
    {

        public FileBrowser()
        {
            InitializeComponent();
        }

        private void FileBrowser_Load(object sender, EventArgs e)
        {
            UpdateDrives();
            ShowFirstDirs();
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
            string[] split = path.Split('\\');

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
            for (int i = 1; i < split.Length; i++)
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
                Debug.WriteLine(baseNode.FullPath);
                baseNode = nextNode;
            }
            treeView1.SelectedNode = baseNode;

        }

        private static string BytesToString(long bytes)
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
    }
}

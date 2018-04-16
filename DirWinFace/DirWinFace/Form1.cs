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

namespace DirWinFace
{
    public partial class Form1 : Form
    {        
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    Parse(fbd.SelectedPath);
                }
            }
        }

        void Parse(string path)
        {
            string[] allfiles = Directory.GetDirectories(path);
            string res = "";
            foreach (var item in allfiles)
            {
                FileAttributes attr = File.GetAttributes(item);

                if (attr.HasFlag(FileAttributes.Directory))
                {
                    res += (item + Environment.NewLine);
                }

            }
            richTextBox1.Text = res;
        }
    }
}

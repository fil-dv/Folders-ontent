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
        string _text = "";

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
            try
            {
                //string[] allfiles = Directory.GetDirectories(path, "*", SearchOption.AllDirectories);
                string[] allfiles = Directory.GetDirectories(path);
                foreach (var item in allfiles)
                {
                    FileAttributes attr = File.GetAttributes(item);

                    if (attr.HasFlag(FileAttributes.Directory))
                    {
                        _text += (item + Environment.NewLine);
                    }
                }
                richTextBox1.Text = _text;
            }
            catch (Exception)
            {
                MessageBox.Show("Похоже нет доступа к некоторым папкам.");
            }           
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SaveFileDialog savefile = new SaveFileDialog();
            savefile.FileName = "Список.txt";
            savefile.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";

            if (savefile.ShowDialog() == DialogResult.OK)
            {
                using (StreamWriter sw = new StreamWriter(savefile.FileName))
                    sw.WriteLine(_text);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            _text = "";
            richTextBox1.Text = "";
        }
    }
}

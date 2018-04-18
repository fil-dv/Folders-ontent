using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace DirWinFace
{
    public partial class Form1 : Form
    {
        string _text = "";
        string _path = "";
        int _selector = 0;
        List<string> _allfiles = new List<string>();
        public event Action<int> WorkInProcess;

        public Form1()
        {
            InitializeComponent();
            InitialHandler();
            InitCombo();
        }

        void InitialHandler()
        {
            WorkInProcess += Signal;
        }

        void Signal(int count)
        {
            this.Text = "Обработано записей: " + count;
        }

        private void InitCombo()
        {
            comboBox1.Items.Add("Вложенные каталоги");
            comboBox1.Items.Add("Вложенные файлы");
            comboBox1.Items.Add("Вложенные каталоги со всеми подкаталогами");
            comboBox1.Items.Add("Вложенные файлы во всех подкаталогах");
            comboBox1.SelectedIndex = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    _path = fbd.SelectedPath;
                    Thread th = new Thread(new ThreadStart(StartParse));
                    th.Start();
                }
            }
        }

        void StartParse()
        {
            Parse(_path);
        }
         
        void ProcessFile(string path)
        {
            _allfiles.Add(path);
        }

        static void ApplyAllFiles(string folder, Action<string> fileAction)
        {
            foreach (string file in Directory.GetFiles(folder))
            {
                fileAction(file);
            }
            foreach (string subDir in Directory.GetDirectories(folder))
            {
                try
                {
                    ApplyAllFiles(subDir, fileAction);
                }
                catch
                {
                    // игнорируем файлы, к которым нет доступа...
                }
            }
        }

        void Parse(string path)
        {
            try
            {
                _text = "";
                
                switch (_selector)
                {
                    case 0: // Вложенные каталоги
                        _allfiles = Directory.GetDirectories(path).ToList();
                        break;
                    case 1: // Вложенные каталоги и файлы
                        _allfiles = Directory.GetFiles(path).ToList();
                        break;
                    case 2: // Вложенные каталоги со всеми подкаталогами
                        _allfiles = Directory.GetDirectories(path, "*", SearchOption.AllDirectories).ToList();
                        break;
                    case 3: // Вложенные файлы во всех подкаталогах
                        ApplyAllFiles(path, ProcessFile);
                        break;
                    default:
                        _allfiles = Directory.GetDirectories(path).ToList();
                        break;
                }

                int count = 0;

                foreach (var item in _allfiles)
                {                    
                    if((++count % 100) == 1)
                    {
                        if (WorkInProcess != null)
                        {
                            WorkInProcess(count);
                        }
                    }

                    try
                    {
                        _text += (item + Environment.NewLine);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        continue;
                    }
                }
                
                if (InvokeRequired)
                {
                    Action action = () =>
                    {
                        UpdateText();
                    };
                    Invoke(action);
                }
                else
                {
                    UpdateText();
                } 
            }
            catch (Exception ex)
            {
                MessageBox.Show("Похоже, что-то не сработало..." + Environment.NewLine + ex.Message );
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
                {
                    sw.WriteLine(richTextBox1.Text);
                }
                
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            _text = "";
            richTextBox1.Text = "";
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            _selector = comboBox1.SelectedIndex;                       
        }

        private void UpdateText()
        {
            richTextBox1.Text += _text;
            this.Text = "Содержимое папки";
        }
    }
}

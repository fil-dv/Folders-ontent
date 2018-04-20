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
        public Form1()
        {
            InitializeComponent();
            InitialHandler();
            InitCombo();
        }

        string _text = "";
        string _path = "";
        int _selector = 0;
        int _count = 0;
        List<string> _allfiles = new List<string>();
        public event Action<int> WorkInProcess;
        public event Action<int> PreparInProcess;

        void InitialHandler()
        {
            WorkInProcess += SignalWork;
            PreparInProcess += SignanPrepar;
        }

        void SignalWork(int count)
        {
            if (InvokeRequired)
                {
                    Action action = () =>
                    {
                        TmpText(count, "Обработка записей: ");
                    };
                    Invoke(action);
                }
                else
                {
                    TmpText(count, "Обработка записей: ");
                } 
        }

        void SignanPrepar(int count)
        {
            if (InvokeRequired)
                {
                    Action action = () =>
                    {
                        TmpText(count, "Подготавливается список: ");
                    };
                    Invoke(action);
                }
                else
                {
                    TmpText(count, "Подготавливается список: ");
                }
        }

        void TmpText(int count, string text)
        {

            this.Text = text + count;
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
                    _count = 0;
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
            if((++_count % 100) == 0)                       
            {
                if(PreparInProcess != null)
                {
                    PreparInProcess(_count);
                }
            }
        }

        void ApplyAllFiles(string folder, Action<string> fileAction)
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

        void ApplyAllDirs(string folder, Action<string> fileAction)
        {
            foreach (string subDir in Directory.GetDirectories(folder))
            {
                fileAction(subDir);
                try
                {                    
                    ApplyAllDirs(subDir, fileAction);
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
                        ApplyAllDirs(path, ProcessFile);
                        _count = 0;
                        //_allfiles = Directory.GetDirectories(path, "*", SearchOption.AllDirectories).ToList();
                        break;
                    case 3: // Вложенные файлы во всех подкаталогах
                        ApplyAllFiles(path, ProcessFile);
                        _count = 0;
                        break;
                    default:
                        _allfiles = Directory.GetDirectories(path).ToList();
                        break;
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
            this.Text = "Содержимое папки";
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            _selector = comboBox1.SelectedIndex;                       
        }

        private void UpdateText()
        {
            _count = 0;
            foreach (var item in _allfiles)
            {                
                if ((++_count % 100) == 0)
                {
                    if (WorkInProcess != null)
                    {
                        WorkInProcess(_count);
                    }
                }
                _text += (item + Environment.NewLine);                
            }
            richTextBox1.Text += _text;
            this.Text = _path + "   " + Rooles() ;
        }

        private string Rooles()
        {
            string res = "";
            switch (_count)
            {
                case 1:
                    res = "Найдена 1 запись.";
                    break;
                case 2:                    
                case 3:                    
                case 4:
                    res = "Найдено " + _count + " записи.";
                    break;
                default:
                    res = "Найденo " + _count + " записей.";
                    break;
            }
            return res;
        }
    }
}

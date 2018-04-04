using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileEncryptor
{
    public partial class MainForm : Form
    {
        private string savePath;
        private string fileName;
        private string keyFilePath;
        private byte encryptionFlag;

        public MainForm()
        {
            InitializeComponent();
            //buttonDecrypt.Enabled = false;
            savePath = "";//добавить по умолчанию
            fileName = "";
            keyFilePath = "";
            encryptionFlag = 0;
        }
        
        public string SavePath
        {
            get
            {
                return savePath;
            }
            set
            {
                savePath = value;
            }
        }

        public string FileName
        {
            get
            {
                return fileName;
            }
            set
            {
                fileName = value;
            }
        }

        public string KeyFilePath
        {
            get
            {
                return keyFilePath;
            }
            set
            {
                keyFilePath = value;
            }
        }

        public byte EncryptionFlag
        {
            get
            {
                return encryptionFlag;
            }
            set
            {
                encryptionFlag = value;
            }
        }


        private void Button_SelectFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                FileInfo info = new FileInfo(fileDialog.FileName);
                textBoxFileName.Text = info.Name.ToString();
                textBoxFilePath.Text = info.FullName.ToString();
                FileName = Path.GetFileNameWithoutExtension(info.FullName);
                if (info.Extension.Equals(".encryp"))
                {
                    buttonDecrypt.Enabled = true;
                }
                textBoxLog.Text += "Selected file: " + info.Name.ToString() + "\r\n";
            }
        }

        private void ButtonEncrypt_Click(object sender, EventArgs e)
        {
            EncryptionForm encryptionForm = new EncryptionForm(this);
            encryptionForm.ShowDialog();
            if(encryptionForm.DialogResult == DialogResult.OK)
            {
                encryptionForm.Dispose();
                //passwordForm.Dispose();                 
                //вызов функций шифрования

                //FileStream resFile = File.Create(savePath + "\\" + FileName + ".encryp");
                //FileStream keyFile = File.Create(savePath + "\\" + FileName + ".key");
            }
            if (encryptionForm.DialogResult == DialogResult.Cancel)
            {
                encryptionForm.Dispose();
            }
        }

        private void ButtonDecrypt_Click(object sender, EventArgs e)
        {
            DecryptionForm passwordForm = new DecryptionForm(this);
            passwordForm.ShowDialog();
            if (passwordForm.DialogResult == DialogResult.OK)
            {
                passwordForm.Dispose();

                //вызовы функций дешифровки
            }
            if (passwordForm.DialogResult == DialogResult.Cancel)
            {
                passwordForm.Dispose();
            }
        }

        private void SaveDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog browserDialog = new FolderBrowserDialog();
            //browserDialog.RootFolder = Environment.SpecialFolder.MyDocuments;
            if (browserDialog.ShowDialog() == DialogResult.OK)
            {
                savePath = browserDialog.SelectedPath.ToString();
                textBoxLog.Text += "Save folder is selected: " + browserDialog.SelectedPath.ToString() + "\r\n";
            }
        }
    }
}

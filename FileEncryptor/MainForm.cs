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
using IDEAEncryprion;

namespace FileEncryptor
{
    public partial class MainForm : Form
    {
        private string savePath;
        private string srcFilePath;
        private string fileName;
        private string keyFilePath;
        private byte encryptionFlag;

        public MainForm()
        {
            InitializeComponent();
            buttonDecrypt.Enabled = false;
            buttonEncrypt.Enabled = false;
            textBoxFileName.ReadOnly = true;
            textBoxFilePath.ReadOnly = true;
            textBoxLog.ReadOnly = true;
            savePath = Environment.SpecialFolder.MyDocuments.ToString();
            fileName = "";
            keyFilePath = "";
            encryptionFlag = 0;
        }

        public string FileExtension { get; set; }
        
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

        public string SrcFilePath
        {
            get
            {
                return srcFilePath;
            }
            set
            {
                srcFilePath = value;
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
                SrcFilePath = info.FullName;
                FileName = Path.GetFileNameWithoutExtension(info.FullName);
                FileExtension = Path.GetExtension(info.FullName);
                buttonEncrypt.Enabled = true;
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
                switch (EncryptionFlag)
                {
                    case 1:
                        {
                            //вызовы функций шифрования
                            IDEAEncryption encryption = new IDEAEncryption();

                            //создание файловых потоков
                            FileStream srcFile = File.Open(SrcFilePath, FileMode.Open, FileAccess.Read);
                            FileStream resFile = File.Create(SavePath + "\\" + FileName + ".encryp");
                            FileStream keyFile = File.Create(SavePath + "\\" + FileName + ".key");

                            //шифрование
                            encryption.Encrypt(srcFile, resFile, keyFile, FileExtension);

                            //закрытие файлов
                            srcFile.Close();
                            resFile.Close();
                            keyFile.Close();
                            break;
                        }
                    case 2:
                        {
                            //вызовы функций шифрования

                            //создание файловых потоков
                            FileStream srcFile = File.Open(SrcFilePath, FileMode.Open, FileAccess.Read);
                            FileStream resFile = File.Create(SavePath + "\\" + FileName + ".encryp");
                            FileStream keyFile = File.Create(SavePath + "\\" + FileName + ".key");

                            //шифрование

                            //закрытие файлов
                            srcFile.Close();
                            resFile.Close();
                            keyFile.Close();
                            break;
                        }
                }

                //очищение textbox и полей данных
                textBoxFileName.Clear();
                textBoxFilePath.Clear();
                FileName = "";
                EncryptionFlag = 0;

                //вывод информации
                string info = "File was encrypted and located on: \r\n" + SavePath;
                textBoxLog.Text += info + "\r\n";
                InfoForm infoForm = new InfoForm(info);
                infoForm.ShowDialog();
                if (infoForm.DialogResult == DialogResult.OK)
                {
                    infoForm.Dispose();
                }
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


                //очищение textbox и полей данных
                textBoxFileName.Clear();
                textBoxFilePath.Clear();
                FileName = "";
                KeyFilePath = "";
                EncryptionFlag = 0;

                //вывод информации
                string info = "File was decrypted and located on: \r\n" + SavePath;
                InfoForm infoForm = new InfoForm(info);
                infoForm.ShowDialog();
                if (infoForm.DialogResult == DialogResult.OK)
                {
                    infoForm.Dispose();
                }
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

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
        public MainForm()
        {
            InitializeComponent();
            buttonDecrypt.Enabled = false;
            buttonEncrypt.Enabled = false;
            textBoxFileName.ReadOnly = true;
            textBoxFilePath.ReadOnly = true;
            textBoxLog.ReadOnly = true;
            SavePath = Environment.SpecialFolder.MyDocuments.ToString();
            FileName = "";
            KeyFilePath = "";
            EncryptionFlag = 0;
        }

        public string FileExtension { get; set; }

        public string SavePath { get; set; }

        public string SrcFilePath { get; set; }

        public string FileName { get; set; }

        public string KeyFilePath { get; set; }

        public byte EncryptionFlag { get; set; }


        private void Button_SelectFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                FileInfo info = new FileInfo(fileDialog.FileName);
                textBoxFileName.Text = info.Name;
                textBoxFilePath.Text = info.FullName;
                SrcFilePath = info.FullName;
                FileName = Path.GetFileNameWithoutExtension(info.FullName);
                FileExtension = Path.GetExtension(info.FullName);
                if (info.Extension.Equals(".encryp"))
                {
                    buttonDecrypt.Enabled = true;
                    buttonEncrypt.Enabled = false;
                }
                else
                {
                    buttonEncrypt.Enabled = true;
                    buttonDecrypt.Enabled = false;
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
                            //создание файловых потоков
                            using (FileStream srcFileStream = File.Open(SrcFilePath, FileMode.Open, FileAccess.Read),
                                              resFileStream = File.Create(SavePath + "\\" + FileName + ".encryp"),
                                              keyFileStream = File.Create(SavePath + "\\" + FileName + ".key"))
                            {
                                //вызовы функций шифрования
                                IDEAEncryption encryption = new IDEAEncryption();
                                encryption.Encrypt(srcFileStream, resFileStream, keyFileStream, FileExtension);
                            }
                            break;
                        }
                    case 2:
                        {
                            //создание файловых потоков
                            using (FileStream srcFileStream = File.Open(SrcFilePath, FileMode.Open, FileAccess.Read),
                                              resFileStream = File.Create(SavePath + "\\" + FileName + ".encryp"),
                                              keyFileStream = File.Create(SavePath + "\\" + FileName + ".key"))
                            {
                                //вызовы функций шифрования

                            }
                            break;
                        }
                }

                //очищение textbox и полей данных
                textBoxFileName.Clear();
                textBoxFilePath.Clear();
                FileExtension = "";
                SavePath = "";
                SrcFilePath = "";
                FileName = "";
                KeyFilePath = "";
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
            DecryptionForm decryptionForm = new DecryptionForm(this);
            decryptionForm.ShowDialog();
            if (decryptionForm.DialogResult == DialogResult.OK)
            {
                decryptionForm.Dispose();
                using (FileStream keyFileStream = new FileStream(KeyFilePath, FileMode.Open, FileAccess.Read),
                                  encryptedFileStream = new FileStream(SrcFilePath, FileMode.Open, FileAccess.Read))
                {
                    EncryptionFlag = (byte)encryptedFileStream.ReadByte();

                    byte[] hashKeyFile = new byte[16];
                    byte[] hashEncryptedFile = new byte[16];
                    keyFileStream.Seek(0, SeekOrigin.Begin);
                    encryptedFileStream.Seek(1, SeekOrigin.Begin);
                    keyFileStream.Read(hashKeyFile, 0, 16);
                    encryptedFileStream.Read(hashEncryptedFile, 0, 16);
                    bool b = true;
                    for(int i = 0; i < 16; i++)
                    {
                        if (hashEncryptedFile[i] != hashKeyFile[i])
                            b = false;
                    }
                    if (!b)
                    {
                        InfoForm form = new InfoForm("The key file does not math the encrypted file");
                        form.ShowDialog();
                        if (form.DialogResult == DialogResult.OK)
                        {
                            form.Dispose();
                            return;
                        }
                    }
                    byte[] data = new byte[100];
                    keyFileStream.Seek(120, SeekOrigin.Begin);
                    keyFileStream.Read(data, 0, (int)(keyFileStream.Length - 120));
                    FileExtension = BitConverter.ToString(data);
                    FileInfo fileInfo = new FileInfo(SrcFilePath);
                    FileName = Path.GetFileNameWithoutExtension(fileInfo.FullName);
                }

                switch (EncryptionFlag)
                {
                    case 1:
                        {
                            //создание файловых потоков
                            using (FileStream srcFileStream = new FileStream(SrcFilePath, FileMode.Open, FileAccess.Read),
                                              keyFileStream = new FileStream(KeyFilePath, FileMode.Open, FileAccess.Read),
                                              resFileStream = File.Create(SavePath + "\\" + FileName + FileExtension))
                            {
                                //вызовы функций дешифровки
                                //IDEADecryption iDEADecryption

                            }
                            break;
                        }
                    case 2:
                        {
                            //создание файловых потоков
                            using (FileStream srcFileStream = new FileStream(SrcFilePath, FileMode.Open, FileAccess.Read),
                                              keyFileStream = new FileStream(KeyFilePath, FileMode.Open, FileAccess.Read),
                                              resFileStream = File.Create(SavePath + "\\" + FileName + FileExtension))
                            {
                                //вызовы функций дешифровки

                            }
                            break;
                        }
                }

                //очищение textbox и полей данных
                textBoxFileName.Clear();
                textBoxFilePath.Clear();
                FileExtension = "";
                SavePath = "";
                SrcFilePath = "";
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
            if (decryptionForm.DialogResult == DialogResult.Cancel)
            {
                decryptionForm.Dispose();
            }
        }

        private void SaveDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog browserDialog = new FolderBrowserDialog();
            //browserDialog.RootFolder = Environment.SpecialFolder.MyDocuments;
            if (browserDialog.ShowDialog() == DialogResult.OK)
            {
                SavePath = browserDialog.SelectedPath.ToString();
                textBoxLog.Text += "Save folder is selected: " + browserDialog.SelectedPath.ToString() + "\r\n";
            }
        }
    }
}

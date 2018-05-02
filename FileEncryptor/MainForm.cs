using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using IDEAEncryprion;
using ElgamalEncryption;

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
            SavePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
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
                textBoxLog.Text += DateTime.Now.ToString() + " Selected file: " + info.Name.ToString() + "\r\n";
            }
        }

        private void ButtonEncrypt_Click(object sender, EventArgs e)
        {
            EncryptionForm encryptionForm = new EncryptionForm(this);
            encryptionForm.ShowDialog();
            FileInfo fileInfo = new FileInfo(SrcFilePath);
            if(encryptionForm.DialogResult == DialogResult.OK)
            {
                encryptionForm.Dispose();
                if (fileInfo.Length > 10240 && EncryptionFlag == 2)
                {
                    string text = "The file size for the selected method should not exceed 10 KB";
                    string caption1 = "Error";
                    MessageBox.Show(text, caption1, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    //очищение textbox и полей данных
                    ClearData();
                    buttonEncrypt.Enabled = false;
                    return;
                }
                if (fileInfo.Length > 1024 * 1024 * 4 && EncryptionFlag == 1)
                {
                    string text = "The file size for the selected method should not exceed 4 GB";
                    string caption1 = "Error";
                    MessageBox.Show(text, caption1, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    //очищение textbox и полей данных
                    ClearData();
                    buttonEncrypt.Enabled = false;
                    return;
                }
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
                                ElGamalEncryption elGamalEncryption = new ElGamalEncryption();
                                elGamalEncryption.Encrypt(srcFileStream, keyFileStream, resFileStream, FileExtension);
                            }
                            break;
                        }
                }

                //очищение textbox и полей данных
                ClearData();
                buttonEncrypt.Enabled = false;

                //вывод информации
                string info = "File was encrypted and located on: \r\n" + SavePath;
                string caption = "Encryption complete";
                textBoxLog.Text += DateTime.Now.ToString() + " " + info + "\r\n";
                MessageBox.Show(info, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
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

                    //сравнение хешей
                    if (!CompareHash(keyFileStream, encryptedFileStream))
                    {
                        string text = "The key file does not math the encrypted file";
                        string caption1 = "Error";
                        MessageBox.Show(text, caption1, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        KeyFilePath = "";
                        return;
                    }
                }

                switch (EncryptionFlag)
                {
                    case 1:
                        {
                            //взятие расширения
                            using (FileStream keyFileStream = new FileStream(KeyFilePath, FileMode.Open, FileAccess.Read))
                            {                               
                                byte[] data = new byte[keyFileStream.Length - 120];
                                keyFileStream.Seek(120, SeekOrigin.Begin);
                                keyFileStream.Read(data, 0, (int)(keyFileStream.Length - 120));
                                FileExtension = Encoding.ASCII.GetString(data);
                                FileInfo fileInfo = new FileInfo(SrcFilePath);
                                FileName = Path.GetFileNameWithoutExtension(fileInfo.FullName);
                            }
                            //создание файловых потоков
                            using (FileStream srcFileStream = new FileStream(SrcFilePath, FileMode.Open, FileAccess.Read),
                                              keyFileStream = new FileStream(KeyFilePath, FileMode.Open, FileAccess.Read),
                                              resFileStream = File.Create(SavePath + "\\" + FileName + FileExtension))
                            {
                                //вызовы функций дешифровки
                                IDEADecryption decryption = new IDEADecryption();
                                decryption.Decrypt(srcFileStream, resFileStream, keyFileStream);
                            }
                            break;
                        }
                    case 2:
                        {
                            using (FileStream keyFileStream = new FileStream(KeyFilePath, FileMode.Open, FileAccess.Read))
                            {
                                byte[] data = new byte[keyFileStream.Length - 24];
                                keyFileStream.Seek(24, SeekOrigin.Begin);
                                keyFileStream.Read(data, 0, (int)(keyFileStream.Length - 24));
                                FileExtension = Encoding.ASCII.GetString(data);
                                FileInfo fileInfo = new FileInfo(SrcFilePath);
                                FileName = Path.GetFileNameWithoutExtension(fileInfo.FullName);
                            }
                            //создание файловых потоков
                            using (FileStream srcFileStream = new FileStream(SrcFilePath, FileMode.Open, FileAccess.Read),
                                              keyFileStream = new FileStream(KeyFilePath, FileMode.Open, FileAccess.Read),
                                              resFileStream = File.Create(SavePath + "\\" + FileName + FileExtension))
                            {
                                //вызовы функций дешифровки
                                ElGamalDecryption elGamalDecryption = new ElGamalDecryption();
                                elGamalDecryption.Decrypt(srcFileStream, keyFileStream, resFileStream);
                            }
                            break;
                        }
                }

                //очищение textbox и полей данных
                ClearData();
                buttonDecrypt.Enabled = false;

                //вывод информации
                string info = "File was decrypted and located on: \r\n" + SavePath;
                string caption = "Decyption complete";
                textBoxLog.Text += DateTime.Now.ToString() + " " + info + "\r\n";
                MessageBox.Show(info, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                textBoxLog.Text += DateTime.Now.ToString() + " Save folder is selected: " + browserDialog.SelectedPath.ToString() + "\r\n";
            }
        }

        /// <summary>
        /// Compare hash from key file with hash from encrypted file
        /// </summary>
        /// <param name="keyFileStream">Key file stream</param>
        /// <param name="encryptedFileStream">File stream of the encrypted file</param>
        /// <returns></returns>
        private bool CompareHash(FileStream keyFileStream, FileStream encryptedFileStream)
        {
            byte[] hashKeyFile = new byte[16];
            byte[] hashEncryptedFile = new byte[16];
            keyFileStream.Seek(0, SeekOrigin.Begin);
            encryptedFileStream.Seek(1, SeekOrigin.Begin);
            keyFileStream.Read(hashKeyFile, 0, 16);
            encryptedFileStream.Read(hashEncryptedFile, 0, 16);
            for (int i = 0; i < 16; i++)
            {
                if (hashEncryptedFile[i] != hashKeyFile[i])
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Clearing data fields
        /// </summary>
        private void ClearData()
        {
            textBoxFileName.Clear();
            textBoxFilePath.Clear();
            FileExtension = "";
            SrcFilePath = "";
            FileName = "";
            KeyFilePath = "";
            EncryptionFlag = 0;
        }
    }
}

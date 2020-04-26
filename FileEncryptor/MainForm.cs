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
            SavePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\FileEncryptor";
            KeyFilePath = "";
            FileInfo = null;
        }

        public FileInfo FileInfo { get; set; }

        public string SavePath { get; set; }

        public string KeyFilePath { get; set; }

        private void Button_SelectFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                FileInfo = new FileInfo(fileDialog.FileName);
                textBoxFileName.Text = FileInfo.Name;
                textBoxFilePath.Text = FileInfo.FullName;
                if (FileInfo.Extension.Equals(".encryp"))
                {
                    buttonDecrypt.Enabled = true;
                    buttonEncrypt.Enabled = false;
                }
                else
                {
                    buttonEncrypt.Enabled = true;
                    buttonDecrypt.Enabled = false;
                }
                textBoxLog.Text += DateTime.Now.ToString() + " Selected file: " + FileInfo.Name + "\r\n";
            }
            fileDialog.Dispose();
        }

        private void ButtonEncrypt_Click(object sender, EventArgs e)
        {
            /*if (fileInfo.Length > 1024 * 1024 * 4 && EncryptionFlag == 1)
            {
                string text = "The file size for the selected method should not exceed 4 GB";
                string caption1 = "Error";
                MessageBox.Show(text, caption1, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                //очищение textbox и полей данных
                ClearData();
                buttonEncrypt.Enabled = false;
                return;
            }*/
            //создание файловых потоков
            using (FileStream srcFileStream = File.Open(FileInfo.FullName, FileMode.Open, FileAccess.Read),
                              resFileStream = File.Create(SavePath + "\\" + Path.GetFileNameWithoutExtension(FileInfo.FullName) + ".encryp"),
                              keyFileStream = File.Create(SavePath + "\\" + Path.GetFileNameWithoutExtension(FileInfo.FullName) + ".key"))
            {
                //вызовы функций шифрования
                IDEAEncryption encryption = new IDEAEncryption();
                encryption.Encrypt(srcFileStream, resFileStream, keyFileStream, FileInfo.Extension);
            }
            //}

            //очищение textbox и полей данных
            ClearData();
            buttonEncrypt.Enabled = false;

            //вывод информации
            string info = "File was encrypted and located on: \r\n" + SavePath;
            string caption = "Encryption complete";
            textBoxLog.Text += DateTime.Now.ToString() + " " + info + "\r\n";
            MessageBox.Show(info, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ButtonDecrypt_Click(object sender, EventArgs e)
        {
            DecryptionForm decryptionForm = new DecryptionForm(this);
            decryptionForm.ShowDialog();
            if (decryptionForm.DialogResult == DialogResult.OK)
            {
                decryptionForm.Dispose();
                using (FileStream keyFileStream = new FileStream(KeyFilePath, FileMode.Open, FileAccess.Read),
                                  srcFileStream = new FileStream(FileInfo.FullName, FileMode.Open, FileAccess.Read),
                                  resFileStream = File.Create(SavePath + "\\" + Path.GetFileNameWithoutExtension(FileInfo.FullName) + GetExtension(keyFileStream)))
                {
                    //сравнение хешей
                    if (!CompareHash(keyFileStream, srcFileStream))
                    {
                        string text = "The key file does not math the encrypted file";
                        string caption1 = "Error";
                        MessageBox.Show(text, caption1, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        KeyFilePath = "";
                        return;
                    }

                    //вызовы функций дешифровки
                    IDEADecryption decryption = new IDEADecryption();
                    decryption.Decrypt(srcFileStream, resFileStream, keyFileStream);
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
            decryptionForm.Dispose();
        }

        private string GetExtension(FileStream keyFileStream)
        {
            byte[] data = new byte[keyFileStream.Length - 120];
            keyFileStream.Seek(120, SeekOrigin.Begin);
            keyFileStream.Read(data, 0, (int)(keyFileStream.Length - 120));
            return Encoding.ASCII.GetString(data);
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
            browserDialog.Dispose();
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
            encryptedFileStream.Seek(0, SeekOrigin.Begin);
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
            KeyFilePath = "";
            FileInfo = null;
        }
    }
}

using ElgamalEncryption;
using IDEAEncryprion;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileEncryptor
{
    internal class FileService
    {
        FileInfo FileInfo { get; set; }
        string SavePath { get; set; }
        string KeyFilePath { get; set; }


        public FileService(FileInfo fileInfo, string savePath)
        {
            FileInfo = fileInfo;
            SavePath = savePath;
        }
        public FileService(FileInfo fileInfo, string savePath, string keyFilePath)
        {
            FileInfo = fileInfo;
            SavePath = savePath;
            KeyFilePath = keyFilePath;
        }

        public void UseEncryption(string encryptionMethod)
        {

            using (FileStream srcFileStream = File.Open(FileInfo.FullName, FileMode.Open, FileAccess.Read),
                  resFileStream = File.Create(SavePath + "\\" + Path.GetFileNameWithoutExtension(FileInfo.FullName) + ".encryp"),//падает здесь IOException directory cannot found
                  keyFileStream = File.Create(SavePath + "\\" + Path.GetFileNameWithoutExtension(FileInfo.FullName) + ".key"))
            {
                switch (encryptionMethod)
                {
                    case "IDEA":
                        IDEAEncryption IdeaEncryption = new IDEAEncryption(srcFileStream, resFileStream, keyFileStream, FileInfo.Extension);
                        IdeaEncryption.Encrypt();
                        break;
                    case "Elgamal":
                        ElGamalEncryption elgamalEncryption = new ElGamalEncryption(srcFileStream, resFileStream, keyFileStream, FileInfo.Extension);
                        elgamalEncryption.Encrypt();
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }

        }

        public void UseDecryption()
        {
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
                    return;
                }
                switch (GetEncryptionMethod(keyFileStream))
                {
                    case 1:
                        IDEADecryption IdeaDecryption = new IDEADecryption(srcFileStream, resFileStream, keyFileStream);
                        IdeaDecryption.Decrypt();
                        break;
                    case 2:
                        ElGamalDecryption elGamalDecryption = new ElGamalDecryption(srcFileStream, keyFileStream, resFileStream);
                        elGamalDecryption.Decrypt();
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        private byte GetEncryptionMethod(FileStream keyFileStream)
        {
            //исправить чтение и запись файлов. последний байт в ключ-файле - метод шифрования
            throw new NotImplementedException();
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

        private string GetExtension(FileStream keyFileStream)
        {
            byte[] data = new byte[keyFileStream.Length - 120];
            keyFileStream.Seek(120, SeekOrigin.Begin);
            keyFileStream.Read(data, 0, (int)(keyFileStream.Length - 120));
            return Encoding.ASCII.GetString(data);
        }
    }
}

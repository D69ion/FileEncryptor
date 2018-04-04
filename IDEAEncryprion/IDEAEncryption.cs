using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace IDEAEncryprion
{
    public class IDEAEncryption
    {
        private static RNGCryptoServiceProvider rNG = new RNGCryptoServiceProvider();

        public ushort[] Key { get; set; }

        /// <summary>
        /// Encrypt file with IDEA encryption
        /// </summary>
        /// <param name="srcFileStream">Input source file stream</param>
        /// <param name="encryptedFileStream">Output encrypted file stream</param>
        /// <param name="keyFileStream">Output key file stream</param>
        public void Encrypt(FileStream srcFileStream, FileStream encryptedFileStream, FileStream keyFileStream, string extension)
        {
            GenerateKey();
            CreateKeyFile(srcFileStream, keyFileStream);

            //запись флага шифрования в зашифрованный файл
            encryptedFileStream.WriteByte(1);

            //запись MD5 хеша в зашифрованный файл
            var md5 = MD5.Create().ComputeHash(srcFileStream);
            encryptedFileStream.Write(md5, 0, md5.Length);

            //запись расширения файла в зашифрованный файл
            byte[] temp = Encoding.Default.GetBytes(extension);
            encryptedFileStream.Write(temp, 0, temp.Length);

            //шифрование файла
            for(long i = 0; i < srcFileStream.Length / 8 + 1; i++)//неточность
            {
                EncryptionRounds(srcFileStream, encryptedFileStream, i);
            }
            encryptedFileStream.Flush();
        }

        /// <summary>
        /// First 8 rounds of encryption
        /// </summary>
        /// <param name="srcFileStream">Input source file stream</param>
        /// <param name="encryptedFileStream">Output encrypted file stream</param>
        /// <param name="startIndex">The position from which the reading starts in the source file stream</param>
        private void EncryptionRounds(FileStream srcFileStream, FileStream encryptedFileStream, long startIndex)
        {
            byte[] data = new byte[8];
            srcFileStream.Position = startIndex * 8;
            srcFileStream.Read(data, 0, 8);

            //преобразование в 16 битные(2 байтные) блоки
            ushort[] blocks = new ushort[4];
            for(int i = 0; i < 4; i++)
            {
                blocks[i] = BitConverter.ToUInt16(data, i * 2);
            }

            //раунды шифрования
            for(int i = 0; i < 48; i += 6)
            {
                if (blocks[0] == 0)
                    blocks[0] = (ushort)((65536 * Key[i]) % 65537);
                else
                    blocks[0] = (ushort)((blocks[0] * Key[i]) % 65537);
                blocks[1] = (ushort)((blocks[1] + Key[i + 1]) % 65536);
                blocks[2] = (ushort)((blocks[2] + Key[i + 2]) % 65536);
                if (blocks[3] == 0)
                    blocks[3] = (ushort)((65536 * Key[i]) % 65537);
                else
                    blocks[3] = (ushort)((blocks[3] * Key[i + 3]) % 65537);
                ushort temp1 = (ushort)(blocks[0] ^ blocks[2]);
                ushort temp2 = (ushort)(blocks[1] ^ blocks[3]);
                if (temp1 == 0)
                    temp1 = (ushort)((65536 * Key[i + 4]) % 65537);
                else
                    temp1 = (ushort)((temp1 * Key[i + 4]) % 65537);
                temp2 = (ushort)((temp1 + temp2) % 65536);
                if(temp2==0)
                    temp2 = (ushort)((65536 * Key[i + 5]) % 65537);
                else
                    temp2 = (ushort)((temp2 * Key[i + 5]) % 65537);
                temp1 = (ushort)((temp1 + temp2) % 65536);
                blocks[0] = (ushort)(blocks[0] ^ temp2);
                blocks[1] = (ushort)(blocks[1] ^ temp1);
                blocks[2] = (ushort)(blocks[2] ^ temp2);
                blocks[3] = (ushort)(blocks[3] ^ temp1);
            }
            EncryptionLastRound(blocks);

            //преобразование обратно в байты
            byte[] temp = null;
            for(int i = 0; i < 8; i += 2)
            {
                temp = BitConverter.GetBytes(blocks[i / 2]);
                byte temp1 = temp[0];
                byte temp2 = temp[1];
                data[i] = temp1;
                data[i + 1] = temp2;
            }

            //запись в файл
            encryptedFileStream.Write(data, 0, 8);
        }

        /// <summary>
        /// Last half round of encryption
        /// </summary>
        /// <param name="blocks">Data block</param>
        private void EncryptionLastRound(ushort[] blocks)
        {
            if(blocks[0]==0)
                blocks[0] = (ushort)((65536 * Key[48]) % 65537);
            else
                blocks[0] = (ushort)((blocks[0] * Key[48]) % 65537);
            blocks[1] = (ushort)((blocks[1] + Key[49]) % 65536);
            blocks[2] = (ushort)((blocks[2] + Key[50]) % 65536);
            if(blocks[3]==0)
                blocks[3] = (ushort)((65536 * Key[51]) % 65537);
            else
                blocks[3] = (ushort)((blocks[3] * Key[51]) % 65537);
        }

        /// <summary>
        /// Create key file
        /// </summary>
        /// <param name="srcFileStream">Input source file stream</param>
        /// <param name="keyFileStream">Output key file stream</param>
        private void CreateKeyFile(FileStream srcFileStream, FileStream keyFileStream)
        {
            //запись MD5 хеша в файл с ключом
            var md5 = MD5.Create().ComputeHash(srcFileStream);
            keyFileStream.Write(md5, 0, md5.Length);

            //запись ключей в файл с ключом
            for(int i = 0; i < 52; i++)
            {
                keyFileStream.Write(BitConverter.GetBytes(Key[i]), 0, 2);
            }
            keyFileStream.Flush();
        }

        /// <summary>
        /// Generate new encryption key
        /// </summary>
        private void GenerateKey()
        {
            byte[] byteKey = new byte[104];
            //создание 128 битного ключа
            byte[] random = new byte[16];
            rNG.GetBytes(random);

            byte[] temp = random;
            for(int i = 0; i < 16; i++)
            {
                byteKey[i] = temp[i];
            }
            for(int i = 1; i < 6; i++)
            {
                temp = LeftShift(KeyToString(temp));
                for(int j = 0; j < 16; j++)
                {
                    byteKey[i * 16 + j] = temp[j];
                }
            }
            temp = LeftShift(KeyToString(temp));
            for(int i = 0; i < 8; i++)
            {
                byteKey[96 + i] = temp[i];
            }

            ushort[] key = new ushort[52];
            for(int i = 0; i < 52; i++)
            {
                key[i] = BitConverter.ToUInt16(byteKey, i * 2);
            }
            Key = key;
        }

        /// <summary>
        /// Preparation key to a shift
        /// </summary>
        /// <param name="key">Random generated bytes</param>
        /// <returns></returns>
        private StringBuilder KeyToString(byte[] key)
        {
            string temp = "";
            StringBuilder res = new StringBuilder(128);
            for (int i = 0; i < 16; i++)
            {
                temp = Convert.ToString(key[i], 2);
                if (temp.Length < 8)
                {
                    string temp1 = "";
                    for (int a = temp.Length - 1; a > -1; a--)
                    {
                        temp1 += temp[a];
                    }
                    for (int j = 0; j < 8 - temp.Length; j++)
                    {
                        temp1 += "0";
                    }
                    temp = null; ;
                    for (int a = temp1.Length - 1; a > -1; a--)
                    {
                        temp += temp1[a];
                    }
                }
                res.Append(temp);
            }
            return res;
        }

        /// <summary>
        /// Left shift 25 characters
        /// </summary>
        /// <param name="res"></param>
        /// <returns></returns>
        private byte[] LeftShift(StringBuilder res)
        {
            string temp = null;
            char[] charAr = new char[25];
            res.CopyTo(0, charAr, 0, 25);
            temp = new string(charAr);
            res.Remove(0, 25);
            res.Append(temp);
            byte[] key = new byte[16];
            char[] keyChar = new char[8];
            for (int j = 0; j < 16; j++)
            {
                res.CopyTo(j * 8, keyChar, 0, 8);
                key[j] = Convert.ToByte(new string(keyChar), 2);
            }
            return key;
        }       
    }
}

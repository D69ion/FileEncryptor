using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;


namespace IDEAEncryprion
{
    class IDEADecryption
    {
        public ushort[] Key { get; set; }
        const int mod = 65537;//2^16 + 1

        public void Decryption(FileStream srcFileStream, FileStream decryptedFileStream, FileStream keyFileStream)
        {
            GenerateKey(srcFileStream, keyFileStream);

        }

        private void DecryptionRounds(FileStream srcFileStream, FileStream decryptedFileStream)
        {

        }

        private void DecryptionLastRound(ushort[] data)
        {

        }

        private void GenerateKey(FileStream srcFileStream, FileStream keyFileStream)
        {
            keyFileStream.Position = 16;
            ushort[] keyData = new ushort[52];
            byte[] temp = null;
            for(int i = 0; i < 52; i++)
            {
                keyFileStream.Read(temp, 0, 2);
                keyData[i] = BitConverter.ToUInt16(temp, 0);
            }

            sbyte count = -1;
            for(int i = 50; i > 8; i -= 6)
            {
                Key[++count] = MultiplicativeInversion(keyData[i]);
                Key[++count] = AdditiveInversion(keyData[i + 1]);
                Key[++count] = AdditiveInversion(keyData[i + 2]);
                Key[++count] = MultiplicativeInversion(keyData[i + 3]);
                Key[++count] = keyData[i - 2];
                Key[++count] = keyData[i - 1];
            }
            Key[++count] = MultiplicativeInversion(keyData[0]);
            Key[++count] = AdditiveInversion(keyData[1]);
            Key[++count] = AdditiveInversion(keyData[2]);
            Key[++count] = MultiplicativeInversion(keyData[3]);
        }

        /// <summary>
        /// Аддитивная инверсия числа
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private ushort AdditiveInversion(ushort value)
        {
            return (ushort)(65536 - value);
        }

        /// <summary>
        /// Мультипликативная инверсия числа
        /// </summary>
        /// <param name="value">Число</param>
        /// <returns></returns>
        private ushort MultiplicativeInversion(ushort value)
        {
            return BinPow(value, mod - 2, mod);
        }

        /// <summary>
        /// Быстрое возведение в степень по модулю
        /// </summary>
        /// <param name="value">Число</param>
        /// <param name="pow">Степень</param>
        /// <param name="mod">Модуль</param>
        /// <returns></returns>
        private ushort BinPow(ushort value, int pow, int mod)
        {
            ushort res = 1;
            while (pow > 0)
            {
                if (pow % 2 == 1)
                    res = (ushort)((res * value) % mod);
                pow >>= 1;
                value = (ushort)((value * value) % mod);
            }
            return res;
        }
    }
}

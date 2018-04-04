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
        const int mod = 65537;

        public void Decryption(FileStream srcFileStream, FileStream decryptedFileStream, FileStream keyFileStream)
        {

        }

        private void DecryptionRounds(FileStream srcFileStream, FileStream decryptedFileStream)
        {

        }

        private void DecryptionLastRound(ushort[] data)
        {

        }

        private void GenerateKey(FileStream srcFileStream, FileStream keyFileStream)
        {

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
        /// <param name="value"></param>
        /// <returns></returns>
        private ushort MultiplicativeInversion(ushort value)
        {
            return BinPow(value, mod - 2, mod);
        }

        /// <summary>
        /// Быстрое возведение в степень по модулю
        /// </summary>
        /// <param name="value"></param>
        /// <param name="pow"></param>
        /// <param name="mod"></param>
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

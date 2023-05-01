using System;
using System.IO;
using System.Numerics;

namespace ElgamalEncryption
{
    public class ElGamalDecryption
    {
        private int P { get; set; } //случайное большое простое число
        private int X { get; set; } //число X такое, что  1 < X < P - 1 (открытый ключ)
        private const int mod = 65537;

        private FileStream SrcFileStream { get; set; }
        private FileStream KeyFileStream { get; set; }
        private FileStream ResFileStream { get; set; }

        public ElGamalDecryption(FileStream srcFileStream, FileStream keyFileStream, FileStream resFileStream)
        {
            SrcFileStream = srcFileStream;
            KeyFileStream = keyFileStream;  
            ResFileStream = resFileStream;
        }

        /// <summary>
        /// Decrypt file with ElGamal scheme
        /// </summary>
        /// <param name="SrcFileStream">Input file stream of the source file</param>
        /// <param name="KeyFileStream">Output file stream of the key file</param>
        /// <param name="ResFileStream">Output file stream of the decrypted file</param>
        public void Decrypt()
        {
            GetKeys(KeyFileStream);
            SrcFileStream.Seek(17, SeekOrigin.Begin);
            ResFileStream.Seek(0, SeekOrigin.Begin);
            byte[] data = new byte[8];
            for(long i = 17; i < SrcFileStream.Length; i += 8)
            {
                SrcFileStream.Read(data, 0, 8);
                int a = BitConverter.ToInt32(data, 0);
                int b = BitConverter.ToInt32(data, 4);
                BigInteger temp = b * BigInteger.Pow(a, (P - X - 1)) % P;
                ResFileStream.Write(BitConverter.GetBytes((ushort)temp), 0, 2);
            }
            ResFileStream.Flush();
        }

        /// <summary>
        /// Read keys from key-file
        /// </summary>
        /// <param name="keyFileStream">Output file stream of the key file</param>
        public void GetKeys(FileStream keyFileStream)
        {
            keyFileStream.Seek(16, SeekOrigin.Begin);
            byte[] data = new byte[4];
            keyFileStream.Read(data, 0, 4);
            P = BitConverter.ToInt32(data, 0);
            keyFileStream.Read(data, 0, 4);
            X = BitConverter.ToInt32(data, 0);
        }
    }
}

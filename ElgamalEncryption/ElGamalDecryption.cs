using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Numerics;

namespace ElgamalEncryption
{
    public class ElGamalDecryption
    {
        private int P { get; set; } //случайное большое простое число
        private int X { get; set; } //число X такое, что  1 < X < P - 1 (открытый ключ)
        private const int mod = 65537;
        
        public void Decrypt(FileStream srcFileStream, FileStream keyFileStream, FileStream resFileStream)
        {
            GetKeys(keyFileStream);
            srcFileStream.Seek(17, SeekOrigin.Begin);
            resFileStream.Seek(0, SeekOrigin.Begin);
            byte[] data = new byte[8];
            for(long i = 17; i < srcFileStream.Length; i += 8)
            {
                srcFileStream.Read(data, 0, 8);
                int a = BitConverter.ToInt32(data, 0);
                int b = BitConverter.ToInt32(data, 4);
                BigInteger temp = b * BigInteger.Pow(a, (P - X - 1)) % P;
                resFileStream.Write(BitConverter.GetBytes((ushort)temp), 0, 2);
            }
            resFileStream.Flush();
        }

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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;

namespace GithubSync
{
    public static class Crypto
    {
        private static SymmetricAlgorithm symAlg;
        private static HashAlgorithm hash;
        private static AsymmetricAlgorithm asymAlg;
        const int bufferSize = 4096;

        static Crypto()
        {
            symAlg = SymmetricAlgorithm.Create("DES");
            asymAlg = AsymmetricAlgorithm.Create("RSA");
            hash = HashAlgorithm.Create("SHA1");
            hash.Initialize();
        }

        public static void GenKey()
        {
            symAlg.GenerateIV();
            symAlg.GenerateKey();
        }

        public static void SaveKey()
        {
            var iv = Convert.ToBase64String(symAlg.IV, Base64FormattingOptions.None);
            var key = Convert.ToBase64String(symAlg.Key, Base64FormattingOptions.None);

            File.WriteAllLines(Constants.CryptoCfg, new string[] { iv, key });
        }

        public static void LoadKey()
        {
            try
            {
                var lines = File.ReadAllLines(Constants.CryptoCfg);

                symAlg.IV = Convert.FromBase64String(lines[0]);
                symAlg.Key = Convert.FromBase64String(lines[1]);
            }
            catch
            {
                GenKey();
                SaveKey();
            }
        }

        public static string Encrypt(string clearText)
        {
            using (var bstream = new MemoryStream())
            using (var cstream = new CryptoStream(bstream, symAlg.CreateEncryptor(), CryptoStreamMode.Write))
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(clearText)))
            {
                stream.WriteTo(cstream);
                cstream.FlushFinalBlock();

                ArraySegment<byte> buffer;
                if (!bstream.TryGetBuffer(out buffer))
                    return String.Empty;

                return Convert.ToBase64String(buffer.ToArray());
            }
        }

        public static string Decrypt(string encText)
        {
            using (var bstream = new MemoryStream())
            using (var cstream = new CryptoStream(bstream, symAlg.CreateDecryptor(), CryptoStreamMode.Write))
            using (var stream = new MemoryStream(Convert.FromBase64String(encText)))
            {
                stream.WriteTo(cstream);
                cstream.FlushFinalBlock();

                ArraySegment<byte> buffer;
                if (!bstream.TryGetBuffer(out buffer))
                    return String.Empty;

                return Encoding.UTF8.GetString(buffer.ToArray());
            }
        }

        public static string GetSHA1(string v)
        {
            return BitConverter.ToString(hash.ComputeHash(Encoding.UTF8.GetBytes(v))).Replace("-", string.Empty);
        }

    }
}

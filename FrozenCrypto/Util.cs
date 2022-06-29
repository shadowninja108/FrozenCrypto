using Org.BouncyCastle.Crypto.Macs;
using System;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;

namespace FrozenCrypto
{
    public static class Util
    {
        public static Span<byte> AsSpan<T>(ref T val) where T : unmanaged
        {
            Span<T> valSpan = MemoryMarshal.CreateSpan(ref val, 1);
            return MemoryMarshal.Cast<T, byte>(valSpan);
        }

        public static DirectoryInfo GetDirectory(this DirectoryInfo dir, string sub)
        {
            return new DirectoryInfo(Path.Combine(dir.FullName, sub));
        }

        public static FileInfo GetFile(this DirectoryInfo dir, string name)
        {
            return new FileInfo(Path.Combine(dir.FullName, name));
        }

        public static T Read<T>(this Stream stream) where T : struct
        {
            byte[] buffer = new byte[Unsafe.SizeOf<T>()];
            stream.Read(buffer, 0, buffer.Length);
            GCHandle handle = default;
            try
            {
                handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                return (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            }
            finally
            {
                if (handle.IsAllocated)
                    handle.Free();
            }
        }

        public static void Write<T>(this Stream stream, T data) where T : struct
        {
            byte[] buffer = new byte[Unsafe.SizeOf<T>()];
            GCHandle handle = default;
            try
            {
                handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                Marshal.StructureToPtr(data, handle.AddrOfPinnedObject(), true);
            }
            finally
            {
                if (handle.IsAllocated)
                    handle.Free();
            }
            stream.Write(buffer);
        }

        public static byte[] ToByteArray<T>(this T[] array) where T : IBinaryInteger<T>, IShiftOperators<T, T>
        {
            var tsize = Unsafe.SizeOf<T>();
            var count = array.Length * tsize;
            var buffer = new byte[count];

            for (var i = 0; i < buffer.Length; i++)
            {
                var vidx = i / tsize;
                var bidx = i % tsize;
                buffer[i] = IBinaryInteger<byte>.CreateTruncating(array[vidx] >> (bidx * 8));
            }

            return buffer;
        }

        public static T[] FromByteArray<T>(this byte[] array) where T : IBinaryInteger<T>, IShiftOperators<T, T>
        {
            var tsize = Unsafe.SizeOf<T>();
            var count = array.Length / tsize;
            var buffer = new T[count];

            for (var i = 0; i < array.Length; i++)
            {
                var bidx = i / tsize;
                buffer[bidx] <<= 8;
                buffer[bidx] |= IBinaryInteger<T>.CreateTruncating(array[i]);
            }

            return buffer;
        }

        public static byte[] Decrypt(this Stream stream, byte[] key, byte[] iv, int length)
        {
            using var aes = Aes.Create();
            aes.BlockSize = 128;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.None;
            aes.Key = key;
            aes.IV = iv;

            var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var cs = new CryptoStream(stream, decryptor, CryptoStreamMode.Read);

            byte[] decrypted = new byte[length];
            cs.Read(decrypted);

            return decrypted;
        }

        public static byte[] Encrypt(this byte[] data, byte[] key, byte[] iv, int length)
        {
            using var aes = Aes.Create();
            aes.BlockSize = 128;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.None;
            aes.Key = key;
            aes.IV = iv;

            var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            byte[] encrypted = new byte[length];
            using var cs = new CryptoStream(new MemoryStream(encrypted), encryptor, CryptoStreamMode.Write);
            cs.Write(data);

            return encrypted;
        }

        public static byte[] Cmac(this byte[] data, byte[] key)
        {
            var mac = new CMac(new AesEngine());
            mac.Init(new KeyParameter(key));
            mac.BlockUpdate(data, 0, data.Length);

            var result = new byte[mac.GetMacSize()];
            mac.DoFinal(result, 0);
            return result;
        }
    }
}

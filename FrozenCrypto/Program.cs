using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace FrozenCrypto
{
    public class Program
    {
        static void Main(string[] args)
        {

            if (args.Length != 2 & args.Length != 3)
            {
                Console.WriteLine("Args: [-d] input output");
                return;
            }

            bool decrypt = args.Length == 3;

            string inputStr;
            string outputStr;
            if (decrypt)
            {
                inputStr = args[1];
                outputStr = args[2];
            }
            else
            {
                inputStr = args[0];
                outputStr = args[1];
            }

            FileInfo inputFi = new(inputStr);
            FileInfo outputFi = new(outputStr);

            if (!inputFi.Exists)
            {
                Console.WriteLine($"{inputFi.FullName} does not exist.");
                return;
            }

            if (outputFi.Exists)
            {
                Console.WriteLine($"{outputFi.FullName} already exists. Are you sure? (y/n)");
                while (true)
                {
                    var l = Console.ReadLine();
                    if (l == "y")
                        break;
                    if (l == "n")
                        return;
                }
            }

            if(decrypt)
                Decrypt(outputFi, inputFi);
            else
                Encrypt(outputFi, inputFi);
        }

        public static void Decrypt(FileInfo output, FileInfo input)
        {
            using var inputStream = input.OpenRead();
            var header = inputStream.Read<Types.SaveHeader>();

            SeadRandom random = new(
                header.RandomSeeds[0],
                header.RandomSeeds[1],
                header.RandomSeeds[2],
                header.RandomSeeds[3]
            );
            random.GetUInt32();
            random.GetUInt32();
            random.GetUInt32();
            random.GetUInt32();

            random.GetContext(
                out var rand1,
                out var rand2,
                out var rand3,
                out var rand4
            );

            uint[] iv =
            {
                rand2,
                rand1,
                rand4,
                rand3,
            };

            var bodyKey = random.GenKey();
            byte[] data = inputStream.Decrypt(bodyKey.ToByteArray(), iv.ToByteArray(), (int)header.BodySize);

            var macKey = random.GenKey();
            var fileMac = data.Cmac(macKey.ToByteArray());

            if (!fileMac.SequenceEqual(header.Cmac))
            {
                Console.WriteLine("Invalid CMAC in header!");
            }

            using var os = output.Create();
            os.Write(data);
        }

        public static void Encrypt(FileInfo output, FileInfo input)
        {
            var seed = RandomNumberGenerator.GetBytes(Unsafe.SizeOf<uint>() * 4);
            var seedU32 = seed.FromByteArray<uint>();

            SeadRandom random = new(
                seedU32[0],
                seedU32[1],
                seedU32[2],
                seedU32[3]
            );
            random.GetUInt32();
            random.GetUInt32();
            random.GetUInt32();
            random.GetUInt32();

            random.GetContext(
                out var rand1,
                out var rand2,
                out var rand3,
                out var rand4
            );

            uint[] iv =
            {
                rand2,
                rand1,
                rand4,
                rand3,
            };

            var header = new Types.SaveHeader
            {
                Version = 1,
                BodySize = (uint)(input.Length),
                RandomSeeds = seedU32
            };

            using var inputStream = input.OpenRead();
            var body = new byte[inputStream.Length];
            inputStream.Read(body);

            var bodyKey = random.GenKey();
            var encBody = body.Encrypt(bodyKey.ToByteArray(), iv.ToByteArray(), (int)header.BodySize);

            var macKey = random.GenKey();
            var fileMac = body.Cmac(macKey.ToByteArray());
            header.Cmac = fileMac;

            using var outputStream = output.Create();
            outputStream.Write(header);
            outputStream.Write(encBody);
        }
    }
}

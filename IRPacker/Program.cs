using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IRPacker
{
    public enum IImpulseProperty
    {
        SampleRate
    }

    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 2)
            {
                string cmd = args[0].ToLower();

                if (cmd == "pack")
                {
                    Pack(args[1]);

                    return;
                }
                else if (cmd == "unpack")
                {
                    Unpack(args[1]);

                    return;
                }
            }

            Console.WriteLine("Usage: IRPacker pack|unpack <path>");
        }

        static void Pack(string path)
        {
            foreach (string directory in Directory.GetDirectories(path))
            {
                string irName = Path.GetFileNameWithoutExtension(directory);

                using (BinaryWriter writer = new BinaryWriter(new FileStream(Path.Combine(path, irName) + ".cir", FileMode.Create)))
                {
                    Console.Out.WriteLine("Processing [" + irName + "]");

                    foreach (string irFile in Directory.GetFiles(directory, "*.raw"))
                    {
                        uint numProperties = 1; // Just sample rate right now

                        writer.Write(numProperties);

                        string irRateStr = Path.GetFileNameWithoutExtension(irFile);

                        uint irRate = uint.Parse(irRateStr);

                        Console.Out.WriteLine(" - " + irRate);

                        writer.Write((uint)IImpulseProperty.SampleRate);
                        writer.Write(irRate);

                        byte[] irData = File.ReadAllBytes(irFile);

                        Console.Out.WriteLine(" - IR is " + irData.Length + " bytes");

                        writer.Write((uint)irData.Length);

                        writer.Write(irData, 0, irData.Length);
                    }
                }
            }
        }

        static void Unpack(string path)
        {
            foreach (string irFile in Directory.GetFiles(path, "*.cir"))
            {
                string irName = Path.GetFileNameWithoutExtension(irFile);

                Console.WriteLine("Unpacking [" + irFile + "]");

                using (BinaryReader reader = new BinaryReader(new FileStream(irFile, FileMode.Open, FileAccess.Read)))
                {
                    string irPath = Path.Combine(path, irName);

                    if (!Directory.Exists(irPath))
                    {
                        Directory.CreateDirectory(irPath);
                    }

                    while (reader.BaseStream.Position < reader.BaseStream.Length)
                    {
                        uint numProperties = reader.ReadUInt32();

                        Console.WriteLine(numProperties + " properties");

                        uint sampleRate = 0;

                        for (int propNum = 0; propNum < numProperties; propNum++)
                        {
                            IImpulseProperty property = (IImpulseProperty)reader.ReadUInt32();
                            uint propertyValue = reader.ReadUInt32();

                            switch (property)
                            {
                                case IImpulseProperty.SampleRate:
                                    sampleRate = propertyValue;
                                    break;
                                default:
                                    Console.Write("Unknown property: " + property);
                                    break;
                            }
                        }

                        uint fileSize = reader.ReadUInt32();

                        Console.WriteLine("IR is " + fileSize + " bytes");

                        byte[] data = new byte[fileSize];

                        reader.Read(data, 0, (int)fileSize);

                        using (BinaryWriter writer = new BinaryWriter(new FileStream(Path.Combine(irPath, sampleRate.ToString()) + ".raw", FileMode.Create)))
                        {
                            writer.Write(data, 0, (int)fileSize);
                        }
                    }
                }
            }
        }
    }
}

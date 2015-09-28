using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace CppAutoLib
{

    public class LibArchive
    {

        private static byte[] GlobalHeader = new byte[] { (byte)'!', (byte)'<', (byte)'a', (byte)'r', (byte)'c', (byte)'h', (byte)'>', 0x0A };

        public bool IsValid { get; private set; }

        public List<string> MangledNames { get; private set; } = new List<string>();

        private string ReadCString(BinaryReader reader)
        {
            string ret = "";
            byte c = 0;
            while ((c = reader.ReadByte()) != 0)
                ret += (char) c;
            return ret;
        }

        private List<string> ParseNames(BinaryReader reader)
        {
            reader.BaseStream.Seek(reader.ReadUInt32()*4, SeekOrigin.Current);

            uint count = reader.ReadUInt32();
            reader.BaseStream.Seek(count * 2, SeekOrigin.Current);

            List<string> ret = new List<string>((int)count);
            for(int i = 0; i < count; i++) 
                ret.Add(ReadCString(reader));

            return ret;
        }

        private void ProcessFile(BinaryReader reader, bool extractNames)
        {
            // Members aligned on even boundries
            if (reader.BaseStream.Position % 2 != 0 && reader.ReadByte() != (byte)'\n')
                return;

            // Header
            string name = System.Text.Encoding.ASCII.GetString(reader.ReadBytes(16)).TrimEnd();

            if (name != "/")
                return;

            reader.BaseStream.Seek(32, SeekOrigin.Current);
            int size = int.Parse(System.Text.Encoding.ASCII.GetString(reader.ReadBytes(10)));
            reader.BaseStream.Seek(2, SeekOrigin.Current);

            if (extractNames)
            {
                MangledNames.AddRange(ParseNames(reader));

            }
            else
            {
                reader.BaseStream.Seek(size, SeekOrigin.Current);
            }

        }

        public LibArchive(string path)
        {
            IsValid = false;
            var reader = new BinaryReader(File.OpenRead(path));
            var header = reader.ReadBytes(8);
            if (!header.SequenceEqual(GlobalHeader))
            {
                IsValid = false;
                return;
            }

            ProcessFile(reader, false); // Legacy linker data
            ProcessFile(reader, true);  // Short names

            IsValid = true;
        }
    }
}
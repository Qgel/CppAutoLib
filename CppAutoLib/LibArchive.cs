using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CppAutoLib
{

    public class LibArchive
    {
        /// <summary>
        /// "!<arch>\x0a"
        /// </summary>
        private static ulong GlobalHeaderValue = 0x0a3e686372613c21;

        private readonly BinaryReader _reader;

        public bool IsValid { get; private set; }

        public List<string> MangledNames { get; private set; }

        public string Path { get; }
        public override string ToString()
        {
            return Path;
        }

        private void ReadSymbolNames()
        {
            // skip irrelevant parts of archive file header
            SkipBytes(48);
            int size = ReadSize();
            SkipBytes(2);
            // skip archive names
            var archiveNamesSize = _reader.ReadInt32() * 4;
            size -= 4 + archiveNamesSize;
            SkipBytes(archiveNamesSize);
            int symbolCount = _reader.ReadInt32();
            size -= 4 + (symbolCount*2);
            // skip symbol <-> archive matchings
            SkipBytes(symbolCount*2);
            // size has the remaining bytes in the second archive file, only containing symbol names
            var rawSymbolNames = _reader.ReadBytes(size);

            // read symbol names as 0-terminated c strings from rawSymbolNames
            MangledNames = new List<string>(symbolCount);
            var curString = new StringBuilder();
            int p = 0;
            for (int i = 0; i < symbolCount; i++)
            {
                byte b;
                while ((b = rawSymbolNames[p++]) != 0)
                    curString.Append((char) b);
                MangledNames.Add(curString.ToString());
                curString.Clear();
            }
        }

        private int ReadSize()
        {
            return int.Parse(Encoding.ASCII.GetString(_reader.ReadBytes(10)));
        }

        private void SkipBytes(int num)
        {
            _reader.BaseStream.Seek(num, SeekOrigin.Current);
        }

        public LibArchive(string path)
        {
            Path = path;

            IsValid = false;
            _reader = new BinaryReader(File.OpenRead(path));
            if (_reader.ReadUInt64() != GlobalHeaderValue)
                return;

            // skip over first file containing legacy linker data
            SkipBytes(48);
            SkipBytes(ReadSize() + 2);
            // archive files are aligned to even byte offsets
            if ((_reader.BaseStream.Position & 1) == 1)
                SkipBytes(1);
            ReadSymbolNames();

            IsValid = true;
        }
    }
}
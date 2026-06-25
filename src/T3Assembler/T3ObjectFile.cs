using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TritTypes;

namespace T3Assembler
{
    /// <summary>
    /// Object file format for T3 linker.
    /// Layout: magic + header + sections + symbol_table + relocation_table
    /// </summary>
    public class T3ObjectFile
    {
        public const int MAGIC = 0x54334F42; // "T3OB"
        public const int VERSION = 1;

        public List<Int128> TextSection = new();    // .text — executable code
        public List<Int128> DataSection = new();    // .data — initialized data
        public int BssSize;                          // .bss — zero-initialized (size in words)

        public struct Symbol
        {
            public string Name;
            public SymbolType Type;      // LOCAL, GLOBAL, EXTERN
            public SectionType Section;  // TEXT, DATA, BSS, ABS
            public int Offset;           // offset within section
        }

        public enum SymbolType { LOCAL = 0, GLOBAL = 1, EXTERN = 2 }
        public enum SectionType { TEXT = 0, DATA = 1, BSS = 2, ABS = 3 }

        public List<Symbol> Symbols = new();

        public struct Relocation
        {
            public int SectionOffset;    // offset within text/data where LIMM immediate lives
            public int SymbolIndex;      // index into Symbols[]
            public RelocationType Type;  // LIMM_ABSOLUTE
        }

        public enum RelocationType { LIMM_ABSOLUTE = 0 }

        public List<Relocation> Relocations = new();

        public void Write(string path)
        {
            using var w = new BinaryWriter(File.OpenWrite(path));
            w.Write(MAGIC);
            w.Write(VERSION);
            w.Write(TextSection.Count);
            w.Write(DataSection.Count);
            w.Write(BssSize);
            w.Write(Symbols.Count);
            w.Write(Relocations.Count);

            foreach (var word in TextSection) w.Write((long)word);
            foreach (var word in DataSection) w.Write((long)word);
            foreach (var sym in Symbols)
            {
                w.Write(sym.Name);
                w.Write((int)sym.Type);
                w.Write((int)sym.Section);
                w.Write(sym.Offset);
            }
            foreach (var rel in Relocations)
            {
                w.Write(rel.SectionOffset);
                w.Write(rel.SymbolIndex);
                w.Write((int)rel.Type);
            }
        }

        public static T3ObjectFile Read(string path)
        {
            using var r = new BinaryReader(File.OpenRead(path));
            var obj = new T3ObjectFile();
            int magic = r.ReadInt32();
            if (magic != MAGIC) throw new Exception($"Invalid object file: bad magic {magic:X8}");
            int ver = r.ReadInt32();
            if (ver != VERSION) throw new Exception($"Unsupported version: {ver}");
            int textCount = r.ReadInt32();
            int dataCount = r.ReadInt32();
            obj.BssSize = r.ReadInt32();
            int symCount = r.ReadInt32();
            int relCount = r.ReadInt32();

            for (int i = 0; i < textCount; i++) obj.TextSection.Add((Int128)r.ReadInt64());
            for (int i = 0; i < dataCount; i++) obj.DataSection.Add((Int128)r.ReadInt64());
            for (int i = 0; i < symCount; i++)
                obj.Symbols.Add(new Symbol { Name = r.ReadString(), Type = (SymbolType)r.ReadInt32(), Section = (SectionType)r.ReadInt32(), Offset = r.ReadInt32() });
            for (int i = 0; i < relCount; i++)
                obj.Relocations.Add(new Relocation { SectionOffset = r.ReadInt32(), SymbolIndex = r.ReadInt32(), Type = (RelocationType)r.ReadInt32() });

            return obj;
        }
    }
}
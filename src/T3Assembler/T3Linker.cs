using System;
using System.Collections.Generic;
using System.Linq;
using TritTypes;

namespace T3Assembler
{
    public class T3Linker
    {
        /// <summary>
        /// Link multiple object files into a single executable image.
        /// Layout: [crt0] [all .text] [all .data] followed by .bss (zero-filled at load)
        /// </summary>
        public List<Int128> Link(List<T3ObjectFile> objects)
        {
            var text = new List<(T3ObjectFile obj, int offset)>();
            var data = new List<(T3ObjectFile obj, int offset)>();
            var globalSymbols = new Dictionary<string, int>(); // name → final address

            int textBase = 0;
            foreach (var obj in objects)
            {
                text.Add((obj, textBase));
                foreach (var sym in obj.Symbols.Where(s => s.Type == T3ObjectFile.SymbolType.GLOBAL && s.Section == T3ObjectFile.SectionType.TEXT))
                    globalSymbols[sym.Name] = textBase + sym.Offset;
                textBase += obj.TextSection.Count;
            }

            int dataBase = textBase;
            foreach (var obj in objects)
            {
                data.Add((obj, dataBase));
                foreach (var sym in obj.Symbols.Where(s => s.Type == T3ObjectFile.SymbolType.GLOBAL && s.Section == T3ObjectFile.SectionType.DATA))
                    globalSymbols[sym.Name] = dataBase + sym.Offset;
                dataBase += obj.DataSection.Count;
            }

            // Build final image: copy all .text then all .data
            var image = new List<Int128>();
            foreach (var (obj, offset) in text) image.AddRange(obj.TextSection);
            foreach (var (obj, offset) in data) image.AddRange(obj.DataSection);

            // Apply relocations
            foreach (var obj in objects)
            {
                foreach (var rel in obj.Relocations)
                {
                    if (rel.Type == T3ObjectFile.RelocationType.LIMM_ABSOLUTE)
                    {
                        if (rel.SymbolIndex < 0 || rel.SymbolIndex >= obj.Symbols.Count) continue;
                        var sym = obj.Symbols[rel.SymbolIndex];
                        if (!globalSymbols.TryGetValue(sym.Name, out int targetAddr))
                            throw new Exception($"Undefined symbol: {sym.Name}");

                        // The LIMM instruction is at rel.SectionOffset, the immediate word follows
                        int imgOffset = rel.SectionOffset + (obj == text.FirstOrDefault(x => x.obj == obj).obj ? text.First(x => x.obj == obj).offset : data.First(x => x.obj == obj).offset);
                        if (imgOffset + 1 >= image.Count) continue;
                        image[imgOffset + 1] = (Int128)targetAddr;
                    }
                }
            }

            return image;
        }

        /// <summary>
        /// Resolve references in a single object file (no linking, just fixup).
        /// </summary>
        public List<Int128> LinkSingle(T3ObjectFile obj)
        {
            return Link(new List<T3ObjectFile> { obj });
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using TritTypes;

namespace T3Assembler
{
    /// <summary>
    /// Linker for T3 object files (.o).
    /// Layout: [all .text] [all .data] [.bss zero-fill at load]
    /// Supports: GLOBAL/EXTERN symbols, LIMM_ABSOLUTE and PC_RELATIVE relocations.
    /// </summary>
    public class T3Linker
    {
        /// <summary>
        /// Link multiple object files into a single executable image.
        /// </summary>
        public List<Int128> Link(List<T3ObjectFile> objects)
        {
            // Phase 1: Collect all symbols and detect duplicates
            var globalSymbols = new Dictionary<string, (int address, T3ObjectFile obj)>(StringComparer.Ordinal);
            var unresolvedExterns = new List<(T3ObjectFile obj, T3ObjectFile.Symbol sym)>();

            int textBase = 0;
            var textOffsets = new Dictionary<T3ObjectFile, int>();
            foreach (var obj in objects)
            {
                textOffsets[obj] = textBase;
                foreach (var sym in obj.Symbols)
                {
                    if (sym.Type == T3ObjectFile.SymbolType.GLOBAL)
                    {
                        int addr = sym.Section == T3ObjectFile.SectionType.TEXT
                            ? textBase + sym.Offset
                            : sym.Offset; // DATA/BSS resolved later
                        if (globalSymbols.TryGetValue(sym.Name, out var existing))
                            throw new Exception($"Duplicate symbol '{sym.Name}' in object files");
                        globalSymbols[sym.Name] = (addr, obj);
                    }
                    else if (sym.Type == T3ObjectFile.SymbolType.EXTERN)
                    {
                        unresolvedExterns.Add((obj, sym));
                    }
                }
                textBase += obj.TextSection.Count;
            }

            // Resolve DATA section bases
            int dataBase = textBase;
            var dataOffsets = new Dictionary<T3ObjectFile, int>();
            foreach (var obj in objects)
            {
                dataOffsets[obj] = dataBase;
                foreach (var sym in obj.Symbols)
                {
                    if (sym.Type == T3ObjectFile.SymbolType.GLOBAL && sym.Section == T3ObjectFile.SectionType.DATA)
                    {
                        int addr = dataBase + sym.Offset;
                        globalSymbols[sym.Name] = (addr, obj);
                    }
                }
                dataBase += obj.DataSection.Count;
            }

            // Resolve BSS section bases
            int bssBase = dataBase;
            var bssOffsets = new Dictionary<T3ObjectFile, int>();
            foreach (var obj in objects)
            {
                bssOffsets[obj] = bssBase;
                foreach (var sym in obj.Symbols)
                {
                    if (sym.Type == T3ObjectFile.SymbolType.GLOBAL && sym.Section == T3ObjectFile.SectionType.BSS)
                    {
                        int addr = bssBase + sym.Offset;
                        globalSymbols[sym.Name] = (addr, obj);
                    }
                }
                bssBase += obj.BssSize;
            }

            // Check unresolved EXTERN symbols
            foreach (var (obj, sym) in unresolvedExterns)
            {
                if (!globalSymbols.ContainsKey(sym.Name))
                    throw new Exception($"Undefined external symbol '{sym.Name}'");
            }

            // Phase 2: Build final image
            var image = new List<Int128>();
            foreach (var obj in objects) image.AddRange(obj.TextSection);
            foreach (var obj in objects) image.AddRange(obj.DataSection);
            // .bss is zero-filled at load time, no need to add words

            // Phase 3: Apply relocations
            foreach (var obj in objects)
            {
                foreach (var rel in obj.Relocations)
                {
                    if (rel.SymbolIndex < 0 || rel.SymbolIndex >= obj.Symbols.Count) continue;
                    var sym = obj.Symbols[rel.SymbolIndex];

                    if (!globalSymbols.TryGetValue(sym.Name, out var target))
                        throw new Exception($"Undefined symbol: {sym.Name}");

                    // Determine base offset for this relocation's section
                    int baseOff = rel.Section switch
                    {
                        T3ObjectFile.SectionType.TEXT => textOffsets[obj],
                        T3ObjectFile.SectionType.DATA => dataOffsets[obj],
                        T3ObjectFile.SectionType.BSS => bssOffsets[obj],
                        _ => throw new Exception($"Unknown section for relocation: {rel.Section}")
                    };

                    int imgOffset = baseOff + rel.SectionOffset;

                    if (rel.Type == T3ObjectFile.RelocationType.LIMM_ABSOLUTE)
                    {
                        // LIMM instruction: the immediate word follows the LIMM opcode
                        if (imgOffset + 1 >= image.Count) continue;
                        image[imgOffset + 1] = (Int128)target.address;
                    }
                    else if (rel.Type == T3ObjectFile.RelocationType.PC_RELATIVE)
                    {
                        // PC-relative: target - (pc + 1) where pc = imgOffset
                        // The LIMM at imgOffset loads the offset, the next word is the immediate
                        if (imgOffset + 1 >= image.Count) continue;
                        int offset = target.address - (imgOffset + 1);
                        image[imgOffset + 1] = (Int128)offset;
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
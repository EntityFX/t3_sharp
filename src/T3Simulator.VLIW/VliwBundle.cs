using System;
using System.Numerics;
using T3Simulator.Common;
using TritTypes;

namespace T3Simulator.VLIW
{
    /// <summary>
    /// Represents a VLIW bundle consisting of three 18-trit slots.
    /// A bundle is packed into a single Word54.
    /// </summary>
    public readonly struct VliwBundle<TWord> where TWord : IT3Word<TWord>
    {
        public readonly VliwSlot<TWord> Slot0;
        public readonly VliwSlot<TWord> Slot1;
        public readonly VliwSlot<TWord> Slot2;

        public VliwBundle(VliwSlot<TWord> s0, VliwSlot<TWord> s1, VliwSlot<TWord> s2)
        {
            Slot0 = s0;
            Slot1 = s1;
            Slot2 = s2;
        }

        /// <summary>
        /// Decodes a Word54 into a VLIW bundle.
        /// </summary>
        public static VliwBundle<TWord> Decode(TWord word)
        {
            // Word is converted to trit string. Each slot is 18 trits.
            // 18 * 3 = 54.
            string s = word.ToTritString();
            
            string s0 = s.Substring(0, 18);
            string s1 = s.Substring(18, 18);
            string s2 = s.Substring(36, 18);

            return new VliwBundle<TWord>(
                new VliwSlot<TWord>(InstructionDecoder.DecodeVliwSlot<TWord>(s0)),
                new VliwSlot<TWord>(InstructionDecoder.DecodeVliwSlot<TWord>(s1)),
                new VliwSlot<TWord>(InstructionDecoder.DecodeVliwSlot<TWord>(s2))
            );
        }
    }
}
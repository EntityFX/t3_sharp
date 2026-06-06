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
    public readonly struct VliwBundle
    {
        public readonly VliwSlot Slot0;
        public readonly VliwSlot Slot1;
        public readonly VliwSlot Slot2;

        public VliwBundle(VliwSlot s0, VliwSlot s1, VliwSlot s2)
        {
            Slot0 = s0;
            Slot1 = s1;
            Slot2 = s2;
        }

        /// <summary>
        /// Decodes a Word54 into a VLIW bundle.
        /// </summary>
        public static VliwBundle Decode(BigInteger word)
        {
            // Word54 is 54 trits. Each slot is 18 trits.
            // 18 * 3 = 54.
            string s = new Word54(word).ToTritString();
            
            string s0 = s.Substring(0, 18);
            string s1 = s.Substring(18, 18);
            string s2 = s.Substring(36, 18);

            return new VliwBundle(
                new VliwSlot(InstructionDecoder.DecodeVliwSlot(s0)),
                new VliwSlot(InstructionDecoder.DecodeVliwSlot(s1)),
                new VliwSlot(InstructionDecoder.DecodeVliwSlot(s2))
            );
        }
    }
}
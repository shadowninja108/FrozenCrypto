using System.Runtime.InteropServices;

namespace FrozenCrypto
{
    public class Types
    {
        [StructLayout(LayoutKind.Explicit, Size = 0x28)]
        public struct SaveHeader
        {
            [FieldOffset(0x00)]
            public uint Version;

            [FieldOffset(0x04)]
            public uint BodySize;

            [FieldOffset(0x08)] 
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public uint[] RandomSeeds;

            [FieldOffset(0x18)]
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x10)]
            public byte[] Cmac;

        }
    }
}

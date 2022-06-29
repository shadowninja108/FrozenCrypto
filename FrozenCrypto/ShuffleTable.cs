namespace FrozenCrypto
{
    public class ShuffleTable
    {
        public static readonly uint[] Table = GenTable();

        /* These constants seem suspiciously...not random? Last few integers of one of Splatoon 2's save shuffle tables. */
        private const uint Seed1 = 0x6508E3B3;
        private const uint Seed2 = 0xF13D7C92;
        private const uint Seed3 = 0xD7DA45D4;
        private const uint Seed4 = 0xA01D9485;

        private static uint[] GenTable()
        {
            uint[] table = new uint[64];
            var seed1 = Seed1;
            var seed2 = Seed2;
            var seed3 = Seed3;
            var seed4 = Seed4;

            for (int i = 0; i < table.Length; i++)
            {
                var x = seed1 ^ (seed1 << 11);
                seed1 = seed2;
                seed2 = seed3;
                seed3 = seed4;
                seed4 ^= (seed4 >> 19) ^ x ^ (x >> 8);
                table[i] = seed4;
            }

            return table;
        }
    }
}

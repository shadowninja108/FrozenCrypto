namespace FrozenCrypto
{
    public class SeadRandom
    {
        private readonly uint[] State;

        public SeadRandom(uint seed)
        {
            State = new uint[4];
            State[0] = 1812433253 * (seed ^ (seed >> 30)) + 1;
            State[1] = 1812433253 * (State[0] ^ (State[0] >> 30)) + 2;
            State[2] = 1812433253 * (State[1] ^ (State[1] >> 30)) + 3;
            State[3] = 1812433253 * (State[2] ^ (State[2] >> 30)) + 4;
        }

        public SeadRandom(uint seedOne, uint seedTwo, uint seedThree, uint seedFour)
        {
            State = new uint[] { seedOne, seedTwo, seedThree, seedFour };
        }

        public uint GetUInt32()
        {
            uint v1;
            uint v2;
            uint v3;

            v1 = State[0] ^ (State[0] << 11);
            State[0] = State[1];
            v2 = State[3];
            v3 = v1 ^ (v1 >> 8) ^ v2 ^ (v2 >> 19);
            State[1] = State[2];
            State[2] = v2;
            State[3] = v3;

            return v3;
        }

        /* Blitz moment. */
        public uint[] GenKey()
        {
            var key = new uint[4];

            /* Iterate through each uint. */
            for (var i = 0; i < 4; i++)
            {
                /* Iterate through each byte in the uint. */
                for (var j = 0; j < 4; j++)
                {
                    /* Shift bytes up. */
                    key[i] <<= 8;
                    /* Write next byte. */
                    key[i] |= (ShuffleTable.Table[GetUInt32() >> 26] >> ((int)(GetUInt32() >> 27) & 0x18)) & byte.MaxValue;
                }
            }

            return key;
        }

        public void GetContext(out uint seedOne, out uint seedTwo, out uint seedThree, out uint seedFour)
        {
            seedOne = State[0];
            seedTwo = State[1];
            seedThree = State[2];
            seedFour = State[3];
        }

    }
}

using System.IO;
using System.Text;

namespace LiquidTechnologies.FastInfoset
{
    /// <summary>
    /// Implementation of Fast Infoset Restricted Alphabet [X.891 Section 8.2].
    /// </summary>
    internal class FIRestrictedAlphabet : FIEncoding
    {
        /// <summary>
        /// Creates an instance of FIRestrictedAlphabet using the specified characters.
        /// </summary>
        /// <param name="alphabetChars">Characters that form the Restricted Alphabet.</param>
        /// <remarks><para>Each entry in the restricted alphabet table shall be an ordered set of distinct ISO/IEC 10646 characters of any size between 2 and 220 characters.</para><para>NOTE – A restricted alphabet permits a compact encoding of any character string consisting entirely of characters from that set, through the assignment of progressive integers to the characters in the set and the use of those integers to encode the characters of the string [X.891 Section 7.17.6].</para></remarks>
        internal FIRestrictedAlphabet(string alphabetChars)
        {
            _alphabetChars = alphabetChars;
        }

        public override byte[] Encode(object data)
        {
            string strVal = data as string;

            if (string.IsNullOrEmpty(strVal)) return null;

            int alphabetCount = AlphabetChars.Length;

            // 8.2.2
            if (alphabetCount < 2 || alphabetCount > FIConsts.TWO_POWER_TWENTY)
                throw new LtFastInfosetException(
                    "Failed to write FastInfoset. Invalid Restricted Alphabet. Alphabet must contain between 2 and 2^20 characters.");

            // see how many bits we need per char
            int bits = 2;
            while ((1 << bits) <= alphabetCount) bits++;

            // populate memory buffer withe encoded bytes
            MemoryStream buffer = new MemoryStream();
            int len = strVal.Length;

            if (bits == 8) {
                // easy 1 to 1 mapping

                int n = 0;
                while (n < len) {
                    char c = strVal[n++];
                    int pos = AlphabetChars.IndexOf(c);
                    if (pos == -1)
                        throw new LtFastInfosetException(
                            "Failed to write FastInfoset. Character not found in Restricted Alphabet [" + c + "].");

                    buffer.WriteByte((byte) AlphabetChars[pos]);
                }
            }
            else if (bits == 4) {
                // semi-easy 2 to 1 mapping

                int n = 0;
                while (n < len) {
                    char c = strVal[n++];
                    int pos = AlphabetChars.IndexOf(c);
                    if (pos == -1)
                        throw new LtFastInfosetException(
                            "Failed to write FastInfoset. Character not found in Restricted Alphabet [" + c + "].");

                    if (n == len) {
                        // fill last 4 bits with terminator value 1111
                        buffer.WriteByte((byte) ((AlphabetChars[pos] << 4) | 0xF));
                    }
                    else {
                        char c2 = strVal[n++];
                        int pos2 = AlphabetChars.IndexOf(c2);
                        if (pos2 == -1)
                            throw new LtFastInfosetException(
                                "Failed to write FastInfoset. Character not found in Restricted Alphabet [" + c2 +
                                "].");

                        buffer.WriteByte((byte) ((AlphabetChars[pos] << 4) | AlphabetChars[pos2]));
                    }
                }
            }
            else {
                // tricky arbitry mapping
                throw new LtFastInfosetException(
                    "Failed to write FastInfoset. Unsupported Feature in FIRestrictedAlphabet Encode.");
            }

            return buffer.ToArray();
        }

        public override string Decode(byte[] data)
        {
            int alphabetCount = AlphabetChars.Length;

            // 8.2.2
            if (alphabetCount < 2 || alphabetCount > FIConsts.TWO_POWER_TWENTY)
                throw new LtFastInfosetException(
                    "Failed to parse FastInfoset. Invalid Encoding of Restricted Alphabet. Alphabet must contain between 2 and 2^20 characters.");

            // see how many bits we need per char
            int bits = 2;
            while ((1 << bits) <= alphabetCount) bits++;

            // populate string buffer withe decoded chars
            StringBuilder buffer = null;
            int len = data.Length;

            if (bits == 8) {
                // easy 1 to 1 mapping

                buffer = new StringBuilder(len);

                for (int n = 0; n < len; n++) {
                    buffer[n] = AlphabetChars[data[n]];
                }
            }
            else if (bits == 4) {
                // semi-easy 2 to 1 mapping

                buffer = new StringBuilder(len * 2);

                int nChars = 0;
                for (int n = 0; n < len; n++) {
                    buffer[nChars++] = AlphabetChars[data[n] >> 4];

                    // check for terminator xxxx1111
                    if ((data[n] & 0xF) == 0xF) break;

                    buffer[nChars++] = AlphabetChars[data[n] & 0xF];
                }
            }
            else {
                // tricky arbitry mapping

                // if we can fit an additional char in the last octet then
                // the char bits are all set to 1
                int terminator = (1 << bits) - 1;

                // see how many chars there are
                int charCount = (len * 8) / bits;

                // populate buffer from stream
                buffer = new StringBuilder(charCount);

                int pos = 0;
                int bitsLen = bits;
                int bitsRemainder = 8;
                byte bitsLastVal = data[pos++];

                for (int bufferOffset = 0; bufferOffset < charCount; bufferOffset++) {
                    int offset = bitsLastVal;
                    while (true) {
                        if (bitsRemainder == bitsLen) {
                            bitsRemainder = 0;
                            bitsLastVal = 0;
                            break;
                        }
                        else if (bitsRemainder > bitsLen) {
                            bitsRemainder -= bitsLen;
                            bitsLastVal &= FIConsts.BIT_MASKS[bitsRemainder];
                            offset >>= bitsRemainder;
                            break;
                        }
                        else {
                            offset <<= 8;
                            offset |= data[pos++];
                            bitsRemainder = 8;
                        }
                    }

                    if (offset == terminator) break;

                    buffer[bufferOffset] = AlphabetChars[offset];
                }
            }

            return buffer.ToString();
        }

        internal string AlphabetChars
        {
            get { return _alphabetChars; }
        }

        private string _alphabetChars;
    }
}
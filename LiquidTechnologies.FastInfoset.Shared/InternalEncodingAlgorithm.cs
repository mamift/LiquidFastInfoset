using System;
using System.Globalization;
using System.Text;

namespace LiquidTechnologies.FastInfoset
{
    internal class InternalEncodingAlgorithm : FIEncoding
    {
        internal enum EncodingType
        {
            None,
            HexadecimalEncoding,
            Base64Encoding,
            ShortEncoding,
            IntEncoding,
            LongEncoding,
            BooleanEncoding,
            FloatEncoding,
            DoubleEncoding,
            UUIDEncoding,
            CDataEncoding
        }

        internal InternalEncodingAlgorithm()
        {
            _type = EncodingType.None;
        }

        internal static byte[] ShortEncoding(short s)
        {
            byte[] data = new byte[2];
            data[0] = (byte) (s >> 8);
            data[1] = (byte) (s & 0xFF);
            return data;
        }

        internal static byte[] IntEncoding(int i)
        {
            byte[] data = new byte[4];
            data[0] = (byte) (i >> 24);
            data[1] = (byte) (i >> 16);
            data[2] = (byte) (i >> 8);
            data[3] = (byte) (i & 0xFF);
            return data;
        }

        internal static byte[] LongEncoding(long l)
        {
            byte[] data = new byte[8];
            data[0] = (byte) (l >> 56);
            data[1] = (byte) (l >> 48);
            data[2] = (byte) (l >> 40);
            data[3] = (byte) (l >> 32);
            data[4] = (byte) (l >> 24);
            data[5] = (byte) (l >> 16);
            data[6] = (byte) (l >> 8);
            data[7] = (byte) (l & 0xFF);
            return data;
        }

        internal static byte[] BooleanEncoding(bool b)
        {
            byte[] data = new byte[1];
            // first 4 bits specify number of unused bits
            if (b)
                // 00111000
                data[0] = 0x38;
            else
                // 00110000
                data[0] = 0x30;
            return data;
        }

        internal static byte[] FloatEncoding(float f)
        {
            byte[] data = new byte[4];
            // convert bits into an int
            Int32 i = BitConverter.ToInt32(BitConverter.GetBytes(f), 0);
            data[0] = (byte) (i >> 24);
            data[1] = (byte) (i >> 16);
            data[2] = (byte) (i >> 8);
            data[3] = (byte) (i & 0xFF);
            return data;
        }

        internal static byte[] DoubleEncoding(double d)
        {
            byte[] data = new byte[8];

            // convert bits into an int
            long l = BitConverter.ToInt64(BitConverter.GetBytes(d), 0);
            data[0] = (byte) (l >> 56);
            data[1] = (byte) (l >> 48);
            data[2] = (byte) (l >> 40);
            data[3] = (byte) (l >> 32);
            data[4] = (byte) (l >> 24);
            data[5] = (byte) (l >> 16);
            data[6] = (byte) (l >> 8);
            data[7] = (byte) (l & 0xFF);
            return data;
        }

        internal static String HexDecoding(byte[] data)
        {
            if (data == null) return string.Empty;

            int length = data.Length;

            char[] a = new char[length * 2];
            for (uint i = 0; i < length; i++) {
                a[i * 2] = _hexLookup[(0xF0 & data[i]) >> 4];
                a[i * 2 + 1] = _hexLookup[(0x0F & data[i])];
            }

            return new string(a);
        }

        internal static string ShortDecoding(byte[] data)
        {
            if (data == null) return string.Empty;

            int length = data.Length;

            if ((length % 2) != 0)
                throw new LtFastInfosetException("Invalid value [" + data + "] for SHORT byte encoding.");

            StringBuilder sb = new StringBuilder();

            int dw = 0;
            while (true) {
                sb.Append(Convert.ToInt16((Int16) ((data[dw++] << 8) | data[dw++])).ToString());

                if (dw == length) break;

                sb.Append(" ");
            }

            return sb.ToString();
        }

        internal static string IntDecoding(byte[] data)
        {
            if (data == null) return string.Empty;

            int length = data.Length;

            if ((length % 4) != 0)
                throw new LtFastInfosetException("Invalid value [" + data + "] for INT byte encoding.");

            StringBuilder sb = new StringBuilder();

            int dw = 0;
            while (true) {
                sb.Append(Convert.ToInt32((data[dw++] << 24) | (data[dw++] << 16) | (data[dw++] << 8) | data[dw++])
                    .ToString());

                if (dw == length) break;

                sb.Append(" ");
            }

            return sb.ToString();
        }

        internal static string LongDecoding(byte[] data)
        {
            if (data == null) return string.Empty;

            int length = data.Length;

            if ((length % 8) != 0)
                throw new LtFastInfosetException("Invalid value [" + data + "] for LONG byte encoding.");

            StringBuilder sb = new StringBuilder();

            int dw = 0;
            while (true) {
                Int64 ll = ((Int64) data[dw++] << 56);
                ll |= ((Int64) data[dw++] << 48);
                ll |= ((Int64) data[dw++] << 40);
                ll |= ((Int64) data[dw++] << 32);
                ll |= ((Int64) data[dw++] << 24);
                ll |= ((Int64) data[dw++] << 16);
                ll |= ((Int64) data[dw++] << 8);
                ll |= ((Int64) data[dw++]);

                sb.Append(Convert.ToInt64(ll).ToString());

                if (dw == length) break;

                sb.Append(" ");
            }

            return sb.ToString();
        }

        internal static string BooleanDecoding(byte[] data)
        {
            if (data == null) return string.Empty;

            int length = data.Length;

            StringBuilder sb = new StringBuilder();

            int dw = 0;

            byte by = data[dw++];

            // first 4 bits specify number of unused bits in last byte
            int nLastUnusedBits = ((by & 0xF0) >> 4);
            int nCurrentBit = 3;
            int nUnusedBits = 0;

            while (true) {
                if (dw == length) nUnusedBits = nLastUnusedBits;

                for (; nCurrentBit >= nUnusedBits; nCurrentBit--) {
                    sb.Append((Convert.ToBoolean((by >> nCurrentBit) & 0x1)) ? "true" : "false");
                }

                if (dw == length) break;

                by = data[dw++];

                sb.Append(" ");

                nCurrentBit = 7;
            }

            return sb.ToString();
        }

        internal static string FloatDecoding(byte[] data)
        {
            if (data == null) return string.Empty;

            int length = data.Length;

            if ((length % 4) != 0)
                throw new LtFastInfosetException("Invalid value [" + data + "] for FLOAT byte encoding.");

            StringBuilder sb = new StringBuilder();

            int dw = 0;
            while (true) {
                // get float value into an int
                int n = ((data[dw++] << 24) | (data[dw++] << 16) | (data[dw++] << 8) | data[dw++]);

                // convert bits to float
                sb.Append(BitConverter.ToSingle(BitConverter.GetBytes(n), 0).ToString(NumberFormatInfo.InvariantInfo));

                if (dw == length) break;

                sb.Append(" ");
            }

            return sb.ToString();
        }

        internal static string DoubleDecoding(byte[] data)
        {
            if (data == null) return string.Empty;

            int length = data.Length;

            if ((length % 8) != 0)
                throw new LtFastInfosetException("Invalid value [" + data + "] for DOUBLE byte encoding.");

            StringBuilder sb = new StringBuilder();

            int dw = 0;
            while (true) {
                Int64 ll = ((Int64) data[dw++] << 56);
                ll |= ((Int64) data[dw++] << 48);
                ll |= ((Int64) data[dw++] << 40);
                ll |= ((Int64) data[dw++] << 32);
                ll |= ((Int64) data[dw++] << 24);
                ll |= ((Int64) data[dw++] << 16);
                ll |= ((Int64) data[dw++] << 8);
                ll |= ((Int64) data[dw++]);

                // cast bits to double
                sb.Append(BitConverter.ToDouble(BitConverter.GetBytes(ll), 0).ToString(NumberFormatInfo.InvariantInfo));

                if (dw == length) break;

                sb.Append(" ");
            }

            return sb.ToString();
        }

        internal static string UUIDDecoding(byte[] data)
        {
            if (data == null) return string.Empty;

            int length = data.Length;

            if ((length % 16) != 0)
                throw new LtFastInfosetException("Invalid value [" + data + "] for UUID byte encoding.");

            StringBuilder sb = new StringBuilder();

            byte[] tempBuffer = new byte[16];
            int dw = 0;
            while (true) {
                Buffer.BlockCopy(data, dw, tempBuffer, 0, 16);
                sb.Append(HexDecoding(tempBuffer));
                dw += 16;

                if (dw == length) break;

                sb.Append(" ");
            }

            return sb.ToString();
        }

        public override byte[] Encode(object val)
        {
            byte[] data = null;

            if (val != null) {
                switch (_type) {
                    case EncodingType.HexadecimalEncoding:
                        data = (byte[]) val;
                        break;
                    case EncodingType.Base64Encoding:
                        data = (byte[]) val;
                        break;
                    case EncodingType.ShortEncoding:
                        data = ShortEncoding((short) val);
                        break;
                    case EncodingType.IntEncoding:
                        data = IntEncoding((int) val);
                        break;
                    case EncodingType.LongEncoding:
                        data = LongEncoding((long) val);
                        break;
                    case EncodingType.BooleanEncoding:
                        data = BooleanEncoding((bool) val);
                        break;
                    case EncodingType.FloatEncoding:
                        data = FloatEncoding((float) val);
                        break;
                    case EncodingType.DoubleEncoding:
                        data = DoubleEncoding((double) val);
                        break;
                    case EncodingType.UUIDEncoding:
                        data = (byte[]) val;
                        break;
                    case EncodingType.CDataEncoding:
                        data = System.Text.Encoding.UTF8.GetBytes((string) val);
                        break;
                    default:
                        throw new LtFastInfosetException("Unknown Encoding");
                }
            }

            return data;
        }

        public override string Decode(byte[] data)
        {
            string val = null;

            switch (_type) {
                case EncodingType.HexadecimalEncoding:
                    val = HexDecoding(data);
                    break;
                case EncodingType.Base64Encoding:
                    val = Convert.ToBase64String(data);
                    break;
                case EncodingType.ShortEncoding:
                    val = ShortDecoding(data);
                    break;
                case EncodingType.IntEncoding:
                    val = IntDecoding(data);
                    break;
                case EncodingType.LongEncoding:
                    val = LongDecoding(data);
                    break;
                case EncodingType.BooleanEncoding:
                    val = BooleanDecoding(data);
                    break;
                case EncodingType.FloatEncoding:
                    val = FloatDecoding(data);
                    break;
                case EncodingType.DoubleEncoding:
                    val = DoubleDecoding(data);
                    break;
                case EncodingType.UUIDEncoding:
                    val = UUIDDecoding(data);
                    break;
                case EncodingType.CDataEncoding:
                    // GetString(data) doesn't exist in WindowsCE
                    val = System.Text.Encoding.UTF8.GetString(data, 0, data.Length);
                    break;
                default:
                    throw new LtFastInfosetException("Unknown Encoding");
            }

            return val;
        }

        internal EncodingType Encoding
        {
            get { return _type; }
            set {
                _type = value;
                TableIndex = (int) value;
            }
        }

        protected EncodingType _type;

        private static char[] _hexLookup = {
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F'
        };
    }
}
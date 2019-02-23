using System;
using System.IO;

namespace LiquidTechnologies.FastInfoset
{
    internal sealed partial class FIParser
    {
        internal class StreamBuffer
        {
            internal StreamBuffer(Stream input)
            {
                _buffer = new byte[_blockSize];
                _input = input;
            }

            internal void Close()
            {
                _input.Close();
                _input = null;
                _buffer = null;
            }

            internal void MoveBack(int len)
            {
                // WARNING: this method moves back current buffer offset NOT stream
                if (len > _bufferOffset)
                    throw new LtFastInfosetException(string.Format(
                        "Internal Error in StreamBuffer::MoveBack. Length = {0}, Offset = {1}", len, _bufferOffset));

                _bufferOffset -= len;
            }

            internal byte ReadByte()
            {
                if (_bufferOffset == _bufferSize) ReadStream();

                return _buffer[_bufferOffset++];
            }

            internal byte[] ReadBytes(int len)
            {
                if (len < 1)
                    throw new LtFastInfosetException("Internal Error in StreamBuffer::ReadBytes. Length = " +
                                                     len.ToString());

                byte[] outBuffer = new byte[len];

                int bytesLeftInBuffer = (_bufferSize - _bufferOffset);
                if (bytesLeftInBuffer == 0) {
                    ReadStream();
                    bytesLeftInBuffer = _bufferSize;
                }

                int offset = 0;
                int bytesToCopy = len;

                while (true) {
                    if (bytesLeftInBuffer >= bytesToCopy) {
                        // enough bytes in buffer, so just copy to output buffer
                        Buffer.BlockCopy(_buffer, _bufferOffset, outBuffer, offset, bytesToCopy);
                        _bufferOffset += bytesToCopy;
                        break;
                    }

                    // not enough bytes in buffer, so copy what's left and then get more bytes from stream
                    Buffer.BlockCopy(_buffer, _bufferOffset, outBuffer, offset, bytesLeftInBuffer);
                    offset += bytesLeftInBuffer;
                    bytesToCopy -= bytesLeftInBuffer;

                    ReadStream();
                    bytesLeftInBuffer = _bufferSize;
                }

                return outBuffer;
            }

            private void ReadStream()
            {
                int offset = 0;
                int count = _blockSize;

                do {
                    int bytesRead = _input.Read(_buffer, offset, count);
                    if (bytesRead == 0) break;

                    offset += bytesRead;
                    count -= bytesRead;
                } while (count > 0);

                if (offset == 0) throw new LtFastInfosetException("Unexpected End of File.");

                // may not have enough bytes, so set buffer size to actual number read
                _bufferSize = offset;
                _bufferOffset = 0;
            }

            private Stream _input;
            private int _blockSize = 4096; // 4K
            private byte[] _buffer;
            private int _bufferSize = 0;
            private int _bufferOffset = 0;
        }
    }
}
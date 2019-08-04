namespace UglyToad.PdfPig.Fonts.TrueType
{
    using System;
    using System.IO;

    internal class TrueTypeDataWriter
    {
        private readonly Stream stream = new MemoryStream();
        public long Position => stream.Position;

        public void Write32Fixed(double value)
        {
            var integer = (int) Math.Floor(value);
            var floating = value - integer;
            WriteSignedShort((short)integer);
            WriteUnsignedShort((ushort)(floating * 65536));
        }

        public void WriteSignedShort(short value)
        {
            stream.WriteByte((byte)(value >> 8));
            stream.WriteByte((byte)value);
        }

        public void WriteUnsignedShort(ushort value)
        {
            stream.WriteByte((byte)(value >> 8));
            stream.WriteByte((byte)value);
        }

        public byte[] GetBytes()
        {
            using (var memoryStream = new MemoryStream())
            {
                var replayTo = stream.Position;
                stream.Seek(0, SeekOrigin.Begin);
                stream.CopyTo(memoryStream);
                stream.Seek(replayTo, SeekOrigin.Begin);
                return memoryStream.ToArray();
            }
        }

        public void WriteTag(string tag)
        {
            const int tagLength = 4;
            if (tag == null)
            {
                throw new ArgumentNullException(nameof(tag));
            }

            if (tag.Length != tagLength)
            {
                throw new ArgumentException($"Length of TrueType tags must be 4, got: {tag}.");
            }

            stream.Write(new[]
            {
                (byte)tag[0],
                (byte)tag[1],
                (byte)tag[2],
                (byte)tag[3]
            }, 0, tagLength);
        }

        public void WriteUnsignedInt(uint value)
        {
            var buffer = new byte[4];
            stream.Write(buffer, 0, buffer.Length);
        }

        public void WriteDateTime(DateTime dateTime)
        {
        }
    }
}
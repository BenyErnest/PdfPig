namespace UglyToad.PdfPig.Fonts.TrueType
{
    using System;
    using System.IO;

    internal class TrueTypeDataWriter
    {
        private readonly Stream stream = new MemoryStream();
        
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
    }
}
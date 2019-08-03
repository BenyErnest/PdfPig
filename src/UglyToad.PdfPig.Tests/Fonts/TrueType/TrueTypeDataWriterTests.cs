namespace UglyToad.PdfPig.Tests.Fonts.TrueType
{
    using PdfPig.Fonts.TrueType;
    using Xunit;

    public class TrueTypeDataWriterTests
    {
        [Fact]
        public void WritesFixed32Correctly()
        {
            var writer = new TrueTypeDataWriter();

            writer.Write32Fixed(1);

            var result = writer.GetBytes();

            Assert.Equal(new byte[]{ 0, 1, 0, 0 }, result);
        }
    }
}

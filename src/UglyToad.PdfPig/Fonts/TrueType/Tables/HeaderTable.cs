namespace UglyToad.PdfPig.Fonts.TrueType.Tables
{
    using System;
    using Geometry;

    /// <summary>
    /// The 'head' table contains global information about the font. 
    /// It contains things like as the font version number, the creation and modification dates, revision number and basic typographic data that applies to the font as a whole.
    /// </summary>
    internal class HeaderTable : ITable
    {
        public string Tag => TrueTypeHeaderTable.Head;
        
        public TrueTypeHeaderTable DirectoryTable { get; }

        public decimal Version { get; }

        public decimal Revision { get; }

        public long CheckSumAdjustment { get; }

        public uint MagicNumber { get; }

        public ushort Flags { get; }

        public ushort UnitsPerEm { get; }

        public DateTime Created { get; }

        public DateTime Modified { get; }

        public PdfRectangle Bounds { get; }

        public HeaderMacStyle MacStyle { get; }

        /// <summary>
        /// Smallest readable size in pixels.
        /// </summary>
        public ushort LowestRecommendedPpem { get; }

        public FontDirection FontDirectionHint { get; }

        /// <summary>
        /// 0 for short offsets, 1 for long.
        /// </summary>
        public short IndexToLocFormat { get; }

        /// <summary>
        /// 0 for current format.
        /// </summary>
        public short GlyphDataFormat { get; }

        public HeaderTable(TrueTypeHeaderTable directoryTable, decimal version, decimal revision, long checkSumAdjustment, 
            uint magicNumber, ushort flags, ushort unitsPerEm, 
            DateTime created, DateTime modified, 
            short xMin, short yMin, 
            short xMax, short yMax, 
            int macStyle, 
            ushort lowestRecommendedPpem, 
            short fontDirectionHint, 
            short indexToLocFormat, 
            short glyphDataFormat)
        {
            DirectoryTable = directoryTable;
            Version = version;
            Revision = revision;
            CheckSumAdjustment = checkSumAdjustment;
            MagicNumber = magicNumber;
            Flags = flags;
            UnitsPerEm = unitsPerEm;
            Created = created;
            Modified = modified;
            Bounds = new PdfRectangle(xMin, yMin, xMax, yMax);
            MacStyle = (HeaderMacStyle)macStyle;
            LowestRecommendedPpem = lowestRecommendedPpem;
            FontDirectionHint = (FontDirection)fontDirectionHint;
            IndexToLocFormat = indexToLocFormat;
            GlyphDataFormat = glyphDataFormat;
        }

        public static HeaderTable Load(TrueTypeDataBytes data, TrueTypeHeaderTable table)
        {
            data.Seek(table.Offset);
            var version = data.Read32Fixed();
            var fontRevision = data.Read32Fixed();
            var checkSumAdjustment = data.ReadUnsignedInt();
            var magicNumber = data.ReadUnsignedInt();

            if (magicNumber != 0x5F0F3CF5)
            {
                throw new InvalidOperationException("The magic number for this TrueType font was incorrect. Value was: " + magicNumber);
            }

            var flags = data.ReadUnsignedShort();
            var unitsPerEm = data.ReadUnsignedShort();

            if (unitsPerEm < 16 || unitsPerEm > 16384)
            {
                throw new InvalidOperationException($"The units per em for this TrueType font was incorrect, value should be between 16 and 16384 but found {unitsPerEm} istead.");
            }

            var created = data.ReadInternationalDate();
            var modified = data.ReadInternationalDate();
            var xMin = data.ReadSignedShort();
            var yMin = data.ReadSignedShort();
            var xMax = data.ReadSignedShort();
            var yMax = data.ReadSignedShort();
            var macStyle = data.ReadUnsignedShort();
            var lowestRecPpem = data.ReadUnsignedShort();
            var fontDirectionHint = data.ReadSignedShort();
            var indexToLocFormat = data.ReadSignedShort();
            var glyphDataFormat = data.ReadSignedShort();

            return new HeaderTable(table, (decimal)version, (decimal)fontRevision, checkSumAdjustment,
                magicNumber, flags, unitsPerEm, created, modified,
                xMin, yMin, xMax, yMax, macStyle, lowestRecPpem,
                fontDirectionHint, indexToLocFormat, glyphDataFormat);
        }

        /// <summary>
        /// A value indicating the direction of glyphs in the font.
        /// </summary>
        public enum FontDirection : short
        {
            /// <summary>
            /// Like <see cref="StronglyRightToLeft"/> but also contains neutrals.
            /// </summary>
            StronglyRightToLeftWithNeutrals = -2,
            /// <summary>
            /// Only strongly right to left.
            /// </summary>
            StronglyRightToLeft = -1,
            /// <summary>
            /// Fully mixed directional glyphs.
            /// </summary>
            FullyMixedDirectional = 0,
            /// <summary>
            /// Only strongly left to right.
            /// </summary>
            StronglyLeftToRight = 1,
            /// <summary>
            /// Like <see cref="StronglyLeftToRight"/> but also contains neutrals.
            /// </summary>
            StronglyLeftToRightWithNeutrals = 2
        }

        /// <summary>
        /// The MacStyle of the font (meaning is not documented).
        /// </summary>
        [Flags]
        internal enum HeaderMacStyle : ushort
        {
            /// <summary>
            /// No style set.
            /// </summary>
            None = 0,
            /// <summary>
            /// Bold.
            /// </summary>
            Bold = 1 << 0,
            /// <summary>
            /// Italic.
            /// </summary>
            Italic = 1 << 1,
            /// <summary>
            /// Underline.
            /// </summary>
            Underline = 1 << 2,
            /// <summary>
            /// Outline.
            /// </summary>
            Outline = 1 << 3,
            /// <summary>
            /// Shadow.
            /// </summary>
            Shadow = 1 << 4,
            /// <summary>
            /// Condensed (narrow).
            /// </summary>
            Condensed = 1 << 5,
            /// <summary>
            /// Extended.
            /// </summary>
            Extended = 1 << 6
        }
    }
}

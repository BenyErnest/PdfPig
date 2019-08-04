namespace UglyToad.PdfPig.Fonts.TrueType.Embedding
{
    using System;
    using System.Collections.Generic;
    using Tables;

    internal class TrueTypeSubsetter
    {
        public static IReadOnlyList<byte> Subset(TrueTypeFontProgram font, IReadOnlyList<int> codePoints)
        {
            // Find the corresponding glyph,
            // Keep the normal header
            // Write the glyphs
            // Write the index to location table
            var writer = new TrueTypeDataWriter();

            var numberOfTables = (ushort)6;

            // Required
            var glyf = font.TableRegister.GlyphTable;
            var head = font.TableRegister.HeaderTable;
            var hhea = font.TableRegister.HorizontalHeaderTable;
            var hmtx = font.TableRegister.HorizontalMetricsTable;
            var loca = font.TableRegister.IndexToLocationTable;
            var maxp = font.TableRegister.MaximumProfileTable;

            WriteOffsetTable(font, writer, numberOfTables);

            var tableRecordOffsets = ReserveTableRecordEntries(writer);

            var headerOffset = WriteHeaderTable(head, writer);

            return writer.GetBytes();
        }

        private static void WriteOffsetTable(TrueTypeFontProgram font, TrueTypeDataWriter writer, ushort numberOfTables)
        {
            ushort GetMaxSetBitPosition(ushort value)
            {
                ushort setCount = 0;
                while (value != 0)
                {
                    value >>= 1;
                    setCount++;
                }

                return setCount;
            }
            
            var maxPowerOf2LessThanOrEqualNumberOfTables = 1 << (GetMaxSetBitPosition(numberOfTables) - 1);
            var searchRange = (ushort)(maxPowerOf2LessThanOrEqualNumberOfTables * 16);
            var entrySelector = (ushort)Math.Log(maxPowerOf2LessThanOrEqualNumberOfTables, 2);
            var rangeShift = (ushort)(numberOfTables * 16 - searchRange);

            /*
             * Offset table format:
             * 32 Fixed |   Version
             * ushort   |   Number of Tables
             * ushort   |   Search Range (Power of 2 <= number of tables) * 16
             * ushort   |   Entry Selector Log2(Power of 2 <= number of tables)
             * ushort   |   Range Shift (Number of tables * 16 - Search Range)
             */

            writer.Write32Fixed((double)font.Version);
            writer.WriteUnsignedShort(numberOfTables);
            writer.WriteUnsignedShort(searchRange);
            writer.WriteUnsignedShort(entrySelector);
            writer.WriteUnsignedShort(rangeShift);

        }

        private static TableRecordEntryOffsets ReserveTableRecordEntries(TrueTypeDataWriter writer)
        {
            long ReserveEntry(string tag)
            {
                writer.WriteTag(tag);
                var position = writer.Position;

                // Checksum
                writer.WriteUnsignedInt(0);
                // Offset from beginning of file
                writer.WriteUnsignedInt(0);
                // Length of the table
                writer.WriteUnsignedInt(0);

                return position;
            }

            return new TableRecordEntryOffsets(ReserveEntry("head"),
                ReserveEntry("hhea"),
                ReserveEntry("hmtx"),
                ReserveEntry("loca"),
                ReserveEntry("maxp"),
                ReserveEntry("glyf"));
        }

        private static long WriteHeaderTable(HeaderTable header, TrueTypeDataWriter writer)
        {
            var startsAt = writer.Position;

            // Major version
            writer.WriteUnsignedShort(1);
            // Minor version
            writer.WriteUnsignedShort(0);
            writer.Write32Fixed((double)header.Revision);

            var checksumAdjustmentLocation = writer.Position;
            writer.WriteUnsignedInt(0);

            writer.WriteUnsignedInt(header.MagicNumber);
            writer.WriteUnsignedShort(header.Flags);

            writer.WriteUnsignedShort(header.UnitsPerEm);

            writer.WriteDateTime(header.Created);
            writer.WriteDateTime(header.Modified);

            writer.WriteSignedShort((short)header.Bounds.Left);
            writer.WriteSignedShort((short)header.Bounds.Bottom);
            writer.WriteSignedShort((short)header.Bounds.Right);
            writer.WriteSignedShort((short)header.Bounds.Top);

            writer.WriteUnsignedShort((ushort)header.MacStyle);
            writer.WriteUnsignedShort(header.LowestRecommendedPpem);
            writer.WriteSignedShort((short)header.FontDirectionHint);
            writer.WriteSignedShort(header.IndexToLocFormat);
            writer.WriteSignedShort(header.GlyphDataFormat);

            return startsAt;
        }

        private class TableRecordEntryOffsets
        {
            public long Header { get; }

            public long HorizontalHeader { get; }

            public long HorizontalMetrics { get; }

            public long IndexToLocation { get; }

            public long MaximumProfile { get; }

            public long Glyph { get; }

            public TableRecordEntryOffsets(long header, long horizontalHeader, long horizontalMetrics, 
                long indexToLocation, 
                long maximumProfile, 
                long glyph)
            {
                Header = header;
                HorizontalHeader = horizontalHeader;
                HorizontalMetrics = horizontalMetrics;
                IndexToLocation = indexToLocation;
                MaximumProfile = maximumProfile;
                Glyph = glyph;
            }
        }
    }
}

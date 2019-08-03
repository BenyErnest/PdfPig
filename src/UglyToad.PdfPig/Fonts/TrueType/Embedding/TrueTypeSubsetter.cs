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

        private static void WriteHeadTable(HeaderTable header)
        {

        }
    }
}

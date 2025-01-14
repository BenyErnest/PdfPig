﻿namespace UglyToad.PdfPig.Filters
{
    using System;
    using System.Collections.Generic;
    using Tokens;
    using Util;

    internal class LzwFilter : IFilter
    {
        private const int DefaultColors = 1;
        private const int DefaultBitsPerComponent = 8;
        private const int DefaultColumns = 1;

        private const int ClearTable = 256;
        private const int EodMarker = 257;

        private const int NineBitBoundary = 511;
        private const int TenBitBoundary = 1023;
        private const int ElevenBitBoundary = 2047;

        private readonly IDecodeParameterResolver decodeParameterResolver;
        private readonly IPngPredictor pngPredictor;

        public LzwFilter(IDecodeParameterResolver decodeParameterResolver, IPngPredictor pngPredictor)
        {
            this.decodeParameterResolver = decodeParameterResolver ?? throw new ArgumentNullException(nameof(decodeParameterResolver));
            this.pngPredictor = pngPredictor ?? throw new ArgumentNullException(nameof(pngPredictor));
        }

        public byte[] Decode(IReadOnlyList<byte> input, DictionaryToken streamDictionary, int filterIndex)
        {
            var parameters = decodeParameterResolver.GetFilterParameters(streamDictionary, filterIndex);

            var predictor = parameters.GetIntOrDefault(NameToken.Predictor, -1);

            var earlyChange = parameters.GetIntOrDefault(NameToken.EarlyChange, 1);

            if (predictor > 1)
            {
                var decompressed = Decode(input, earlyChange == 1);

                var colors = Math.Min(parameters.GetIntOrDefault(NameToken.Colors, DefaultColors), 32);
                var bitsPerComponent = parameters.GetIntOrDefault(NameToken.BitsPerComponent, DefaultBitsPerComponent);
                var columns = parameters.GetIntOrDefault(NameToken.Columns, DefaultColumns);

                var result = pngPredictor.Decode(decompressed, predictor, colors, bitsPerComponent, columns);

                return result;
            }

            var data = Decode(input, earlyChange == 1);

            return data;
        }

        private static byte[] Decode(IReadOnlyList<byte> input, bool isEarlyChange)
        {
            var result = new List<byte>();

            var table = GetDefaultTable();

            var codeBits = 9;

            var data = new BitStream(input);

            var codeOffset = isEarlyChange ? 0 : 1;

            var previous = -1;

            while (true)
            {
                var next = data.Get(codeBits);

                if (next == EodMarker)
                {
                    break;
                }
                
                if (next == ClearTable)
                {
                    table = GetDefaultTable();
                    previous = -1;
                    codeBits = 9;
                    continue;
                }

                if (table.TryGetValue(next, out var b))
                {
                    result.AddRange(b);

                    if (previous >= 0)
                    {
                        var lastSequence = table[previous];

                        var newSequence = new byte[lastSequence.Length + 1];

                        Array.Copy(lastSequence, newSequence, lastSequence.Length);

                        newSequence[lastSequence.Length] = b[0];

                        table[table.Count] = newSequence;
                    }
                }
                else
                {
                    var lastSequence = table[previous];

                    var newSequence = new byte[lastSequence.Length + 1];

                    Array.Copy(lastSequence, newSequence, lastSequence.Length);

                    newSequence[lastSequence.Length] = lastSequence[0];

                    result.AddRange(newSequence);

                    table[table.Count] = newSequence;
                }
                
                previous = next;

                if (table.Count >= ElevenBitBoundary + codeOffset)
                {
                    codeBits = 12;
                }
                else if (table.Count >= TenBitBoundary + codeOffset)
                {
                    codeBits = 11;
                }
                else if (table.Count >= NineBitBoundary + codeOffset)
                {
                    codeBits = 10;
                }
                else
                {
                    codeBits = 9;
                }
            }

            return result.ToArray();
        }

        private static Dictionary<int, byte[]> GetDefaultTable()
        {
            var table = new Dictionary<int, byte[]>();

            for (var i = 0; i < 256; i++)
            {
                table[i] = new[] { (byte)i };
            }

            table[ClearTable] = null;
            table[EodMarker] = null;

            return table;
        }
    }
}
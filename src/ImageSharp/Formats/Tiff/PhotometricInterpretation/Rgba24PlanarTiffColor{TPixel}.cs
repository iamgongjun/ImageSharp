// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Buffers;
using SixLabors.ImageSharp.Formats.Tiff.Utils;
using SixLabors.ImageSharp.Memory;
using SixLabors.ImageSharp.PixelFormats;

namespace SixLabors.ImageSharp.Formats.Tiff.PhotometricInterpretation
{
    /// <summary>
    /// Implements the 'RGB' photometric interpretation with an alpha channel and with 'Planar' layout for each color channel with 24 bit.
    /// </summary>
    internal class Rgba24PlanarTiffColor<TPixel> : TiffBasePlanarColorDecoder<TPixel>
        where TPixel : unmanaged, IPixel<TPixel>
    {
        private readonly bool isBigEndian;

        private readonly TiffExtraSampleType? extraSamplesType;

        /// <summary>
        /// Initializes a new instance of the <see cref="Rgba24PlanarTiffColor{TPixel}" /> class.
        /// </summary>
        /// <param name="extraSamplesType">The extra samples type.</param>
        /// <param name="isBigEndian">if set to <c>true</c> decodes the pixel data as big endian, otherwise as little endian.</param>
        public Rgba24PlanarTiffColor(TiffExtraSampleType? extraSamplesType, bool isBigEndian)
        {
            this.extraSamplesType = extraSamplesType;
            this.isBigEndian = isBigEndian;
        }

        /// <inheritdoc/>
        public override void Decode(IMemoryOwner<byte>[] data, Buffer2D<TPixel> pixels, int left, int top, int width, int height)
        {
            // Note: due to an issue with netcore 2.1 and default values and unpredictable behavior with those,
            // we define our own defaults as a workaround. See: https://github.com/dotnet/runtime/issues/55623
            var color = default(TPixel);
            color.FromScaledVector4(TiffUtils.Vector4Default);
            Span<byte> buffer = stackalloc byte[4];
            int bufferStartIdx = this.isBigEndian ? 1 : 0;

            Span<byte> redData = data[0].GetSpan();
            Span<byte> greenData = data[1].GetSpan();
            Span<byte> blueData = data[2].GetSpan();
            Span<byte> alphaData = data[3].GetSpan();
            Span<byte> bufferSpan = buffer.Slice(bufferStartIdx);

            bool hasAssociatedAlpha = this.extraSamplesType.HasValue && this.extraSamplesType == TiffExtraSampleType.AssociatedAlphaData;
            int offset = 0;
            for (int y = top; y < top + height; y++)
            {
                Span<TPixel> pixelRow = pixels.DangerousGetRowSpan(y).Slice(left, width);
                if (this.isBigEndian)
                {
                    for (int x = 0; x < pixelRow.Length; x++)
                    {
                        redData.Slice(offset, 3).CopyTo(bufferSpan);
                        ulong r = TiffUtils.ConvertToUIntBigEndian(buffer);
                        greenData.Slice(offset, 3).CopyTo(bufferSpan);
                        ulong g = TiffUtils.ConvertToUIntBigEndian(buffer);
                        blueData.Slice(offset, 3).CopyTo(bufferSpan);
                        ulong b = TiffUtils.ConvertToUIntBigEndian(buffer);
                        alphaData.Slice(offset, 3).CopyTo(bufferSpan);
                        ulong a = TiffUtils.ConvertToUIntBigEndian(buffer);

                        offset += 3;

                        pixelRow[x] = hasAssociatedAlpha ?
                            TiffUtils.ColorScaleTo24BitPremultiplied(r, g, b, a, color) :
                            TiffUtils.ColorScaleTo24Bit(r, g, b, a, color);
                    }
                }
                else
                {
                    for (int x = 0; x < pixelRow.Length; x++)
                    {
                        redData.Slice(offset, 3).CopyTo(bufferSpan);
                        ulong r = TiffUtils.ConvertToUIntLittleEndian(buffer);
                        greenData.Slice(offset, 3).CopyTo(bufferSpan);
                        ulong g = TiffUtils.ConvertToUIntLittleEndian(buffer);
                        blueData.Slice(offset, 3).CopyTo(bufferSpan);
                        ulong b = TiffUtils.ConvertToUIntLittleEndian(buffer);
                        alphaData.Slice(offset, 3).CopyTo(bufferSpan);
                        ulong a = TiffUtils.ConvertToUIntLittleEndian(buffer);

                        offset += 3;

                        pixelRow[x] = hasAssociatedAlpha ?
                            TiffUtils.ColorScaleTo24BitPremultiplied(r, g, b, a, color) :
                            TiffUtils.ColorScaleTo24Bit(r, g, b, a, color);
                    }
                }
            }
        }
    }
}

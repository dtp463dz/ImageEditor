using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ImageEditor.Modelss
{
    public class ColorAdjuster
    {
        /// Điều chỉnh từng kênh màu RGB riêng biệt
        public static BitmapImage AdjustColorChannels(BitmapImage source, int redAmount, int greenAmount, int blueAmount)
        {
            try
            {
                double redFactor = (redAmount + 100) / 100.0;
                double greenFactor = (greenAmount + 100) / 100.0;
                double blueFactor = (blueAmount + 100) / 100.0;
                WriteableBitmap writeable = new WriteableBitmap(source);
                writeable.Lock();
                unsafe
                {
                    byte* pPixels = (byte*)writeable.BackBuffer;
                    int stride = writeable.BackBufferStride;
                    int width = writeable.PixelWidth;
                    int height = writeable.PixelHeight;

                    for (int y = 0; y < height; y++)
                    {
                        int rowOffset = y * stride;
                        for (int x = 0; x < width; x++)
                        {
                            int offset = rowOffset + x * 4;
                            byte b = pPixels[offset];
                            byte g = pPixels[offset + 1];
                            byte r = pPixels[offset + 2];
                            byte a = pPixels[offset + 3];

                            // Điều chỉnh từng channel
                            b = ClampByte((int)(b * blueFactor));
                            g = ClampByte((int)(g * greenFactor));
                            r = ClampByte((int)(r * redFactor));

                            pPixels[offset] = b;
                            pPixels[offset + 1] = g;
                            pPixels[offset + 2] = r;
                            pPixels[offset + 3] = a;
                        }
                    }
                }
                writeable.Unlock();
                return ConvertWriteableBitmapToImage(writeable);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi điều chỉnh màu: {ex.Message}");
                return source;
            }
        }

        // xoay sắc độ (hue shift)
        // RGB -> HSL -> Rotate H -> HSL ->RGB
        public static BitmapImage AdjustHueShift(BitmapImage source, int hueShift)
        {
            try
            {
                double hueShiftNorm = hueShift / 180.0;
                WriteableBitmap writeable = new WriteableBitmap(source);
                writeable.Lock();
                unsafe
                {
                    byte* pPixels = (byte*)writeable.BackBuffer;
                    int stride = writeable.BackBufferStride;
                    int width = writeable.PixelWidth;
                    int height = writeable.PixelHeight;

                    for (int y = 0; y < height; y++)
                    {
                        int rowOffset = y * stride;
                        for (int x = 0; x < width; x++)
                        {
                            int offset = rowOffset + x * 4;
                            byte b = pPixels[offset];
                            byte g = pPixels[offset + 1];
                            byte r = pPixels[offset + 2];
                            byte a = pPixels[offset + 3];
                            // Chuyển RGB -> HSL
                            ColorConverter.RgbToHsl(r, g, b, out double h, out double s, out double l);
                            // Xoay hue
                            h += hueShiftNorm;
                            if (h > 1.0) h -= 1.0;
                            if (h < 0) h += 1.0;
                            // chuyển lại rgb
                            ColorConverter.HslToRgb(h, s, l, out r, out g, out b);
                            pPixels[offset] = b;
                            pPixels[offset + 1] = g;
                            pPixels[offset + 2] = r;
                            pPixels[offset + 3] = a;
                        }
                    }
                }
                writeable.Unlock();
                return ConvertWriteableBitmapToImage(writeable);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi điều chỉnh màu: {ex.Message}");
                return source;
            }
        }

        private static byte ClampByte(int value)
        {
            if (value < 0) return 0;
            if (value > 255) return 255;
            return (byte)value;
        }

        private static BitmapImage ConvertWriteableBitmapToImage(WriteableBitmap writeableBitmap)
        {
            BitmapImage image = new BitmapImage();
            using (MemoryStream memoryStream = new MemoryStream())
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(writeableBitmap));
                encoder.Save(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);
                image.BeginInit();
                image.StreamSource = memoryStream;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.EndInit();
                image.Freeze();
            }
            return image;
        }
    }
}
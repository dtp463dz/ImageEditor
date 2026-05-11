using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ImageEditor.Modelss
{
    /// <summary>
    /// Xử lý các chỉnh sửa về sáng tối: Brightness, Contrast, Saturation
    /// </summary>
    public class BrightnessAdjuster
    {
        // Thực hiện (implementation) : sáng tối
        // Điều chỉnh độ sáng :
        // Công thức: NewPixel = OldPixel + Amount 
        public static BitmapImage AdjustBrightness(BitmapImage source, int brightnessAmount)
        {
            try
            {
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
                            // bgra format
                            byte b = pPixels[offset];
                            byte g = pPixels[offset + 1];
                            byte r = pPixels[offset + 2];
                            byte a = pPixels[offset + 3];
                            // điều chỉnh độ sáng
                            b = ClampByte(b + brightnessAmount);
                            g = ClampByte(g + brightnessAmount);
                            r = ClampByte(r + brightnessAmount);
                            // ghi lại
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
                MessageBox.Show($"Lỗi điều chỉnh độ sáng: {ex.Message}");
                return source;
            }
        }
        // Điều chỉnh độ tương phản :
        // Công thức: NewPixel = (OldPixel - 128) * Factor + 128
        public static BitmapImage AdjustContrast(BitmapImage source, int contrastAmount)
        {
            try
            {
                double factor = (contrastAmount + 100) / 100.0;
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
                            // bgra format
                            byte b = pPixels[offset];
                            byte g = pPixels[offset + 1];
                            byte r = pPixels[offset + 2];
                            byte a = pPixels[offset + 3];
                            // điều chỉnh độ sáng
                            b = ClampByte((int)((b - 128) * factor + 128));
                            g = ClampByte((int)((g - 128) * factor + 128));
                            r = ClampByte((int)((r - 128) * factor + 128));
                            // ghi lại
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
                MessageBox.Show($"Lỗi điều chỉnh độ tương phản: {ex.Message}");
                return source;
            }
        }
        // Điều chỉnh độ bão hòa :
        // RGB -> HSL -> Điều chỉnh S -> HSL -> RGB
        public static BitmapImage AdjustSaturation(BitmapImage source, int saturationAmount)
        {
            try
            {
                double factor = (saturationAmount + 100) / 100.0;
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
                            // bgra format
                            byte b = pPixels[offset];
                            byte g = pPixels[offset + 1];
                            byte r = pPixels[offset + 2];
                            byte a = pPixels[offset + 3];
                            // chuyển rgb sang hsl
                            ColorConverter.RgbToHsl(r, g, b, out double h, out double s, out double l);
                            // điều chỉnh bão hòa
                            s = Math.Min(s * factor, 1.0);
                            // chuyển lại rgb
                            ColorConverter.HslToRgb(h, s, l, out r, out g, out b);
                            // ghi lại
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
                MessageBox.Show($"Lỗi điều chỉnh độ tương phản: {ex.Message}");
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
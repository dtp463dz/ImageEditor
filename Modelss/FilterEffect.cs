using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media.Imaging;

namespace ImageEditor.Modelss
{
    /// <summary>
    /// Các hiệu ứng filter sẵn có: Grayscale, Sepia, Vintage....
    /// </summary>
    public class FilterEffect
    {
        // 1. Grayscale
        // Chuyển ảnh thành trắng đen: Gray = (R+G+B)/3
        public static BitmapImage ApplyGrayscale(BitmapImage source)
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
                            // Công thức: Gray = (R * 0.3 + G * 0.59 + B * 0.11)
                            byte gray = (byte)(r * 0.3 + g * 0.59 + b * 0.11);
                            pPixels[offset] = gray;
                            pPixels[offset + 1] = gray;
                            pPixels[offset + 2] = gray;
                            pPixels[offset + 3] = a;
                        }
                    }
                }
                writeable.Unlock();
                return ConvertWriteableBitmapToImage(writeable);
            }
            catch
            {
                return source;
            }
        }
        // 2. SEPIA (Màu nâu cổ điển)
        // Hiệu ứng ảnh cũ
        /// NewR = (R * 0.393 + G * 0.769 + B * 0.189)
        /// NewG = (R * 0.349 + G * 0.686 + B * 0.168)
        /// NewB = (R * 0.272 + G * 0.534 + B * 0.131)
        public static BitmapImage ApplySepia(BitmapImage source)
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

                            byte newR = ClampByte((int)(r * 0.393 + g * 0.769 + b * 0.189));
                            byte newG = ClampByte((int)(r * 0.393 + g * 0.769 + b * 0.189));
                            byte newB = ClampByte((int)(r * 0.393 + g * 0.769 + b * 0.189));

                            pPixels[offset] = newB;
                            pPixels[offset + 1] = newG;
                            pPixels[offset + 2] = newR;
                            pPixels[offset + 3] = a;
                        }
                    }
                }
                writeable.Unlock();
                return ConvertWriteableBitmapToImage(writeable);
            }
            catch
            {
                return source;
            }
        }
        // 3. Vintage 
        // Hiệu ứng vintage: Sepia + giảm contrast + tăng brightness
        public static BitmapImage ApplyVintage(BitmapImage source)
        {
            try
            {
                // áp dụng sepia
                BitmapImage sepia = ApplySepia(source);
                // Tăng brightnetss (+20)
                WriteableBitmap writeable = new WriteableBitmap(sepia);
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
                            pPixels[offset] = ClampByte(pPixels[offset] + 20);
                            pPixels[offset + 1] = ClampByte(pPixels[offset + 1] + 20);
                            pPixels[offset + 2] = ClampByte(pPixels[offset + 2] + 20);
                        }
                    }
                }
                writeable.Unlock();
                return ConvertWriteableBitmapToImage(writeable);
            }
            catch
            {
                return source;
            }
        }
        // 4. Bright(tươi sáng)
        // Tươi sáng: Tăng brightness(+30) + tăng saturation (+20)
        public static BitmapImage ApplyBright(BitmapImage source)
        {
            try
            {
                // Tăng brightImage
                BitmapImage brightImage = BrightnessAdjuster.AdjustBrightness(source, 30);
                // tăng saturation
                BitmapImage result = BrightnessAdjuster.AdjustSaturation(source, 20);
                return result;
            }
            catch
            {
                return source;
            }
        }
        //5. Cool tone: Tăng blue channel, giảm red
        public static BitmapImage ApplyCool(BitmapImage source)
        {
            try
            {
                return ColorAdjuster.AdjustColorChannels(source, -15, 0, 30);
            }
            catch
            {
                return source;
            }
        }
        //6. Warm: ấm: màu cam/vàng
        public static BitmapImage ApplyWarm(BitmapImage source)
        {
            try
            {
                return ColorAdjuster.AdjustColorChannels(source, 30, 10, -15);
            }
            catch
            {
                return source;
            }
        }
        // 7.Cinema
        public static BitmapImage ApplyCinema(BitmapImage source)
        {
            try
            {
                // Bước 1: Tăng contrast
                BitmapImage contrast = BrightnessAdjuster.AdjustContrast(source, 20);
                // Bước 2: Thêm blue tone
                BitmapImage cinema = ColorAdjuster.AdjustColorChannels(contrast, 10, 0, 25);
                return cinema;
            }
            catch
            {
                return source;
            }
        } 
        // 8. NOIR: đen trắng đối lập cao
        public static BitmapImage ApplyNoir(BitmapImage source)
        {
            try
            {
                BitmapImage gray = ApplyGrayscale(source);
                BitmapImage noir = BrightnessAdjuster.AdjustContrast(gray, 40);
                return noir;
            }
            catch
            {
                return source;
            }
        }
        // Fade 
        public static BitmapImage ApplyFade(BitmapImage source)
        {
            try
            {
                BitmapImage desaturate = BrightnessAdjuster.AdjustSaturation(source, -30);
                BitmapImage fade = BrightnessAdjuster.AdjustContrast(desaturate, -15);
                return fade;
            }
            catch
            {
                return source;
            }
        }
        // Vivid (rực rỡ, màu sắc đậm đà)
        public static BitmapImage ApplyVivid(BitmapImage source)
        {
            try
            {
                BitmapImage saturate = BrightnessAdjuster.AdjustSaturation(source, 40);
                BitmapImage vivid = BrightnessAdjuster.AdjustContrast(saturate, 25);
                return vivid;
            }
            catch
            {
                return source;
            }
        }
        // hepler
        private static byte ClampByte(int value)
        {
            if (value < 0) return 0;
            if (value > 255) return 255;
            return (byte)value;
        }
        private static BitmapImage ConvertWriteableBitmapToImage(WriteableBitmap writeableBitmap)
        {
            BitmapImage image = new BitmapImage();
            using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream())
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(writeableBitmap));
                encoder.Save(memoryStream);

                memoryStream.Seek(0, System.IO.SeekOrigin.Begin);

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

using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageEditor.Modelss
{
    /// <summary>
    /// Selection Tool với 2 mode:
    ///  1. Rectangle Selection - Chọn vùng hình chữ nhật
    ///  2. Freeform Selection - Chọn vùng tự do
    /// </summary>
    public class SelectionTool
    {
        public enum SelectionMode
        {
            Rectangle, 
            Freeform
        }
        // Rectangle selection
        // Tạo selection hình chữ nhật dựa trên tọa độ
        // Trả về bitmap của vùng được chọn
        public static BitmapImage GetRectangleSelection(BitmapImage source, int x, int y, int width, int height)
        {
            try
            {
                if (source == null || width <= 0 || height <= 0)
                    return null;
                // Đảm bảo coordinates nằm trong ảnh
                int maxWidth = source.PixelWidth;
                int maxHeight = source.PixelHeight;
                if (x < 0) x = 0;
                if (y < 0) y = 0;
                if (x + width > maxWidth) width = maxWidth - y;
                if (y + height > maxHeight) height = maxHeight - y;
                // cắt vùng
                CroppedBitmap cropped = new CroppedBitmap(source, new Int32Rect(x, y, width, height));
                return ConvertCroppedBitmapToImage(cropped);
            }
            catch
            {
                return null;
            }
        }
        // Vẽ rectangle border lên ảnh (hiển thị selection)
        public static BitmapImage DrawRectangleBorder(BitmapImage source, int x, int y, int width, int height, int lineWidth = 2)
        {
            try
            {
                WriteableBitmap writeable = new WriteableBitmap(source);
                writeable.Lock();
                unsafe
                {
                    byte* pPixels = (byte*)writeable.BackBuffer;
                    int stride = writeable.BackBufferStride;
                    int srcWidth = writeable.PixelWidth;
                    int srcHeight = writeable.PixelHeight;

                    if (x < 0) x = 0;
                    if (y < 0) y = 0;
                    if (x + width > srcWidth) width = srcWidth - x;
                    if (y + height > srcHeight) height = srcHeight - y;
                    // draw top line
                    for(int px = x; px < x + width; px++)
                    {
                        for(int line = 0; line < lineWidth; line++)
                        {
                            int py = y + line;
                            if(py >= 0 && py < srcHeight)
                            {
                                int offset = py * stride + px * 4;
                                pPixels[offset] = 0;  // B
                                pPixels[offset + 1] = 255; // G
                                pPixels[offset + 2] = 0; // R
                                pPixels[offset + 3] = 255; // A
                            }
                        }
                    }
                    // draw bottom line
                    for (int px = x; px < x + width; px++)
                    {
                        for(int line = 0; line < lineWidth; line++)
                        {
                            int py = y + height - 1 - line;
                            if(py >= 0 && py < srcHeight)
                            {
                                int offset = py * stride + px * 4;
                                pPixels[offset] = 0;
                                pPixels[offset + 1] = 255;
                                pPixels[offset + 2] = 0;
                                pPixels[offset + 3] = 255;
                            }
                        }
                    }
                    // draw left line
                    for(int py = y; py < y + height; py++)
                    {
                        for(int line = 0; line < lineWidth; line++)
                        {
                            int px = x + line;
                            if(px >= 0 && px < srcWidth && py >= 0 && py < srcHeight)
                            {
                                int offset = py * stride + px * 4;
                                pPixels[offset] = 0;
                                pPixels[offset + 1] = 255;
                                pPixels[offset + 2] = 0;
                                pPixels[offset + 3] = 255;
                            }
                        }
                    }
                    // draw right line
                    for(int py = y; py < y + height; py++)
                    {
                        for(int line = 0; line < lineWidth; line++)
                        {
                            int px = x + width - 1 - line;
                            if(px >= 0 && px < srcWidth && py >= 0 && py < srcHeight)
                            {
                                int offset = py * stride + px * 4;
                                pPixels[offset] = 0;
                                pPixels[offset + 1] = 255;
                                pPixels[offset + 2] = 0;
                                pPixels[offset + 3] = 255;
                            }
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

        // Freeform selection
        // Tạo selection tự do từ danh sách điểm, sử dụng flood fill algorithm để tô vùng
        public static BitmapImage GetFreeformSelection(BitmapImage source, List<System.Windows.Point> points)
        {
            try
            {
                if (source == null || points == null || points.Count < 3)
                    return null;
                // tạo mask bitmap(trắng/đen để đánh dấu vùng chọn)
                WriteableBitmap mask = new WriteableBitmap(source.PixelWidth, source.PixelHeight,
                    source.DpiX, source.DpiY, PixelFormats.Bgra32, null);
                // Vẽ đường viền frreform lên mask
                DrawFreeformPath(mask, points);

                // dùng Flood fill để tô vùng
                FloodFillFreeform(mask, (int)points[0].X, (int)points[0].Y);

                // Copy vùng được chọn từ source ảnh
                return ApplyMaskToImage(source, mask);
            }
            catch
            {
                return null;
            }
        }
        // Vẽ đường viền freeform lên bitmap
        private static void DrawFreeformPath(WriteableBitmap bitmap, List<System.Windows.Point> points)
        {
            try
            {
                bitmap.Lock();
                unsafe
                {
                    byte* pPixels = (byte*)bitmap.BackBuffer;
                    int stride = bitmap.BackBufferStride;
                    // Vẽ đường nối các điểm
                    for(int i = 0; i < points.Count; i++)
                    {
                        DrawLineBetweenPoints(pPixels, stride, points[i], points[i + 1], bitmap.PixelWidth, bitmap.PixelHeight); ;
                    }
                    // Nối điểm cuối với điểm đầu
                    DrawLineBetweenPoints(pPixels, stride, points[points.Count - 1], points[0], bitmap.PixelWidth, bitmap.PixelHeight);
                    bitmap.Unlock();
                }
            }
            catch { }
        }
        // Vẽ đường thẳng giữa 2 điểm (Breseham algorithm vẽ đường thẳng ngắn nhất giữa 2 điểm pixel)
        private static unsafe void DrawLineBetweenPoints(byte* pPixels, int stride, System.Windows.Point p1, System.Windows.Point p2, int width, int height)
        {
            int x0 = (int)p1.X;
            int y0 = (int)p1.Y;
            int x1 = (int)p2.X;
            int y1 = (int)p2.Y;

            int dx = Math.Abs(x1 - x0);
            int dy = Math.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;

            while (true)
            {
                if(x0 >= 0 && x0 < width && y0 >= 0 && y0 < height)
                {
                    int offset = y0 * stride + x0 * 4;
                    pPixels[offset] = 0;      // B
                    pPixels[offset + 1] = 0; // G
                    pPixels[offset + 2] = 0; // R (Black)
                    pPixels[offset + 3] = 255; // A
                }
                if (x0 == x1 && y0 == y1) break;
                int e2 = 2 * err;
                if(e2 > -dy) { err -= dy; x0 += sx; }
                if(e2 < dx) { err += dx; y0 += sy; }
            }
        }

        // Flood fill từ 1 điểm (tô màu vùng được bao quanh)
        private static void FloodFillFreeform(WriteableBitmap bitmap, int startX, int startY)
        {
            try
            {
                bitmap.Lock();
                unsafe
                {
                    byte* pPixels = (byte*)bitmap.BackBuffer;
                    int stride = bitmap.BackBufferStride;
                    int width = bitmap.PixelWidth;
                    int height = bitmap.PixelHeight;

                    Queue<System.Windows.Point> queue = new Queue<System.Windows.Point>();
                    queue.Enqueue(new System.Windows.Point(startX, startY));
                    
                    while(queue.Count > 0)
                    {
                        System.Windows.Point p = queue.Dequeue();
                        int x = (int)p.X;
                        int y = (int)p.Y;
                        if (x < 0 || x >= width || y < 0 || y >= height)
                            continue;
                        int offset = y * stride + x * 4;
                        byte b = pPixels[offset];
                        // Nếu pixel chưa được tô (không phải đen/không phải trắng)
                        if(b != 255) // 255 = đã tô, 0 = border
                        {
                            pPixels[offset] = 255;
                            pPixels[offset + 1] = 255;
                            pPixels[offset + 2] = 255;
                            pPixels[offset + 3] = 255;
                            // thêm các điểm lân cận
                            queue.Enqueue(new System.Windows.Point(x + 1, y));
                            queue.Enqueue(new System.Windows.Point(x - 1, y));
                            queue.Enqueue(new System.Windows.Point(x, y + 1));
                            queue.Enqueue(new System.Windows.Point(x, y - 1));
                        }
                    }
                }
                bitmap.Unlock();
            }
            catch { }
        }

        // Áp dụng mask lên ảnh gốc
        // trả về ảnh chỉ chứa vùng được chọn
        private static BitmapImage ApplyMaskToImage(BitmapImage source, WriteableBitmap mask)
        {
            try
            {
                WriteableBitmap result = new WriteableBitmap(source);
                result.Lock();
                mask.Lock();
                unsafe
                {
                    byte* pResult = (byte*)result.BackBuffer;
                    byte* pMask = (byte*)mask.BackBuffer;
                    int stride = result.BackBufferStride;
                    int width = result.PixelWidth;
                    int height = result.PixelHeight;
                    for(int y = 0; y < height; y++)
                    {
                        int rowOffset = y * stride;
                        for(int x = 0; x < width; x++)
                        {
                            int offset = rowOffset + x * 4;
                            byte maskR = pMask[offset + 2];
                            // nếu mask là đen (0), đặt pixel thành trong suốt
                            if(maskR < 128)
                            {
                                pResult[offset + 3] = 0;
                            }
                        }
                    }
                }
                result.Unlock();
                mask.Unlock();
                return ConvertWriteableBitmapToImage(result);
            }
            catch
            {
                return source;
            }
        }

        // Copy/paste clipboard
        public static void CopySelectionToClipboard(BitmapImage selectedRegion)
        {
            try
            {
                if (selectedRegion != null)
                {
                    System.Windows.IDataObject dataObject = new System.Windows.DataObject();
                    dataObject.SetData(System.Windows.DataFormats.Bitmap, selectedRegion);
                    System.Windows.Clipboard.SetDataObject(dataObject);
                }
            }
            catch { }
        }
        /// Paste từ clipboard
        /// Trả về ảnh từ clipboard nếu có
        public static BitmapImage PasteFromClipboard()
        {
            try
            {
                System.Windows.IDataObject dataObject = System.Windows.Clipboard.GetDataObject();
                if (dataObject != null && dataObject.GetDataPresent(System.Windows.DataFormats.Bitmap))
                {
                    object bitmapObject = dataObject.GetData(System.Windows.DataFormats.Bitmap);
                    if (bitmapObject is BitmapSource bitmapSource)
                    {
                        BitmapImage image = new BitmapImage();
                        using (var memoryStream = new System.IO.MemoryStream())
                        {
                            var encoder = new PngBitmapEncoder();
                            encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
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
            catch { }
            return null;
        }

        // Helpers
        private static BitmapImage ConvertCroppedBitmapToImage(CroppedBitmap croppedBitmap)
        {
            BitmapImage image = new BitmapImage();
            // Tạo bộ nhớ ram
            using (MemoryStream memoryStream = new MemoryStream())
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(croppedBitmap));
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
        private static BitmapImage ConvertWriteableBitmapToImage(WriteableBitmap writeableBitmap)
        {
            BitmapImage image = new BitmapImage();
            // Tạo bộ nhớ ram
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

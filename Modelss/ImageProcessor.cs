using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageEditor.Modelss
{
    /// <summary>
    /// Xử lý các chức năng: Load, Delete, Crop, Rotate, Save
    /// </summary>
    public class ImageProcessor
    {
        public static BitmapImage LoadImage()
        {
            try
            {
                // tạo dialog để chọn ảnh
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Image Files (*.jpg, *.jpeg, *.png, *.bmp, *.gif)|*.jpg;*.jpeg;*.png;*.bmp;*.gif|" +
                    "All Files (*.*)|*.*";
                openFileDialog.Title = "Chọn ảnh để mở";
                // hiển thị dialog
                if (openFileDialog.ShowDialog() == true)
                {
                    return LoadImageFromPath(openFileDialog.FileName);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi tải ảnh: {ex.Message}");
            }
            return null;
        }

        public static BitmapImage LoadImageFromPath(string imagePath)
        {
            try
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(imagePath, UriKind.Absolute); // đặt nguồn ảnh từ file path
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.DecodePixelWidth = 2048;
                bitmap.EndInit();
                return bitmap;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi tải ảnh từ đường dẫn: {ex.Message}");
            }
        }

        // crop theo tọa độ pixel
        public static BitmapImage CropImage(BitmapImage source, int x, int y, int width, int height)
        {
            try
            {
                // kiểm tra ảnh có tồn tại
                if (source == null || width <= 0 || height <= 0 || x < 0 || y < 0)
                    throw new Exception("Giá trị không hợp lệ");
                
                // đảm bảo vùng crop ko cắt vượt quá kích thước ảnh
                int maxWidth = source.PixelWidth;
                int maxHeight = source.PixelHeight;
                if (x + width > maxWidth)
                    width = maxWidth - x;
                if (y + height > maxHeight)
                    height = maxHeight - y;

                // cập nhật ảnh sau khi được chuyển đổi
                CroppedBitmap croppedBitmap = new CroppedBitmap(source, new System.Windows.Int32Rect(x, y, width, height));
                return ConvertBitmapSourceToBitmapImage(croppedBitmap);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi cắt ảnh: {ex.Message}");
            }
        }
        // crop theo phần trăm
        public static BitmapImage CropImageByPercentage(BitmapImage source, double leftPercent, double topPercent, double widthPercent, double heightPercent)
        {
            try
            {
                if (source == null)
                    throw new Exception("Không có ảnh để thực hiện");

                // Tính toán tọa độ và kích thước theo phần trăm
                int x = (int)(source.PixelWidth * leftPercent / 100);
                int y = (int)(source.PixelHeight * topPercent / 100);
                int width = (int)(source.PixelWidth * widthPercent / 100);
                int height = (int)(source.PixelHeight * heightPercent / 100);
                return CropImage(source, x, y, width, height);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi cắt theo phần trăm: {ex.Message}");
            }
        }
        // rotage ảnh (0, 90, 180, 270)
        public static BitmapImage RotateImage(BitmapImage source, int angle)
        {
            try
            {
                if (source == null)
                    throw new Exception("Không có ảnh để thực hiện");

                angle = angle % 360;
                if (angle < 0) angle += 360;
                if(angle % 90 != 0)
                    throw new Exception("Góc xoay phải là bội số của 90 độ");
                WriteableBitmap writeable = new WriteableBitmap(source);

                // sử dụng TransformesBitmap: dựa trên góc quay
                TransformedBitmap transformedBitmap = new TransformedBitmap();
                transformedBitmap.BeginInit();
                transformedBitmap.Source = writeable;

                switch (angle)
                {
                    case 90:
                        transformedBitmap.Transform = new System.Windows.Media.RotateTransform(90);
                        break;
                    case 180:
                        transformedBitmap.Transform = new System.Windows.Media.RotateTransform(180);
                        break;
                    case 270:
                        transformedBitmap.Transform = new System.Windows.Media.RotateTransform(270);
                        break;
                    case 0:
                    default:
                        return source; // ko quay
                }
                transformedBitmap.EndInit();

                return ConvertTransformedBitmapToImage(transformedBitmap);
                
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi xoay ảnh: {ex.Message}");
            }
        }

        // Flip Image
        public static BitmapImage FlipImage(BitmapImage source, bool isHorizontal)
        {
            try
            {
                if (source == null)
                    throw new Exception("Không có ảnh để thực hiện");

                // khởi tạo
                TransformedBitmap flippedBitmap = new TransformedBitmap();
                flippedBitmap.BeginInit();
                flippedBitmap.Source = source;
                // logic lật ảnh bằng ScaleTransform
                // ScaleX = -1: Lật ngang, ScaleY = -1: Lật dọc
                if (isHorizontal)
                    flippedBitmap.Transform = new ScaleTransform(-1, 1, 0.5, 0.5);
                else
                    flippedBitmap.Transform = new ScaleTransform(1, -1, 0.5, 0.5);
                flippedBitmap.EndInit();
                flippedBitmap.Freeze();
                // chuyển đổi và cập nhật UI
                return source = ConvertFlippedBitmapToImage(flippedBitmap);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lật ảnh: {ex.Message}");
            }
        }

        // Lưu ảnh
        public static void SaveImage(BitmapImage image)
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "PNG Image (*.png)|*.png|" +
                    "JPEG Image (*.jpg)|*.jpg|" +
                    "BMP Image (*.bmp)|*.bmp|" +
                    "All Files (*.*)|*.*";
                saveFileDialog.Title = "Lưu ảnh";
                saveFileDialog.DefaultExt = "png";
                if (saveFileDialog.ShowDialog() == true)
                {
                    string filePath = saveFileDialog.FileName;
                    SaveImageToFile(image, filePath);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lưu ảnh: {ex.Message}");
            }
        }
        public static void SaveImageToFile(BitmapImage image, string filePath)
        {
            try
            {
                BitmapEncoder encoder;
                string extension = Path.GetExtension(filePath).ToLower();
                switch (extension)
                {
                    case ".jpg":
                    case ".jpeg":
                        encoder = new JpegBitmapEncoder { QualityLevel = 95 };
                        break;
                    case ".bmp":
                        encoder = new BmpBitmapEncoder();
                        break;
                    case ".png":
                    default:
                        encoder = new PngBitmapEncoder();
                        break;
                }
                encoder.Frames.Add(BitmapFrame.Create(image));
                using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    encoder.Save(fileStream);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lưu file: {ex.Message}");
            }
        }
        // Helpers
        private static BitmapImage ConvertBitmapSourceToBitmapImage(CroppedBitmap croppedBitmap)
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
                image.CacheOption = BitmapCacheOption.OnLoad; // QUAN TRỌNG: đặt trước
                image.EndInit();
                image.Freeze();
            }
            return image;
        }
        private static BitmapImage ConvertTransformedBitmapToImage(TransformedBitmap transformedBitmap)
        {
            BitmapImage image = new BitmapImage();
            // Tạo bộ nhớ ram
            using (MemoryStream memoryStream = new MemoryStream())
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(transformedBitmap));
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
        private static BitmapImage ConvertFlippedBitmapToImage(TransformedBitmap flippedBitmap)
        {
            BitmapImage image = new BitmapImage();
            // Tạo bộ nhớ ram
            using (MemoryStream memoryStream = new MemoryStream())
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(flippedBitmap));
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

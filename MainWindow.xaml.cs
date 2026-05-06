using System;
using System.Windows;
using System.Windows.Media.Imaging;
using System.IO;
using Microsoft.Win32;

namespace ImageEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        // Lưu ảnh gốc
        private BitmapImage originalImage = null;
        // lưu ảnh hiện tại đang hiển thị
        private BitmapImage displayImage = null;

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new ImageViewModel { DisplayImageSource = null };
        }

        // Event handler từ giao diện
        private void BtnOpenImage_Click(object sender, RoutedEventArgs e)
        {
            LoadImage();
        }
        private void BtnSaveImage_Click(object sender, RoutedEventArgs e)
        {
            SaveImage();
        }
        private void BtnDeleteImage_Click(object sender, RoutedEventArgs e)
        {
            DeleteImage();
        }
        private void BtnCropImage_Click(object sender, RoutedEventArgs e)
        {
            CropImage(100, 100, 400, 300);
        }
        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            ResetToOriginal();
        }

        // Menu item event handler
        private void MenuItem_OpenImage(object sender, RoutedEventArgs e)
        {
            LoadImage();
        }
        private void MenuItem_SaveImage(object sender, RoutedEventArgs e)
        {
            SaveImage();
        }
        private void MenuItem_Exit(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void MenuItem_DeleteImage(object sender, RoutedEventArgs e)
        {
            DeleteImage();
        }
        private void MenuItem_CropImage(object sender, RoutedEventArgs e)
        {
            CropImage(100,100,400,300);
        }
        private void MenuItem_CropPercentImage(object sender, RoutedEventArgs e)
        {
            CropImageByPercentage(10, 10, 80, 80);
        }
        private void MenuItem_ResetImage(object sender, RoutedEventArgs e)
        {
            ResetToOriginal();
        }
        // Hiển thị ảnh
        public void LoadImage()
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
                    string imagePath = openFileDialog.FileName;
                    // đọc ảnh từ file
                    LoadImageFromPath(imagePath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi mở ảnh: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Hàm phụ: đọc ảnh từ đường dẫn và hiển thị
        private void LoadImageFromPath(string imagePath)
        {
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            {
                bitmap.UriSource = new Uri(imagePath, UriKind.Absolute); // đặt nguồn ảnh từ file path
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.DecodePixelWidth = 2048;
            }
            bitmap.EndInit();
            originalImage = bitmap; // lưu bản gốc
            displayImage = bitmap; // lưu ảnh hiện tại bằng bản gốc
            // Hiên thị ảnh lên giao diện
            UpdateDisplayImage(displayImage);
            UpdateStatusBar();
        }

        // Cập nhật hiển thị ảnh trên giao diện
        private void UpdateDisplayImage(BitmapImage image)
        {
            if(image != null)
            {
                this.DataContext = new ImageViewModel
                {
                    DisplayImageSource = image,
                    ImagePreview = image
                };
            }
        }
        private void UpdateStatusBar()
        {
            if(displayImage != null)
            {
                StatusText.Text = $"{displayImage.PixelWidth} x {displayImage.PixelHeight} px";
            }
            else
            {
                StatusText.Text = "Chưa tải ảnh";
            }
        }

        // Xóa ảnh
        public void DeleteImage()
        {
            try
            {
                //check ảnh có tồn tại
                if(originalImage == null)
                {
                    MessageBox.Show($"Không có ảnh để xóa", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                // xác nhận
                MessageBoxResult result = MessageBox.Show(
                    "Xác nhận xóa ảnh ?",
                    "Xác nhận",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                );
                if(result == MessageBoxResult.Yes)
                {
                    // xóa ảnh
                    originalImage = null;
                    displayImage = null;
                    // update giao diện
                    this.DataContext = new ImageViewModel
                    {
                        DisplayImageSource = null
                    };
                    UpdateStatusBar();
                    MessageBox.Show($"Ảnh được xóa thành công", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Lỗi khi xóa ảnh: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // cắt ảnh
        public void CropImage(int x, int y, int width, int height)
        {
            try
            {
                // kiểm tra ảnh có tồn tại
                if (displayImage == null)
                {
                    MessageBox.Show($"Không có ảnh để crop", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                // check giá trị có hợp lệ hay k
                if(width <= 0 || height <= 0 || x < 0 || y < 0)
                {
                    MessageBox.Show($"Kích thước cắt không hợp lệ!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                // đảm bảo vùng crop ko cắt vượt quá kích thước ảnh
                int maxWidth = displayImage.PixelWidth;
                int maxHeight = displayImage.PixelHeight;
                if (x + width > maxWidth)
                    width = maxWidth - x;
                if (y + height > maxHeight)
                    height = maxHeight - y;

                // tạo croppedbitmap
                CroppedBitmap croppedBitmap = new CroppedBitmap(
                    displayImage,
                    new Int32Rect(x, y, width, height)
                );
                BitmapImage croppedImage = ConvertCroppedBitmapToImage(croppedBitmap);

                // cập nhật ảnh sau khi được chuyển đổi
                displayImage = croppedImage;
                UpdateDisplayImage(croppedImage);
                UpdateStatusBar();
                MessageBox.Show(
                    $"Ảnh đã được cắt thành công\n\n" + 
                    $"Vùng cắt: ({x}, {y}\n) + " +
                    $"Kích thước: {width} x {height}",
                    "Thành công",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Lỗi khi cắt ảnh: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // hàm chuyển đổi CroppedBitmap sang BitmapImage (chuyển từ ảnh cắt sang ảnh hiển thị ở UI)
        private BitmapImage ConvertCroppedBitmapToImage(CroppedBitmap croppedBitmap)
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
                image.CacheOption = BitmapCacheOption.OnLoad; // QUAN TRỌNG: đặt trước
                image.StreamSource = memoryStream;
                image.EndInit();
                image.Freeze();
                image.Freeze();
            }
            return image;
        }

        // Cắt ảnh theo tỉ lệ phần trăm
        public void CropImageByPercentage(double leftPercent, double topPercent, double widthPercent, double heightPercent)
        {
            try
            {
                if(displayImage == null)
                {
                    MessageBox.Show("Không có ảnh để cắt!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                // Tính toán tọa độ và kích thước theo phần trăm
                int x = (int)(displayImage.PixelWidth * leftPercent / 100);
                int y = (int)(displayImage.PixelHeight * topPercent / 100);
                int width = (int)(displayImage.PixelWidth * widthPercent / 100);
                int height = (int)(displayImage.PixelHeight * heightPercent / 100);
                CropImage(x, y, width, height);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // reset hoàn tác lại ảnh ban đầu
        public void ResetToOriginal()
        {
            try
            {
                if(originalImage == null)
                {
                    MessageBox.Show("Không có ảnh gốc!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                displayImage = originalImage;
                UpdateDisplayImage(displayImage);
                UpdateStatusBar();
                MessageBox.Show("Đã quay lại ảnh gốc!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Lưu ảnh hiện tại vào file
        public void SaveImage()
        {
            try
            {
                if(displayImage == null)
                {
                    MessageBox.Show("Không có ảnh để lưu!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "PNG Image (*.png)|*.png|" +
                    "JPEG Image (*.jpg)|*.jpg|" +
                    "BMP Image (*.bmp)|*.bmp|" +
                    "All Files (*.*)|*.*";
                saveFileDialog.Title = "Lưu ảnh";
                saveFileDialog.DefaultExt = "png";
                if(saveFileDialog.ShowDialog() == true)
                {
                    string filePath = saveFileDialog.FileName;
                    SaveImageToFile(displayImage, filePath);
                    MessageBox.Show($"Ảnh đã được lưu tại: \n{filePath}", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lưu ảnh: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveImageToFile(BitmapImage image, string filePath)
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

    }
    // Viewmodel : dùng cho data binding
    public class ImageViewModel
    {
        public BitmapImage DisplayImageSource { get; set; }
        public BitmapImage ImagePreview { get; set; }
    }
}
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

        // Menu item event handler
        private void MenuItem_OpenImage(object sender, RoutedEventArgs e)
        {
            LoadImage();
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

    }
    // Viewmodel : dùng cho data binding
    public class ImageViewModel
    {
        public BitmapImage DisplayImageSource { get; set; }
        public BitmapImage ImagePreview { get; set; }
    }
}
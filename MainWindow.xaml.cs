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
        // lưu state chỉnh sửa hiện tại
        private int currentBrightness = 0;
        private int currentContrast = 0;
        private int currentSaturation = 0;
        
        private int currentRed = 0;
        private int currentGreen = 0;
        private int currentBlue = 0;
        private int currentHue = 0;

        public MainWindow()
        {
            try
            {
                InitializeComponent();
                this.DataContext = new ImageViewModel { DisplayImageSource = null };
                UpdateStatusBar();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Init error: {ex.Message}\n{ex.StackTrace}", "Error");
            }
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

        // chỉnh sửa sáng tối
        private void BtnBrightness_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (displayImage == null)
                {
                    MessageBox.Show("Vui lòng tải ảnh trước");
                    return;
                }
                if (TabBrightness != null)
                {
                    TabBrightness.IsSelected = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi");
            }
        }

        // mix color
        private void BtnColorMix_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (displayImage == null)
                {
                    MessageBox.Show("Vui lòng tải ảnh trước !");
                    return;
                }
                if (TabColorMix != null)
                {
                    TabColorMix.IsSelected = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi");
            }
        }
        private void BtnResetAdj_Click(object sender, RoutedEventArgs e)
        {
            ResetAdjustmentControls();
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

        // menu sáng tối
        private void MenuItem_ShowBrightnessPanel(object sender, RoutedEventArgs e)
        {
            BtnBrightness_Click(null, null);
        }

        // menu show color
        private void MenuItem_ShowColorPanel(object sender, RoutedEventArgs e)
        {
            BtnColorMix_Click(null, null);
        }

        private void MenuItem_ResetAdjustments(object sender, RoutedEventArgs e)
        {
            ResetAdjustmentControls();
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
            try
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(imagePath, UriKind.Absolute); // đặt nguồn ảnh từ file path
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.DecodePixelWidth = 2048;
                bitmap.EndInit();

                originalImage = bitmap; // lưu bản gốc
                displayImage = bitmap; // lưu ảnh hiện tại bằng bản gốc
                ResetAdjustmentControls();
                // Hiên thị ảnh lên giao diện
                UpdateDisplayImage(displayImage);
                UpdateStatusBar();

                MessageBox.Show($"Đang tải ảnh!\n\nSize: {bitmap.PixelWidth}x{bitmap.PixelHeight}", "Thành công");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải ảnh: {ex.Message}", "Lỗi");
            }
        }

        // Cập nhật hiển thị ảnh trên giao diện
        private void UpdateDisplayImage(BitmapImage image)
        {
            try
            {
                if (image != null)
                {
                    this.DataContext = new ImageViewModel
                    {
                        DisplayImageSource = image,
                        ImagePreview = image
                    };
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi hiển thị ảnh: {ex.Message}", "Lỗi");
            }
        }
        private void UpdateStatusBar()
        {
            try
            {
                if(StatusText != null)
                {
                    if (displayImage != null)
                        StatusText.Text = $"{displayImage.PixelWidth} x {displayImage.PixelHeight} px";
                    else
                        StatusText.Text = "Chưa tải ảnh";
                }
            }
            catch { }
        }

        // Xóa ảnh
        public void DeleteImage()
        {
            try
            {
                //check ảnh có tồn tại
                if(originalImage == null)
                {
                    MessageBox.Show($"Không có ảnh để xóa", "Thông báo");
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
                    ResetAdjustmentControls();
                    MessageBox.Show($"Ảnh được xóa thành công", "Thành công");
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Lỗi khi xóa ảnh: {ex.Message}", "Lỗi");
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
                    MessageBox.Show($"Không có ảnh để crop", "Thông báo");
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
                MessageBox.Show($"Lỗi khi cắt ảnh: {ex.Message}", "Lỗi");
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
            }
            return image;
        }

        // hàm chuyển đổi WriteableBitmap sang BitmapImage
        private BitmapImage ConvertWriteableBitmapToImage(WriteableBitmap writeableBitmap)
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
                    MessageBox.Show("Không có ảnh gốc!", "Thông báo");
                    return;
                }
                displayImage = originalImage;
                UpdateDisplayImage(displayImage);
                UpdateStatusBar();
                ResetAdjustmentControls();
                MessageBox.Show("Đã quay lại ảnh gốc!", "Thành công");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi");
            }
        }

        // Lưu ảnh hiện tại vào file
        public void SaveImage()
        {
            try
            {
                if(displayImage == null)
                {
                    MessageBox.Show("Không có ảnh để lưu!", "Thông báo");
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
                    MessageBox.Show($"Ảnh đã được lưu tại: \n{filePath}", "Thành công");
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

        // Brightness slider
        private void SliderBrightness_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
           // khi bấm chuột vào slider 
        }
        private void SliderBrightness_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // khi thả chuột
        }
        private void SliderBrightness_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            // khi kéo slider
            if (SliderBrightness.IsMouseCaptureWithin)
            {
                currentBrightness = (int)SliderBrightness.Value;
                BrightnessValue.Text = $"{currentBrightness}%";
            }
        }
        // contrast slider
        private void SliderContrast_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) { }
        
        private void SliderContrast_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e) { }
        
        private void SliderContrast_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            // khi kéo slider
            if (SliderContrast.IsMouseCaptureWithin)
            {
                currentContrast = (int)SliderContrast.Value;
                ContrastValue.Text = $"{currentContrast}%";
            }
        }

        // saturation slider
        private void SliderSaturation_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) { }

        private void SliderSaturation_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e) { }

        private void SliderSaturation_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            // khi kéo slider
            if (SliderSaturation != null && SliderSaturation.IsMouseCaptureWithin)
            {
                currentSaturation = (int)SliderSaturation.Value;
                if(SaturationValue != null)
                    SaturationValue.Text = $"{currentContrast}%";
            }
        }

        // Button apply brightness click
        private void BtnApplyBrightness_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if(displayImage == null)
                {
                    MessageBox.Show("Không có ảnh");
                    return;
                }
                // Thực hiện chỉnh sửa từng cái một
                BitmapImage adjusted = displayImage;
                // sáng tối
                if(currentBrightness != 0)
                {
                    adjusted = AdjustBrightness(adjusted, currentBrightness);
                }
                // tương phản
                if(currentContrast != 0)
                {
                    adjusted = AdjustContrast(adjusted, currentContrast);
                }
                // bão hòa
                if(currentSaturation != 0)
                {
                    adjusted = AdjustSaturation(adjusted, currentSaturation);
                }
                // Cập nhật ảnh hiển thị
                displayImage = adjusted;
                UpdateDisplayImage(displayImage);
                UpdateStatusBar();

                MessageBox.Show($"Chỉnh sửa thành công!\n\n" +
                                $"- Độ sáng: {currentBrightness}%\n" +
                                $"- Độ tương phản: {currentContrast}%\n"+
                                $"- Độ bão hòa: {currentSaturation}%",
                                "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        // red slider
        private void SliderRed_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) { }
        private void SliderRed_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e) { }
        private void SliderRed_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            try
            {
                if (SliderRed != null && SliderRed.IsMouseCaptureWithin)
                {
                    currentRed = (int)SliderRed.Value;
                    if(RedValue != null) RedValue.Text = $"{currentRed}%";
                    UpdateColorPreview();
                }
            }
            catch { }
            
        }
        // green slider
        private void SliderGreen_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) { }
        private void SliderGreen_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e) { }
        private void SliderGreen_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            try
            {
                if (SliderGreen != null && SliderGreen.IsMouseCaptureWithin)
                {
                    currentGreen = (int)SliderGreen.Value;
                    if(GreenValue != null) GreenValue.Text = $"{currentGreen}%";
                    UpdateColorPreview();
                }
            }
            catch { }
            
        }
        // blue slider
        private void SliderBlue_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) { }
        private void SliderBlue_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e) { }
        private void SliderBlue_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            try
            {
                if (SliderBlue != null && SliderBlue.IsMouseCaptureWithin)
                {
                    currentBlue = (int)SliderBlue.Value;
                    if(BlueValue != null) BlueValue.Text = $"{currentBlue}%";
                    UpdateColorPreview();
                }
            }
            catch {}
            
        }
        // hue slider
        private void SliderHue_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) { }
        private void SliderHue_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e) { }
        private void SliderHue_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            try
            {
                if (SliderHue != null && SliderHue.IsMouseCaptureWithin)
                {
                    currentHue = (int)SliderHue.Value;
                    if(HueValue != null) HueValue.Text = $"{currentHue}°";
                    UpdateColorPreview();
                }
            }
            catch { }
            
        }
        // Cập nhật xem trước màu sắc
        private void UpdateColorPreview()
        {
            try
            {
                // tính toán màu dựa trên RGB values
                int red = CalculateColorValue(currentRed);
                int green = CalculateColorValue(currentGreen);
                int blue = CalculateColorValue(currentBlue);

                // cập nhật preview color
                var color = new System.Windows.Media.Color
                {
                    A = 255,
                    R = (byte)red,
                    G = (byte)green,
                    B = (byte)blue
                };
                PreviewColor.Fill = new System.Windows.Media.SolidColorBrush(color);
                PreviewHex.Text = color.ToString();
            }
            catch { }
        }
        // Tính giá trị màu từ phần trăm (-100 đến 100)
        private int CalculateColorValue(int percentage)
        {
            // -100 = 0, 0 = 128, +100 = 255
            int value = (int)(128 + (percentage * 1.28));
            if (value < 0) value = 0;
            if (value > 255) value = 255;
            return value;
        }

        // button apply color
        private void BtnApplyColor_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (displayImage == null)
                {
                    MessageBox.Show("Không có ảnh để thực hiện");
                    return;
                }

                BitmapImage adjusted = displayImage;
                if (currentRed != 0 || currentGreen != 0 || currentBlue != 0)
                    adjusted = AdjustColorChannels(adjusted, currentRed, currentGreen, currentBlue);
                if (currentHue != 0)
                    adjusted = AdjustHueShift(adjusted, currentHue);
                displayImage = adjusted;
                UpdateDisplayImage(displayImage);
                MessageBox.Show("Đã áp dụng", "Thành công");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Thực hiện (implementation) : sáng tối
        // Điều chỉnh độ sáng :
        // Công thức: NewPixel = OldPixel + Amount
        private BitmapImage AdjustBrightness(BitmapImage source, int brightnessAmount)
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

                    for(int y = 0; y < height; y++)
                    {
                        int rowOffset = y * stride;
                        for(int x = 0; x < width; x++)
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
        private BitmapImage AdjustContrast(BitmapImage source, int contrastAmount)
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
        private BitmapImage AdjustSaturation(BitmapImage source, int saturationAmount)
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
                            RgbToHsl(r, g, b, out double h, out double s, out double l);
                            // điều chỉnh bão hòa
                            s = Math.Min(s * factor, 1.0);
                            // chuyển lại rgb
                            HslToRgb(h, s, l, out r, out g, out b);
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

        // Thực hiện (implementation) : color mix
        // Điều chỉnh từng kênh màu (red, green, blue)
        // Công thức: NewChannel = OldChannel * Factor
        private BitmapImage AdjustColorChannels(BitmapImage source, int redAmount, int greenAmount, int blueAmount)
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

                    for(int y = 0; y < height; y++)
                    {
                        int rowOffset = y * stride;
                        for(int x = 0; x < width; x++)
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
        private BitmapImage AdjustHueShift(BitmapImage source, int hueShift)
        {
            try
            {
                double hueShiftNorm = hueShift  / 180.0;
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
                            RgbToHsl(r, g, b, out double h, out double s, out double l);
                            // Xoay hue
                            h += hueShiftNorm;
                            if (h > 1.0) h -= 1.0;
                            if (h < 0) h += 1.0;
                            // chuyển lại rgb
                            HslToRgb(h, s, l, out r, out g, out b);

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

        //----------------
        // Helper functions
        //----------------

        // đảm bảo giá trị byte nằm trong range 0 - 255
        private byte ClampByte(int value)
        {
            if (value < 0) return 0;
            if (value > 255) return 255;
            return (byte)value;
        }

        // chuyển rgb sang hsl 
        private void RgbToHsl(byte r, byte g, byte b, out double h, out double s, out double l)
        {
            double rNorm = r / 255.0;
            double gNorm = g / 255.0;
            double bNorm = b / 255.0;
            double max = Math.Max(Math.Max(rNorm, gNorm), bNorm);
            double min = Math.Min(Math.Min(rNorm, gNorm), bNorm);
            double delta = max - min;

            l = (max + min) / 2.0;
            if(delta == 0)
            {
                s = 0;
                h = 0;
            }
            else
            {
                s = l < 0.5 ? delta / (max + min) : delta / (2.0 - max - min);
                if (max == rNorm)
                    h = (gNorm - bNorm) / delta + (gNorm < bNorm ? 6 : 0);
                else if (max == gNorm)
                    h = (bNorm - rNorm) / delta + 2;
                else
                    h = (rNorm - gNorm) / delta + 4;
                h /= 6.0;
            }
        }

        // chuyển hsl sang rgb
         private void HslToRgb(double h, double s, double l, out byte r, out byte g, out byte b)
        {
            double rNorm, gNorm, bNorm;
            if (s == 0)
            {
                rNorm = gNorm = bNorm = l;
            }
            else
            {
                double q = l < 0.5 ? l * (1 + s) : l + s - l * s;
                double p = 2 * l - q;
                rNorm = HueToRgb(p, q, h + 1.0 / 3.0);
                gNorm = HueToRgb(p, q, h);
                bNorm = HueToRgb(p, q, h - 1.0 / 3.0);
            }
            r = (byte)(rNorm * 255);
            g = (byte)(gNorm * 255);
            b = (byte)(bNorm * 255);
        }

        // hsl conversion
        private double HueToRgb(double p, double q, double t)
        {
            if (t < 0) t += 1;
            if (t > 1) t -= 1;
            if (t < 1.0 / 6.0) return p + (q - p) * 6 * t;
            if (t < 1.0 / 2.0) return q;
            if (t < 2.0 / 3.0) return p + (q - p) * (2.0 / 3.0 - t) * 6;
            return p;
        }

        // reset tất cả các control chỉnh sửa
        private void ResetAdjustmentControls()
        {
            currentBrightness = 0;
            currentContrast = 0;
            currentSaturation = 0;
            if (SliderBrightness != null) SliderBrightness.Value = 0;
            if (SliderContrast != null) SliderContrast.Value = 0;
            if (SliderSaturation != null) SliderSaturation.Value = 0;
            if (BrightnessValue != null) BrightnessValue.Text = "0%";
            if (ContrastValue != null) ContrastValue.Text = "0%";
            if (SaturationValue != null) SaturationValue.Text = "0%";

            // Color channels
            currentRed = 0;
            currentGreen = 0;
            currentBlue = 0;
            currentHue = 0;

            if (SliderRed != null) SliderRed.Value = 0;
            if (SliderGreen != null) SliderGreen.Value = 0;
            if (SliderBlue != null) SliderBlue.Value = 0;
            if (SliderHue != null) SliderHue.Value = 0;

            if (RedValue != null) RedValue.Text = "0%";
            if (GreenValue != null) GreenValue.Text = "0%";
            if (BlueValue != null) BlueValue.Text = "0%";
            if (HueValue != null) HueValue.Text = "0°";

            if(PreviewColor != null || PreviewHex != null)
                UpdateColorPreview();
        }
    }
    // Viewmodel : dùng cho data binding
    public class ImageViewModel
    {
        public BitmapImage DisplayImageSource { get; set; }
        public BitmapImage ImagePreview { get; set; }
    }
}
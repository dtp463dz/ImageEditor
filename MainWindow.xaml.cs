using System;
using System.Windows;
using System.Windows.Media.Imaging;
using System.IO;
using Microsoft.Win32;
using System.Windows.Media;
using ImageEditor.Modelss;
using ImageEditor.ViewModels;

namespace ImageEditor
{
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
        private void BtnOpenImage_Click(object sender, RoutedEventArgs e) => MenuItem_OpenImage(null, null);
        private void BtnSaveImage_Click(object sender, RoutedEventArgs e) => MenuItem_SaveImage(null, null);
        private void BtnDeleteImage_Click(object sender, RoutedEventArgs e) => MenuItem_DeleteImage(null, null);
        private void BtnCropImage_Click(object sender, RoutedEventArgs e) => MenuItem_CropImage(null, null);
        private void BtnReset_Click(object sender, RoutedEventArgs e) => MenuItem_ResetImage(null, null);
        // chỉnh sửa sáng tối
        private void BtnBrightness_Click(object sender, RoutedEventArgs e)
        {
            if (displayImage == null)
            {
                MessageBox.Show("Vui lòng tải ảnh trước");
                return;
            }
            if (this.DataContext is UIViewModel vm)
            {
                vm.ShowBrightness = true;
                vm.ShowColor = true;
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
                if (this.DataContext is UIViewModel vm)
                {
                    vm.ShowColor = true;
                    vm.ShowBrightness = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi");
            }
        }
        private void BtnResetAdj_Click(object sender, RoutedEventArgs e)
        {
            ResetAllControls();
        }
        // xoay ảnh
        private void BtnRotate90_Click(object sender, RoutedEventArgs e) => RotateImage(90);
        private void BtnRotate180_Click(object sender, RoutedEventArgs e) => RotateImage(180);
        private void BtnRotate270_Click(object sender, RoutedEventArgs e) => RotateImage(270);

        // Menu item event handler
        private void MenuItem_OpenImage(object sender, RoutedEventArgs e)
        {
            try
            {
                BitmapImage bitmap = ImageProcessor.LoadImage();
                if(bitmap != null)
                {
                    originalImage = bitmap;
                    displayImage = bitmap;
                    ResetAllControls();
                    UpdateDisplay();
                    MessageBox.Show($"Tải Size: {bitmap.PixelWidth}x{bitmap.PixelHeight}", "Thành công");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi");
            }
        }
        private void MenuItem_SaveImage(object sender, RoutedEventArgs e)
        {
            try
            {
                if (originalImage == null) return;
                displayImage = originalImage;
                UpdateDisplay();
                ResetAllControls();
                MessageBox.Show("Reset", "Thành công");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi");
            }
        }
        private void MenuItem_Exit(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void MenuItem_DeleteImage(object sender, RoutedEventArgs e)
        {
            try
            {
                if (originalImage == null)
                {
                    MessageBox.Show("Không có ảnh!", "Thông tin");
                    return;
                }

                if (MessageBox.Show("Xóa?", "Xác nhận", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    originalImage = null;
                    displayImage = null;
                    UpdateDisplay();
                    ResetAllControls();
                    MessageBox.Show("Đã xóa!", "Thành công");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi");
            }
        }
        private void MenuItem_CropImage(object sender, RoutedEventArgs e)
        {
            try
            {
                if (displayImage == null) return;
                displayImage = ImageProcessor.CropImage(displayImage, 100, 100, 400, 300);
                UpdateDisplay();
                MessageBox.Show("Đã cắt", "Thành công");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi");
            }
        }
        private void MenuItem_CropPercentImage(object sender, RoutedEventArgs e)
        {
            try
            {
                if (displayImage == null) return;
                displayImage = ImageProcessor.CropImageByPercentage(displayImage, 10, 10, 80, 80);
                UpdateDisplay();
                MessageBox.Show("Đã cắt", "Thành công");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi");
            }
        }
        private void MenuItem_ResetImage(object sender, RoutedEventArgs e)
        {
            try
            {
                if (originalImage == null) return;
                displayImage = originalImage;
                UpdateDisplay();
                ResetAllControls();
                MessageBox.Show("Quay lại", "Thành công");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi");
            }
        }
        // menu sáng tối
        private void MenuItem_ShowBrightnessPanel(object sender, RoutedEventArgs e) => BtnBrightness_Click(null, null);
        // menu show color
        private void MenuItem_ShowColorPanel(object sender, RoutedEventArgs e) => BtnColorMix_Click(null, null);
        private void MenuItem_ResetAdjustments(object sender, RoutedEventArgs e) => ResetAllControls();
        // lật ảnh
        private void MenuItem_FlopHorizontalImage(object sender, RoutedEventArgs e) => FlipImage(isHorizontal: true);
        private void MenuItem_FlopVerticalImage(object sender, RoutedEventArgs e) => FlipImage(isHorizontal: false);
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
        // Brightness slider
        private void SliderBrightness_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            try
            {
                // khi kéo slider
                if (SliderBrightness!= null && SliderBrightness.IsMouseCaptureWithin)
                {
                    currentBrightness = (int)SliderBrightness.Value;
                    if(BrightnessValue != null) BrightnessValue.Text = $"{currentBrightness}%";
                }
            }
            catch { }
        }
        // contrast slider
        private void SliderContrast_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            try
            {
                // khi kéo slider
                if (SliderContrast != null && SliderContrast.IsMouseCaptureWithin)
                {
                    currentContrast = (int)SliderContrast.Value;
                    if (ContrastValue != null) ContrastValue.Text = $"{currentContrast}%";
                }
            }
            catch { }
        }
        // saturation slider
        private void SliderSaturation_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            try
            {
                // khi kéo slider
                if (SliderSaturation != null && SliderSaturation.IsMouseCaptureWithin)
                {
                    currentSaturation = (int)SliderSaturation.Value;
                    if (SaturationValue != null)
                        SaturationValue.Text = $"{currentContrast}%";
                }
            }
            catch { }
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
                    adjusted = BrightnessAdjuster.AdjustBrightness(adjusted, currentBrightness);
                // tương phản
                if(currentContrast != 0)
                    adjusted = BrightnessAdjuster.AdjustContrast(adjusted, currentContrast);
                // bão hòa
                if(currentSaturation != 0)
                    adjusted = BrightnessAdjuster.AdjustSaturation(adjusted, currentSaturation);
                // Cập nhật ảnh hiển thị
                displayImage = adjusted;
                UpdateDisplay();
                MessageBox.Show("Đã áp dụng!", "Thành công");
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }
        // red slider
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
                    adjusted = ColorAdjuster.AdjustColorChannels(adjusted, currentRed, currentGreen, currentBlue);
                if (currentHue != 0)
                    adjusted = ColorAdjuster.AdjustHueShift(adjusted, currentHue);
                displayImage = adjusted;
                UpdateDisplay();
                MessageBox.Show("Đã áp dụng", "Thành công");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi");
            }
        }
        // Rotage Image
        private void RotateImage(int angle)
        {
            try
            {
                if(displayImage == null)
                {
                    MessageBox.Show("Không có ảnh để thực hiện", "Thông báo");
                    return;
                }
                displayImage = ImageProcessor.RotateImage(displayImage, angle);
                UpdateDisplay();
                MessageBox.Show($"Xoay góc: {angle}°!", "Thành công");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xoay ảnh: {ex.Message}", "Lỗi");
            }
        }
        // flip image
        private void FlipImage(bool isHorizontal)
        {
            try
            {
                if(displayImage == null)
                {
                    MessageBox.Show("Không có ảnh để thực hiện", "Thông báo");
                    return;
                }
                displayImage = ImageProcessor.FlipImage(displayImage, isHorizontal);
                UpdateDisplay();
                MessageBox.Show($"Đã lật ảnh", "Thành công");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi");
            }
        }
        //----------------
        // Helper functions
        //----------------
        private void UpdateDisplay()
        {
            try
            {
                if(this.DataContext is UIViewModel vm && displayImage != null)
                {
                    vm.DisplayImageSource = displayImage;
                }
                UpdateStatusBar();
            }
            catch { }
        }
        // reset tất cả các control chỉnh sửa
        private void ResetAllControls()
        {
            currentBrightness = currentContrast = currentSaturation = 0;
            currentRed = currentBlue = currentGreen = currentHue = 0;

            if (SliderBrightness != null) SliderBrightness.Value = 0;
            if (SliderContrast != null) SliderContrast.Value = 0;
            if (SliderSaturation != null) SliderSaturation.Value = 0;
            if (SliderRed != null) SliderRed.Value = 0;
            if (SliderGreen != null) SliderGreen.Value = 0;
            if (SliderBlue != null) SliderBlue.Value = 0;
            if (SliderHue != null) SliderHue.Value = 0;

            if (BrightnessValue != null) BrightnessValue.Text = "0%";
            if (ContrastValue != null) ContrastValue.Text = "0%";
            if (SaturationValue != null) SaturationValue.Text = "0%";
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
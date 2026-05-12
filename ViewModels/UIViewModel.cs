using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;

namespace ImageEditor.ViewModels
{
    /// <summary>
    /// ViewModel cho MainWindow - quản lý Data Binding
    /// Hiển thị/ẩn panels, cập nhật UI 
    /// </summary>
    public class UIViewModel : INotifyPropertyChanged
    {
        private BitmapImage displayImageSource;
        private bool showBrightness = true;
        private bool showColor = false;

        // ảnh hiển thị trên canvas
        public BitmapImage DisplayImageSource
        {
            get => displayImageSource;
            set
            {
                if(displayImageSource != value)
                {
                    displayImageSource = value;
                    OnPropertyChanged();
                }
            }
        }

        // Kiểm soát hiển thị tab Sáng tối
        public bool ShowBrightness
        {
            get => showBrightness;
            set
            {
                if(showBrightness != value)
                {
                    showBrightness = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(BrightnessVisible));
                }
            }
        }
        // kiểm soát hiển thị tab color mix
        public bool ShowColor
        {
            get => showColor;
            set
            {
                if(showColor != value)
                {
                    showColor = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ColorVisible));
                }
            }
        }
        /// Computed property: không lưu trữ, tính toán từ ShowBrightness
        public System.Windows.Visibility BrightnessVisible
            => ShowBrightness ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
        /// Computed property: không lưu trữ, tính toán từ ShowColor
        public System.Windows.Visibility ColorVisible
            => ShowBrightness ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
        // wpf binding tự động lắng nghe và cập nhật UI
        public event PropertyChangedEventHandler PropertyChanged;
        // gọi PropertyChanged event để thông báo binding
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        // Hiển thị tab Brightness, ẩn color
        public void ShowBrightnessTab()
        {
            ShowBrightness = true;
            ShowColor = false;
        }
        // Hiển thị tab color, ẩn brightness
        public void ShowColorTab()
        {
            ShowBrightness = false;
            ShowColor = true;
        }
        // ẩn tất cả tabs
        public void HideAllTabs()
        {
            ShowBrightness = false;
            ShowColor = false;
        }
        // reset viewmodel về trạng thái ban đầu
        public void Reset()
        {
            DisplayImageSource = null;
            ShowBrightness = true;
            ShowColor = false;
        }
    }
}

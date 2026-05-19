using ImageEditor.Modelss;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media.Imaging;

namespace ImageEditor.ViewModels
{
    /// <summary>
    /// Quản lý trạng thái selection:
    /// - Vùng được chọn
    /// - Mode (Rectangle/Freeform)
    /// </summary>
    public class SelectionViewModel
    {
        // selection state
        private BitmapImage selectedRegion = null;
        private SelectionTool.SelectionMode currentMode = SelectionTool.SelectionMode.Rectangle;
        private List<System.Windows.Point> freeformPoints = new List<System.Windows.Point>();

        // Rectangle mode
        private int rectX = 0, rectY = 0, rectWidth = 0, rectHeight = 0;
        private bool isDrawingRect = false;

        public BitmapImage SelectedRegion
        {
            get => selectedRegion;
            set => selectedRegion = value;
        }
        public SelectionTool.SelectionMode CurrentMode
        {
            get => currentMode;
            set => currentMode = value;
        }
        public List<System.Windows.Point> FreeformPoints
        {
            get => freeformPoints;
            set => freeformPoints = value;
        }
        public int RectX
        {
            get => rectX;
            set => rectX = value;
        }
        public int RectY
        {
            get => rectY;
            set => rectY = value;
        }
        public int RectWidth
        {
            get => rectWidth;
            set => rectWidth = value;
        }
        public int RectHeight
        {
            get => rectHeight;
            set => rectHeight = value;
        }
        public bool IsDrawingRect
        {
            get => isDrawingRect;
            set => isDrawingRect = value;
        }
        public void SwitchToRectangleMode()
        {
            CurrentMode = SelectionTool.SelectionMode.Rectangle;
            ClearSelection();
        }

        public void SwitchToFreeformMode()
        {
            CurrentMode = SelectionTool.SelectionMode.Freeform;
            ClearSelection();
        }

        public void AddFreeformPoint(System.Windows.Point point)
        {
            FreeformPoints.Add(point);
        }

        public void ClearFreeformPoints()
        {
            FreeformPoints.Clear();
        }

        public void ClearSelection()
        {
            SelectedRegion = null;
            FreeformPoints.Clear();
            RectX = RectY = RectWidth = RectHeight = 0;
            IsDrawingRect = false;
        }

        public void SetRectangle(int x, int y, int width, int height)
        {
            RectX = x;
            RectY = y;
            RectWidth = width;
            RectHeight = height;
        }
    }
}

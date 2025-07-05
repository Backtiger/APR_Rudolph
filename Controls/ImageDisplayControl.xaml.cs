using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace APR_Rudolph.Controls
{
    /// <summary>
    /// 이미지를 표시하고, 줌/팬/리셋 등 다양한 뷰어 기능을 제공하는 커스텀 컨트롤.
    /// </summary>
    public partial class ImageDisplayControl : UserControl
    {
        public static readonly DependencyProperty SourceProperty = 
            DependencyProperty.Register(nameof(Source), typeof(BitmapImage), typeof(ImageDisplayControl), 
                new PropertyMetadata(null, OnSourceChanged));

        public static readonly DependencyProperty TitleProperty = 
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(ImageDisplayControl), 
                new PropertyMetadata("이미지"));

        public BitmapImage? Source
        {
            get => (BitmapImage?)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        // 줌/팬 관련 변수들
        private bool _isDragging = false;
        private Point _lastMousePosition;
        private double _zoomLevel = 1.0;
        private const double MIN_ZOOM = 0.1;
        private const double MAX_ZOOM = 5.0;
        private const double ZOOM_FACTOR = 0.1;

        public ImageDisplayControl()
        {
            InitializeComponent();
        }

        private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ImageDisplayControl control)
            {
                control.UpdateImageVisibility();
            }
        }

        private void UpdateImageVisibility()
        {
            if (Source != null)
            {
                NoImageText.Visibility = Visibility.Collapsed;
                DisplayImage.Visibility = Visibility.Visible;
            }
            else
            {
                NoImageText.Visibility = Visibility.Visible;
                DisplayImage.Visibility = Visibility.Collapsed;
            }
        }

        // 마우스 휠로 줌 인/아웃
        private void DisplayImage_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Source == null) return;

            double zoomDelta = e.Delta > 0 ? ZOOM_FACTOR : -ZOOM_FACTOR;
            double newZoom = _zoomLevel + zoomDelta;
            
            if (newZoom >= MIN_ZOOM && newZoom <= MAX_ZOOM)
            {
                _zoomLevel = newZoom;
                ApplyZoom();
            }
        }

        // 마우스 드래그 시작
        private void DisplayImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Source == null) return;

            _isDragging = true;
            _lastMousePosition = e.GetPosition(ScrollViewer);
            DisplayImage.CaptureMouse();
        }

        // 마우스 드래그 종료
        private void DisplayImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
            DisplayImage.ReleaseMouseCapture();
        }

        // 마우스 이동 (팬)
        private void DisplayImage_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDragging || Source == null) return;

            Point currentPosition = e.GetPosition(ScrollViewer);
            Vector delta = currentPosition - _lastMousePosition;

            ScrollViewer.ScrollToHorizontalOffset(ScrollViewer.HorizontalOffset - delta.X);
            ScrollViewer.ScrollToVerticalOffset(ScrollViewer.VerticalOffset - delta.Y);

            _lastMousePosition = currentPosition;
        }

        // 줌 적용
        private void ApplyZoom()
        {
            if (Source == null) return;

            DisplayImage.LayoutTransform = new ScaleTransform(_zoomLevel, _zoomLevel);
        }

        // 줌 리셋
        public void ResetZoom()
        {
            _zoomLevel = 1.0;
            ApplyZoom();
            ScrollViewer.ScrollToHorizontalOffset(0);
            ScrollViewer.ScrollToVerticalOffset(0);
        }

        // 더블클릭으로 줌 리셋
        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            ResetZoom();
        }
    }
} 
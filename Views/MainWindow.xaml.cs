using System.Windows;
using System.Windows.Controls;
using APR_Rudolph.ViewModels;
using APR_Rudolph.Controls;

namespace APR_Rudolph.Views
{
    /// <summary>
    /// 루돌프 코 합성기 메인 윈도우. 커스텀 타이틀바, 닫기 버튼, MVVM 구조, 드래그 이동 등 UI를 담당.
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnSourceInitialized(System.EventArgs e)
        {
            base.OnSourceInitialized(e);
        }

        protected override void OnClosed(System.EventArgs e)
        {
            base.OnClosed(e);
            if (DataContext is MainViewModel vm)
            {
                vm.StopWebcam();
                // 얼굴 검출 서비스 Dispose (필요시)
                // vm.DisposeFaceDetectionService();
            }
        }
    }
} 
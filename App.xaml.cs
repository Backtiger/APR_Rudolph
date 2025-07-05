using System.Configuration;
using System.Data;
using System.Windows;
using APR_Rudolph.ViewModels;
using APR_Rudolph.Views;
using APR_Rudolph.Services;

namespace APR_Rudolph;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        var mainWindow = new MainWindow();
        mainWindow.Show();
        //mainWindow.DataContext = new ViewModels.MainViewModel();

        // 테스트: 지정 이미지로 두 모델 얼굴 검출 결과 저장
        var service = new ImageProcessingService();
        service.TestFaceDetectionOnImage("C:\\Users\\33052\\OneDrive\\사진\\사사.jpg");

        //this.DispatcherUnhandledException += (sender, args) =>
        //{
        //    MessageBox.Show($"예외 발생: {args.Exception.Message}\n\n상세: {args.Exception.StackTrace}",
        //                  "오류", MessageBoxButton.OK, MessageBoxImage.Error);
        //    args.Handled = true;
        //};
    }
}


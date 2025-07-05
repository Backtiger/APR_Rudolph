using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using APR_Rudolph.Services;
using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Linq;
using Point = System.Drawing.Point;
using System.Windows.Forms;
using APR_Rudolph.Views;
using APR_Rudolph.Models;

namespace APR_Rudolph.ViewModels
{
    /// <summary>
    /// 메인 윈도우의 ViewModel. 이미지/웹캠 얼굴 검출, 루돌프 코 합성, UI 상태 관리 등 전체 MVVM 로직을 담당합니다.
    /// </summary>
    public partial class MainViewModel : ObservableObject
    {
        private readonly IFaceDetectionService _faceDetectionService;
        private readonly IImageProcessingService _imageProcessingService;
        private readonly IRealtimeFaceDetectionService _realtimeFaceDetectionService;
        private readonly IWebcamService _webcamService;
        private bool _isRudolphNoseEnabled = false;
        private List<(System.Drawing.Point Position, int Size)> _currentDetectedFaces = new List<(System.Drawing.Point Position, int Size)>();

        [ObservableProperty]
        private BitmapImage? inputImage;
        [ObservableProperty]
        private BitmapImage? outputImage;
        [ObservableProperty]
        private BitmapImage? webcamImage;
        [ObservableProperty]
        private int selectedTabIndex = 0;

        partial void OnSelectedTabIndexChanged(int value)
        {
            // 탭 변경 시 상태 초기화
            ResetTabState(value);
        }

        private void ResetTabState(int tabIndex)
        {
            switch (tabIndex)
            {
                case 0: // 이미지 탭
                    // 웹캠이 실행 중이면 중지
                    if (_webcamService.IsRunning)
                    {
                        StopWebcam();
                    }
                    
                    // 웹캠 이미지 초기화
                    WebcamImage = null;
                    
                    // 루돌프 코 비활성화
                    _isRudolphNoseEnabled = false;
                    if (_webcamService is WebcamService ws1)
                    {
                        ws1.EnableRudolphNose(false);
                    }
                    
                    // 얼굴 검출 결과 초기화
                    _currentDetectedFaces.Clear();
                    
                    System.Diagnostics.Debug.WriteLine($"이미지 탭 활성화 - 웹캠 상태 초기화");
                    break;
                    
                case 1: // 비디오 탭
                    // 입력/출력 이미지 초기화
                    InputImage = null;
                    OutputImage = null;
                    
                    // 루돌프 코 비활성화
                    _isRudolphNoseEnabled = false;
                    if (_webcamService is WebcamService ws2)
                    {
                        ws2.EnableRudolphNose(false);
                    }
                    
                    // 얼굴 검출 결과 초기화
                    _currentDetectedFaces.Clear();
                    
                    System.Diagnostics.Debug.WriteLine($"비디오 탭 활성화 - 이미지 상태 초기화");
                    break;
            }
        }

        public MainViewModel()
        {
            // 서비스 초기화
            _imageProcessingService = new ImageProcessingService();
            _faceDetectionService = new FaceDetectionService();
            _realtimeFaceDetectionService = new RealtimeFaceDetectionService(_faceDetectionService, _imageProcessingService);
            _webcamService = new WebcamService(_imageProcessingService, _realtimeFaceDetectionService);

            // 얼굴 검출 서비스 초기화
            _faceDetectionService.Initialize();

            // 웹캠 서비스 이벤트 구독
            _webcamService.FrameUpdated += OnWebcamFrameUpdated;
            _webcamService.ErrorOccurred += OnWebcamError;
            _webcamService.FacesDetected += OnWebcamFacesDetected;
        }

        /// <summary>
        /// 웹캠 서비스를 반환합니다.
        /// </summary>
        public IWebcamService GetWebcamService()
        {
            return _webcamService;
        }

        private void OnWebcamFrameUpdated(object? sender, BitmapImage frame)
        {
            try
            {
                if (frame != null)
                {
                    // 루돌프 코가 활성화되어 있고 얼굴이 검출된 경우 합성
                    if (_isRudolphNoseEnabled && _currentDetectedFaces.Count > 0)
                    {
                        var nosePoints = _currentDetectedFaces.Select(x => x.Position).ToArray();
                        var noseSizes = _currentDetectedFaces.Select(x => x.Size).ToArray();
                        // 백그라운드에서 합성 처리 (UI 블로킹 방지)
                        Task.Run(() =>
                        {
                            try
                            {
                                var processedImage = _imageProcessingService.AddRudolphNose(frame, nosePoints, noseSizes);
                                // UI 스레드에서 이미지 업데이트
                                if (Application.Current != null && Application.Current.Dispatcher != null)
                                {
                                    Application.Current.Dispatcher.Invoke(() =>
                                    {
                                        WebcamImage = processedImage;
                                    });
                                }
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"루돌프 코 합성 오류: {ex.Message}");
                                // 오류 발생시 원본 프레임 표시
                                if (Application.Current != null && Application.Current.Dispatcher != null)
                                {
                                    Application.Current.Dispatcher.Invoke(() =>
                                    {
                                        WebcamImage = frame;
                                    });
                                }
                            }
                        });
                    }
                    else
                    {
                        WebcamImage = frame;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"OnWebcamFrameUpdated 오류: {ex.Message}");
                WebcamImage = frame; // 오류 발생시 원본 프레임 표시
            }
        }

        private void OnWebcamError(object? sender, string error)
        {
            CustomMessageBox.Show(error);
        }

        private void OnWebcamFacesDetected(object? sender, List<(System.Drawing.Point Position, int Size)> faces)
        {
            try
            {
                _currentDetectedFaces = faces;
                System.Diagnostics.Debug.WriteLine($"웹캠에서 얼굴 {faces.Count}개 검출됨");
                
                // 얼굴 위치 정보 출력
                for (int i = 0; i < faces.Count; i++)
                {
                    var face = faces[i];
                    System.Diagnostics.Debug.WriteLine($"  얼굴 {i + 1}: 위치({face.Position.X}, {face.Position.Y}), 크기: {face.Size}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"OnWebcamFacesDetected 오류: {ex.Message}");
            }
        }

        // 햄버거 메뉴 명령
        [RelayCommand]
        private void SwitchToImage()
        {
            SelectedTabIndex = 0;
        }

        [RelayCommand]
        private void SwitchToVideo()
        {
            SelectedTabIndex = 1;
        }

        [RelayCommand]
        private void Exit()
        {
            Application.Current.Shutdown();
        }

        // 이미지 처리 명령
        [RelayCommand]
        private void OpenImage()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog { Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp" };
            if (dlg.ShowDialog() == true)
            {
                var bmp = new Bitmap(dlg.FileName);
                InputImage = _imageProcessingService.BitmapToBitmapImage(bmp);
                OutputImage = null;
            }
        }

        [RelayCommand]
        private void ProcessImage()
        {
            try
            {
                if (InputImage == null)
                {
                    CustomMessageBox.Show("먼저 이미지를 열어주세요.");
                    return;
                }

                if (!_faceDetectionService.IsInitialized)
                {
                    CustomMessageBox.Show("얼굴 검출 서비스가 초기화되지 않았습니다.");
                    return;
                }

                var bmp = _imageProcessingService.BitmapImageToBitmap(InputImage);
                var nosePointsWithSize = _faceDetectionService.DetectNosePositionsWithSize(bmp);

                if (nosePointsWithSize.Count == 0)
                {
                    CustomMessageBox.Show("이미지에서 얼굴을 찾을 수 없습니다.\n다른 이미지를 시도해보거나 조명을 개선해보세요.");
                    return;
                }

                // 루돌프 코 합성
                var nosePoints = nosePointsWithSize.Select(x => x.Position).ToArray();
                var noseSizes = nosePointsWithSize.Select(x => x.Size).ToArray();
                OutputImage = _imageProcessingService.AddRudolphNose(InputImage, nosePoints, noseSizes);

                CustomMessageBox.Show($"루돌프 코 합성 완료! {nosePointsWithSize.Count}개의 얼굴에서 코를 찾았습니다.");
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"오류 발생: {ex.Message}");
            }
        }

        [RelayCommand]
        private void SaveOutput()
        {
            if (OutputImage == null) return;
            var dlg = new Microsoft.Win32.SaveFileDialog { Filter = "PNG Image|*.png" };
            if (dlg.ShowDialog() == true)
            {
                _imageProcessingService.SaveImage(OutputImage, dlg.FileName);
            }
        }

        // 웹캠 처리 명령
        [RelayCommand]
        private async Task StartWebcam()
        {
            if (_webcamService.IsRunning) return;
            await _webcamService.StartAsync();
        }

        /// <summary>
        /// 웹캠을 중지하고 관련 상태를 초기화합니다.
        /// </summary>
        [RelayCommand]
        public void StopWebcam()
        {
            _webcamService.Stop();
            
            // 웹캠 이미지 완전 초기화
            WebcamImage = null;
            
            // 루돌프 코 비활성화
            _isRudolphNoseEnabled = false;
            if (_webcamService is WebcamService ws)
            {
                ws.EnableRudolphNose(false);
            }
            
            // 얼굴 검출 결과 초기화
            _currentDetectedFaces.Clear();
            
            System.Diagnostics.Debug.WriteLine("웹캠 중지 - 화면 및 상태 완전 초기화");
        }

        [RelayCommand]
        private void ToggleRudolphNose()
        {
            _isRudolphNoseEnabled = !_isRudolphNoseEnabled;
            
            // 웹캠 서비스에 루돌프 코 활성화 상태 전달
            if (_webcamService is WebcamService ws)
            {
                ws.EnableRudolphNose(_isRudolphNoseEnabled);
            }
            
            System.Diagnostics.Debug.WriteLine($"루돌프 코 토글: {(_isRudolphNoseEnabled ? "ON" : "OFF")}");
            CustomMessageBox.Show($"루돌프 코: {(_isRudolphNoseEnabled ? "ON" : "OFF")}");
        }
    }
}
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace APR_Rudolph.Services
{
    /// <summary>
    /// 웹캠에서 실시간으로 프레임을 받아 얼굴 검출 및 루돌프 코 합성 처리를 담당하는 서비스입니다.
    /// </summary>
    public class WebcamService : IWebcamService
    {
        private VideoCapture? _webcam;
        private bool _isRunning = false;
        private readonly IImageProcessingService _imageProcessingService;
        private readonly IRealtimeFaceDetectionService _faceDetectionService;
        private bool _isRudolphNoseEnabled = false;
        
        // 성능 최적화를 위한 변수들
        private int _frameCount = 0;
        private const int FACE_DETECTION_INTERVAL = 2; // 2프레임마다 얼굴 검출 (더 자주)
        private const int TARGET_FPS = 30;
        private const int FRAME_DELAY_MS = 1000 / TARGET_FPS; // 약 33ms

        public bool IsRunning => _isRunning;

        public event EventHandler<BitmapImage>? FrameUpdated;
        public event EventHandler<string>? ErrorOccurred;
        public event EventHandler<List<(System.Drawing.Point Position, int Size)>>? FacesDetected;

        public WebcamService(IImageProcessingService imageProcessingService, IRealtimeFaceDetectionService faceDetectionService)
        {
            _imageProcessingService = imageProcessingService;
            _faceDetectionService = faceDetectionService;
        }

        public void EnableRudolphNose(bool enabled)
        {
            _isRudolphNoseEnabled = enabled;
            System.Diagnostics.Debug.WriteLine($"루돌프 코 기능: {(enabled ? "활성화" : "비활성화")}");
        }

        // 웹캠 설정 조절 메서드들
        public void SetBrightness(double value)
        {
            if (_webcam != null && _webcam.IsOpened())
            {
                _webcam.Set(VideoCaptureProperties.Brightness, value);
                System.Diagnostics.Debug.WriteLine($"밝기 설정: {value}");
            }
        }

        public void SetContrast(double value)
        {
            if (_webcam != null && _webcam.IsOpened())
            {
                _webcam.Set(VideoCaptureProperties.Contrast, value);
                System.Diagnostics.Debug.WriteLine($"대비 설정: {value}");
            }
        }

        public void SetExposure(double value)
        {
            if (_webcam != null && _webcam.IsOpened())
            {
                _webcam.Set(VideoCaptureProperties.Exposure, value);
                System.Diagnostics.Debug.WriteLine($"노출 설정: {value}");
            }
        }

        public void SetGain(double value)
        {
            if (_webcam != null && _webcam.IsOpened())
            {
                _webcam.Set(VideoCaptureProperties.Gain, value);
                System.Diagnostics.Debug.WriteLine($"게인 설정: {value}");
            }
        }

        public void SetAutoExposure(bool enabled)
        {
            if (_webcam != null && _webcam.IsOpened())
            {
                _webcam.Set(VideoCaptureProperties.AutoExposure, enabled ? 1 : 0);
                System.Diagnostics.Debug.WriteLine($"자동 노출 설정: {(enabled ? "활성화" : "비활성화")}");
            }
        }

        public (double Brightness, double Contrast, double Exposure, double Gain, bool AutoExposure) GetCurrentSettings()
        {
            if (_webcam != null && _webcam.IsOpened())
            {
                return (
                    _webcam.Get(VideoCaptureProperties.Brightness),
                    _webcam.Get(VideoCaptureProperties.Contrast),
                    _webcam.Get(VideoCaptureProperties.Exposure),
                    _webcam.Get(VideoCaptureProperties.Gain),
                    _webcam.Get(VideoCaptureProperties.AutoExposure) > 0
                );
            }
            return (0, 0, 0, 0, false);
        }

        public async Task StartAsync()
        {
            if (_isRunning) return;

            try
            {
                System.Diagnostics.Debug.WriteLine("웹캠 초기화 시작...");
                
                // 여러 웹캠 인덱스 시도
                for (int cameraIndex = 0; cameraIndex < 3; cameraIndex++)
                {
                    try
                    {
                        _webcam = new VideoCapture(cameraIndex);
                        if (_webcam.IsOpened())
                        {
                            System.Diagnostics.Debug.WriteLine($"웹캠 {cameraIndex} 연결 성공");
                            _webcam.Set(VideoCaptureProperties.FrameWidth, 640);
                            _webcam.Set(VideoCaptureProperties.FrameHeight, 480);
                            _webcam.Set(VideoCaptureProperties.Fps, TARGET_FPS);
                            // _webcam.Set(VideoCaptureProperties.FourCC, VideoWriter.FourCC('M', 'J', 'P', 'G'));
                            // _webcam.Set(VideoCaptureProperties.Format, 16); // RGB 형식
                            // 첫 프레임 테스트
                            using (var testFrame = _webcam.RetrieveMat())
                            {
                                if (!testFrame.Empty())
                                {
                                    System.Diagnostics.Debug.WriteLine($"웹캠 {cameraIndex} 테스트 프레임 성공: {testFrame.Width}x{testFrame.Height}");
                                    break;
                                }
                                else
                                {
                                    System.Diagnostics.Debug.WriteLine($"웹캠 {cameraIndex} 테스트 프레임 실패");
                                    _webcam.Dispose();
                                    _webcam = null;
                                }
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"웹캠 {cameraIndex} 연결 실패");
                            _webcam.Dispose();
                            _webcam = null;
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"웹캠 {cameraIndex} 초기화 오류: {ex.Message}");
                        _webcam?.Dispose();
                        _webcam = null;
                    }
                }

                if (_webcam == null || !_webcam.IsOpened())
                {
                    ErrorOccurred?.Invoke(this, "사용 가능한 웹캠을 찾을 수 없습니다.");
                    return;
                }

                System.Diagnostics.Debug.WriteLine("웹캠 초기화 완료");
                _isRunning = true;
                _ = Task.Run(WebcamLoop);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"웹캠 시작 오류: {ex.Message}");
                ErrorOccurred?.Invoke(this, $"웹캠 시작 오류: {ex.Message}");
            }
        }

        public void Stop()
        {
            _isRunning = false;
            _webcam?.Release();
            _webcam?.Dispose();
            _webcam = null;
        }

        private BitmapImage CreateEmptyFrame()
        {
            try
            {
                // 1x1 픽셀의 투명한 이미지 생성
                var bitmap = new Bitmap(1, 1);
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    graphics.Clear(Color.Transparent);
                }
                
                return _imageProcessingService.BitmapToBitmapImage(bitmap);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"빈 프레임 생성 오류: {ex.Message}");
                return null;
            }
        }

        public BitmapImage? GetCurrentFrame()
        {
            if (!_isRunning || _webcam == null) return null;

            try
            {
                using (var frame = _webcam.RetrieveMat())
                {
                    if (frame.Empty()) 
                    {
                        return null;
                    }

                    var bitmap = MatToBitmapOptimized(frame);
                    return _imageProcessingService.BitmapToBitmapImage(bitmap);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetCurrentFrame 오류: {ex.Message}");
                return null;
            }
        }

        private async void WebcamLoop()
        {
            System.Diagnostics.Debug.WriteLine("웹캠 루프 시작");

            while (_isRunning && _webcam != null)
            {
                try
                {
                    using (var frame = _webcam.RetrieveMat())
                    {
                        if (frame.Empty()) 
                        {
                            await Task.Delay(50);
                            continue;
                        }

                        var bitmap = MatToBitmapOptimized(frame);
                        
                        // 루돌프 코가 활성화된 경우 더 자주 얼굴 검출 수행
                        if (_isRudolphNoseEnabled && _frameCount % FACE_DETECTION_INTERVAL == 0)
                        {
                            // Bitmap 복사본 생성 (리소스 충돌 방지)
                            Bitmap bitmapCopy;
                            try
                            {
                                bitmapCopy = new Bitmap(bitmap);
                            }
                            catch (InvalidOperationException)
                            {
                                // Bitmap이 사용 중이므로 얼굴 검출 건너뛰기
                                System.Diagnostics.Debug.WriteLine("Bitmap이 사용 중이므로 얼굴 검출 건너뛰기");
                                bitmapCopy = null;
                            }
                            
                            if (bitmapCopy != null)
                            {
                                // 백그라운드에서 얼굴 검출 수행 (UI 블로킹 방지)
                                _ = Task.Run(async () =>
                                {
                                    try
                                    {
                                        using (bitmapCopy)
                                        {
                                            var detectedFaces = await _faceDetectionService.DetectFacesFromFrame(bitmapCopy);
                                            FacesDetected?.Invoke(this, detectedFaces);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        System.Diagnostics.Debug.WriteLine($"백그라운드 얼굴 검출 오류: {ex.Message}");
                                    }
                                });
                            }
                        }

                        var bitmapImage = _imageProcessingService.BitmapToBitmapImage(bitmap);

                        if (bitmapImage != null)
                        {
                            FrameUpdated?.Invoke(this, bitmapImage);
                        }

                        _frameCount++;
                        
                        // 프레임 레이트 조절
                        await Task.Delay(FRAME_DELAY_MS);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"WebcamLoop 오류: {ex.Message}");
                    await Task.Delay(100);
                }
            }
            
            System.Diagnostics.Debug.WriteLine("웹캠 루프 종료");
        }

        // OpenCV Extensions의 ToBitmap 메서드 사용 (최적화된 버전)
        private Bitmap MatToBitmapOptimized(Mat mat)
        {
            try
            {
                if (mat.Empty() || mat.Width <= 0 || mat.Height <= 0)
                    return new Bitmap(1, 1);

                // 색상 변환 없이 바로 Bitmap 변환
                return OpenCvSharp.Extensions.BitmapConverter.ToBitmap(mat);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"MatToBitmapOptimized 오류: {ex.Message}");
                return new Bitmap(1, 1);
            }
        }

        // 기존 MatToBitmap 메서드 (백업용)
        private Bitmap MatToBitmap(Mat mat)
        {
            try
            {
                if (mat.Empty() || mat.Width <= 0 || mat.Height <= 0)
                    return new Bitmap(1, 1);

                // 색상 변환 없이 바로 Bitmap 변환
                return OpenCvSharp.Extensions.BitmapConverter.ToBitmap(mat);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"MatToBitmap 오류: {ex.Message}");
                return new Bitmap(1, 1);
            }
        }
    }
} 
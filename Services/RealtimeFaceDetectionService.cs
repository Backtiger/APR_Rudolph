using APR_Rudolph.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using Point = System.Drawing.Point;

namespace APR_Rudolph.Services
{
    public class RealtimeFaceDetectionService : IRealtimeFaceDetectionService
    {
        private readonly IFaceDetectionService _faceDetectionService;
        private readonly IImageProcessingService _imageProcessingService;
        private bool _isRunning = false;
        private List<(System.Drawing.Point Position, int Size)> _detectedFaces = new List<(System.Drawing.Point Position, int Size)>();
        private int _consecutiveFailures = 0;
        private readonly int _maxConsecutiveFailures = 1; // 연속 실패 허용 횟수 더 줄임 (더 민감하게)

        public List<(System.Drawing.Point Position, int Size)> DetectedFaces => _detectedFaces;
        public bool IsRunning => _isRunning;

        public event EventHandler<List<(System.Drawing.Point Position, int Size)>>? FacesDetected;

        public RealtimeFaceDetectionService(IFaceDetectionService faceDetectionService, IImageProcessingService imageProcessingService)
        {
            _faceDetectionService = faceDetectionService;
            _imageProcessingService = imageProcessingService;
        }

        public Task StartAsync()
        {
            if (_isRunning) return Task.CompletedTask;
            _isRunning = true;
            _consecutiveFailures = 0;
            System.Diagnostics.Debug.WriteLine("실시간 얼굴 검출 시작");
            return Task.CompletedTask;
        }

        public void Stop()
        {
            _isRunning = false;
            _detectedFaces.Clear();
            System.Diagnostics.Debug.WriteLine("실시간 얼굴 검출 중지");
        }

        /// <summary>
        /// 웹캠 프레임에서 얼굴 검출 (최적화된 버전)
        /// </summary>
        public Task<List<(System.Drawing.Point Position, int Size)>> DetectFacesFromFrame(Bitmap frame)
        {
            if (!_isRunning || !_faceDetectionService.IsInitialized)
            {
                return Task.FromResult(_detectedFaces);
            }

            try
            {
                // 기본 얼굴 검출 (전처리 없이)
                var detectedFaces = _faceDetectionService.DetectNosePositionsWithSize(frame);
                
                if (detectedFaces.Count > 0)
                {
                    // 검출 성공
                    _detectedFaces = detectedFaces;
                    _consecutiveFailures = 0;
                    
                    // 성능을 위해 디버그 출력 최소화
                    if (_consecutiveFailures > 0)
                    {
                        System.Diagnostics.Debug.WriteLine($"웹캠 얼굴 검출 성공: {detectedFaces.Count}개");
                    }
                    
                    FacesDetected?.Invoke(this, _detectedFaces);
                }
                else
                {
                    _consecutiveFailures++;
                    
                    // 연속 실패가 많으면 이전 결과 유지
                    if (_consecutiveFailures > _maxConsecutiveFailures)
                    {
                        _detectedFaces.Clear();
                        FacesDetected?.Invoke(this, _detectedFaces);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"웹캠 얼굴 검출 오류: {ex.Message}");
                _consecutiveFailures++;
            }

            return Task.FromResult(_detectedFaces);
        }
    }
} 
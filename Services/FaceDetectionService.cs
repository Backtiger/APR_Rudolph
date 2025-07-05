using APR_Rudolph.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using Point = System.Drawing.Point;
using APR_Rudolph.Views;

namespace APR_Rudolph.Services
{
    /// <summary>
    /// 얼굴 검출 및 랜드마크 추출을 담당하는 서비스입니다. MMOD/dlib 기반.
    /// </summary>
    public class FaceDetectionService : IFaceDetectionService, IDisposable
    {
        private BaseFaceDetector? _faceDetector;
        private bool _isInitialized = false;

        public bool IsInitialized => _isInitialized;

        public void Initialize()
        {
            try
            {
                // 기본 얼굴 검출기 사용 (이전 로직으로 롤백)
                //_faceDetector = new FaceDetector();
                //_isInitialized = true;

                // Haar Cascade 기반 얼굴 검출기로 교체 (실시간용)
                _faceDetector = new HaarCascadeFaceDetector();
                System.Diagnostics.Debug.WriteLine("HaarCascadeFaceDetector 초기화 성공 (실시간 검출기)");
                _isInitialized = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"HaarCascadeFaceDetector 초기화 실패: {ex.Message}");
                try
                {
                    // Haar Cascade 실패시 전처리 검출기로 폴백
                    _faceDetector = new PreprocessedFaceDetector();
                    System.Diagnostics.Debug.WriteLine("PreprocessedFaceDetector 초기화 성공 (폴백)");
                    _isInitialized = true;
                }
                catch (Exception ex2)
                {
                    System.Diagnostics.Debug.WriteLine($"PreprocessedFaceDetector 초기화 실패: {ex2.Message}");
                    CustomMessageBox.Show($"얼굴 검출 모델 로드 실패: {ex2.Message}\nshape_predictor_68_face_landmarks.dat 파일을 확인해주세요.");
                    _isInitialized = false;
                }
            }
        }

        public List<(Point Position, int Size)> DetectNosePositionsWithSize(Bitmap image)
        {
            if (!_isInitialized || _faceDetector == null)
            {
                throw new InvalidOperationException("얼굴 검출 서비스가 초기화되지 않았습니다.");
            }

            // 기본 검출기 사용
            var result = _faceDetector.DetectNosePositionsWithSize(image);
            System.Diagnostics.Debug.WriteLine($"얼굴 검출: {result.Count}개 발견");
            return result;
        }

        public List<Point> DetectNosePositions(Bitmap image)
        {
            var positionsWithSize = DetectNosePositionsWithSize(image);
            var result = new List<Point>();

            foreach (var item in positionsWithSize)
            {
                result.Add(item.Position);
            }

            return result;
        }

        public void Dispose()
        {
            _faceDetector?.Dispose();
            _isInitialized = false;
        }
    }
} 
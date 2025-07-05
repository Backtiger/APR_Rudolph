using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using Point = System.Drawing.Point;

namespace APR_Rudolph.Models
{
    /// <summary>
    /// OpenCV Haar Cascade 기반 얼굴 검출 클래스
    /// </summary>
    public class HaarCascadeFaceDetector : BaseFaceDetector
    {
        private readonly CascadeClassifier _faceCascade;
        private readonly string _cascadePath = "haarcascade_frontalface_default.xml";

        public HaarCascadeFaceDetector()
        {
            // 모델 파일이 없으면 예외 발생
            if (!System.IO.File.Exists(_cascadePath))
                throw new Exception($"Haar Cascade 파일이 없습니다: {_cascadePath}");
            _faceCascade = new CascadeClassifier(_cascadePath);
        }

        public override List<(Point Position, int Size)> DetectNosePositionsWithSize(Bitmap image)
        {
            var result = new List<(Point Position, int Size)>();
            using (var mat = OpenCvSharp.Extensions.BitmapConverter.ToMat(image))
            {
                // 그레이스케일 변환
                using (var gray = new Mat())
                {
                    Cv2.CvtColor(mat, gray, ColorConversionCodes.BGR2GRAY);
                    // 얼굴 검출
                    var faces = _faceCascade.DetectMultiScale(gray, 1.1, 4);
                    foreach (var rect in faces)
                    {
                        // 코 위치는 얼굴 중앙 하단 근처로 임의 추정 (정밀 X)
                        var noseX = rect.X + rect.Width / 2;
                        var noseY = rect.Y + (int)(rect.Height * 0.6);
                        var noseSize = Math.Max(15, Math.Min(40, rect.Width / 8));
                        result.Add((new Point(noseX, noseY), noseSize));
                    }
                }
            }
            return result;
        }

        public override List<Point> DetectNosePositions(Bitmap image)
        {
            var withSize = DetectNosePositionsWithSize(image);
            var result = new List<Point>();
            foreach (var item in withSize)
                result.Add(item.Position);
            return result;
        }
    }
} 
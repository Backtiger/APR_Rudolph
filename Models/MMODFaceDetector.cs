using DlibDotNet;
using DlibDotNet.Dnn;
using DlibDotNet.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Point = System.Drawing.Point;

namespace APR_Rudolph.Models
{
    /// <summary>
    /// MMOD 기반의 얼굴 검출 기능을 제공하는 클래스입니다.
    /// </summary>
    public class MMODFaceDetector : BaseFaceDetector
    {
        private readonly LossMmod _cnnDetector;
        private readonly string _mmodPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mmod_human_face_detector.dat");


        public MMODFaceDetector() : base()
        {
            this._cnnDetector = LossMmod.Deserialize(_mmodPath);
        }

        // CNN 얼굴 검출 및 코 위치 반환
        public override List<Point> DetectNosePositions(Bitmap image)
        {
            return DetectNosePositions(image, 1);
        }

        public override List<(Point Position, int Size)> DetectNosePositionsWithSize(Bitmap image)
        {
            var result = new List<(Point Position, int Size)>();

            // 1. Bitmap → Array2D<RgbPixel>
            using (var array2D = image.ToArray2D<RgbPixel>())
            {
                // 2. Array2D → Matrix<RgbPixel>
                using (var matrix = new Matrix<RgbPixel>(array2D))
                {
                    // 3. CNN 얼굴 검출
                    var detections = this._cnnDetector.Operator<RgbPixel>(matrix, 1);

                    foreach (var face in detections[0])
                    {
                        var shape = this._shapePredictor.Detect(array2D, face.Rect);
                        var nosePoint = shape.GetPart(33);
                        
                        var faceSize = Math.Max(face.Rect.Width, face.Rect.Height);
                        var noseSize = Math.Max(15, Math.Min(40, faceSize / 8));
                        
                        result.Add((new Point(nosePoint.X, nosePoint.Y), (int)noseSize));
                    }
                }
            }

            return result;
        }

        public List<Point> DetectNosePositions(Bitmap image, ulong upsample)
        {
            var result = new List<Point>();

            // 1. Bitmap → Array2D<RgbPixel>
            using (var array2D = image.ToArray2D<RgbPixel>())
            {
                // 2. Array2D → Matrix<RgbPixel>
                using (var matrix = new Matrix<RgbPixel>(array2D))
                {
                    // 3. CNN 얼굴 검출
                    var detections = this._cnnDetector.Operator<RgbPixel>(matrix, upsample);

                    foreach (var face in detections[0])
                    {
                        var shape = this._shapePredictor.Detect(array2D, face.Rect);
                        var nosePoint = shape.GetPart(33);
                        result.Add(new Point(nosePoint.X, nosePoint.Y));
                    }
                }
            }

            return result;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this._cnnDetector?.Dispose();
            }

            base.Dispose();
        }
    }
}

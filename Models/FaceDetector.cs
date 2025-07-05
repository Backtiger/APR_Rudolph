using DlibDotNet;
using DlibDotNet.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Point = System.Drawing.Point;

namespace APR_Rudolph.Models
{
    /// <summary>
    /// 얼굴 검출의 기본 기능을 제공하는 클래스입니다.
    /// </summary>
    public class FaceDetector : BaseFaceDetector
    {
        private readonly FrontalFaceDetector _faceDetector;

        public FaceDetector() : base()
        {
            this._faceDetector = Dlib.GetFrontalFaceDetector();

        }

        // 얼굴 검출 및 코 위치와 크기 정보 반환 (이미지 입력)
        public override List<(Point Position, int Size)> DetectNosePositionsWithSize(Bitmap image)
        {
            var result = new List<(Point Position, int Size)>();

            using (var dlibImg = BitmapToArray2D(image))
            {
                // 검출 임계값을 더 낮춤 (0.5 -> 0.1)
                var faces = this._faceDetector.Operator(dlibImg, 0.1);
                System.Diagnostics.Debug.WriteLine($"Dlib 얼굴 검출: {faces.Length}개 발견");

                foreach (var face in faces)
                {
                    try
                    {
                        var shape = this._shapePredictor.Detect(dlibImg, face);
                        // 68개 랜드마크 중 코 tip은 34번째(인덱스 33)
                        var nosePoint = shape.GetPart(33);

                        // 얼굴 크기 계산 (얼굴 영역의 너비를 기준으로)
                        var faceWidth = face.Width;
                        var faceHeight = face.Height;
                        var faceSize = Math.Max(faceWidth, faceHeight);

                        // 얼굴 크기에 따라 코 크기 조절 (기본 25에서 최소 15, 최대 40)
                        var noseSize = Math.Max(15, Math.Min(40, faceSize / 8));

                        result.Add((new Point(nosePoint.X, nosePoint.Y), (int)noseSize));
                        System.Diagnostics.Debug.WriteLine($"코 위치: ({nosePoint.X}, {nosePoint.Y}), 크기: {noseSize}, 얼굴크기: {faceSize}");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"랜드마크 검출 오류: {ex.Message}");
                    }
                }
            }
            return result;
        }

        // Bitmap을 Dlib Array2D로 변환하는 메서드 (간단한 방법)
        private Array2D<RgbPixel> BitmapToArray2D(Bitmap bitmap)
        {
            // 24bppRgb 포맷으로 변환
            using (var rgbBitmap = new Bitmap(bitmap.Width, bitmap.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb))
            {
                using (var g = Graphics.FromImage(rgbBitmap))
                {
                    g.DrawImage(bitmap, 0, 0, bitmap.Width, bitmap.Height);
                }
                // DlibDotNet.Extensions의 확장 메서드 사용
                return rgbBitmap.ToArray2D<RgbPixel>();
            }
        }

        // 기존 메서드 (호환성 유지)
        public override List<Point> DetectNosePositions(Bitmap image)
        {
            var result = new List<Point>();
            var positionsWithSize = DetectNosePositionsWithSize(image);

            foreach (var item in positionsWithSize)
            {
                result.Add(item.Position);
            }

            return result;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this._faceDetector?.Dispose();
            }

            base.Dispose();
        }
    }
}
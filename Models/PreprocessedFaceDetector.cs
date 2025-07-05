using DlibDotNet;
using DlibDotNet.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using Point = System.Drawing.Point;

namespace APR_Rudolph.Models
{
    /// <summary>
    /// 전처리 기반 얼굴 검출 기능을 제공하는 클래스입니다.
    /// </summary>
    public class PreprocessedFaceDetector : BaseFaceDetector
    {
        private readonly FrontalFaceDetector _faceDetector;
        private readonly ImagePreprocessor _preprocessor;
        private readonly float _confidenceThreshold = 0.01f; // 더 낮은 임계값으로 얼굴 검출 민감도 증가

        public PreprocessedFaceDetector() : base()
        {
            this._faceDetector = Dlib.GetFrontalFaceDetector();
            this._preprocessor = new ImagePreprocessor();
        }

        /// <summary>
        /// 전처리된 이미지로 얼굴 검출
        /// </summary>
        public override List<(Point Position, int Size)> DetectNosePositionsWithSize(Bitmap image)
        {
            var result = new List<(Point Position, int Size)>();

            // 1. 이미지 전처리
            var preprocessedImage = _preprocessor.PreprocessForFaceDetection(image);
            System.Diagnostics.Debug.WriteLine("이미지 전처리 완료");

            // 2. 전처리된 이미지로 얼굴 검출
            using (var dlibImg = BitmapToArray2D(preprocessedImage))
            {
                var faces = this._faceDetector.Operator(dlibImg, _confidenceThreshold);
                System.Diagnostics.Debug.WriteLine($"전처리 후 얼굴 검출: {faces.Length}개 발견");

                foreach (var face in faces)
                {
                    try
                    {
                        var shape = this._shapePredictor.Detect(dlibImg, face);
                        var nosePoint = shape.GetPart(33);

                        var faceWidth = face.Width;
                        var faceHeight = face.Height;
                        var faceSize = Math.Max(faceWidth, faceHeight);
                        var noseSize = Math.Max(15, Math.Min(40, faceSize / 8));

                        result.Add((new Point(nosePoint.X, nosePoint.Y), (int)noseSize));
                        System.Diagnostics.Debug.WriteLine($"전처리 검출 - 코 위치: ({nosePoint.X}, {nosePoint.Y}), 크기: {noseSize}");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"전처리 검출 랜드마크 오류: {ex.Message}");
                    }
                }
            }

            // 3. 원본 이미지로도 검출 시도 (전처리로 놓친 얼굴이 있을 수 있음)
            if (result.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("전처리된 이미지에서 얼굴을 찾지 못함. 원본 이미지로 재시도...");
                using (var dlibImg = BitmapToArray2D(image))
                {
                    var faces = this._faceDetector.Operator(dlibImg, _confidenceThreshold);
                    System.Diagnostics.Debug.WriteLine($"원본 이미지 얼굴 검출: {faces.Length}개 발견");

                    foreach (var face in faces)
                    {
                        try
                        {
                            var shape = this._shapePredictor.Detect(dlibImg, face);
                            var nosePoint = shape.GetPart(33);

                            var faceWidth = face.Width;
                            var faceHeight = face.Height;
                            var faceSize = Math.Max(faceWidth, faceHeight);
                            var noseSize = Math.Max(15, Math.Min(40, faceSize / 8));

                            result.Add((new Point(nosePoint.X, nosePoint.Y), (int)noseSize));
                            System.Diagnostics.Debug.WriteLine($"원본 검출 - 코 위치: ({nosePoint.X}, {nosePoint.Y}), 크기: {noseSize}");
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"원본 검출 랜드마크 오류: {ex.Message}");
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 다양한 전처리 방법으로 얼굴 검출 시도
        /// </summary>
        public List<(Point Position, int Size)> DetectWithMultiplePreprocessing(Bitmap image)
        {
            var result = new List<(Point Position, int Size)>();

            // 1. 기본 전처리
            var preprocessedImage = _preprocessor.PreprocessForFaceDetection(image);
            var faces1 = DetectFromImage(preprocessedImage);
            result.AddRange(faces1);
            System.Diagnostics.Debug.WriteLine($"기본 전처리 검출: {faces1.Count}개");

            // 2. 어두운 이미지용 전처리 (얼굴이 어둡게 나온 경우)
            if (result.Count == 0)
            {
                var darkPreprocessed = _preprocessor.PreprocessForDarkImage(image);
                var faces2 = DetectFromImage(darkPreprocessed);
                result.AddRange(faces2);
                System.Diagnostics.Debug.WriteLine($"어두운 이미지 전처리 검출: {faces2.Count}개");
            }

            // 3. 노이즈 제거 전처리 (노이즈가 많은 이미지)
            if (result.Count == 0)
            {
                var noisePreprocessed = _preprocessor.PreprocessForNoisyImage(image);
                var faces3 = DetectFromImage(noisePreprocessed);
                result.AddRange(faces3);
                System.Diagnostics.Debug.WriteLine($"노이즈 제거 전처리 검출: {faces3.Count}개");
            }

            // 4. 다양한 크기로 검출 시도
            if (result.Count == 0)
            {
                var scaledFaces = DetectFromMultipleScales(image);
                result.AddRange(scaledFaces);
                System.Diagnostics.Debug.WriteLine($"다중 크기 검출: {scaledFaces.Count}개");
            }

            // 5. 원본 이미지 (마지막 시도)
            if (result.Count == 0)
            {
                var faces4 = DetectFromImage(image);
                result.AddRange(faces4);
                System.Diagnostics.Debug.WriteLine($"원본 이미지 검출: {faces4.Count}개");
            }

            return result;
        }

        /// <summary>
        /// 다양한 크기로 얼굴 검출 시도
        /// </summary>
        private List<(Point Position, int Size)> DetectFromMultipleScales(Bitmap image)
        {
            var result = new List<(Point Position, int Size)>();
            var scales = new[] { 0.5f, 0.75f, 1.0f, 1.25f, 1.5f };

            foreach (var scale in scales)
            {
                try
                {
                    var scaledWidth = (int)(image.Width * scale);
                    var scaledHeight = (int)(image.Height * scale);
                    
                    if (scaledWidth < 50 || scaledHeight < 50) continue; // 너무 작으면 건너뛰기

                    var scaledImage = new Bitmap(image, scaledWidth, scaledHeight);
                    var faces = DetectFromImage(scaledImage);

                    // 좌표를 원본 크기로 스케일링
                    foreach (var face in faces)
                    {
                        var originalPoint = new Point(
                            (int)(face.Position.X / scale),
                            (int)(face.Position.Y / scale)
                        );
                        result.Add((originalPoint, face.Size));
                    }

                    scaledImage.Dispose();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"크기 {scale} 검출 오류: {ex.Message}");
                }
            }

            return result;
        }

        private List<(Point Position, int Size)> DetectFromImage(Bitmap image)
        {
            var result = new List<(Point Position, int Size)>();

            using (var dlibImg = BitmapToArray2D(image))
            {
                var faces = this._faceDetector.Operator(dlibImg, _confidenceThreshold);

                foreach (var face in faces)
                {
                    try
                    {
                        var shape = this._shapePredictor.Detect(dlibImg, face);
                        var nosePoint = shape.GetPart(33);

                        var faceWidth = face.Width;
                        var faceHeight = face.Height;
                        var faceSize = Math.Max(faceWidth, faceHeight);
                        var noseSize = Math.Max(15, Math.Min(40, faceSize / 8));

                        result.Add((new Point(nosePoint.X, nosePoint.Y), (int)noseSize));
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"검출 랜드마크 오류: {ex.Message}");
                    }
                }
            }

            return result;
        }

        private Array2D<RgbPixel> BitmapToArray2D(Bitmap bitmap)
        {
            using (var rgbBitmap = new Bitmap(bitmap.Width, bitmap.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb))
            {
                using (var g = Graphics.FromImage(rgbBitmap))
                {
                    g.DrawImage(bitmap, 0, 0, bitmap.Width, bitmap.Height);
                }
                return rgbBitmap.ToArray2D<RgbPixel>();
            }
        }

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

            base.Dispose(disposing);
        }
    }
} 
using OpenCvSharp;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;
using Point = System.Drawing.Point;
using APR_Rudolph.Models;

namespace APR_Rudolph.Services
{
    /// <summary>
    /// 이미지 변환, 루돌프 코 합성 등 이미지 처리 기능을 담당하는 서비스입니다.
    /// </summary>
    public class ImageProcessingService : IImageProcessingService
    {
        public BitmapImage AddRudolphNose(BitmapImage inputImage, Point[] nosePoints, int[] noseSizes)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"AddRudolphNose 시작: {nosePoints.Length}개 얼굴");
                
                // BitmapImage를 Bitmap으로 변환
                var bmp = BitmapImageToBitmap(inputImage);
                if (bmp == null || bmp.Width <= 0 || bmp.Height <= 0)
                {
                    System.Diagnostics.Debug.WriteLine("AddRudolphNose: 유효하지 않은 Bitmap");
                    return inputImage; // 원본 반환
                }
                
                // Bitmap에 직접 루돌프 코 그리기 (OpenCV 사용하지 않음)
                using (var g = Graphics.FromImage(bmp))
                {
                    for (int i = 0; i < nosePoints.Length; i++)
                    {
                        var nosePoint = nosePoints[i];
                        var noseSize = noseSizes[i];
                        // 좌표 유효성 검사
                        if (nosePoint.X >= 0 && nosePoint.X < bmp.Width && 
                            nosePoint.Y >= 0 && nosePoint.Y < bmp.Height)
                        {
                            // 루돌프 코(빨간 원) 그리기
                            using (var brush = new SolidBrush(Color.Red))
                            {
                                g.FillEllipse(brush, nosePoint.X - noseSize, nosePoint.Y - noseSize, noseSize * 2, noseSize * 2);
                            }
                            System.Diagnostics.Debug.WriteLine($"루돌프 코 그리기: 위치({nosePoint.X}, {nosePoint.Y}), 크기: {noseSize}");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"유효하지 않은 좌표: ({nosePoint.X}, {nosePoint.Y})");
                        }
                    }
                }
                
                // Bitmap을 BitmapImage로 변환
                var result = BitmapToBitmapImage(bmp);
                System.Diagnostics.Debug.WriteLine("AddRudolphNose 완료");
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AddRudolphNose 오류: {ex.Message}");
                return inputImage; // 오류 발생시 원본 반환
            }
        }

        public Bitmap BitmapImageToBitmap(BitmapImage bitmapImage)
        {
            try
            {
                using (var ms = new MemoryStream())
                {
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
                    encoder.Save(ms);
                    ms.Position = 0;
                    return new Bitmap(ms);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"BitmapImageToBitmap 오류: {ex.Message}");
                return null;
            }
        }

        public BitmapImage BitmapToBitmapImage(Bitmap bitmap)
        {
            try
            {
                // 입력 검증
                if (bitmap == null || bitmap.Width <= 0 || bitmap.Height <= 0)
                {
                    System.Diagnostics.Debug.WriteLine("BitmapToBitmapImage: 유효하지 않은 Bitmap");
                    return CreateEmptyBitmapImage();
                }

                System.Diagnostics.Debug.WriteLine($"BitmapToBitmapImage: {bitmap.Width}x{bitmap.Height}, Format: {bitmap.PixelFormat}");
                
                // Bitmap 복사본 생성 (리소스 충돌 방지)
                Bitmap bitmapCopy;
                try
                {
                    bitmapCopy = new Bitmap(bitmap);
                }
                catch (InvalidOperationException)
                {
                    // Bitmap이 사용 중인 경우 잠시 대기 후 재시도
                    System.Threading.Thread.Sleep(10);
                    bitmapCopy = new Bitmap(bitmap);
                }
                
                using (bitmapCopy)
                using (var ms = new MemoryStream())
                {
                    bitmapCopy.Save(ms, ImageFormat.Png);
                    ms.Position = 0;

                    var bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.StreamSource = ms;
                    bitmapImage.EndInit();
                    bitmapImage.Freeze(); // UI 스레드에서 안전하게 사용하기 위해

                    System.Diagnostics.Debug.WriteLine("BitmapImage 변환 성공");
                    return bitmapImage;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"BitmapToBitmapImage 오류: {ex.Message}");
                // 오류 발생시 빈 이미지 반환
                return CreateEmptyBitmapImage();
            }
        }

        private BitmapImage CreateEmptyBitmapImage()
        {
            try
            {
                var bitmap = new Bitmap(640, 480);
                using (var g = Graphics.FromImage(bitmap))
                {
                    g.Clear(Color.Gray); // 회색 배경으로 변경
                    g.DrawString("웹캠 연결 중...", new Font("Arial", 20), Brushes.White, 200, 200);
                }
                
                using (var ms = new MemoryStream())
                {
                    bitmap.Save(ms, ImageFormat.Png);
                    ms.Position = 0;
                    
                    var bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.StreamSource = ms;
                    bitmapImage.EndInit();
                    bitmapImage.Freeze();
                    
                    return bitmapImage;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CreateEmptyBitmapImage 오류: {ex.Message}");
                // 최후의 수단으로 1x1 픽셀 이미지 반환
                var tinyBitmap = new Bitmap(1, 1);
                using (var ms = new MemoryStream())
                {
                    tinyBitmap.Save(ms, ImageFormat.Png);
                    ms.Position = 0;
                    
                    var bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.StreamSource = ms;
                    bitmapImage.EndInit();
                    bitmapImage.Freeze();
                    
                    return bitmapImage;
                }
            }
        }

        public void SaveImage(BitmapImage image, string filePath)
        {
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(image));
                encoder.Save(fileStream);
            }
        }

        private Mat BitmapToMat(Bitmap bitmap)
        {
            try
            {
                using (var ms = new MemoryStream())
                {
                    bitmap.Save(ms, ImageFormat.Bmp);
                    ms.Position = 0;
                    return Cv2.ImDecode(ms.ToArray(), ImreadModes.Color);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"BitmapToMat 오류: {ex.Message}");
                return new Mat();
            }
        }

        private Bitmap MatToBitmap(Mat mat)
        {
            try
            {
                // 입력 검증
                if (mat.Empty() || mat.Width <= 0 || mat.Height <= 0)
                {
                    System.Diagnostics.Debug.WriteLine("MatToBitmap: 유효하지 않은 Mat");
                    return new Bitmap(1, 1);
                }

                // BGR에서 RGB로 변환
                using (var rgbMat = new Mat())
                {
                    Cv2.CvtColor(mat, rgbMat, ColorConversionCodes.BGR2RGB);
                    var imageBytes = rgbMat.ImEncode(".bmp");
                    
                    if (imageBytes == null || imageBytes.Length == 0)
                    {
                        System.Diagnostics.Debug.WriteLine("MatToBitmap: 이미지 인코딩 실패");
                        return new Bitmap(1, 1);
                    }
                    
                    using (var ms = new MemoryStream(imageBytes))
                    {
                        return new Bitmap(ms);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"MatToBitmap 오류: {ex.Message}");
                // 오류 발생시 빈 비트맵 반환
                return new Bitmap(1, 1);
            }
        }

        private BitmapImage MatToBitmapImage(Mat mat)
        {
            var bitmap = MatToBitmap(mat);
            return BitmapToBitmapImage(bitmap);
        }

        // 테스트용: 두 얼굴 검출 모델로 이미지 검출 결과를 각각 저장
        public void TestFaceDetectionOnImage(string imagePath)
        {
            if (!System.IO.File.Exists(imagePath))
            {
                System.Diagnostics.Debug.WriteLine($"이미지 파일이 존재하지 않습니다: {imagePath}");
                return;
            }
            var bmp = new Bitmap(imagePath);

            // Dlib(PreprocessedFaceDetector) 결과
            var dlibDetector = new PreprocessedFaceDetector();
            var dlibResults = dlibDetector.DetectNosePositionsWithSize(bmp);
            var dlibNosePoints = new System.Drawing.Point[dlibResults.Count];
            var dlibNoseSizes = new int[dlibResults.Count];
            for (int i = 0; i < dlibResults.Count; i++)
            {
                dlibNosePoints[i] = dlibResults[i].Position;
                dlibNoseSizes[i] = dlibResults[i].Size;
            }
            var dlibResultImage = AddRudolphNose(BitmapToBitmapImage(bmp), dlibNosePoints, dlibNoseSizes);
            SaveImage(dlibResultImage, "dlib_result.png");
            System.Diagnostics.Debug.WriteLine($"Dlib 결과 저장 완료: dlib_result.png");

            // HaarCascade 결과
            try
            {
                var haarDetector = new HaarCascadeFaceDetector();
                var haarResults = haarDetector.DetectNosePositionsWithSize(bmp);
                var haarNosePoints = new System.Drawing.Point[haarResults.Count];
                var haarNoseSizes = new int[haarResults.Count];
                for (int i = 0; i < haarResults.Count; i++)
                {
                    haarNosePoints[i] = haarResults[i].Position;
                    haarNoseSizes[i] = haarResults[i].Size;
                }
                var haarResultImage = AddRudolphNose(BitmapToBitmapImage(bmp), haarNosePoints, haarNoseSizes);
                SaveImage(haarResultImage, "haar_result.png");
                System.Diagnostics.Debug.WriteLine($"HaarCascade 결과 저장 완료: haar_result.png");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"HaarCascadeFaceDetector 오류: {ex.Message}");
            }
        }
    }
} 
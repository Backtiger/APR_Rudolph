using OpenCvSharp;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace APR_Rudolph.Models
{
    /// <summary>
    /// 이미지 전처리 기능을 제공하는 클래스입니다.
    /// </summary>
    public class ImagePreprocessor
    {
        /// <summary>
        /// 이미지 전처리 옵션들
        /// </summary>
        public class PreprocessingOptions
        {
            /// <summary>
            /// 히스토그램 평활화 적용 여부. 이미지의 대비를 개선하여 얼굴 검출 정확도를 향상시킵니다.
            /// </summary>
            public bool ApplyHistogramEqualization { get; set; } = true;
            
            /// <summary>
            /// 가우시안 블러 적용 여부. 노이즈를 줄이지만 얼굴 검출 정확도가 떨어질 수 있습니다.
            /// </summary>
            public bool ApplyGaussianBlur { get; set; } = false;
            
            /// <summary>
            /// 양방향 필터 적용 여부. 엣지를 보존하면서 노이즈를 제거하여 얼굴 검출에 유리합니다.
            /// </summary>
            public bool ApplyBilateralFilter { get; set; } = true;
            
            /// <summary>
            /// 대비 향상 적용 여부. 이미지의 대비를 강화하여 얼굴 검출 정확도를 향상시킵니다.
            /// </summary>
            public bool ApplyContrastEnhancement { get; set; } = true;
            
            /// <summary>
            /// 노이즈 제거 적용 여부. 중간값 필터를 사용하여 노이즈를 제거합니다.
            /// </summary>
            public bool ApplyNoiseReduction { get; set; } = true;
            
            /// <summary>
            /// 밝기 조정값. -1.0(어둡게) ~ 1.0(밝게) 범위에서 조정 가능합니다.
            /// </summary>
            public double BrightnessAdjustment { get; set; } = 0.0; // -1.0 ~ 1.0
            
            /// <summary>
            /// 대비 조정값. 0.5(낮은 대비) ~ 2.0(높은 대비) 범위에서 조정 가능합니다.
            /// </summary>
            public double ContrastAdjustment { get; set; } = 1.2; // 0.5 ~ 2.0
            
            /// <summary>
            /// 가우시안 블러의 커널 크기. 홀수 값(3, 5, 7 등)을 사용하며, 클수록 더 강한 블러가 적용됩니다.
            /// </summary>
            public int GaussianKernelSize { get; set; } = 3;
            
            /// <summary>
            /// 양방향 필터의 색상 표준편차. 클수록 더 많은 색상이 혼합되어 스무딩 효과가 강해집니다.
            /// </summary>
            public double BilateralSigmaColor { get; set; } = 75.0;
            
            /// <summary>
            /// 양방향 필터의 공간 표준편차. 클수록 더 먼 픽셀들이 혼합되어 스무딩 효과가 강해집니다.
            /// </summary>
            public double BilateralSigmaSpace { get; set; } = 75.0;
            
            /// <summary>
            /// 샤프닝(선명도 강화) 적용 여부. 이미지의 선명도를 강화하여 얼굴 검출 정확도를 향상시킵니다.
            /// </summary>
            public bool ApplySharpening { get; set; } = false;
        }

        /// <summary>
        /// 이미지 전처리 수행
        /// </summary>
        public Bitmap PreprocessImage(Bitmap originalImage, PreprocessingOptions? options = null)
        {
            options ??= new PreprocessingOptions();

            using (var mat = BitmapToMat(originalImage))
            {
                var processedMat = mat.Clone();

                // 1. 노이즈 제거
                if (options.ApplyNoiseReduction)
                {
                    processedMat = ApplyNoiseReduction(processedMat);
                }

                // 2. 가우시안 블러 (선택적)
                if (options.ApplyGaussianBlur)
                {
                    processedMat = ApplyGaussianBlur(processedMat, options.GaussianKernelSize);
                }

                // 3. 양방향 필터 (엣지 보존 스무딩)
                if (options.ApplyBilateralFilter)
                {
                    processedMat = ApplyBilateralFilter(processedMat, options.BilateralSigmaColor, options.BilateralSigmaSpace);
                }

                // 4. 히스토그램 평활화
                if (options.ApplyHistogramEqualization)
                {
                    processedMat = ApplyHistogramEqualization(processedMat);
                }

                // 5. 밝기/대비 조정
                if (options.BrightnessAdjustment != 0.0 || options.ContrastAdjustment != 1.0)
                {
                    processedMat = ApplyBrightnessContrast(processedMat, options.BrightnessAdjustment, options.ContrastAdjustment);
                }

                // 6. 대비 향상
                if (options.ApplyContrastEnhancement)
                {
                    processedMat = ApplyContrastEnhancement(processedMat);
                }

                // 7. 샤프닝(선명도 강화)
                if (options.ApplySharpening)
                {
                    processedMat = ApplySharpening(processedMat);
                }

                return MatToBitmap(processedMat);
            }
        }

        /// <summary>
        /// 노이즈 제거 (중간값 필터)
        /// </summary>
        private Mat ApplyNoiseReduction(Mat input)
        {
            var output = new Mat();
            Cv2.MedianBlur(input, output, 3);
            return output;
        }

        /// <summary>
        /// 가우시안 블러 적용
        /// </summary>
        private Mat ApplyGaussianBlur(Mat input, int kernelSize)
        {
            var output = new Mat();
            Cv2.GaussianBlur(input, output, new OpenCvSharp.Size(kernelSize, kernelSize), 0, 0);
            return output;
        }

        /// <summary>
        /// 양방향 필터 적용 (엣지 보존 스무딩)
        /// </summary>
        private Mat ApplyBilateralFilter(Mat input, double sigmaColor, double sigmaSpace)
        {
            var output = new Mat();
            Cv2.BilateralFilter(input, output, 9, sigmaColor, sigmaSpace);
            return output;
        }

        /// <summary>
        /// 히스토그램 평활화 (CLAHE 사용)
        /// </summary>
        private Mat ApplyHistogramEqualization(Mat input)
        {
            var output = new Mat();
            
            // LAB 색공간으로 변환
            Cv2.CvtColor(input, output, ColorConversionCodes.BGR2Lab);
            
            // L 채널에 대해 CLAHE 적용
            var labChannels = new Mat[3];
            Cv2.Split(output, out labChannels);
            
            var clahe = Cv2.CreateCLAHE(2.0, new OpenCvSharp.Size(8, 8));
            clahe.Apply(labChannels[0], labChannels[0]);
            
            // 다시 합치기
            Cv2.Merge(labChannels, output);
            
            // BGR로 변환
            Cv2.CvtColor(output, output, ColorConversionCodes.Lab2BGR);
            
            return output;
        }

        /// <summary>
        /// 밝기/대비 조정
        /// </summary>
        private Mat ApplyBrightnessContrast(Mat input, double brightness, double contrast)
        {
            var output = new Mat();
            input.ConvertTo(output, -1, contrast, brightness * 255);
            return output;
        }

        /// <summary>
        /// 대비 향상
        /// </summary>
        private Mat ApplyContrastEnhancement(Mat input)
        {
            var output = new Mat();
            
            // 히스토그램 계산
            var hist = new Mat();
            Cv2.CalcHist(new[] { input }, new[] { 0 }, null, hist, 1, new[] { 256 }, new[] { new Rangef(0, 256) });
            
            // 히스토그램 스트레칭
            double minVal, maxVal;
            Cv2.MinMaxLoc(input, out minVal, out maxVal);
            
            var stretched = new Mat();
            input.ConvertTo(stretched, -1, 255.0 / (maxVal - minVal), -minVal * 255.0 / (maxVal - minVal));
            
            return stretched;
        }

        /// <summary>
        /// 샤프닝(선명도 강화) 함수 추가
        /// </summary>
        private Mat ApplySharpening(Mat input)
        {
            var output = new Mat();
            float[] kernelData = { 0, -1, 0, -1, 5, -1, 0, -1, 0 };
            var kernel = new Mat(3, 3, MatType.CV_32F);
            Marshal.Copy(kernelData, 0, kernel.Data, kernelData.Length);
            Cv2.Filter2D(input, output, input.Type(), kernel);
            return output;
        }

        /// <summary>
        /// 얼굴 검출에 최적화된 전처리
        /// </summary>
        public Bitmap PreprocessForFaceDetection(Bitmap originalImage)
        {
            var options = new PreprocessingOptions
            {
                ApplyHistogramEqualization = true,
                ApplyGaussianBlur = false,
                ApplyBilateralFilter = true,
                ApplyContrastEnhancement = true,
                ApplyNoiseReduction = true,
                BrightnessAdjustment = 0.2, // 더 밝게
                ContrastAdjustment = 1.7,   // 더 강한 대비
                BilateralSigmaColor = 100.0,
                BilateralSigmaSpace = 100.0,
                ApplySharpening = true // 샤프닝 적용
            };

            return PreprocessImage(originalImage, options);
        }

        /// <summary>
        /// 어두운 이미지용 전처리
        /// </summary>
        public Bitmap PreprocessForDarkImage(Bitmap originalImage)
        {
            var options = new PreprocessingOptions
            {
                ApplyHistogramEqualization = true,
                ApplyGaussianBlur = false,
                ApplyBilateralFilter = true,
                ApplyContrastEnhancement = true,
                ApplyNoiseReduction = true,
                BrightnessAdjustment = 0.3, // 더 밝게
                ContrastAdjustment = 1.5,   // 대비 더 강화
                BilateralSigmaColor = 100.0,
                BilateralSigmaSpace = 100.0
            };

            return PreprocessImage(originalImage, options);
        }

        /// <summary>
        /// 노이즈가 많은 이미지용 전처리
        /// </summary>
        public Bitmap PreprocessForNoisyImage(Bitmap originalImage)
        {
            var options = new PreprocessingOptions
            {
                ApplyHistogramEqualization = true,
                ApplyGaussianBlur = true,
                ApplyBilateralFilter = true,
                ApplyContrastEnhancement = true,
                ApplyNoiseReduction = true,
                BrightnessAdjustment = 0.0,
                ContrastAdjustment = 1.2,
                GaussianKernelSize = 5,
                BilateralSigmaColor = 50.0,
                BilateralSigmaSpace = 50.0
            };

            return PreprocessImage(originalImage, options);
        }

        /// <summary>
        /// Bitmap을 Mat으로 변환
        /// </summary>
        private Mat BitmapToMat(Bitmap bitmap)
        {
            using (var ms = new System.IO.MemoryStream())
            {
                bitmap.Save(ms, ImageFormat.Bmp);
                ms.Position = 0;
                return Cv2.ImDecode(ms.ToArray(), ImreadModes.Color);
            }
        }

        /// <summary>
        /// Mat을 Bitmap으로 변환
        /// </summary>
        private Bitmap MatToBitmap(Mat mat)
        {
            var imageBytes = mat.ImEncode(".bmp");
            using (var ms = new System.IO.MemoryStream(imageBytes))
            {
                return new Bitmap(ms);
            }
        }
    }
} 
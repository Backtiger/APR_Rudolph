using System.Drawing;
using System.Windows.Media.Imaging;

namespace APR_Rudolph.Services
{
    public interface IImageProcessingService
    {
        /// <summary>
        /// 이미지에 루돌프 코 합성
        /// </summary>
        BitmapImage AddRudolphNose(BitmapImage inputImage, System.Drawing.Point[] nosePoints, int[] noseSizes);

        /// <summary>
        /// BitmapImage를 Bitmap으로 변환
        /// </summary>
        Bitmap BitmapImageToBitmap(BitmapImage bitmapImage);

        /// <summary>
        /// Bitmap을 BitmapImage로 변환
        /// </summary>
        BitmapImage BitmapToBitmapImage(Bitmap bitmap);

        /// <summary>
        /// 이미지 저장
        /// </summary>
        void SaveImage(BitmapImage image, string filePath);
    }
} 
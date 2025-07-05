using DlibDotNet;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Point = System.Drawing.Point;

namespace APR_Rudolph.Models
{
    /// <summary>
    /// 얼굴 검출기 클래스의 추상 베이스 클래스입니다.
    /// </summary>
    public abstract class BaseFaceDetector : IDisposable
    {
        private bool _disposed = false;
        protected readonly ShapePredictor _shapePredictor;

        public BaseFaceDetector()
        {
            var shapePredictorPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "shape_predictor_68_face_landmarks.dat");
            this._shapePredictor = ShapePredictor.Deserialize(shapePredictorPath);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);  // 소멸자 호출 막기
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                // 관리되는 리소스 해제
                _shapePredictor?.Dispose();
            }

            // unmanaged 리소스 해제 (있다면)
            _disposed = true;
        }        

        ~BaseFaceDetector()
        {
            Dispose(false);
        }

        // 추상 메서드들
        public abstract List<(Point Position, int Size)> DetectNosePositionsWithSize(Bitmap image);
        public abstract List<Point> DetectNosePositions(Bitmap image);
    }
}

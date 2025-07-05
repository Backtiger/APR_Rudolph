using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using Point = System.Drawing.Point;

namespace APR_Rudolph.Services
{
    public interface IRealtimeFaceDetectionService
    {
        /// <summary>
        /// 실시간 얼굴 검출 시작
        /// </summary>
        Task StartAsync();

        /// <summary>
        /// 실시간 얼굴 검출 중지
        /// </summary>
        void Stop();

        /// <summary>
        /// 검출된 얼굴 위치들
        /// </summary>
        List<(Point Position, int Size)> DetectedFaces { get; }

        /// <summary>
        /// 얼굴 검출 결과 업데이트 이벤트
        /// </summary>
        event EventHandler<List<(Point Position, int Size)>> FacesDetected;

        /// <summary>
        /// 서비스 상태
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// 프레임에서 얼굴 검출
        /// </summary>
        Task<List<(System.Drawing.Point Position, int Size)>> DetectFacesFromFrame(Bitmap frame);
    }
} 
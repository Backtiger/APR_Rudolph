using System;
using System.Collections.Generic;
using System.Drawing;
using Point = System.Drawing.Point;

namespace APR_Rudolph.Services
{
    public interface IFaceDetectionService
    {
        /// <summary>
        /// 얼굴 검출 및 코 위치 반환
        /// </summary>
        List<(Point Position, int Size)> DetectNosePositionsWithSize(Bitmap image);

        /// <summary>
        /// 코 위치만 반환 (크기 정보 없음)
        /// </summary>
        List<Point> DetectNosePositions(Bitmap image);

        /// <summary>
        /// 서비스 초기화 상태 확인
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// 서비스 초기화
        /// </summary>
        void Initialize();

        /// <summary>
        /// 리소스 해제
        /// </summary>
        void Dispose();
    }
} 
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace APR_Rudolph.Services
{
    public interface IWebcamService
    {
        /// <summary>
        /// 웹캠 시작
        /// </summary>
        Task StartAsync();

        /// <summary>
        /// 웹캠 중지
        /// </summary>
        void Stop();

        /// <summary>
        /// 웹캠 상태
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// 현재 프레임 가져오기
        /// </summary>
        BitmapImage? GetCurrentFrame();

        /// <summary>
        /// 프레임 업데이트 이벤트
        /// </summary>
        event EventHandler<BitmapImage> FrameUpdated;

        /// <summary>
        /// 웹캠 오류 이벤트
        /// </summary>
        event EventHandler<string> ErrorOccurred;

        /// <summary>
        /// 얼굴 검출 결과 이벤트
        /// </summary>
        event EventHandler<List<(System.Drawing.Point Position, int Size)>>? FacesDetected;

        // 웹캠 설정 조절 메서드들
        /// <summary>
        /// 밝기 설정 (-1.0 ~ 1.0)
        /// </summary>
        void SetBrightness(double value);

        /// <summary>
        /// 대비 설정 (0.0 ~ 2.0)
        /// </summary>
        void SetContrast(double value);

        /// <summary>
        /// 노출 설정 (-1.0 ~ 1.0)
        /// </summary>
        void SetExposure(double value);

        /// <summary>
        /// 게인 설정 (0.0 ~ 1.0)
        /// </summary>
        void SetGain(double value);

        /// <summary>
        /// 자동 노출 설정
        /// </summary>
        void SetAutoExposure(bool enabled);

        /// <summary>
        /// 현재 웹캠 설정값 가져오기
        /// </summary>
        (double Brightness, double Contrast, double Exposure, double Gain, bool AutoExposure) GetCurrentSettings();

  
    }
} 
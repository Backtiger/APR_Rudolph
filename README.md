# APR_Rudolph (루돌프 코 합성기)

> **중요: 얼굴 검출 모델 파일 다운로드 안내**
> 
> 본 프로그램은 dlib 기반 얼굴 검출 모델 파일(`shape_predictor_68_face_landmarks.dat`, `mmod_human_face_detector.dat`)이 필요합니다. 이 파일들은 용량 문제로 깃허브에 포함되어 있지 않으니, 아래 링크에서 직접 다운로드하여 `APR_Rudolph` 폴더(실행 파일과 같은 위치)에 넣어주세요.
>
> - [shape_predictor_68_face_landmarks.dat 다운로드](https://dlib.net/files/shape_predictor_68_face_landmarks.dat.bz2)
>   (압축 해제 후 `shape_predictor_68_face_landmarks.dat` 파일 사용)
> - [mmod_human_face_detector.dat 다운로드](https://dlib.net/files/mmod_human_face_detector.dat.bz2)
>   (압축 해제 후 `mmod_human_face_detector.dat` 파일 사용)
>
> **파일 위치:**
> - `APR_Rudolph/shape_predictor_68_face_landmarks.dat`
> - `APR_Rudolph/mmod_human_face_detector.dat`

## 프로그램 개요

APR_Rudolph는 WPF(.NET) 기반의 실시간 얼굴 인식 및 루돌프 코 합성 프로그램입니다. 웹캠 또는 이미지에서 얼굴을 검출하고, 각 얼굴의 코 위치에 빨간 루돌프 코를 합성해줍니다. MVVM 패턴, 커스텀 메시지박스, Behavior 기반 윈도우 드래그 등 현대적인 WPF 구조를 적용했습니다.

---

## 개발 환경 설정

### 필수 요구사항
- **Visual Studio 2022** 
- **.NET 8.0 SDK**
- **Windows 10/11** (WPF 애플리케이션)

### 개발 환경 구축
1. **소스코드 다운로드**
   ```bash
   git clone https://github.com/backtiger/APR_Rudolph.git
   cd APR_Rudolph
   ```

2. **NuGet 패키지 복원**
   - Visual Studio에서 솔루션을 열면 자동으로 복원됩니다
   - 또는 명령줄에서: `dotnet restore`

3. **모델 파일 다운로드** (위의 다운로드 링크 참조)
   - `shape_predictor_68_face_landmarks.dat`
   - `mmod_human_face_detector.dat`
   - 파일들을 `APR_Rudolph` 폴더에 위치시킵니다

4. **빌드 및 실행**
   ```bash
   dotnet build
   dotnet run --project APR_Rudolph
   ```

---

## 주요 기능
- 실시간 웹캠 얼굴 검출 및 루돌프 코 합성
- 정적 이미지 얼굴 검출 및 루돌프 코 합성
- 커스텀 메시지박스(테마 일치, 드래그 이동 가능)
- 커스텀 타이틀바/닫기 버튼, Behavior 기반 윈도우 드래그
- 이미지 줌/팬/리셋 등 뷰어 기능
- 탭 전환 시 상태 자동 초기화
- MVVM 패턴, DI, 커맨드, Behavior 등 적용

---

## 개발/실행 환경
- **운영체제:** Windows 10 이상
- **프레임워크:** .NET 8.0 이상 (WPF)
- **주요 라이브러리:**
  - OpenCvSharp (웹캠/이미지 처리)
  - DlibDotNet (얼굴 검출/랜드마크)
  - CommunityToolkit.Mvvm (MVVM)
  - Microsoft즈
---

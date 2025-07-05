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
- **Visual Studio 2022** 또는 **Visual Studio Code** (C# 개발 환경)
- **.NET 8.0 SDK** 이상
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
  - Microsoft.Xaml.Behaviors.Wpf (Behavior)
- **추가 파일:**
  - shape_predictor_68_face_landmarks.dat (dlib 랜드마크 모델)
  - mmod_human_face_detector.dat (dlib MMOD 얼굴 검출 모델)

---

## 실행 방법
1. `git clone` 또는 소스 다운로드
2. Visual Studio에서 솔루션 열기
3. NuGet 패키지 복원
4. `APR_Rudolph` 프로젝트를 시작 프로젝트로 설정
5. 빌드 및 실행

---

## 폴더 구조
- `ViewModels/` : MVVM ViewModel
- `Views/` : MainWindow, CustomMessageBox 등 WPF 창/컨트롤
- `Controls/` : 이미지/웹캠/합성 등 UserControl
- `Services/` : 얼굴 검출, 이미지 처리, 웹캠 등 서비스
- `Models/` : 얼굴 검출기, 전처리 등 모델
- `Behaviors/` : MVVM 스타일 Behavior (윈도우 드래그 등)
- `Resources/` : 리소스 파일

---

## 기타 참고사항
- 얼굴 검출 정확도는 조명, 얼굴 각도, 해상도 등에 따라 달라질 수 있습니다.
- dlib 모델 파일(약 100MB)은 반드시 프로젝트 루트에 위치해야 합니다.
- 커스텀 메시지박스, Behavior 등은 MVVM 패턴을 최대한 준수하여 구현되었습니다.

# APR_Rudolph 사용법

## 소개
APR_Rudolph는 웹캠 또는 이미지에서 얼굴을 검출하고, 얼굴에 루돌프 코(빨간 코)를 합성해주는 윈도우 데스크탑 프로그램입니다.

---

## 설치 및 실행

1. **설치 파일(Setup.exe) 실행**
   - 제공된 설치 파일을 실행하여 프로그램을 설치합니다.
   - 설치가 완료되면 바탕화면 또는 시작 메뉴에서 "APR_Rudolph"를 실행하세요.

2. **직접 빌드(개발자용)**
   - Visual Studio에서 솔루션을 열고 빌드 후 실행할 수 있습니다.

---

## 주요 기능

- **실시간 웹캠 얼굴 검출 및 루돌프 코 합성**
- **이미지 파일에서 얼굴 검출 및 루돌프 코 합성**
- **여러 얼굴이 있을 경우 모두 인식**
- **간단한 UI, 버튼 클릭만으로 사용 가능**

---

## 사용법

### 1. 실시간 웹캠 모드

1. 프로그램 실행 후, "실시간 웹캠" 탭으로 이동합니다.
2. `📹 웹캠 시작` 버튼을 클릭하면 웹캠 영상이 표시됩니다.
3. `🎅 루돌프 코 ON` 버튼을 클릭하면 얼굴이 검출되고, 얼굴에 빨간 코가 합성됩니다.
4. `⏹️ 웹캠 중지` 버튼으로 웹캠을 종료할 수 있습니다.

> **주의:**
> - 얼굴이 잘 검출되지 않으면 조명을 밝게 하고, 카메라를 정면으로 바라보세요.
> - 웹캠 화질이 너무 낮거나 어두우면 얼굴 검출이 어려울 수 있습니다.

### 2. 이미지 파일 모드

1. "이미지" 탭으로 이동합니다.
2. `이미지 열기` 버튼을 클릭하여 얼굴이 포함된 이미지를 선택합니다.
3. `루돌프 코 합성` 버튼을 클릭하면 얼굴에 빨간 코가 합성된 결과가 표시됩니다.
4. `저장` 버튼으로 결과 이미지를 PNG 파일로 저장할 수 있습니다.

---

## 기타 안내

- **지원 카메라:** 대부분의 노트북 내장 웹캠, USB 웹캠 지원
- **권장 환경:**
  - Windows 10/11
  - .NET 8.0 이상
  - 밝은 환경, 정면 얼굴, 깨끗한 렌즈
- **오류/문의:**
  - 얼굴이 검출되지 않거나 프로그램이 비정상적으로 동작하면, 카메라 환경을 점검하거나 관리자에게 문의하세요.

---

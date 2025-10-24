## 작업 정리 (2025-10-25)

### 무엇을 만들었나
- URP Full Screen Pass 기반 리플(Post-Effect)
- HLSL 셰이더: Hidden/Fullscreen/Ripple URP
- 인터랙션 스크립트: FullscreenRippleInteractive.cs (마우스·터치 위치를 리플 중심으로 전달, 슬라이더로 진폭·주파수·속도 제어)

### 산출물 링크
- 셰이더: [Assets/Shaders/FullscreenRipple.shader](Assets/Shaders/FullscreenRipple.shader)
- 머티리얼: [Assets/Materials/M_FullscreenRipple.mat](Assets/Materials/M_FullscreenRipple.mat)
- 스크립트: [Assets/Scripts/FullscreenRippleInteractive.cs](Assets/Scripts/FullscreenRippleInteractive.cs)
- 데모 캡처(권장 위치): [docs/demo.gif](docs/demo.gif), [docs/demo.mp4](docs/demo.mp4)

### 핵심 설정 스냅샷
- Renderer Asset: PC_RPAsset
- Renderer Data: PC_Renderer
  - Renderer Features → Full Screen Pass (이름 예: RipplePass)
    - Injection: After Rendering Post Processing
    - Fetch/Require Color Buffer: On
    - Pass Material: M_FullscreenRipple
  - Compatibility → Intermediate Texture = Always
- Project Settings
  - Graphics / Quality 모두 PC_RPAsset 사용
- Main Camera
  - Rendering → Renderer = Default(PC_Renderer)

### 해결한 문제와 원인
- 머티리얼이 URP/Lit로 남아 커스텀 파라미터 미노출
  - 머티리얼의 Shader를 Hidden/Fullscreen/Ripple URP로 교체하여 해결
- Fullscreen.hlsl 경로 에러로 핑크 화면
  - URP 버전 의존을 제거하고 SV_VertexID 풀스크린 삼각형 방식으로 수정
- 경고: Full Screen feature will not execute - no material is assigned
  - PC_Renderer → RipplePass → Pass Material 참조 끊김을 재지정하여 해결
- 이펙트 미표시
  - Fetch Color Buffer 비활성, Renderer/Asset 미스매치 등을 정리하여 해결

### 사용/재현 방법
1. 빈 오브젝트 RippleController 생성 후 FullscreenRippleInteractive.cs 추가
2. Ripple Mat 슬롯에 M_FullscreenRipple 연결
3. Canvas에 Slider 3개(Amplitude, Frequency, Speed) 만들고 스크립트 슬롯에 각각 연결
4. Play 실행
   - 포인터 위치에서 리플이 생성되고 슬라이더로 강도·속도가 조절되면 성공

### 배운 점
- Renderer Data(UniversalRendererData)에 Feature를 추가해야 하며 Renderer Asset과 혼동하지 않는다
- Injection 시점에 따라 후처리 결과가 달라진다
  - After Post: 최종 화면을 통째로 왜곡
  - Before Post: 왜곡 후 블룸/컬러그레이딩이 적용
- Color Buffer 확보가 선행되어야 카메라 컬러 텍스처 샘플링이 가능하다
- Canvas Overlay UI는 기본적으로 후처리 영향 밖에 있으므로 필요 시 Camera 모드 UI를 고려한다

// 파일: Assets/Scripts/FullscreenRippleInteractive.cs
// 목적: 마우스/터치 좌표를 리플 중심(_Center)으로 전달 + 슬라이더로 파라미터 제어
// 업그레이드: 값 스무딩(Lerp), 안전 클램프, TextMeshPro/UGUI 텍스트 동시 지원, 프리셋 버튼 메서드 제공
// 주의:
//  - rippleMat(머티리얼) 반드시 지정
//  - 슬라이더/텍스트는 없어도 동작(Null 가드)

using UnityEngine;
using UnityEngine.UI;
#if TMP_PRESENT
using TMPro; // TextMeshPro 패키지가 있으면 사용
#endif

public class FullscreenRippleInteractive : MonoBehaviour
{
    [Header("Ripple Material (M_FullscreenRipple)")]
    public Material rippleMat;

    [Header("Sliders (optional)")]
    public Slider amplitude;   // 0~0.1
    public Slider frequency;   // 1~40
    public Slider speed;       // 0~10

    [Header("Value Texts (optional)")]
    public Text amplitudeText;   // uGUI Text (있으면 표시)
    public Text frequencyText;
    public Text speedText;
#if TMP_PRESENT
    public TMP_Text amplitudeTMP; // TMP Text (있으면 표시)
    public TMP_Text frequencyTMP;
    public TMP_Text speedTMP;
#endif

    [Header("Smoothing")]
    [Tooltip("슬라이더 값 → 실제 적용값 사이의 보간 속도(0=즉시, 1=매우 빠름)")]
    [Range(0f, 1f)] public float lerpSpeed = 0.25f;

    // 내부 상태(스무딩용)
    float _amp, _freq, _spd;

    // UV Y반전 보정
    bool invertY;

    void Awake()
    {
        invertY = SystemInfo.graphicsUVStartsAtTop;
    }

    void Start()
    {
        if (!rippleMat)
        {
            Debug.LogError("[RippleInteractive] rippleMat 미지정. 컴포넌트 비활성화.");
            enabled = false;
            return;
        }

        // 초기값(슬라이더 없으면 기본값 사용)
        _amp = amplitude ? amplitude.value : 0.03f;
        _freq = frequency ? frequency.value : 20f;
        _spd = speed ? speed.value : 2f;

        ApplyParamsImmediate();
        UpdateValueTexts();
        rippleMat.SetVector("_Center", new Vector4(0.5f, 0.5f, 0, 0));
    }

    void Update()
    {
        if (!rippleMat) return;

        // 1) 입력 좌표 → Viewport UV(0~1)
        Vector2 uv = GetPointerUV();
        if (invertY) uv.y = 1f - uv.y;
        uv = new Vector2(Mathf.Clamp01(uv.x), Mathf.Clamp01(uv.y));
        rippleMat.SetVector("_Center", new Vector4(uv.x, uv.y, 0, 0));

        // 2) 슬라이더 목표값 읽기(없으면 기존 값 유지)
        float targetAmp = amplitude ? amplitude.value : _amp;
        float targetFreq = frequency ? frequency.value : _freq;
        float targetSpd = speed ? speed.value : _spd;

        // 3) 안전 클램프(과도 왜곡 방지)
        targetAmp = Mathf.Clamp(targetAmp, 0f, 0.1f);
        targetFreq = Mathf.Clamp(targetFreq, 1f, 40f);
        targetSpd = Mathf.Clamp(targetSpd, 0f, 10f);

        // 4) 스무딩 적용(Lerp)
        float t = 1f - Mathf.Pow(1f - lerpSpeed, Time.unscaledDeltaTime * 60f);
        _amp = Mathf.Lerp(_amp, targetAmp, t);
        _freq = Mathf.Lerp(_freq, targetFreq, t);
        _spd = Mathf.Lerp(_spd, targetSpd, t);

        // 5) 머티리얼에 반영
        rippleMat.SetFloat("_Amplitude", _amp);
        rippleMat.SetFloat("_Frequency", _freq);
        rippleMat.SetFloat("_Speed", _spd);

        // 6) 값 표시 텍스트 갱신
        UpdateValueTexts();
    }

    Vector2 GetPointerUV()
    {
        if (Input.touchCount > 0)
        {
            Vector2 pos = Input.touches[0].position;
            if (Camera.main) return (Vector2)Camera.main.ScreenToViewportPoint(pos);
            return new Vector2(pos.x / Screen.width, pos.y / Screen.height);
        }
        else
        {
            Vector2 pos = Input.mousePosition;
            if (Camera.main) return (Vector2)Camera.main.ScreenToViewportPoint(pos);
            return new Vector2(pos.x / Screen.width, pos.y / Screen.height);
        }
    }

    void ApplyParamsImmediate()
    {
        rippleMat.SetFloat("_Amplitude", _amp);
        rippleMat.SetFloat("_Frequency", _freq);
        rippleMat.SetFloat("_Speed", _spd);
    }

    void UpdateValueTexts()
    {
        string a = _amp.ToString("0.000");
        string f = _freq.ToString("0.0");
        string s = _spd.ToString("0.0");

        if (amplitudeText) amplitudeText.text = a;
        if (frequencyText) frequencyText.text = f;
        if (speedText) speedText.text = s;
#if TMP_PRESENT
        if (amplitudeTMP)   amplitudeTMP.text   = a;
        if (frequencyTMP)   frequencyTMP.text   = f;
        if (speedTMP)       speedTMP.text       = s;
#endif
    }

    // -------- 프리셋 버튼용 메서드들 --------

    [ContextMenu("Preset/Calm")]
    public void PresetCalm()
    {
        SetTargets(0.010f, 15f, 0.8f);
    }

    [ContextMenu("Preset/Medium")]
    public void PresetMedium()
    {
        SetTargets(0.030f, 20f, 2.0f);
    }

    [ContextMenu("Preset/Storm")]
    public void PresetStorm()
    {
        SetTargets(0.070f, 24f, 3.0f);
    }

    void SetTargets(float amp, float freq, float spd)
    {
        if (amplitude) amplitude.value = amp; else _amp = amp;
        if (frequency) frequency.value = freq; else _freq = freq;
        if (speed) speed.value = spd; else _spd = spd;
    }
}

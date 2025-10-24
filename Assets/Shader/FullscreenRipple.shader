// 파일: Assets/Shaders/FullscreenRipple.shader
// 목적: URP Full Screen Pass에서 화면 전체에 리플(물결) 왜곡 적용.
// 포인트: URP 버전에 없는 Fullscreen.hlsl 대신 SV_VertexID로 풀스크린 삼각형을 직접 생성.
// 연결: PC_Renderer → Renderer Features → Full Screen Pass 의 Pass Material 로 이 머티리얼을 지정.

Shader "Hidden/Fullscreen/Ripple URP"
{
    Properties
    {
        _Amplitude ("Amplitude", Range(0, 0.1)) = 0.03   // 왜곡 강도
        _Frequency ("Frequency", Range(1, 40))  = 20     // 파형 촘촘함
        _Speed     ("Speed", Range(0, 10))      = 2      // 진행 속도
        _Center    ("Center", Vector)           = (0.5, 0.5, 0, 0) // 리플 중심(0~1 UV)
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" }
        ZWrite Off
        ZTest  Always
        Cull   Off

        Pass
        {
            Name "FullscreenPass"
            HLSLPROGRAM
            #pragma vertex   Vert
            #pragma fragment Frag

            // URP 공통 유틸(행렬/시간/텍스처 매크로)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // 카메라 컬러 텍스처(_BlitTexture)는 Full Screen Pass가 공급
            TEXTURE2D_X(_BlitTexture);
            SAMPLER(sampler_BlitTexture);

            // SRP Batcher 호환: 머티리얼 상수는 UnityPerMaterial CBUFFER에 보관
            CBUFFER_START(UnityPerMaterial)
                float  _Amplitude;
                float  _Frequency;
                float  _Speed;
                float4 _Center;   // xy만 사용
            CBUFFER_END

            // 프래그먼트로 넘길 데이터
            struct Varyings {
                float4 positionCS : SV_POSITION;  // 클립공간 좌표
                float2 uv         : TEXCOORD0;    // 화면 UV
            };

            // 풀스크린 삼각형(3개 정점)을 SV_VertexID로 직접 생성
            Varyings Vert(uint vid : SV_VertexID)
            {
                Varyings o;

                // 화면을 꽉 채우는 큰 삼각형( -1~+1 클립공간 )
                float4 pos[3] = {
                    float4(-1.0, -1.0, 0.0, 1.0),
                    float4(-1.0,  3.0, 0.0, 1.0),
                    float4( 3.0, -1.0, 0.0, 1.0)
                };

                // 대응 UV(0~2로 넓게 잡아 가장자리 샘플 공백 방지)
                float2 uvs[3] = {
                    float2(0.0, 0.0),
                    float2(0.0, 2.0),
                    float2(2.0, 0.0)
                };

                uint i = vid % 3;
                o.positionCS = pos[i];
                o.uv         = uvs[i];
                return o;
            }

            // 중심 기준 원형 사인파로 UV를 살짝 이동시켜 왜곡
            float2 RippleUV(float2 uv, float2 center, float t)
            {
                float2 d = uv - center;
                float  r = max(length(d), 1e-4);
                float  wave = sin(r * _Frequency - t * _Speed) * _Amplitude;
                return uv + d / r * wave;
            }

            half4 Frag (Varyings i) : SV_Target
            {
                float  t   = _Time.y;
                float2 ctr = _Center.xy;
                float2 uv  = RippleUV(i.uv, ctr, t);

                // _BlitTexture(카메라 컬러)에서 샘플
                return SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, uv);
            }
            ENDHLSL
        }
    }
}

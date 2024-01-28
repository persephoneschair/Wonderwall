Shader "Custom/Lattice"
{
    Properties
    {
        [ToggleUI] _GammaCorrect ("Gamma Correction", Float) = 1
        _Resolution ("Resolution (Change if AA is bad)", Range(1, 1024)) = 1
        _LineColor ("Line Color", Color) = (1, 1, 1, 1)
        _RotationX ("Rotation X", Range(0,1)) = 0.5
        _RotationY ("Rotation Y", Range(0,1)) = 0.25
        _MoveSpeed ("Move Speed", Vector) = (7.5,0,5.0,0)
        _MoveSpeedMultiplier ("Move Speed Multiplier", Range(0,1)) = 0.5
    }
    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            // Built-in properties
            sampler2D _MainTex;   float4 _MainTex_TexelSize;
            sampler2D _SecondTex; float4 _SecondTex_TexelSize;
            sampler2D _ThirdTex;  float4 _ThirdTex_TexelSize;
            sampler2D _FourthTex; float4 _FourthTex_TexelSize;
            float4 _Mouse;
            float _GammaCorrect;
            float _Resolution;
            float3 _LineColor;
            float _RotationSpeed;
            float _RotationX;
            float _RotationY;
            float3 _MoveSpeed;
            float _MoveSpeedMultiplier;

            // GLSL Compatability macros
            #define glsl_mod(x,y) (((x)-(y)*floor((x)/(y))))
            #define texelFetch(ch, uv, lod) tex2Dlod(ch, float4((uv).xy * ch##_TexelSize.xy + ch##_TexelSize.xy * 0.5, 0, lod))
            #define textureLod(ch, uv, lod) tex2Dlod(ch, float4(uv, 0, lod))
            #define iResolution float3(_Resolution, _Resolution, _Resolution)
            #define iFrame (floor(_Time.y / 60))
            #define iChannelTime float4(_Time.y, _Time.y, _Time.y, _Time.y)
            #define iDate float4(2020, 6, 18, 30)
            #define iSampleRate (44100)
            #define iChannelResolution float4x4(                      \
                _MainTex_TexelSize.z,   _MainTex_TexelSize.w,   0, 0, \
                _SecondTex_TexelSize.z, _SecondTex_TexelSize.w, 0, 0, \
                _ThirdTex_TexelSize.z,  _ThirdTex_TexelSize.w,  0, 0, \
                _FourthTex_TexelSize.z, _FourthTex_TexelSize.w, 0, 0)

            // Global access to uv data
            static v2f vertex_output;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv =  v.uv;
                return o;
            }

            #define sEPS 0.05
            #define FAR 20.
            float getGrey(float3 p)
            {
                return p.x*0.299+p.y*0.587+p.z*0.114;
            }

            float sminP(float a, float b, float smoothing)
            {
                float h = clamp(0.5+0.5*(b-a)/smoothing, 0., 1.);
                return lerp(b, a, h)-smoothing*h*(1.-h);
            }

            float2x2 rot(float th)
            {
                float cs = cos(th), si = sin(th);
                return transpose(float2x2(cs, -si, si, cs));
            }

            float3 tex3D(sampler2D tex, in float3 p, in float3 n)
            {
                n = max((abs(n)-0.2)*7., 0.001);
                n /= n.x+n.y+n.z;
                return (tex2D(tex, p.yz)*n.x+tex2D(tex, p.zx)*n.y+tex2D(tex, p.xy)*n.z).xyz;
            }

            float3 blackbodyPalette(float t)
            {
                t *= 4000.;
                float cx = (0.86011773+0.00015411826*t+0.00000012864122*t*t)/(1.+0.0008424202*t+0.00000070814514*t*t);
                float cy = (0.31739873+0.000042280626*t+0.000000042048168*t*t)/(1.-0.000028974182*t+0.00000016145606*t*t);
                float d = 2.*cx-8.*cy+4.;
                float3 XYZ = float3(3.*cx/d, 2.*cy/d, 1.-(3.*cx+2.*cy)/d);
                float3 RGB = mul(transpose(float3x3(3.240479, -0.969256, 0.055648, -1.53715, 1.875992, -0.204043, -0.498535, 0.041556, 1.057311)),float3(1./XYZ.y*XYZ.x, 1., 1./XYZ.y*XYZ.z));
                return max(RGB, 0.)*pow(t*0.0004, 4.);
            }

            float bumpSurf3D(in float3 p, in float3 n)
            {
                p = abs(glsl_mod(p, 0.0625)-0.03125);
                float x = min(p.x, min(p.y, p.z))/0.03125;
                p = sin(p*380.+sin(p.yzx*192.+64.));
                float surfaceNoise = p.x*p.y*p.z;
                return clamp(x+surfaceNoise*0.05, 0., 1.);
            }

            float3 doBumpMap(in float3 p, in float3 nor, float bumpfactor)
            {
                const float eps = 0.001;
                float ref = bumpSurf3D(p, nor);
                float3 grad = float3(bumpSurf3D(float3(p.x-eps, p.y, p.z), nor)-ref, bumpSurf3D(float3(p.x, p.y-eps, p.z), nor)-ref, bumpSurf3D(float3(p.x, p.y, p.z-eps), nor)-ref)/eps;
                grad -= nor*dot(nor, grad);
                return normalize(nor+bumpfactor*grad);
            }

            float map(float3 p)
            {
                p = glsl_mod(p, 2.5)-1.0;
                float x1 = sminP(length(p.xy), sminP(length(p.yz), length(p.xz), 0.05), 0.05)-0.015;

                // Add an offset to push the objects further away
                float offset = 0.04; // Adjust the offset value as needed
                x1 += offset;
                return x1;
            }

            float raymarch(float3 ro, float3 rd)
            {
                float d, t = 0.;
                for (int i = 0; i < 128; i++)
                {
                    d = map(ro + rd * t);
                    if (d < sEPS || t > FAR)
                        break;

                    t += d * 0.75;
                }

                return t;
            }

            float calculateAO(float3 p, float3 n)
            {
                const float AO_SAMPLES = 5.;
                float r = 0., w = 1., d;
                for (float i = 1.;i<AO_SAMPLES+1.1; i++)
                {
                    d = i/AO_SAMPLES;
                    r += w*(d-map(p+n*d));
                    w *= 0.5;
                }
                return 1.-clamp(r, 0., 1.);
            }

            float softShadow(float3 ro, float3 rd, float start, float end, float k)
            {
                float shade = 1.;
                const int maxIterationsShad = 16;
                float dist = start;
                float stepDist = end/float(maxIterationsShad);
                for (int i = 0;i<maxIterationsShad; i++)
                {
                    float h = map(ro+rd*dist);
                    shade = min(shade, k*h/dist);
                    dist += clamp(h, 0.0005, stepDist*2.);
                    if (h<0.001||dist>end)
                        break;
                        
                }
                return min(max(shade, 0.)+0.4, 1.);
            }

            float3 getNormal(in float3 p)
            {
                const float eps = 0.001;
                return normalize(float3(map(float3(p.x+eps, p.y, p.z))-map(float3(p.x-eps, p.y, p.z)), map(float3(p.x, p.y+eps, p.z))-map(float3(p.x, p.y-eps, p.z)), map(float3(p.x, p.y, p.z+eps))-map(float3(p.x, p.y, p.z-eps))));
            }

            float curve(in float3 p)
            {
                float2 e = float2(-1., 1.)*0.05;
                float t1 = map(p+e.yxx), t2 = map(p+e.xxy);
                float t3 = map(p+e.xyx), t4 = map(p+e.yyy);
                return 7.*(t1+t2+t3+t4-4.*map(p));
            }

            float4 frag (v2f __vertex_output) : SV_Target
            {
                vertex_output = __vertex_output;
                float4 fragColor = 0;
                float2 fragCoord = vertex_output.uv * _Resolution;
                float2 uv = (fragCoord-iResolution.xy*0.5)/iResolution.y;

                float3 rots = _RotationSpeed;
                float rx = _RotationX;
                float ry = _RotationY;

                float3 rd = normalize(float3(uv, 0.5));
                rd.xy = mul(rd.xy,rot(_Time.y * rx));
                rd.xz = mul(rd.xz,rot(_Time.y * ry));

                float3 mv = _MoveSpeed;
                float mvm = _MoveSpeedMultiplier;

                float3 ro = float3(_Time.y * mv.x, sin(_Time.y * mv.y), cos(_Time.y * mv.z)) * mvm;
                // Add an offset to the camera position
                float cameraOffset = 2.0; // Adjust the offset value as needed
                ro += normalize(rd) * cameraOffset;

                float3 lp = float3(0., 0.125, -0.125);
                lp.xy = mul(lp.xy, rot(_Time.y * mv.x));
                lp.xz = mul(lp.xz, rot(_Time.y * mv.z));
                lp += ro + float3(0., 1., 0.);


                float3 sceneCol = ((float3)0.);
                float dist = raymarch(ro, rd);
                if (dist<FAR)
                {
                    float3 sp = ro+rd*dist;
                    float3 sn = getNormal(sp);
                    sn = doBumpMap(sp, sn, 0.01);
                    float3 ld = lp-sp;
                    float3 objCol = tex3D(_MainTex, sp, sn);
                    objCol *= bumpSurf3D(sp, sn)*0.5+0.5;
                    float lDist = max(length(ld), 0.001);
                    ld /= lDist;
                    float atten = min(1./(lDist*0.5+lDist*lDist*0.1), 1.);
                    float ambient = 0.1;
                    float diffuse = max(0., dot(sn, ld));
                    float specular = max(0., dot(reflect(-ld, sn), -rd));
                    specular = pow(specular, 16.);  // Adjust specular power
                    float shadow = softShadow(sp, ld, sEPS*2., lDist, 32.);
                    float ao = calculateAO(sp, sn)*0.5+0.5;
                    sceneCol = objCol*(float3(0.2, 0.8, 1.0)*diffuse+ambient)+float3(0.2, 0.8, 1.0)*specular*0.75;

                    float3 electricBlue = float3(1.0, 1.0, 1.0) * _LineColor.rgb;
                    sceneCol += electricBlue;

                    sceneCol *= atten * ao * shadow;
                    float glowIntensity = 1.0 - exp(-dist);
                    sceneCol += float3(0.2, 0.8, 1.0) * glowIntensity * 0.5;
                    float alpha = 0.5; // You can adjust the alpha value
                    sceneCol.rgb = lerp(sceneCol.rgb, float3(0.0, 0.0, 1.0), alpha);

                    // Add bloom effect
                    float bloomThreshold = .8; // Adjust the threshold to control the bloom intensity
                    if (glowIntensity > bloomThreshold)
                    {
                        float bloomFactor = (glowIntensity - bloomThreshold) / (1.0 - bloomThreshold);
                        float3 bloomColor = electricBlue; // Adjust the color of the bloom
                        bloomColor *= bloomFactor;

                        // Apply bloom to a separate buffer
                        float3 bloomBuffer = bloomColor * 2.0; // Increase intensity for better effect

                        // Combine bloom buffer with scene color
                        sceneCol += bloomBuffer;
                    }
                }

                fragColor = float4(clamp(sceneCol, 0., 1.), 1);

                if (_GammaCorrect) fragColor.rgb = pow(fragColor.rgb, 2.2);

                return fragColor;
            }
            ENDCG
        }
    }
}
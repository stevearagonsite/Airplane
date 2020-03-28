// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

#ifndef CRYSTAL_GLASS_INCLUDED
#define CRYSTAL_GLASS_INCLUDED

samplerCUBE _EnvTex;
float _IOR, _IOROffset;
float _FresnelPower, _FresnelAlpha, _Transparency;
fixed3 _FresnelColor1, _FresnelColor2;

// bump
sampler2D _NormalTex;
float4 _NormalTex_ST;

// cube sample lod
float _Lod;

// sparkle
sampler2D _SparkleNoiseTex;
float4 _SparkleNoiseTex_ST;
float _SparklePower, _SparkleAnimSpeed;

struct v2f
{
	float4 pos  : POSITION;
	float3 view : TEXCOORD0;  // world space view
	float3 norm : TEXCOORD1;  // world space normal
	float2 sparkleUv : TEXCOORD2;
#ifdef CRYSTAL_GLASS_BUMP
	float2 tex  : TEXCOORD4;
	float3 tan  : TEXCOORD5;   // world space tangent
	float3 bin  : TEXCOORD6;   // world space binormal
#endif
};
v2f vert (appdata_tan v)
{
	v2f o;
	o.pos = UnityObjectToClipPos(v.vertex);
	o.norm = mul((float3x3)unity_ObjectToWorld, v.normal);
	o.view = WorldSpaceViewDir(v.vertex);
#ifdef CRYSTAL_GLASS_BUMP
	o.tex = TRANSFORM_TEX(v.texcoord, _NormalTex);
	TANGENT_SPACE_ROTATION;
	o.tan = mul((float3x3)unity_ObjectToWorld, v.tangent.xyz);
	o.bin = mul((float3x3)unity_ObjectToWorld, binormal);
#endif
	o.sparkleUv = TRANSFORM_TEX(v.texcoord, _SparkleNoiseTex);
	return o;
}
float4 frag (v2f i) : COLOR
{
	float3 N = normalize(i.norm);
#ifdef CRYSTAL_GLASS_BUMP
	float3 bump = tex2D(_NormalTex, i.tex).rgb;
	bump = normalize(bump * 2.0 - 1.0);
	float3 T = normalize(i.tan);
	float3 B = normalize(i.bin);
	N = normalize(N + T * bump.x - B * bump.y);
#endif
	
	// reflection color
	float3 refl;
	float3 V = normalize(i.view);
	float3 R = reflect(-V, N);
#ifdef CRYSTAL_GLASS_LOD
	#ifdef CRYSTAL_GLASS_ReflectionProbe
		float4 reflHdr = UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, float4(R, _Lod), 0.1);
		refl = DecodeHDR(reflHdr, unity_SpecCube0_HDR).rgb;
	#else
		refl = texCUBElod(_EnvTex, float4(R, _Lod)).rgb;
	#endif
#else
	#ifdef CRYSTAL_GLASS_ReflectionProbe
		float4 reflHdr = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, R);
		refl = DecodeHDR(reflHdr, unity_SpecCube0_HDR).rgb;
	#else
		refl = texCUBE(_EnvTex, R).rgb;
	#endif
#endif

	// refraction color
	float3 tr = refract(V, N, _IOR + _IOROffset);
	float3 tg = refract(V, N, _IOR);
	float3 tb = refract(V, N, _IOR - _IOROffset);

	float3 refr;
#ifdef CRYSTAL_GLASS_LOD
	#ifdef CRYSTAL_GLASS_ReflectionProbe
		float4 hdr = UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, float4(tr, _Lod), 0.1);
		refr.r = DecodeHDR(hdr, unity_SpecCube0_HDR).r;

		hdr = UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, float4(tg, _Lod), 0.1);
		refr.g = DecodeHDR(hdr, unity_SpecCube0_HDR).g;

		hdr = UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, float4(tb, _Lod), 0.1);
		refr.b = DecodeHDR(hdr, unity_SpecCube0_HDR).b;
	#else
		refr.r = texCUBElod(_EnvTex, float4(tr, _Lod)).r;
		refr.g = texCUBElod(_EnvTex, float4(tg, _Lod)).g;
		refr.b = texCUBElod(_EnvTex, float4(tb, _Lod)).b;
	#endif
#else
	#ifdef CRYSTAL_GLASS_ReflectionProbe
		float4 hdr = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, tr);
		refr.r = DecodeHDR(hdr, unity_SpecCube0_HDR).r;

		hdr = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, tg);
		refr.g = DecodeHDR(hdr, unity_SpecCube0_HDR).g;
	
		hdr = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, tb);
		refr.b = DecodeHDR(hdr, unity_SpecCube0_HDR).b;
	#else
		refr.r = texCUBE(_EnvTex, tr).r;
		refr.g = texCUBE(_EnvTex, tg).g;
		refr.b = texCUBE(_EnvTex, tb).b;
	#endif
#endif
	float fresnel = saturate(-dot(N, -V));
	fresnel = pow(fresnel, _FresnelPower);
	
	refl *= _FresnelColor1;
	refr *= _FresnelColor2;

	float3 c = refl * fresnel + refr * (1.0 - fresnel);
	float a = (1.0 - fresnel * _FresnelAlpha) * _Transparency;
	
#ifdef CRYSTAL_GLASS_SPARKLE
	float3 ns = tex2D(_SparkleNoiseTex, i.sparkleUv + _Time.x * _SparkleAnimSpeed);
	ns -= 0.5;
	ns = normalize(normalize(ns) + N);
	float spk = saturate(dot(V, ns));
	spk = pow(spk, _SparklePower);
	c += spk;
//	float3 spkcol = tex2D(_SparkleNoiseTex, i.sparkleUv).rgb * 3;
//	c = lerp(c, spkcol, spk);
#endif
	
	return float4(c, a);
}

#endif
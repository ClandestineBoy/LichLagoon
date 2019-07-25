// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Water"
{
	Properties
	{
		_WaveStretch("Wave Stretch", Vector) = (0.15,0.02,0,0)
		_WaveTile("Wave Tile", Float) = 1
		_WaveSpeed("Wave Speed", Float) = 1
		_WaveHeight("Wave Height", Float) = 1
		_WaterColor("Water Color", Color) = (0.2352941,0.5411765,0.7019608,0)
		_TopColor("Top Color", Color) = (0.2862745,0.6862745,0.8235294,0)
		_EdgeDistance("Edge Distance", Float) = 1
		_EdgePower("Edge Power", Float) = 1
		_Texture0("Texture 0", 2D) = "white" {}
		_NormalSpeed("Normal Speed", Float) = 1
		_FoamSpeed("Foam Speed", Float) = 1
		_NormalStrength("Normal Strength", Range( 0 , 1)) = 1
		_NormalTile("Normal Tile", Float) = 1
		_SeaFoam("Sea Foam", 2D) = "white" {}
		_EdgeFoamTile("Edge Foam Tile", Float) = 1
		_SeaFoamTile("Sea Foam Tile", Float) = 1
		_RefractAmount("Refract Amount", Float) = 0.1
		_Depth("Depth", Float) = -4
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		GrabPass{ }
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#include "UnityStandardUtils.cginc"
		#include "UnityCG.cginc"
		#include "Tessellation.cginc"
		#pragma target 4.6
		#pragma surface surf Standard keepalpha noshadow vertex:vertexDataFunc tessellate:tessFunction 
		struct Input
		{
			float3 worldPos;
			float4 screenPos;
		};

		uniform float _WaveHeight;
		uniform float _WaveSpeed;
		uniform float2 _WaveStretch;
		uniform float _WaveTile;
		uniform sampler2D _Texture0;
		uniform float _NormalStrength;
		uniform float _NormalSpeed;
		uniform float _NormalTile;
		uniform float4 _WaterColor;
		uniform float4 _TopColor;
		uniform sampler2D _SeaFoam;
		uniform float _SeaFoamTile;
		UNITY_DECLARE_SCREENSPACE_TEXTURE( _GrabTexture )
		uniform float _RefractAmount;
		UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
		uniform float4 _CameraDepthTexture_TexelSize;
		uniform float _Depth;
		uniform float _EdgeDistance;
		uniform float _FoamSpeed;
		uniform float _EdgeFoamTile;
		uniform float _EdgePower;


		float3 mod2D289( float3 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }

		float2 mod2D289( float2 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }

		float3 permute( float3 x ) { return mod2D289( ( ( x * 34.0 ) + 1.0 ) * x ); }

		float snoise( float2 v )
		{
			const float4 C = float4( 0.211324865405187, 0.366025403784439, -0.577350269189626, 0.024390243902439 );
			float2 i = floor( v + dot( v, C.yy ) );
			float2 x0 = v - i + dot( i, C.xx );
			float2 i1;
			i1 = ( x0.x > x0.y ) ? float2( 1.0, 0.0 ) : float2( 0.0, 1.0 );
			float4 x12 = x0.xyxy + C.xxzz;
			x12.xy -= i1;
			i = mod2D289( i );
			float3 p = permute( permute( i.y + float3( 0.0, i1.y, 1.0 ) ) + i.x + float3( 0.0, i1.x, 1.0 ) );
			float3 m = max( 0.5 - float3( dot( x0, x0 ), dot( x12.xy, x12.xy ), dot( x12.zw, x12.zw ) ), 0.0 );
			m = m * m;
			m = m * m;
			float3 x = 2.0 * frac( p * C.www ) - 1.0;
			float3 h = abs( x ) - 0.5;
			float3 ox = floor( x + 0.5 );
			float3 a0 = x - ox;
			m *= 1.79284291400159 - 0.85373472095314 * ( a0 * a0 + h * h );
			float3 g;
			g.x = a0.x * x0.x + h.x * x0.y;
			g.yz = a0.yz * x12.xz + h.yz * x12.yw;
			return 130.0 * dot( m, g );
		}


		inline float4 ASE_ComputeGrabScreenPos( float4 pos )
		{
			#if UNITY_UV_STARTS_AT_TOP
			float scale = -1.0;
			#else
			float scale = 1.0;
			#endif
			float4 o = pos;
			o.y = pos.w * 0.5f;
			o.y = ( pos.y - o.y ) * _ProjectionParams.x * scale + o.y;
			return o;
		}


		float4 tessFunction( appdata_full v0, appdata_full v1, appdata_full v2 )
		{
			return UnityDistanceBasedTess( v0.vertex, v1.vertex, v2.vertex, 0.0,80.0,( _WaveHeight * 8.0 ));
		}

		void vertexDataFunc( inout appdata_full v )
		{
			float temp_output_28_0 = ( _Time.y * _WaveSpeed );
			float2 _Direction = float2(-1,0);
			float3 ase_worldPos = mul( unity_ObjectToWorld, v.vertex );
			float4 appendResult10 = (float4(ase_worldPos.x , ase_worldPos.z , 0.0 , 0.0));
			float4 WorldSpaceTile11 = appendResult10;
			float4 WaveTileUV20 = ( ( WorldSpaceTile11 * float4( _WaveStretch, 0.0 , 0.0 ) ) * _WaveTile );
			float2 panner3 = ( temp_output_28_0 * _Direction + WaveTileUV20.xy);
			float simplePerlin2D1 = snoise( panner3 );
			float2 panner21 = ( temp_output_28_0 * _Direction + ( WaveTileUV20 * float4( 0.1,0.1,0,0 ) ).xy);
			float simplePerlin2D22 = snoise( panner21 );
			float temp_output_27_0 = ( simplePerlin2D1 + simplePerlin2D22 );
			float3 WaveHeight33 = ( ( float3(0,1,0) * _WaveHeight ) * temp_output_27_0 );
			v.vertex.xyz += WaveHeight33;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float3 ase_worldPos = i.worldPos;
			float4 appendResult10 = (float4(ase_worldPos.x , ase_worldPos.z , 0.0 , 0.0));
			float4 WorldSpaceTile11 = appendResult10;
			float4 temp_output_57_0 = ( WorldSpaceTile11 * _NormalTile );
			float2 panner61 = ( 1.0 * _Time.y * ( float2( 1,0 ) * _NormalSpeed ) + temp_output_57_0.xy);
			float2 panner62 = ( 1.0 * _Time.y * ( float2( -1,0 ) * ( _NormalSpeed * 3.0 ) ) + ( temp_output_57_0 * ( _NormalTile * 5.0 ) ).xy);
			float3 NormalMaps72 = BlendNormals( UnpackScaleNormal( tex2D( _Texture0, panner61 ), _NormalStrength ) , UnpackScaleNormal( tex2D( _Texture0, panner62 ), _NormalStrength ) );
			o.Normal = NormalMaps72;
			float2 panner109 = ( 1.0 * _Time.y * float2( 0.08,0.06 ) + ( WorldSpaceTile11 * 0.06 ).xy);
			float simplePerlin2D108 = snoise( panner109 );
			float clampResult114 = clamp( ( tex2D( _SeaFoam, ( ( WorldSpaceTile11 / 10.0 ) * _SeaFoamTile ).xy ).r * simplePerlin2D108 ) , 0.0 , 1.0 );
			float SeaFoam105 = clampResult114;
			float temp_output_28_0 = ( _Time.y * _WaveSpeed );
			float2 _Direction = float2(-1,0);
			float4 WaveTileUV20 = ( ( WorldSpaceTile11 * float4( _WaveStretch, 0.0 , 0.0 ) ) * _WaveTile );
			float2 panner3 = ( temp_output_28_0 * _Direction + WaveTileUV20.xy);
			float simplePerlin2D1 = snoise( panner3 );
			float2 panner21 = ( temp_output_28_0 * _Direction + ( WaveTileUV20 * float4( 0.1,0.1,0,0 ) ).xy);
			float simplePerlin2D22 = snoise( panner21 );
			float temp_output_27_0 = ( simplePerlin2D1 + simplePerlin2D22 );
			float WavePattern30 = temp_output_27_0;
			float clampResult43 = clamp( WavePattern30 , 0.0 , 1.0 );
			float4 lerpResult41 = lerp( _WaterColor , ( _TopColor + SeaFoam105 ) , clampResult43);
			float4 Albedo132 = lerpResult41;
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 ase_grabScreenPos = ASE_ComputeGrabScreenPos( ase_screenPos );
			float4 ase_grabScreenPosNorm = ase_grabScreenPos / ase_grabScreenPos.w;
			float4 screenColor122 = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_GrabTexture,( (ase_grabScreenPosNorm).xyzw + float4( ( _RefractAmount * NormalMaps72 ) , 0.0 ) ).xy);
			float4 clampResult123 = clamp( screenColor122 , float4( 0,0,0,0 ) , float4( 1,1,1,0 ) );
			float4 Refraction124 = clampResult123;
			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			float screenDepth128 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture,UNITY_PROJ_COORD( ase_screenPos )));
			float distanceDepth128 = abs( ( screenDepth128 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( _Depth ) );
			float clampResult130 = clamp( ( 1.0 - distanceDepth128 ) , 0.0 , 1.0 );
			float Depth131 = clampResult130;
			float4 lerpResult137 = lerp( Albedo132 , Refraction124 , Depth131);
			o.Albedo = lerpResult137.rgb;
			float screenDepth44 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture,UNITY_PROJ_COORD( ase_screenPos )));
			float distanceDepth44 = abs( ( screenDepth44 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( _EdgeDistance ) );
			float4 temp_output_79_0 = ( ( WorldSpaceTile11 / 10.0 ) * _EdgeFoamTile );
			float2 panner94 = ( 1.0 * _Time.y * ( float2( 1,0 ) * _FoamSpeed ) + temp_output_79_0.xy);
			float2 panner95 = ( 1.0 * _Time.y * ( float2( -1,0 ) * ( _FoamSpeed * 3.0 ) ) + ( temp_output_79_0 * ( _EdgeFoamTile * 5.0 ) ).xy);
			float4 lerpResult147 = lerp( tex2D( _SeaFoam, panner94 ) , tex2D( _SeaFoam, panner95 ) , float4( 0,0,0,0 ));
			float4 clampResult53 = clamp( ( ( ( 1.0 - distanceDepth44 ) + lerpResult147 ) * _EdgePower ) , float4( 0,0,0,0 ) , float4( 1,1,1,0 ) );
			float4 Edge50 = clampResult53;
			o.Emission = Edge50.rgb;
			o.Smoothness = 0.9;
			o.Alpha = 1;
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=16800
995;73;1563;1288;4920.598;2693.241;1;True;False
Node;AmplifyShaderEditor.WorldPosInputsNode;9;-3416.844,-825.1865;Float;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DynamicAppendNode;10;-3117.645,-818.7863;Float;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;11;-2961.895,-818.8873;Float;False;WorldSpaceTile;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;12;-4239.186,-498.7632;Float;False;11;WorldSpaceTile;1;0;OBJECT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.CommentaryNode;73;-5116.409,1029.852;Float;False;2451.739;1096;Norma s;19;70;72;57;56;58;60;54;59;61;62;63;66;64;67;68;69;71;38;55;;1,1,1,1;0;0
Node;AmplifyShaderEditor.Vector2Node;13;-4208.186,-376.7631;Float;False;Property;_WaveStretch;Wave Stretch;0;0;Create;True;0;0;False;0;0.15,0.02;1,1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.GetLocalVarNode;56;-4879.854,1127.853;Float;False;11;WorldSpaceTile;1;0;OBJECT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;14;-3950.186,-434.7631;Float;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;16;-3944.033,-263.3508;Float;False;Property;_WaveTile;Wave Tile;1;0;Create;True;0;0;False;0;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;58;-4809.854,1268.852;Float;False;Property;_NormalTile;Normal Tile;13;0;Create;True;0;0;False;0;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;67;-4305.855,1508.853;Float;False;Property;_NormalSpeed;Normal Speed;9;0;Create;True;0;0;False;0;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;57;-4562.854,1194.852;Float;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;59;-4712.854,1763.853;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;15;-3750.186,-425.7631;Float;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.CommentaryNode;115;-5346.166,-1401.687;Float;False;1833.399;763.6353;Comment;13;100;99;101;102;103;104;105;108;110;111;109;113;114;;1,1,1,1;0;0
Node;AmplifyShaderEditor.Vector2Node;63;-4187.855,1079.852;Float;False;Constant;_PanDir;Pan Dir;9;0;Create;True;0;0;False;0;1,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;68;-4030.855,1646.853;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;3;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;64;-4214.855,1964.852;Float;False;Constant;_Vector0;Vector 0;9;0;Create;True;0;0;False;0;-1,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.RegisterLocalVarNode;20;-3565.033,-425.3508;Float;False;WaveTileUV;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;100;-5296.166,-1351.687;Float;False;11;WorldSpaceTile;1;0;OBJECT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;111;-5053.551,-799.052;Float;False;Constant;_FoamMask;Foam Mask;17;0;Create;True;0;0;False;0;0.06;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;36;-4480.507,211.6609;Float;False;1934.74;650.6393;Wave Pattern;13;29;6;26;24;28;25;5;3;21;1;22;27;30;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;99;-5102.903,-1121.172;Float;False;Constant;_Float1;Float 1;14;0;Create;True;0;0;False;0;10;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;66;-3981.855,1108.852;Float;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;69;-3897.855,1877.853;Float;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;98;-5507.044,-2870.66;Float;False;2681.351;1459.702;Sea Foram;17;84;78;80;88;83;91;90;79;86;89;51;92;93;87;75;94;96;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;60;-4502.854,1799.853;Float;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;101;-4886.467,-1103.007;Float;False;Property;_SeaFoamTile;Sea Foam Tile;17;0;Create;True;0;0;False;0;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;54;-5066.409,1504.901;Float;True;Property;_Texture0;Texture 0;8;0;Create;True;0;0;False;0;None;None;False;white;Auto;Texture2D;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;102;-4961.294,-1267.51;Float;False;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.PannerNode;62;-3728.855,1788.853;Float;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;110;-4910.551,-892.052;Float;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;26;-4058.006,711.6692;Float;False;20;WaveTileUV;1;0;OBJECT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleTimeNode;6;-4357.61,494.6609;Float;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;29;-4430.506,687.6692;Float;False;Property;_WaveSpeed;Wave Speed;2;0;Create;True;0;0;False;0;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;84;-5263.781,-1826.129;Float;False;Constant;_Float0;Float 0;14;0;Create;True;0;0;False;0;10;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;78;-5457.044,-2056.644;Float;False;11;WorldSpaceTile;1;0;OBJECT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.PannerNode;61;-3809.855,1254.852;Float;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;71;-3845.852,1491.221;Float;False;Property;_NormalStrength;Normal Strength;11;0;Create;True;0;0;False;0;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;75;-4971.749,-2279.322;Float;True;Property;_SeaFoam;Sea Foam;15;0;Create;True;0;0;False;0;None;None;False;white;Auto;Texture2D;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;28;-4105.506,516.6693;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;103;-4766.629,-1263.083;Float;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;25;-3835.006,716.6692;Float;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0.1,0.1,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.PannerNode;109;-4733.551,-859.052;Float;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0.08,0.06;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;38;-3547.221,1303.348;Float;True;Property;_NormalMap0;Normal Map 0;4;0;Create;True;0;0;False;0;None;None;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;24;-4173.597,268.8002;Float;False;20;WaveTileUV;1;0;OBJECT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.Vector2Node;5;-3938.61,392.6609;Float;False;Constant;_Direction;Direction;1;0;Create;True;0;0;False;0;-1,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleDivideOpNode;83;-5122.172,-1972.467;Float;False;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SamplerNode;55;-3447.437,1720.851;Float;True;Property;_NormalMap;Normal Map;10;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;88;-4729.888,-2027.956;Float;False;Property;_FoamSpeed;Foam Speed;10;0;Create;True;0;0;False;0;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;80;-5047.345,-1807.964;Float;False;Property;_EdgeFoamTile;Edge Foam Tile;16;0;Create;True;0;0;False;0;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;3;-3711.609,318.6609;Float;True;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;89;-4446.888,-1889.957;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;3;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;104;-4212.05,-1190.351;Float;True;Property;_TextureSample2;Texture Sample 2;17;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.NoiseGeneratorNode;108;-4502.551,-928.052;Float;True;Simplex2D;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;21;-3685.597,608.3002;Float;True;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;79;-4927.507,-1968.04;Float;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.Vector2Node;90;-4603.888,-2456.958;Float;False;Constant;_Dir;Dir;9;0;Create;True;0;0;False;0;1,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.Vector2Node;91;-4630.888,-1571.957;Float;False;Constant;_Dir2;Dir2;9;0;Create;True;0;0;False;0;-1,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.BlendNormalsNode;70;-3197.038,1453.169;Float;False;0;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;86;-4927.205,-1627.183;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;1;-3449.609,261.6609;Float;True;Simplex2D;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;113;-4104.551,-891.052;Float;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;22;-3417.597,604.3002;Float;True;Simplex2D;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;127;-1866.022,-2502.062;Float;False;1662.738;550.3125;Refraction;9;119;118;116;120;117;121;122;123;124;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;72;-2907.672,1488.279;Float;False;NormalMaps;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;92;-4313.888,-1658.956;Float;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;93;-4397.888,-2427.958;Float;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;87;-4724.637,-1745.03;Float;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.CommentaryNode;51;-4207.668,-2820.66;Float;False;1331.975;1306.727;Edge;12;82;76;50;53;48;46;49;44;45;85;95;147;;1,1,1,1;0;0
Node;AmplifyShaderEditor.PannerNode;95;-4144.888,-1745.956;Float;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;27;-3169.506,457.6691;Float;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;45;-4157.668,-2759.46;Float;False;Property;_EdgeDistance;Edge Distance;6;0;Create;True;0;0;False;0;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;119;-1623.365,-2066.749;Float;False;72;NormalMaps;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.PannerNode;94;-4225.888,-2281.958;Float;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;118;-1717.344,-2208.892;Float;False;Property;_RefractAmount;Refract Amount;18;0;Create;True;0;0;False;0;0.1;0.1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;114;-3830.551,-963.052;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.GrabScreenPosition;116;-1816.022,-2452.062;Float;False;0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;76;-3981.641,-2305.383;Float;True;Property;_TextureSample0;Texture Sample 0;14;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;129;-1840.388,-2840.121;Float;False;Property;_Depth;Depth;19;0;Create;True;0;0;False;0;-4;-4;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;105;-3755.767,-1179.833;Float;False;SeaFoam;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DepthFade;44;-3876.068,-2770.66;Float;False;True;False;True;2;1;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;85;-3936.541,-1987.208;Float;True;Property;_TextureSample1;Texture Sample 1;13;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;120;-1376.672,-2161.902;Float;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ComponentMaskNode;117;-1401.341,-2448.537;Float;False;True;True;True;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.CommentaryNode;133;-1181.201,-1386.852;Float;False;1522.567;700.0085;Color;8;107;40;39;41;106;42;43;132;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;30;-2788.767,521.8248;Float;False;WavePattern;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;107;-1066.796,-903.1268;Float;False;105;SeaFoam;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DepthFade;128;-1684.388,-2846.121;Float;False;True;False;True;2;1;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;121;-1114.706,-2315.792;Float;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.LerpOp;147;-3633.157,-2122.538;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;42;-737.9078,-801.8432;Float;False;30;WavePattern;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;35;-3225.116,-507.8458;Float;False;913.7338;411.4303;Wave UV and Height;5;18;31;19;32;33;;1,1,1,1;0;0
Node;AmplifyShaderEditor.ColorNode;39;-1131.201,-1112.851;Float;False;Property;_TopColor;Top Color;5;0;Create;True;0;0;False;0;0.2862745,0.6862745,0.8235294,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;46;-3612.602,-2699.91;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScreenColorNode;122;-930.2734,-2322.841;Float;False;Global;_GrabScreen0;Grab Screen 0;17;0;Create;True;0;0;False;0;Object;-1;False;False;1;0;FLOAT2;0,0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;31;-3156.382,-218.4154;Float;False;Property;_WaveHeight;Wave Height;3;0;Create;True;0;0;False;0;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;18;-3175.116,-457.8458;Float;False;Constant;_WaveUp;Wave Up;1;0;Create;True;0;0;False;0;0,1,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleAddOpNode;82;-3546.657,-2436.362;Float;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;106;-755.7958,-1052.127;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;49;-3464.38,-2252.455;Float;False;Property;_EdgePower;Edge Power;7;0;Create;True;0;0;False;0;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;40;-1123.201,-1336.852;Float;False;Property;_WaterColor;Water Color;4;0;Create;True;0;0;False;0;0.2352941,0.5411765,0.7019608,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;139;-1434.727,-2842.765;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;43;-437.1077,-846.6431;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;130;-1270.388,-2870.121;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;48;-3341.322,-2471.885;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;41;-295.2009,-1058.451;Float;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;19;-2963.116,-357.8458;Float;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ClampOpNode;123;-655.386,-2325.19;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,1,1,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;132;98.36572,-972.1281;Float;False;Albedo;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;124;-446.2838,-2313.443;Float;False;Refraction;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ClampOpNode;53;-3186.863,-2444.232;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,1,1,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;131;-1080.388,-2839.121;Float;False;Depth;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;17;-1535.689,528.0044;Float;False;Constant;_Tesselation;Tesselation;1;0;Create;True;0;0;False;0;8;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;32;-2769.382,-245.4154;Float;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;138;-750.8472,-167.7296;Float;False;131;Depth;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;142;-1440.06,923.6804;Float;False;Constant;_Max;Max;20;0;Create;True;0;0;False;0;80;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;141;-1489.06,758.6804;Float;False;Constant;_Min;Min;20;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;143;-1378.159,381.4063;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;50;-3097.322,-2550.885;Float;False;Edge;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;135;-824.8472,-348.7296;Float;False;124;Refraction;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;33;-2567.767,-350.131;Float;True;WaveHeight;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;134;-814.7041,-455.6793;Float;False;132;Albedo;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;96;-4297.885,-2085.589;Float;False;Property;_FoamStrength;Foam Strength;12;0;Create;True;0;0;False;0;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;37;-264.2247,117.9806;Float;False;Constant;_Smoothnes;Smoothnes;4;0;Create;True;0;0;False;0;0.9;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;52;-560.9065,235.3478;Float;False;50;Edge;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;137;-506.8472,-410.7296;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.DistanceBasedTessNode;140;-1243.865,643.4637;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;74;-454.9763,-2.16272;Float;False;72;NormalMaps;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;34;-305.254,231.5696;Float;False;33;WaveHeight;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;0,0;Float;False;True;6;Float;ASEMaterialInspector;0;0;Standard;Water;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;False;0;False;Opaque;;Geometry;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;True;2;15;10;25;False;0.5;False;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;10;0;9;1
WireConnection;10;1;9;3
WireConnection;11;0;10;0
WireConnection;14;0;12;0
WireConnection;14;1;13;0
WireConnection;57;0;56;0
WireConnection;57;1;58;0
WireConnection;59;0;58;0
WireConnection;15;0;14;0
WireConnection;15;1;16;0
WireConnection;68;0;67;0
WireConnection;20;0;15;0
WireConnection;66;0;63;0
WireConnection;66;1;67;0
WireConnection;69;0;64;0
WireConnection;69;1;68;0
WireConnection;60;0;57;0
WireConnection;60;1;59;0
WireConnection;102;0;100;0
WireConnection;102;1;99;0
WireConnection;62;0;60;0
WireConnection;62;2;69;0
WireConnection;110;0;100;0
WireConnection;110;1;111;0
WireConnection;61;0;57;0
WireConnection;61;2;66;0
WireConnection;28;0;6;0
WireConnection;28;1;29;0
WireConnection;103;0;102;0
WireConnection;103;1;101;0
WireConnection;25;0;26;0
WireConnection;109;0;110;0
WireConnection;38;0;54;0
WireConnection;38;1;61;0
WireConnection;38;5;71;0
WireConnection;83;0;78;0
WireConnection;83;1;84;0
WireConnection;55;0;54;0
WireConnection;55;1;62;0
WireConnection;55;5;71;0
WireConnection;3;0;24;0
WireConnection;3;2;5;0
WireConnection;3;1;28;0
WireConnection;89;0;88;0
WireConnection;104;0;75;0
WireConnection;104;1;103;0
WireConnection;108;0;109;0
WireConnection;21;0;25;0
WireConnection;21;2;5;0
WireConnection;21;1;28;0
WireConnection;79;0;83;0
WireConnection;79;1;80;0
WireConnection;70;0;38;0
WireConnection;70;1;55;0
WireConnection;86;0;80;0
WireConnection;1;0;3;0
WireConnection;113;0;104;1
WireConnection;113;1;108;0
WireConnection;22;0;21;0
WireConnection;72;0;70;0
WireConnection;92;0;91;0
WireConnection;92;1;89;0
WireConnection;93;0;90;0
WireConnection;93;1;88;0
WireConnection;87;0;79;0
WireConnection;87;1;86;0
WireConnection;95;0;87;0
WireConnection;95;2;92;0
WireConnection;27;0;1;0
WireConnection;27;1;22;0
WireConnection;94;0;79;0
WireConnection;94;2;93;0
WireConnection;114;0;113;0
WireConnection;76;0;75;0
WireConnection;76;1;94;0
WireConnection;105;0;114;0
WireConnection;44;0;45;0
WireConnection;85;0;75;0
WireConnection;85;1;95;0
WireConnection;120;0;118;0
WireConnection;120;1;119;0
WireConnection;117;0;116;0
WireConnection;30;0;27;0
WireConnection;128;0;129;0
WireConnection;121;0;117;0
WireConnection;121;1;120;0
WireConnection;147;0;76;0
WireConnection;147;1;85;0
WireConnection;46;0;44;0
WireConnection;122;0;121;0
WireConnection;82;0;46;0
WireConnection;82;1;147;0
WireConnection;106;0;39;0
WireConnection;106;1;107;0
WireConnection;139;0;128;0
WireConnection;43;0;42;0
WireConnection;130;0;139;0
WireConnection;48;0;82;0
WireConnection;48;1;49;0
WireConnection;41;0;40;0
WireConnection;41;1;106;0
WireConnection;41;2;43;0
WireConnection;19;0;18;0
WireConnection;19;1;31;0
WireConnection;123;0;122;0
WireConnection;132;0;41;0
WireConnection;124;0;123;0
WireConnection;53;0;48;0
WireConnection;131;0;130;0
WireConnection;32;0;19;0
WireConnection;32;1;27;0
WireConnection;143;0;31;0
WireConnection;143;1;17;0
WireConnection;50;0;53;0
WireConnection;33;0;32;0
WireConnection;137;0;134;0
WireConnection;137;1;135;0
WireConnection;137;2;138;0
WireConnection;140;0;143;0
WireConnection;140;1;141;0
WireConnection;140;2;142;0
WireConnection;0;0;137;0
WireConnection;0;1;74;0
WireConnection;0;2;52;0
WireConnection;0;4;37;0
WireConnection;0;11;34;0
WireConnection;0;14;140;0
ASEEND*/
//CHKSM=E789F029B004ED935B5C5463460CC4B394722D63
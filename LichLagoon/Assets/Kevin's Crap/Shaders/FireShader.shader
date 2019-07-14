// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Fire Shader"
{
	Properties
	{
		_TextureSample0("Texture Sample 0", 2D) = "white" {}
		[HDR]_TextureSample1("Texture Sample 1", 2D) = "white" {}
		[HDR]_TextureSample2("Texture Sample 2", 2D) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Standard alpha:fade keepalpha noshadow exclude_path:deferred 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _TextureSample0;
		uniform sampler2D _TextureSample1;
		uniform sampler2D _TextureSample2;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float4 color17 = IsGammaSpace() ? float4(144.9965,56.9358,14.42374,0) : float4(56883.55,7275.251,354.79,0);
			float2 panner7 = ( _Time.y * float2( 0,-1 ) + i.uv_texcoord);
			float4 tex2DNode11 = tex2D( _TextureSample1, panner7 );
			float4 lerpResult14 = lerp( float4( i.uv_texcoord, 0.0 , 0.0 ) , tex2DNode11 , 0.1);
			float4 tex2DNode15 = tex2D( _TextureSample0, lerpResult14.rg );
			float2 panner6 = ( _Time.y * float2( -1,-5 ) + i.uv_texcoord);
			o.Emission = ( color17 * ( tex2DNode15 * ( tex2DNode11 * pow( 1.2 , tex2D( _TextureSample2, panner6 ).r ) ) ) ).rgb;
			o.Alpha = tex2DNode15.a;
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=16800
7;39;2546;1340;2391.616;366.8579;1;True;False
Node;AmplifyShaderEditor.Vector2Node;1;-2273.77,944.3297;Float;False;Constant;_DissolveSpeed;Dissolve Speed;2;0;Create;True;0;0;False;0;-1,-5;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.TextureCoordinatesNode;2;-2281.77,804.3297;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector2Node;3;-2330.103,402.6631;Float;False;Constant;_DistortionSpeed;Distortion Speed;2;0;Create;True;0;0;False;0;0,-1;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.TextureCoordinatesNode;4;-2338.103,262.6631;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleTimeNode;5;-2339.103,586.663;Float;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;6;-1942.771,821.3297;Float;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;7;-1999.104,279.6631;Float;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;11;-1686.104,269.6631;Float;True;Property;_TextureSample1;Texture Sample 1;1;1;[HDR];Create;True;0;0;False;0;None;f11bd6f7a64dbf24596ff346365341f7;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;8;-1585.104,-130.3369;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;10;-1585.104,144.6631;Float;False;Constant;_DistortionAmount;Distortion Amount;2;0;Create;True;0;0;False;0;0.1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;9;-1637.104,779.663;Float;True;Property;_TextureSample2;Texture Sample 2;2;1;[HDR];Create;True;0;0;False;0;None;275a347000eeb4f47a89287de661170c;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;12;-1340.104,633.663;Float;False;Constant;_DissolveAmount;Dissolve Amount;3;0;Create;True;0;0;False;0;1.2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;14;-1276.104,99.66312;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.PowerNode;13;-1117.104,772.663;Float;False;2;0;FLOAT;0;False;1;COLOR;1,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;16;-965.1035,566.663;Float;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;15;-924.1035,78.66312;Float;True;Property;_TextureSample0;Texture Sample 0;0;0;Create;True;0;0;False;0;None;b5eeaa23b5a527c4eb50a7a2bfa6dc48;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;18;-484.1035,338.6631;Float;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;17;-638.1034,-386.3369;Float;False;Constant;_Color0;Color 0;0;1;[HDR];Create;True;0;0;False;0;144.9965,56.9358,14.42374,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;19;-389.1035,-85.33687;Float;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;-2,0;Float;False;True;2;Float;ASEMaterialInspector;0;0;Standard;Fire Shader;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;0.5;True;False;0;False;Transparent;;Transparent;ForwardOnly;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;2;5;False;-1;10;False;-1;2;5;False;-1;10;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;6;0;2;0
WireConnection;6;2;1;0
WireConnection;6;1;5;0
WireConnection;7;0;4;0
WireConnection;7;2;3;0
WireConnection;7;1;5;0
WireConnection;11;1;7;0
WireConnection;9;1;6;0
WireConnection;14;0;8;0
WireConnection;14;1;11;0
WireConnection;14;2;10;0
WireConnection;13;0;12;0
WireConnection;13;1;9;0
WireConnection;16;0;11;0
WireConnection;16;1;13;0
WireConnection;15;1;14;0
WireConnection;18;0;15;0
WireConnection;18;1;16;0
WireConnection;19;0;17;0
WireConnection;19;1;18;0
WireConnection;0;2;19;0
WireConnection;0;9;15;4
ASEEND*/
//CHKSM=6FD7AF57AAC6F5DCB7A6C69D4FB1F2A696E1CE34
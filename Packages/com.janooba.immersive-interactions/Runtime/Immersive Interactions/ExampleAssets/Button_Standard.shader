// Made with Amplify Shader Editor v1.9.1.5
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Janooba/Button Standard"
{
	Properties
	{
		[Enum(UnityEngine.Rendering.CullMode)]_CullMode("Cull Mode", Int) = 2
		_Color("Tint", Color) = (1,1,1,0)
		_MainTex("Diffuse", 2D) = "white" {}
		_BumpMap("Normal", 2D) = "bump" {}
		_MetallicGlossMap("Metallic Smoothness", 2D) = "white" {}
		_Metallic("Metallic", Range( 0 , 1)) = 0
		_Glossiness("Smoothness", Range( 0 , 1)) = 0.5
		[HDR]_EmissionColor("Emission Color", Color) = (0,0,0,0)
		_EmissionMap("Emission", 2D) = "black" {}
		_Icon("Icon", 2D) = "black" {}
		[Toggle]_UseAlpha("UseAlpha", Float) = 0
		[Toggle]_AddIconToEmiss("AddIconToEmiss", Float) = 1
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull [_CullMode]
		CGPROGRAM
		#pragma target 3.5
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform int _CullMode;
		uniform sampler2D _BumpMap;
		uniform float4 _BumpMap_ST;
		uniform float4 _Color;
		uniform sampler2D _MainTex;
		uniform float4 _MainTex_ST;
		uniform sampler2D _Icon;
		uniform float4 _Icon_ST;
		uniform float _UseAlpha;
		uniform float _AddIconToEmiss;
		uniform sampler2D _EmissionMap;
		uniform float4 _EmissionMap_ST;
		uniform float4 _EmissionColor;
		uniform float _Metallic;
		uniform sampler2D _MetallicGlossMap;
		uniform float4 _MetallicGlossMap_ST;
		uniform float _Glossiness;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_BumpMap = i.uv_texcoord * _BumpMap_ST.xy + _BumpMap_ST.zw;
			o.Normal = UnpackNormal( tex2D( _BumpMap, uv_BumpMap ) );
			float2 uv_MainTex = i.uv_texcoord * _MainTex_ST.xy + _MainTex_ST.zw;
			float2 uv_Icon = i.uv_texcoord * _Icon_ST.xy + _Icon_ST.zw;
			float4 tex2DNode36 = tex2D( _Icon, uv_Icon );
			float4 lerpResult37 = lerp( ( _Color * tex2D( _MainTex, uv_MainTex ) ) , tex2DNode36 , (( _UseAlpha )?( tex2DNode36.a ):( tex2DNode36.r )));
			o.Albedo = lerpResult37.rgb;
			float2 uv_EmissionMap = i.uv_texcoord * _EmissionMap_ST.xy + _EmissionMap_ST.zw;
			float4 tex2DNode5 = tex2D( _EmissionMap, uv_EmissionMap );
			float4 Icon38 = tex2DNode36;
			o.Emission = ( (( _AddIconToEmiss )?( ( tex2DNode5 + Icon38 ) ):( tex2DNode5 )) * _EmissionColor ).rgb;
			float2 uv_MetallicGlossMap = i.uv_texcoord * _MetallicGlossMap_ST.xy + _MetallicGlossMap_ST.zw;
			float4 tex2DNode30 = tex2D( _MetallicGlossMap, uv_MetallicGlossMap );
			o.Metallic = ( _Metallic * tex2DNode30.r );
			o.Smoothness = ( tex2DNode30.a * _Glossiness );
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19105
Node;AmplifyShaderEditor.ColorNode;8;-569.0421,-298.832;Inherit;False;Property;_Color;Tint;1;0;Create;False;0;0;0;False;0;False;1,1,1,0;0.8207547,0.2439035,0.2439035,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;9;-327.2606,-214.9772;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.IntNode;1;1019.19,53.89999;Inherit;False;Property;_CullMode;Cull Mode;0;1;[Enum];Create;True;0;0;1;UnityEngine.Rendering.CullMode;True;0;False;2;2;False;0;1;INT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;804.1898,52.9;Float;False;True;-1;3;ASEMaterialInspector;0;0;Standard;Janooba/Button Standard;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;1;False;;1;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;True;_CullMode;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
Node;AmplifyShaderEditor.SamplerNode;16;-656.7131,62.30393;Inherit;True;Property;_TextureSample0;Texture Sample 0;8;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TexturePropertyNode;15;-953.9735,62.08331;Inherit;True;Property;_BumpMap;Normal;3;0;Create;False;0;0;0;False;0;False;None;None;True;bump;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.SamplerNode;2;-654,-125;Inherit;True;Property;_MainTex;Diffuse;2;0;Create;False;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;33;-253.8794,502.5586;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;32;-638.081,557.5565;Inherit;False;Property;_Glossiness;Smoothness;6;0;Create;False;0;0;0;False;0;False;0.5;0.662;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;34;-636.4144,274.5447;Inherit;False;Property;_Metallic;Metallic;5;0;Create;False;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;35;-251.7581,337.7575;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;41;-285.1709,785.9454;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ToggleSwitchNode;40;-139.9601,650.2735;Inherit;False;Property;_AddIconToEmiss;AddIconToEmiss;11;0;Create;True;0;0;0;False;0;False;1;True;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;6;-130.3981,764.6579;Inherit;False;Property;_EmissionColor;Emission Color;7;1;[HDR];Create;False;0;0;0;False;0;False;0,0,0,0;0,0,0,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;7;117.6018,688.6579;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;5;-652.9113,644.3735;Inherit;True;Property;_EmissionMap;Emission;8;0;Create;False;0;0;0;False;0;False;-1;None;None;True;0;False;black;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;30;-655.0422,360.8228;Inherit;True;Property;_MetallicGlossMap;Metallic Smoothness;4;0;Create;False;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;37;-60.02679,-447.7545;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ToggleSwitchNode;43;-341.0732,-405.1427;Inherit;False;Property;_UseAlpha;UseAlpha;10;0;Create;True;0;0;0;False;0;False;0;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;38;-65.5478,-532.9263;Inherit;False;Icon;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;39;-528.5072,836.5081;Inherit;False;38;Icon;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;36;-656.4227,-500.7609;Inherit;True;Property;_Icon;Icon;9;0;Create;True;0;0;0;False;0;False;-1;None;586cf0710e52d6c418f8baa262ced8ea;True;0;False;black;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
WireConnection;9;0;8;0
WireConnection;9;1;2;0
WireConnection;0;0;37;0
WireConnection;0;1;16;0
WireConnection;0;2;7;0
WireConnection;0;3;35;0
WireConnection;0;4;33;0
WireConnection;16;0;15;0
WireConnection;33;0;30;4
WireConnection;33;1;32;0
WireConnection;35;0;34;0
WireConnection;35;1;30;1
WireConnection;41;0;5;0
WireConnection;41;1;39;0
WireConnection;40;0;5;0
WireConnection;40;1;41;0
WireConnection;7;0;40;0
WireConnection;7;1;6;0
WireConnection;37;0;9;0
WireConnection;37;1;36;0
WireConnection;37;2;43;0
WireConnection;43;0;36;1
WireConnection;43;1;36;4
WireConnection;38;0;36;0
ASEEND*/
//CHKSM=5A78A1FD57116F9F7CBBDD66BEE0BC507F8464E7
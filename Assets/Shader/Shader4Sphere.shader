// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Shader4Sphere" {

	Properties{
		_MainTex("Base (RGB)", 2D) = "white" {}
	}

	SubShader{
		Tags{ "RenderType" = "Opaque" }
		Cull front   // TO FLIP THE SURFACES 表面剔除,这样才能从里面看到全景视频
		LOD 100

		Pass{
			CGPROGRAM

		#pragma vertex vert  
		#pragma fragment frag

		#include "UnityCG.cginc"

			// 参考 https://www.cnblogs.com/isayes/p/6532578.html

			// 模型输入
			struct appdata_t {
				// 使用 POSITION 语义得到了模型的顶点位置坐标;
				float4 vertex : POSITION;
				// 使用 TEXCOORD0 表示模型空间的第一套纹理;
				float2 texcoord : TEXCOORD0;
			};

			// 使用一个结构体来定义顶点着色器的输出
			struct v2f {
				//  SV_POSITION 语义告诉 Unity , vertex 里包括了顶点在裁剪空间的位置信息
				float4 vertex : SV_POSITION;
				half2 texcoord : TEXCOORD0;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;  // 讲解: http://blog.sina.com.cn/s/blog_471132920101dayd.html

			// 这一步是把顶点坐标从模型空间转换到裁剪空间
			v2f vert(appdata_t v) {
				v2f o;
				// 转其齐次坐标
				o.vertex = UnityObjectToClipPos(v.vertex);

				v.texcoord.x = 1 - v.texcoord.x; // 左右取反,和表面剔除对应的
				// TRANSFORM_TEX方法比较简单，就是将模型顶点的uv和Tiling、Offset两个变量进行运算，计算出实际显示用的定点uv
				// Tiling表示UV坐标的缩放倍数，Offset表示UV坐标的起始位置。
				o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
				return o;
			}

			// 输出颜色信息
			fixed4 frag(v2f i) : SV_Target
			{
				// 根据纹理坐标采样
				fixed4 col = tex2D(_MainTex, i.texcoord);
				return col;
			}

			ENDCG
		}

	}
}

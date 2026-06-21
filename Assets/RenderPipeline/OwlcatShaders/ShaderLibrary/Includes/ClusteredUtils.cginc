int2 _GeometryClustersRange;

float4x4 GetObjectToWorldMatrix()
{
	return UNITY_MATRIX_M;
}

float4x4 GetWorldToViewMatrix()
{
	return UNITY_MATRIX_V;
}

float4x4 GetWorldToHClipMatrix()
{
	return UNITY_MATRIX_VP;
}

float3 TransformObjectToWorld(float3 positionOS)
{
	return mul(GetObjectToWorldMatrix(), float4(positionOS, 1.0)).xyz;
}

float3 TransformWorldToView(float3 positionWS)
{
	return mul(GetWorldToViewMatrix(), float4(positionWS, 1.0)).xyz;
}

float4 TransformWorldToHClip(float3 positionWS)
{
	return mul(GetWorldToHClipMatrix(), float4(positionWS, 1.0));
}

// Transforms position from object space to homogenous space
float4 TransformObjectToHClip(float3 positionOS)
{
	// More efficient than computing M*VP matrix product
	return mul(GetWorldToHClipMatrix(), mul(GetObjectToWorldMatrix(), float4(positionOS, 1.0)));
}

// from Unity HDRP lightlistbuild-clustered
bool DoesSphereOverlapTile(float3 dir, float halfTileSizeAtZDistOne, float3 sphCen_in, float sphRadiusIn, bool isOrthographic)
{
	float3 V = float3(isOrthographic ? 0.0 : dir.x, isOrthographic ? 0.0 : dir.y, dir.z); // ray direction down center of tile (does not need to be normalized).
	float3 sphCen = float3(sphCen_in.x - (isOrthographic ? dir.x : 0.0), sphCen_in.y - (isOrthographic ? dir.y : 0.0), sphCen_in.z);

#if 0
	float3 maxZdir = float3(-sphCen.z * sphCen.x, -sphCen.z * sphCen.y, sphCen.x * sphCen.x + sphCen.y * sphCen.y); // cross(sphCen,cross(Zaxis,sphCen))
	float len = length(maxZdir);
	float scalarProj = len > 0.0001 ? (maxZdir.z / len) : len; // if len<=0.0001 then either |sphCen|<sphRadius or sphCen is very closely aligned with Z axis in which case little to no additional offs needed.
	float offs = scalarProj * sphRadiusIn;
#else
	float offs = sphRadiusIn;       // more false positives due to larger radius but works too
#endif

									// enlarge sphere so it overlaps the center of the tile assuming it overlaps the tile to begin with.
	float s = sphCen.z + offs;
	//#if USE_LEFT_HAND_CAMERA_SPACE
	//    float s = sphCen.z+offs;
	//#else
	//    float s = -(sphCen.z - offs);
	//#endif
	float sphRadius = sphRadiusIn + (isOrthographic ? 1.0 : s) * halfTileSizeAtZDistOne;

	float a = dot(V, V);
	float CdotV = dot(sphCen, V);
	float c = dot(sphCen, sphCen) - sphRadius * sphRadius;

	float fDescDivFour = CdotV * CdotV - a * c;

	return c < 0 || (fDescDivFour > 0 && CdotV > 0); // if ray hits bounding sphere
}

struct VertexInputClusters
{
	float4 vertex : POSITION;
	#if defined(INSTANCED_GEOMETRY)
		UNITY_VERTEX_INPUT_INSTANCE_ID
	#endif
};

struct FramgentInputClusters
{
	float3 positionVS : TEXCOORD0;
	float4 clipPos : SV_POSITION;
	#if defined(INSTANCED_GEOMETRY)
		UNITY_VERTEX_INPUT_INSTANCE_ID
	#endif
};

struct GeometryOutputClusters
{
	float3 positionVS : TEXCOORD0;
	float4 clipPos : SV_POSITION;
	#if defined(INSTANCED_GEOMETRY) || defined(GEOMETRY)
		uint RTIndex : SV_RenderTargetArrayIndex;
	#endif
	#if defined(INSTANCED_GEOMETRY)
		UNITY_VERTEX_INPUT_INSTANCE_ID
	#endif
};

UNITY_INSTANCING_BUFFER_START(ZSlicesProps)
	UNITY_DEFINE_INSTANCED_PROP(int, _SliceIndices)
UNITY_INSTANCING_BUFFER_END(zSlices)

FramgentInputClusters Vert (VertexInputClusters v)
{
	FramgentInputClusters o;
	#if defined(INSTANCED_GEOMETRY)
		UNITY_SETUP_INSTANCE_ID(v);
		UNITY_TRANSFER_INSTANCE_ID(v, o);
	#endif

	#if defined(GEOMETRY)
		o.positionVS = v.vertex.xyz;
	#else
		// NOTE: нобходимо использовать именно встроенную матрицу, иначе не будет работать инстансинг!!!
		// видимо инстансинг завязан на системные значения
		float3 positionWS = TransformObjectToWorld(v.vertex.xyz);
		o.positionVS = TransformWorldToView(positionWS);

		//flip z-coord, coz Unity Use RH view-matrix
		o.positionVS.z = -o.positionVS.z;
	#endif

	o.clipPos = TransformObjectToHClip(v.vertex.xyz);
	return o;
}

#define MAX_SLICE_TO_COPY 42 // 1028 / 8 / 3 - геометрический шейдер может копировать не больше 1024 float'ов. Одна вершина занимает 8 float, в треугольнике 3 вершины
#if defined(GEOMETRY)
	[maxvertexcount(MAX_SLICE_TO_COPY * 3)]
#else
	[maxvertexcount(3)]
#endif
void Geom(triangle FramgentInputClusters IN[3], inout TriangleStream<GeometryOutputClusters> stream)
{
	#if defined(GEOMETRY)
		for (int j = _GeometryClustersRange.x; j < _GeometryClustersRange.y; j++)
		{
			[unroll]
			for (int i = 0; i < 3; i++)
			{
				FramgentInputClusters input = IN[i];
				GeometryOutputClusters OUT = (GeometryOutputClusters)0;
				float4x4 worldMatrix = _SliceMatrices[j];
				float3 positionOS = input.positionVS;
				float3 positionWS = mul(worldMatrix, float4(positionOS.xyz, 1.0)).xyz;
				OUT.positionVS = TransformWorldToView(positionWS);
				OUT.positionVS.z = -OUT.positionVS.z;
				OUT.clipPos = TransformWorldToHClip(positionWS);
				OUT.RTIndex = j;

				stream.Append(OUT);
			}
		}
	#else
		[unroll]
		for (int i = 0; i < 3; i++)
		{
			FramgentInputClusters input = IN[i];
			GeometryOutputClusters OUT = (GeometryOutputClusters)0;
			UNITY_SETUP_INSTANCE_ID(IN[i])
			UNITY_TRANSFER_INSTANCE_ID(IN[i], OUT);

			OUT.positionVS = input.positionVS;
			OUT.clipPos = input.clipPos;
			#if defined(INSTANCED_GEOMETRY)
				OUT.RTIndex = UNITY_ACCESS_INSTANCED_PROP(zSlices, _SliceIndices);
			#elif defined(GEOMETRY)
				OUT.RTIndex = 0;
			#endif

			stream.Append(OUT);
		}
	#endif

	stream.RestartStrip();
}
			
#ifdef SHADER_API_METAL
uint Frag (GeometryOutputClusters i) : SV_Target
#else
int Frag (GeometryOutputClusters i) : SV_Target
#endif
{
	#if defined(INSTANCED_GEOMETRY) || defined(GEOMETRY)
		int sliceIndex = i.RTIndex;
	#else
		int sliceIndex = _GeometryClustersRange.x;
	#endif

    float2 sliceDepths = _SliceZNearFar[sliceIndex];
	float4 halfTexelSize = _SliceHalfTexelSize[sliceIndex];
	float3 farCenter = i.positionVS;

	int2 zBin = _ZBins[sliceIndex];
	uint result = 0;
	for (int indexInZBin = 0; indexInZBin < zBin.y; indexInZBin++)
	{
		int mapIndex = zBin.x + indexInZBin;
		int lightIndex = _PunctualLightIndexMap[mapIndex];
		float4 lightPosAndRange = _LightPositionVSAndRange[lightIndex];
		bool inFrustum = true;

		inFrustum = DoesSphereOverlapTile(farCenter, _HalfTexelSizeAtNearPlane.x, lightPosAndRange.xyz, lightPosAndRange.w, unity_OrthoParams.w > 0);

		if (inFrustum)
		{
			result |= 1 << indexInZBin;
		}
	}

    #ifdef SHADER_API_METAL
	    return result;
    #else
        return asuint(result);
    #endif
}
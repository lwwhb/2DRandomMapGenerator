/*-----------------
 * File: LandShape.cs
 * Description: 定义陆地形状的工厂函数
 * Author: haibo.wang@unity3d.com
 * Date: 2022-6-7
 */
using Unity.Mathematics;
using UnityEngine;

namespace TinyFlare
{
    namespace RandomMapGenerator
    {
        public class LandShape
        {
			private static float RADIAL_LAND_FACTOR = 1.07f;				// 1.0 表示比较平; 2.0 凸起较多
			private static float RADIAL_LAND_SCALE = 0.5f;					// 越大陆地越小
			private static float RADIAL_LAND_SCALE_RANDOMNESS = 0.0f;		// 随机性(0-1.0f)
			private static float RADIAL_LAND_SLOPE = 0.5f;                  // 大陆架，越接近0，海湾越明显，越接近1，大陆架越明显
			private static float RADIAL_LAND_OFFSET_X = 0.0f;				// 岛屿X方向上偏移 (-0.5 ~ 0.5)
			private static float RADIAL_LAND_OFFSET_Y = 0.0f;               // 岛屿Y方向上偏移 (-0.5 ~ 0.5)
			/*
			 * 基于重叠正弦波
			 */
			public static System.Func<float2, bool> makeRadial(int w, int h, bool needsMoreRandomness)
			{
				var bumps = TilesDiagram.rand.NextUInt(1, 6);
				var startAngle = TilesDiagram.rand.NextFloat(0.0f, 1.0f) * 2 * math.PI;
				var dipAngle = TilesDiagram.rand.NextFloat(0.0f, 1.0f) * 2 * math.PI;

				var random = TilesDiagram.rand.NextFloat(0.0f, 1.0f);
				var start = TilesDiagram.rand.NextFloat(0.0f, 0.5f);
				var end = TilesDiagram.rand.NextFloat(0.5f, 1.0f);

				var dipWidth = (end - start) * random + start;

				System.Func<float2, bool> inside = q =>
				{
					q = new float2(((q.x - RADIAL_LAND_OFFSET_X * w) / w * 2 - 1.0f) + (needsMoreRandomness ? TilesDiagram.rand.NextFloat(0.0f, RADIAL_LAND_SCALE_RANDOMNESS) : 0),
								   ((q.y - RADIAL_LAND_OFFSET_Y * h) / h * 2 - 1.0f) + (needsMoreRandomness ? TilesDiagram.rand.NextFloat(0.0f, RADIAL_LAND_SCALE_RANDOMNESS) : 0));
					var angle = math.atan2(q.y, q.x);
					var length = RADIAL_LAND_SCALE * (math.max(math.abs(q.x), math.abs(q.y)) + math.sqrt(q.x*q.x + q.y*q.y));

					var r1 = start + TilesDiagram.rand.NextFloat(0.0f, 0.5f) * math.sin(startAngle + bumps * angle + math.cos((bumps + 3) * angle));
					var r2 = end - TilesDiagram.rand.NextFloat(0.0f, 0.5f) * math.sin(startAngle + bumps * angle - math.sin((bumps + 2) * angle));
					if (math.abs(angle - dipAngle) < dipWidth
						|| math.abs(angle - dipAngle + 2 * math.PI) < dipWidth
						|| math.abs(angle - dipAngle - 2 * math.PI) < dipWidth)
					{
						r1 = TilesDiagram.rand.NextFloat(0.0f, RADIAL_LAND_SLOPE);
						r2 = TilesDiagram.rand.NextFloat(RADIAL_LAND_SLOPE, 1.0f);
					}
					var result = (length < r1 || (length > r1 * RADIAL_LAND_FACTOR && length < r2));
					return result;
				};

				return inside;
			}


			/*
			 * 基于Unity Mathf的Perlin函数
			 */
			private static float MATHF_PERLIN_LAND_FACTOR = 1.03f;				// 1.0 表示比较平; 2.0 凸起较多
			private static float MATHF_PERLIN_LAND_SCALE = 4.0f;				// 越大陆地越小(1-10)
			private static float MATHF_PERLIN_LAND_SCALE_RANDOMNESS = 0.15f;	// 随机性(0-1.0f)
			private static float MATHF_PERLIN_LAND_SLOPE = 0.5f;				// 海洋面积，越接近与0，陆地越多，越接近于1，海洋越多
			public static System.Func<float2, bool> makeMathfPerlin(int w, int h, bool needsMoreRandomness)
			{
				System.Func<float2, bool> inside = q =>
				{
					var x = q.x / w * MATHF_PERLIN_LAND_SCALE + (needsMoreRandomness ? TilesDiagram.rand.NextFloat(0.0f, MATHF_PERLIN_LAND_SCALE_RANDOMNESS) : 0);
					var y = q.y / h * MATHF_PERLIN_LAND_SCALE + (needsMoreRandomness ? TilesDiagram.rand.NextFloat(0.0f, MATHF_PERLIN_LAND_SCALE_RANDOMNESS) : 0);
					var perlin = Mathf.PerlinNoise(x, y) * MATHF_PERLIN_LAND_FACTOR;
					var result = perlin > MATHF_PERLIN_LAND_SLOPE;
					return result;
				};
				return inside;
			}

			/*
			 * 基于Unity Mathematics的Simplex函数
			 */
			private static float MATHEMATICS_SIMPLEX_LAND_FACTOR = 1.03f;			// 1.0 表示比较平; 2.0 凸起较多
			private static float MATHEMATICS_SIMPLEX_LAND_SCALE = 4.0f;             // 越大陆地越小(1-10)
			private static float MATHEMATICS_SIMPLEX_LAND_SCALE_RANDOMNESS = 0.15f; // 随机性(0-1.0f)
			private static float MATHEMATICS_SIMPLEX_LAND_SLOPE = 0.35f;			// 海洋面积，越接近与0，陆地越多，越接近于1，海洋越多
			private static bool MATHEMATICS_SIMPLEX_WITH_FIXEDGRADIENTS = false;	// 是否开启固定梯度
			private static float MATHEMATICS_SIMPLEX_FIXEDGRADIENTS = 0.2f;         // 固定梯度值(0-1)
			public static System.Func<float2, bool> makeMathematicsSimplex(int w, int h, bool needsMoreRandomness)
			{
				System.Func<float2, bool> inside = q =>
				{
					var x = q.x / w * MATHEMATICS_SIMPLEX_LAND_SCALE + (needsMoreRandomness ? TilesDiagram.rand.NextFloat(0.0f, MATHEMATICS_SIMPLEX_LAND_SCALE_RANDOMNESS) : 0);
					var y = q.y / h * MATHEMATICS_SIMPLEX_LAND_SCALE + (needsMoreRandomness ? TilesDiagram.rand.NextFloat(0.0f, MATHEMATICS_SIMPLEX_LAND_SCALE_RANDOMNESS) : 0);
					var simplex = 0.0f;
					if(MATHEMATICS_SIMPLEX_WITH_FIXEDGRADIENTS)
						simplex = ((noise.srnoise(new float2(x, y), MATHEMATICS_SIMPLEX_FIXEDGRADIENTS) + 1.0f) * 0.5f) * MATHEMATICS_SIMPLEX_LAND_FACTOR;
					else
						simplex = ((noise.snoise(new float2(x, y)) + 1.0f) * 0.5f) * MATHEMATICS_SIMPLEX_LAND_FACTOR;
					var result = simplex > MATHEMATICS_SIMPLEX_LAND_SLOPE;
					return result;
				};
				return inside;
			}

			/*
			 * 基于Unity Mathematics的标准Perlin函数
			 */
			private static float MATHEMATICS_STANDRD_PERLIN_LAND_FACTOR = 1.03f;				// 1.0 表示比较平; 2.0 凸起较多
			private static float MATHEMATICS_STANDRD_PERLIN_LAND_SCALE = 4.0f;					// 越大陆地越小(1-10)
			private static float MATHEMATICS_STANDRD_PERLIN_LAND_SCALE_RANDOMNESS = 0.15f;      // 随机性(0-1.0f)
			private static float MATHEMATICS_STANDRD_PERLIN_LAND_SLOPE = 0.35f;					// 海洋面积，越接近与0，陆地越多，越接近于1，海洋越多
			public static System.Func<float2, bool> makeMathematicsStandrdPerlin(int w, int h, bool needsMoreRandomness)
			{
				System.Func<float2, bool> inside = q =>
				{
					var x = q.x / w * MATHEMATICS_STANDRD_PERLIN_LAND_SCALE + (needsMoreRandomness ? TilesDiagram.rand.NextFloat(0.0f, MATHEMATICS_STANDRD_PERLIN_LAND_SCALE_RANDOMNESS) : 0);
					var y = q.y / h * MATHEMATICS_STANDRD_PERLIN_LAND_SCALE + (needsMoreRandomness ? TilesDiagram.rand.NextFloat(0.0f, MATHEMATICS_STANDRD_PERLIN_LAND_SCALE_RANDOMNESS) : 0);
					var perlin = ((noise.cnoise(new float2(x, y)) + 1.0f) * 0.5f) * MATHEMATICS_STANDRD_PERLIN_LAND_FACTOR;
					var result = perlin > MATHEMATICS_STANDRD_PERLIN_LAND_SLOPE;
					return result;
				};
				return inside;
			}

			/*
			 * 基于Unity Mathematics的标准待周期性的Perlin函数
			 */
			private static float MATHEMATICS_STANDRD_PERLIN_WITH_PERIODICVARIANT_LAND_FACTOR = 1.03f;				// 1.0 表示比较平; 2.0 凸起较多
			private static float MATHEMATICS_STANDRD_PERLIN_WITH_PERIODICVARIANT_LAND_SCALE = 2.0f;					// 越大陆地越小(1-10)
			private static float MATHEMATICS_STANDRD_PERLIN_WITH_PERIODICVARIANT_LAND_SCALE_RANDOMNESS = 0.15f;     // 随机性(0-1.0f)
			private static float MATHEMATICS_STANDRD_PERLIN_WITH_PERIODICVARIANT_LAND_SLOPE = 0.35f;				// 海洋面积，越接近与0，陆地越多，越接近于1，海洋越多
			private static float MATHEMATICS_STANDRD_PERLIN_WITH_PERIODICVARIANT_X = 4.0f;							// X方向周期性
			private static float MATHEMATICS_STANDRD_PERLIN_WITH_PERIODICVARIANT_Y = 2.0f;                          // Y方向周期性
			private static bool MATHEMATICS_STANDRD_PERLIN_WITH_FIXEDGRADIENTS = true;    // 是否开启固定梯度
			private static float MATHEMATICS_STANDRD_PERLIN_FIXEDGRADIENTS = 0.2f;        // 固定梯度值(0-1)
			public static System.Func<float2, bool> makeMathematicsStandrdPerlinWithPeriodicVariant(int w, int h, bool needsMoreRandomness)
			{
				System.Func<float2, bool> inside = q =>
				{
					var x = q.x / w * MATHEMATICS_STANDRD_PERLIN_WITH_PERIODICVARIANT_LAND_SCALE + (needsMoreRandomness ? TilesDiagram.rand.NextFloat(0.0f, MATHEMATICS_STANDRD_PERLIN_WITH_PERIODICVARIANT_LAND_SCALE_RANDOMNESS) : 0);
					var y = q.y / h * MATHEMATICS_STANDRD_PERLIN_WITH_PERIODICVARIANT_LAND_SCALE + (needsMoreRandomness ? TilesDiagram.rand.NextFloat(0.0f, MATHEMATICS_STANDRD_PERLIN_WITH_PERIODICVARIANT_LAND_SCALE_RANDOMNESS) : 0);
					var perlin = 0.0f;
					if (MATHEMATICS_STANDRD_PERLIN_WITH_FIXEDGRADIENTS)
						perlin = ((noise.psrnoise(new float2(x, y), new float2(MATHEMATICS_STANDRD_PERLIN_WITH_PERIODICVARIANT_X, MATHEMATICS_STANDRD_PERLIN_WITH_PERIODICVARIANT_Y), MATHEMATICS_STANDRD_PERLIN_FIXEDGRADIENTS) + 1.0f) * 0.5f) * MATHEMATICS_STANDRD_PERLIN_WITH_PERIODICVARIANT_LAND_FACTOR;
					else
						perlin = ((noise.pnoise(new float2(x, y), new float2(MATHEMATICS_STANDRD_PERLIN_WITH_PERIODICVARIANT_X, MATHEMATICS_STANDRD_PERLIN_WITH_PERIODICVARIANT_Y)) + 1.0f) * 0.5f) * MATHEMATICS_STANDRD_PERLIN_WITH_PERIODICVARIANT_LAND_FACTOR;
					var result = perlin > MATHEMATICS_STANDRD_PERLIN_WITH_PERIODICVARIANT_LAND_SLOPE;
					return result;
				};
				return inside;
			}

			/*
			 * 基于Unity Mathematics的Worley noise函数
			 */
			private static float MATHEMATICS_CELLULAR_LAND_FACTOR = 1.03f;           // 1.0 表示比较平; 2.0 凸起较多
			private static float MATHEMATICS_CELLULAR_LAND_SCALE = 2.0f;             // 越大陆地越小(1-10)
			private static float MATHEMATICS_CELLULAR_LAND_SCALE_RANDOMNESS = 0.15f; // 随机性(0-1.0f)
			private static float MATHEMATICS_CELLULAR_LAND_SLOPE = 0.15f;            // 海洋面积，越接近与0，陆地越多，越接近于1，海洋越多
			public static System.Func<float2, bool> makeMathematicsCellular(int w, int h, bool needsMoreRandomness)
			{
				System.Func<float2, bool> inside = q =>
				{
					var x = q.x / w * MATHEMATICS_CELLULAR_LAND_SCALE + (needsMoreRandomness ? TilesDiagram.rand.NextFloat(0.0f, MATHEMATICS_CELLULAR_LAND_SCALE_RANDOMNESS) : 0);
					var y = q.y / h * MATHEMATICS_CELLULAR_LAND_SCALE + (needsMoreRandomness ? TilesDiagram.rand.NextFloat(0.0f, MATHEMATICS_CELLULAR_LAND_SCALE_RANDOMNESS) : 0);
					var cellular = noise.cellular(new float2(x, y)) * MATHEMATICS_CELLULAR_LAND_FACTOR;
					var result = cellular.x > MATHEMATICS_CELLULAR_LAND_SLOPE && cellular.y > MATHEMATICS_CELLULAR_LAND_SLOPE;
					return result;
				};
				return inside;
			}
		}
    }
}

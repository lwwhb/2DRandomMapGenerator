/*-----------------
 * File: LandShape.cs
 * Description: 定义陆地形状的工厂函数
 * Author: haibo.wang@unity3d.com
 * Date: 2022-6-7
 */
using Unity.Mathematics;

namespace TinyFlare
{
    namespace RandomMapGenerator
    {
        public class LandShape
        {
			private static float LAND_FACTOR = 1.03f;   // 1.0 表示比较平; 2.0 凸起较多
			private static float LAND_SCALE = 0.35f;     // 越大陆地越小
			private static float LAND_SLOPE = 0.7f;		// 大陆架，越接近0，海湾越明显，越接近1，大陆架越明显
			/*
			 * 基于重叠正弦波
			 */
			public static System.Func<float2, bool> makeRadial(int w, int h, float offsetX, float offsetY)
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
					q = new float2(((q.x + offsetX)/ w * 2 - 1.0f), ((q.y + offsetY) / h * 2 - 1.0f));
					var angle = math.atan2(q.y, q.x);
					var length = LAND_SCALE * (math.max(math.abs(q.x), math.abs(q.y)) + math.sqrt(q.x*q.x + q.y*q.y));

					var r1 = start + TilesDiagram.rand.NextFloat(0.0f, 0.5f) * math.sin(startAngle + bumps * angle + math.cos((bumps + 3) * angle));
					var r2 = end - TilesDiagram.rand.NextFloat(0.0f, 0.5f) * math.sin(startAngle + bumps * angle - math.sin((bumps + 2) * angle));
					if (math.abs(angle - dipAngle) < dipWidth
						|| math.abs(angle - dipAngle + 2 * math.PI) < dipWidth
						|| math.abs(angle - dipAngle - 2 * math.PI) < dipWidth)
					{
						r1 = TilesDiagram.rand.NextFloat(0.0f, LAND_SLOPE);
						r2 = TilesDiagram.rand.NextFloat(LAND_SLOPE, 1.0f);
					}
					var result = (length < r1 || (length > r1 * LAND_FACTOR && length < r2));
					return result;
				};

				return inside;
			}
		}
    }
}

/*-----------------
 * File: SimpleTilesDiagram.cs
 * Description: 定义简化版Tile简图信息，包括Site, Edge, Corner的关系
 * Author: haibo.wang@unity3d.com
 * Date: 2022-6-10
 */

using TinyFlare.RandomMapGenerator.MapDataStructures;
using UnityEngine;

namespace TinyFlare
{
	namespace RandomMapGenerator
	{
		/*
		 * 简化版Tiles简图
		 */
		public class SimpleTilesDiagram : TilesDiagram
		{
			public SimpleTilesDiagram()
			{
				needsMoreRandomness = true;
			}
			/*
			 * 初始化
			 */
			public override bool Init(int width, int height, int numX, int numY)
			{
				if ((width % numX != 0) || (height % numY != 0))
				{
					Debug.LogError("地图的长宽不能被分块整除，请调整地图长宽或分块个数以匹配。");
					return false;
				}
				boundRect = new Rect(0, 0, width, height);
				tileWidth = width / numX;
				tileHeight = height / numY;
				tileNumX = numX;
				tileNumY = numY;

				//初始化随机种子
				rand = Unity.Mathematics.Random.CreateFromIndex(randomSeed);

				//初始化地图Site,Edge,Corner基础数据
				InitMapData();

				//构建Site,Edge,Corner关联关系
				BuildRelationship();

				//初始化地形类型（暂时先用一个类型）
				inside = LandShape.makeRadial(width, height, tileWidth * 0.5f, tileHeight * 0.5f);

				//构建Corners海拔
				AssignCornerElevations();
				//基于Corner信息计算Tile的Ocean、Water属性
				AssignOceanCoastAndLand();
				//重新构建Corners海拔
				RedistributeCornersElevations();
				//设置陆地Regions海拔（也就是sites的海拔）
				AssignLandRegionsElevations();
				//设置海洋Regions海拔（也就是sites的海拔）
				AssignOceanRegionsElevations();

				//设置区域的的生物群落（也就是Sites的生物群落）
				AssignRegionsBiomes();
				return true;
			}
			/*
			 * 设置区域的的生物群落（也就是Sites的生物群落）
			 */
			protected override void AssignRegionsBiomes()
			{
				for (int i = 0; i < sites.Length; ++i)
				{
					Site site = sites[i];
					if (site.water && site.biomeType == BiomeType.BT_Ocean)
						continue;
					else if (site.water)
					{
						if (site.elevation < 0.1f)
							site.biomeType = BiomeType.BT_Marsh;
						else if (site.elevation > 0.8f)
							site.biomeType = BiomeType.BT_Ice;
						else
							site.biomeType = BiomeType.BT_Lake;
					}
					else if (site.elevation > 0.8f)
					{
						site.biomeType = BiomeType.BT_Snow;
					}
					else if (site.elevation > 0.5f)
					{
						site.biomeType = BiomeType.BT_Bare;
					}
					else if (site.elevation > 0.1f)
					{
						site.biomeType = BiomeType.BT_Grassland;
					}
					else
					{
						site.water = true;
						site.biomeType = BiomeType.BT_River;
					}
					sites[i] = site;
				}
			}
		}
	}
}

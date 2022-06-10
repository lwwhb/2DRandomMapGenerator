/*-----------------
 * File: TilesDiagram.cs
 * Description: 定义地图必要的Tile简图信息，包括Site, Edge, Corner的关系
 * Author: haibo.wang@unity3d.com
 * Date: 2022-5-31
 */
using System;
using System.Collections.Generic;
using TinyFlare.RandomMapGenerator.MapDataStructures;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace TinyFlare
{
    namespace RandomMapGenerator
    {
		/*
		 * Tiles简图
		 */
		public class TilesDiagram
		{
			public static float SCALE_FACTOR = 1.1f;                        //越大越增加山区面积, 越大海洋深度下降越快
			public static int LAKE_THRESHOLD = 2;							//越大，形成湖的几率越高。数量在1-4之间

			private NativeArray<Site> sites;								//Sites数组
			public NativeArray<Site> Sites { get { return sites; } }
			private NativeArray<Edge> edges;								//Edges数组
			public NativeArray<Edge> Edges { get { return edges; } }
			private NativeArray<Corner> corners;							//Corners数据
			public NativeArray<Corner> Corners { get { return corners; } }
			private Rect boundRect;											//地图边界矩形
			private int tileWidth;											//单个Tile长
			public int TileWidth { get { return tileWidth; } }
			private int tileHeight;											//单个Tile宽
			public int TileHeight { get { return tileHeight; } }
			private int tileNumX, tileNumY;									//水平与垂直方向Tile的个数	

			private Func<float2, bool> inside;

			private uint randomSeed = 10000;		// 待定义
			public static Unity.Mathematics.Random rand;
			private bool needsMoreRandomness = true;

			public TilesDiagram()
			{ }

			/*
			 * 初始化
			 */
			public bool Init(int width, int height, int numX, int numY)
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
				AssignRegionsElevations();
				//设置海洋Regions海拔（也就是sites的海拔）
				AssignOceanRegionsElevations();
				//计算Corners的下游点
				CalculateCornersDownslopes();
				//计算Corners的河流流量图
				CalculateCornersWatersheds();
				//创建河流
				CreateRivers();
				return true;
			}

			/*
			 * 销毁
			 */
			public void Destroy()
			{
				if (sites != null && sites.IsCreated)
					sites.Dispose();
				if (edges != null && edges.IsCreated)
					edges.Dispose();
				if (corners != null && corners.IsCreated)
					corners.Dispose();
			}

			/*
			 * 初始化地图Site,Edge,Corner基础数据，测试后改成job比较性能(待优化)
			 */
			private void InitMapData()
			{
				/*  
				 *  初始化节点数据
				 *   -------------------
				 *   |        |        |
				 *   |    x   |    x   |
				 *   |        |        |
				 *   -------------------
				 *   |        |        |
				 *   |    x   |    x   |
				 *   |        |        |
				 *   -------------------
				 */
				sites = new NativeArray<Site>(tileNumX * tileNumY, Allocator.Persistent);
				for (int i = 0; i < tileNumY; ++i)
				{
					for (int j = 0; j < tileNumX; ++j)
					{
						int index = i * tileNumX + j;
						Site site = sites[index];
						site.index = index;
						site.point = new float2(j * tileWidth, i * tileHeight);
						site.water = false;
						site.island = false;
						site.coast = false;
						if (j == 0 || j == tileNumX - 1 || i == 0 || i == tileNumY - 1)
							site.border = true;
						else
							site.border = false;
						site.biomeType = BiomeType.BT_Cliff;
						sites[index] = site;
					}
				}

				/*  
				 *  初始化角点数据
				 *   0--------1--------2
				 *   |        |        |
				 *   |    x   |    x   |
				 *   |        |        |
				 *   3--------4--------5  
				 *   |        |        |
				 *   |    x   |    x   |
				 *   |        |        |
				 *   6--------7--------8  
				 */
				corners = new NativeArray<Corner>((tileNumX + 1) * (tileNumY + 1), Allocator.Persistent);
				for (int i = 0; i < tileNumY + 1; ++i)
				{
					for (int j = 0; j < tileNumX + 1; ++j)
					{
						int index = i * (tileNumX + 1) + j;
						Corner corner = corners[index];
						corner.index = index;
						corner.point = new float2(j * tileWidth - 0.5f * tileWidth, i * tileHeight - 0.5f * tileHeight);
						corner.water = false;
						corner.coast = false;
						if (j == 0 || j == tileNumX || i == 0 || i == tileNumY)
							corner.border = true;
						else
							corner.border = false;
						corner.biomeType = BiomeType.BT_Cliff;
						corner.downslope = -1;      
						corner.watershed = -1;      
						corner.watershed_size = 0;
						corners[index] = corner;
					}
				}

				/*  
				 *  初始化边数据
				 *   -----6---------7----
				 *   |         |        |
				 *   0    x    1    x   2
				 *   |         |        |
				 *   -----8---------9----
				 *   |         |        |
				 *   3    x    4    x   5
				 *   |         |        |
				 *   ----10--------11----
				 */
				edges = new NativeArray<Edge>((tileNumX + 1) * tileNumY + tileNumX * (tileNumY + 1), Allocator.Persistent);
				//竖边
				for (int i = 0; i < tileNumY; ++i)
				{
					for (int j = 0; j < tileNumX + 1; ++j)
					{
						int index = i * (tileNumX + 1) + j;
						Edge edge = edges[index];
						edge.index = index;
						edge.midpoint = new float2(j * tileWidth - 0.5f * tileWidth, i * tileHeight);
						if (j == 0 || j == tileNumX)
							edge.border = true;
						else
							edge.border = false;
						edge.river = 0;
						edges[index] = edge;
					}
				}
				//横边
				for (int i = 0; i < tileNumY + 1; ++i)
				{
					for (int j = 0; j < tileNumX; ++j)
					{
						int index = i * tileNumX + j + (tileNumX + 1) * tileNumY;
						Edge edge = edges[index];
						edge.index = index;
						edge.midpoint = new float2(j * tileWidth, i * tileHeight - 0.5f * tileHeight);
						if (i == 0 || i == tileNumY )
							edge.border = true;
						else
							edge.border = false;
						edge.river = 0;
						edges[index] = edge;
					}
				}
			}

			/*
			 * 构建Site与site、edge、corner的关系(待优化，构建成单独job)
			 */
			private void BuildSitesRelationship()
			{
				for (int i = 0; i < tileNumY; ++i)
				{
					for (int j = 0; j < tileNumX; ++j)
					{
						int index = i * tileNumX + j;
						Site site = sites[index];
						/*  
						 *  建立邻居节点
						 *  -------------------------
						 *  |       |       |       |
						 *  |   s0  |   s1  |   s2  |
						 *  |       |       |       |
						 *  -------------------------
						 *  |       |       |       |
						 *  |   s3  |   x   |   s4  |
						 *  |       |       |       |
						 *  -------------------------
						 *  |       |       |       |
						 *  |   s5  |   s6  |   s7  |
						 *  |       |       |       |
						 *  -------------------------   
						 */
						if (site.border)
						{
							if (i == 0)
							{
								if (j == 0)
								{
									site.s0 = -1;
									site.s1 = -1;
									site.s2 = -1;
									site.s3 = -1;
									site.s4 = index + 1;
									site.s5 = -1;
									site.s6 = index + tileNumX;
									site.s7 = index + tileNumX + 1;
								}
								else if (j == tileNumX - 1)
								{
									site.s0 = -1;
									site.s1 = -1;
									site.s2 = -1;
									site.s3 = index - 1;
									site.s4 = -1;
									site.s5 = index + tileNumX - 1;
									site.s6 = index + tileNumX;
									site.s7 = -1;
								}
								else
								{
									site.s0 = -1;
									site.s1 = -1;
									site.s2 = -1;
									site.s3 = index - 1;
									site.s4 = index + 1;
									site.s5 = index + tileNumX - 1;
									site.s6 = index + tileNumX;
									site.s7 = index + tileNumX + 1;
								}
							}
							else if (i == tileNumY - 1)
							{
								if (j == 0)
								{
									site.s0 = -1;
									site.s1 = index - tileNumX;
									site.s2 = index - tileNumX + 1;
									site.s3 = -1;
									site.s4 = index + 1;
									site.s5 = -1;
									site.s6 = -1;
									site.s7 = -1;
								}
								else if (j == tileNumX - 1)
								{
									site.s0 = index - tileNumX - 1;
									site.s1 = index - tileNumX;
									site.s2 = -1;
									site.s3 = index - 1;
									site.s4 = -1;
									site.s5 = -1;
									site.s6 = -1;
									site.s7 = -1;
								}
								else
								{
									site.s0 = index - tileNumX - 1;
									site.s1 = index - tileNumX;
									site.s2 = index - tileNumX + 1;
									site.s3 = index - 1;
									site.s4 = index + 1;
									site.s5 = -1;
									site.s6 = -1;
									site.s7 = -1;
								}
							}
							else
							{
								if (j == 0)
								{
									site.s0 = -1;
									site.s1 = index - tileNumX;
									site.s2 = index - tileNumX + 1;
									site.s3 = -1;
									site.s4 = index + 1;
									site.s5 = -1;
									site.s6 = index + tileNumX;
									site.s7 = index + tileNumX + 1;
								}
								else if (j == tileNumX - 1)
								{
									site.s0 = index - tileNumX - 1;
									site.s1 = index - tileNumX;
									site.s2 = -1;
									site.s3 = index - 1;
									site.s4 = -1;
									site.s5 = index + tileNumX - 1;
									site.s6 = index + tileNumX;
									site.s7 = -1;
								}
							}
						}
						else
						{
							site.s0 = index - tileNumX - 1;
							site.s1 = index - tileNumX;
							site.s2 = index - tileNumX + 1;
							site.s3 = index - 1;
							site.s4 = index + 1;
							site.s5 = index + tileNumX - 1;
							site.s6 = index + tileNumX;
							site.s7 = index + tileNumX + 1;
						}
						/*  
						 *  建立周围的边
						 *  -------2-------
						 *  |			  |
						 *  |             |
						 *  0      x      1
						 *  |             |
						 *  |             |
						 *  -------3-------
						 */
						site.e0 = index + i;
						site.e1 = index + i + 1;
						site.e2 = index + (tileNumX + 1) * tileNumY;
						site.e3 = index + (tileNumX + 1) * (tileNumY + 1) - 1;
						/*  
						 *  建立周围的边
						 *  0-------------1
						 *  |			  |
						 *  |             |
						 *  |      x      |
						 *  |             |
						 *  |             |
						 *  2-------------3
						 */
						site.c0 = index + i;
						site.c1 = index + i + 1;
						site.c2 = index + i + (tileNumX + 1);
						site.c3 = index + i + (tileNumX + 1) + 1;

						sites[index] = site;
					}
				}
			}

			/*
			 * 建立corner与site、edge、corner的关系(待优化成job)
			 */
			private void BuildCornersRelationship()
			{
				for (int i = 0; i < tileNumY + 1; ++i)
				{
					for (int j = 0; j < tileNumX + 1; ++j)
					{
						int index = i * (tileNumX + 1) + j;
						Corner corner = corners[index];

						/*  
						 *  建立corner与覆盖Tile的site的关系
						 *   ---------------------
						 *   |         |         |
						 *   |    s0   |    s1   |
						 *   |	       |         |
						 *   |-------  x --------|
						 *   |         |         |
						 *   |	  s2   |    s3   |
						 *   |         |         |
						 *   ---------------------    
						 */
						if (corner.border)
						{
							if (i == 0)
							{
								if (j == 0)
								{
									corner.s0 = -1;
									corner.s1 = -1;
									corner.s2 = -1;
									corner.s3 = index - i;
								}
								else if (j == tileNumX)
								{
									corner.s0 = -1;
									corner.s1 = -1;
									corner.s2 = index - i - 1;
									corner.s3 = -1;
								}
								else
								{
									corner.s0 = -1;
									corner.s1 = -1;
									corner.s2 = index - i - 1;
									corner.s3 = index - i;
								}
							}
							else if (i == tileNumY)
							{
								if (j == 0)
								{
									corner.s0 = -1;
									corner.s1 = index - i - tileNumX;
									corner.s2 = -1;
									corner.s3 = -1;
								}
								else if (j == tileNumX)
								{
									corner.s0 = index - i - (tileNumX + 1);
									corner.s1 = -1;
									corner.s2 = -1;
									corner.s3 = -1;
								}
								else
								{
									corner.s0 = index - i - (tileNumX + 1);
									corner.s1 = index - i - tileNumX;
									corner.s2 = -1;
									corner.s3 = -1;
								}
							}
							else
							{
								if (j == 0)
								{
									corner.s0 = -1;
									corner.s1 = index - i - tileNumX;
									corner.s2 = -1;
									corner.s3 = index - i;
								}
								else if (j == tileNumX)
								{
									corner.s0 = index - i - (tileNumX + 1);
									corner.s1 = -1;
									corner.s2 = index - i - 1;
									corner.s3 = -1;
								}
							}
						}
						else
						{
							corner.s0 = index - i - (tileNumX + 1);
							corner.s1 = index - i - tileNumX;
							corner.s2 = index - i - 1;
							corner.s3 = index - i;
						}
						/*  
						 *  建立corner与覆盖Tile的edge的关系
						 *   -------------------
						 *   |        |        |
						 *   |        e2       |
						 *   |        |        |
						 *   |---e0---x---e1---|
						 *   |        |        |
						 *   |        e3       |
						 *   |        |        |
						 *   -------------------    
						 */
						if (corner.border)
						{
							if (i == 0)
							{
								if (j == 0)
								{
									corner.e0 = -1;
									corner.e1 = index + (tileNumX + 1) * tileNumY - i;
									corner.e2 = -1;
									corner.e3 = index;
								}
								else if (j == tileNumX)
								{
									corner.e0 = index + (tileNumX + 1) * tileNumY - (i + 1);
									corner.e1 = -1;
									corner.e2 = -1;
									corner.e3 = index;
								}
								else
								{
									corner.e0 = index + (tileNumX + 1) * tileNumY - (i + 1);
									corner.e1 = index + (tileNumX + 1) * tileNumY - i;
									corner.e2 = -1;
									corner.e3 = index;
								}
							}
							else if (i == tileNumY)
							{
								if (j == 0)
								{
									corner.e0 = -1;
									corner.e1 = index + (tileNumX + 1) * tileNumY - i;
									corner.e2 = index - (tileNumX + 1);
									corner.e3 = -1;
								}
								else if (j == tileNumX)
								{
									corner.e0 = index + (tileNumX + 1) * tileNumY - (i + 1);
									corner.e1 = -1;
									corner.e2 = index - (tileNumX + 1);
									corner.e3 = -1;
								}
								else
								{
									corner.e0 = index + (tileNumX + 1) * tileNumY - (i + 1);
									corner.e1 = index + (tileNumX + 1) * tileNumY - i;
									corner.e2 = index - (tileNumX + 1);
									corner.e3 = -1;
								}
							}
							else
							{
								if (j == 0)
								{
									corner.e0 = -1;
									corner.e1 = index + (tileNumX + 1) * tileNumY - i;
									corner.e2 = index - (tileNumX + 1);
									corner.e3 = index;
								}
								else if (j == tileNumX)
								{
									corner.e0 = index + (tileNumX + 1) * tileNumY - (i + 1);
									corner.e1 = -1;
									corner.e2 = index - (tileNumX + 1);
									corner.e3 = index;
								}
							}
						}
						else
						{
							corner.e0 = index + (tileNumX + 1) * tileNumY - (i + 1);
							corner.e1 = index + (tileNumX + 1) * tileNumY - i;
							corner.e2 = index - (tileNumX + 1);
							corner.e3 = index;
						}
						/*  
						 *  建立corner与临近的corner的关系
						 *   --------c2---------
						 *   |        |        |
						 *   |        |        |
						 *   |        |        |
						 *   c0-------x-------c1
						 *   |        |        |
						 *   |        |        |
						 *   |        |        |
						 *   --------c3---------    
						 */
						if (corner.border)
						{
							if (i == 0)
							{
								if (j == 0)
								{
									corner.c0 = -1;
									corner.c1 = index + 1;
									corner.c2 = -1;
									corner.c3 = index + (tileNumX + 1);
								}
								else if (j == tileNumX)
								{
									corner.c0 = index - 1;
									corner.c1 = -1;
									corner.c2 = -1;
									corner.c3 = index + (tileNumX + 1);
								}
								else
								{
									corner.c0 = index - 1;
									corner.c1 = index + 1;
									corner.c2 = -1;
									corner.c3 = index + (tileNumX + 1);
								}
							}
							else if (i == tileNumY)
							{
								if (j == 0)
								{
									corner.c0 = -1;
									corner.c1 = index + 1;
									corner.c2 = index - (tileNumX + 1);
									corner.c3 = -1;
								}
								else if (j == tileNumX)
								{
									corner.c0 = index - 1;
									corner.c1 = -1;
									corner.c2 = index - (tileNumX + 1);
									corner.c3 = -1;
								}
								else
								{
									corner.c0 = index - 1;
									corner.c1 = index + 1;
									corner.c2 = index - (tileNumX + 1);
									corner.c3 = -1;
								}
							}
							else
							{
								if (j == 0)
								{
									corner.c0 = index - 1;
									corner.c1 = index + 1;
									corner.c2 = index - (tileNumX + 1);
									corner.c3 = -1;
								}
								else if (j == tileNumX)
								{
									corner.c0 = -1;
									corner.c1 = index + 1;
									corner.c2 = index - (tileNumX + 1);
									corner.c3 = index + (tileNumX + 1);
								}
							}
						}
						else
						{
							corner.c0 = index - 1;
							corner.c1 = index + 1;
							corner.c2 = index - (tileNumX + 1);
							corner.c3 = index + (tileNumX + 1);
						}

						corners[index] = corner;
					}
				}
			}

			/*
			 * 建立edge与site、corner的关系(待优化成job)
			 */
			private void BuildEdgesRelationship()
			{
				//竖边
				/*
				 * -------------c0------------
				 * |            |            |
				 * |            |            |
				 * |     s0     x     s1     |
				 * |            |            |
				 * |            |            |
				 * -------------c1------------
				 */
				for (int i = 0; i < tileNumY; ++i)
				{
					for (int j = 0; j < tileNumX + 1; ++j)
					{
						int index = i * (tileNumX + 1) + j;
						Edge edge = edges[index];
						if (edge.border)
						{
							if (j == 0)
							{
								edge.s0 = -1;
								edge.s1 = index - i;
								edge.c0 = index;
								edge.c1 = index + tileNumX + 1;
							}
							else if (j == tileNumX)
							{
								edge.s0 = index - (i + 1);
								edge.s1 = -1;
								edge.c0 = index;
								edge.c1 = index + tileNumX + 1;
							}

						}
						else
						{
							edge.s0 = index - (i + 1);
							edge.s1 = index - i;
							edge.c0 = index;
							edge.c1 = index + tileNumX + 1;
						}

						edges[index] = edge;
					}
				}
				//横边
				/*
				 * --------------
				 * |            |
				 * |            |
				 * |     s0     | 
				 * |            |
				 * |            |  
				 * c0-----x-----c1
				 * |            |
				 * |            |
				 * |     s1     |
				 * |            |
				 * |            |
				 * --------------
				 */
				for (int i = 0; i < tileNumY + 1; ++i)
				{
					for (int j = 0; j < tileNumX; ++j)
					{
						int index = i * tileNumX + j + (tileNumX + 1) * tileNumY;
						Edge edge = edges[index];
						if (edge.border)
						{
							if (i == 0)
							{
								edge.s0 = -1;
								edge.s1 = index - (tileNumX + 1) * tileNumY;
								edge.c0 = index - (tileNumX + 1) * tileNumY + i;
								edge.c1 = index - (tileNumX + 1) * tileNumY + i + 1;
							}
							else if (i == tileNumY)
							{
								edge.s0 = index - (tileNumX + 1) * tileNumY - tileNumX;
								edge.s1 = -1;
								edge.c0 = index - (tileNumX + 1) * tileNumY + i;
								edge.c1 = index - (tileNumX + 1) * tileNumY + i + 1;
							}

						}
						else
						{
							edge.s0 = index - (tileNumX + 1) * tileNumY - tileNumX;
							edge.s1 = index - (tileNumX + 1) * tileNumY;
							edge.c0 = index - (tileNumX + 1) * tileNumY + i;
							edge.c1 = index - (tileNumX + 1) * tileNumY + i + 1;
						}

						edges[index] = edge;
					}
				}
			}

			/*
			 * 构建Site,Edge,Corner相互关联关系(待优化)
			 *   c----e----c
			 *   |         |
			 *   e    x    e
			 *   |         |
			 *   c----e----c    
			 */
			private void BuildRelationship()
			{
				BuildSitesRelationship();		//构建Site与site、edge、corner的关系
				BuildCornersRelationship();     //建立corner与site、edge、corner的关系
				BuildEdgesRelationship();       //建立edge与site、corner的关系
			}

			/*
             * 构建Corners的海拔
             */
			private void AssignCornerElevations()
			{
				var queue = new NativeQueue<Corner>(Allocator.Temp);
				for (int i = 0; i < corners.Length; ++i)
				{
					Corner corner = corners[i];
					corner.water = !inside(corner.point);         ///设置水面属性
					//水面的Corner，海拔设置为 0
					if (corner.water)		
					{
						corner.elevation = 0;
						queue.Enqueue(corner);
					}
					else
						corner.elevation = float.PositiveInfinity;
					corners[i] = corner;
				}

				/*
				 * 遍历简图为每个Corner指定海拔。为远离水面边界的Corner增加海拔。保证河流始终有一条通往海岸的路下坡（无局部最小值）。
				 */
				while (!queue.IsEmpty())
				{
					var corner = queue.Dequeue();
					var adjacentCorners = new NativeList<int>(Allocator.Temp);
					adjacentCorners.Add(corner.c0);
					adjacentCorners.Add(corner.c1);
					adjacentCorners.Add(corner.c2);
					adjacentCorners.Add(corner.c3);
					for (int i = 0; i < adjacentCorners.Length; ++i)
					{
						int adjacentIndex = adjacentCorners[i];
						if (adjacentIndex != -1)
						{
							Corner adjacentCorner = corners[adjacentIndex];
							var newElevation = 0.01f + corner.elevation;
							if (!corner.water && !adjacentCorner.water)
							{
								newElevation += 1;
								if (needsMoreRandomness)
									newElevation += rand.NextFloat(0.0f, 1.0f);
							}
							// 如果这一个Corner发生了变化，我们将把它添加到队列中，这样我们也可以处理它的临近Corners。
							if (newElevation < adjacentCorner.elevation)
							{
								adjacentCorner.elevation = newElevation;
								queue.Enqueue(adjacentCorner);
								corners[adjacentIndex] = adjacentCorner;
							}
						}
					}
				}
			}
			/**
			 * 基于Corner信息计算Tile的Ocean、Water属性
			 */
			private void AssignOceanCoastAndLand()
			{
				//根据Corner信息计算Tile的海洋属性
				var queue = new NativeQueue<Site>(Allocator.Temp);
				for (int i = 0; i < sites.Length; ++i)
				{
					Site site = sites[i];
					int numWater = 0;
					var adjacentCorners = new NativeList<int>(Allocator.Temp); 
					adjacentCorners.Add(site.c0);
					adjacentCorners.Add(site.c1);
					adjacentCorners.Add(site.c2);
					adjacentCorners.Add(site.c3);
					for (int j = 0; j < adjacentCorners.Length; ++j)
					{
						int adjacentIndex = adjacentCorners[j];
						if (adjacentIndex != -1)
						{
							Corner adjacentCorner = corners[adjacentIndex];
							if (adjacentCorner.water)
							{
								if (adjacentCorner.border)
								{
									site.biomeType = BiomeType.BT_Ocean;
									queue.Enqueue(site);

								}
								numWater += 1;
							}
						}
					}
					site.water = (site.biomeType == BiomeType.BT_Ocean || numWater >= LAKE_THRESHOLD); //大于等于LAKE_THRESHOLD个边都是水的话，认为该Tile为水
					sites[i] = site;
				}
				// 循环设置所有Tiles的海洋属性
				while (!queue.IsEmpty())
				{
					var site = queue.Dequeue();
					var adjacentSites = new NativeList<int>(Allocator.Temp); //只取周边4个
					//adjacentSites.Add(site.s0);
					adjacentSites.Add(site.s1);
					//adjacentSites.Add(site.s2);
					adjacentSites.Add(site.s3);
					adjacentSites.Add(site.s4);
					//adjacentSites.Add(site.s5);
					adjacentSites.Add(site.s6);
					//adjacentSites.Add(site.s7);
					for (int i = 0; i < adjacentSites.Length; ++i)
					{
						int adjacentIndex = adjacentSites[i];
						if (adjacentIndex != -1)
						{
							Site adjacentSite = sites[adjacentIndex];
							if (adjacentSite.water && adjacentSite.biomeType != BiomeType.BT_Ocean)
							{
								adjacentSite.biomeType = BiomeType.BT_Ocean;
								queue.Enqueue(adjacentSite);
								sites[adjacentIndex] = adjacentSite;
							}
						}
					}
				}
				//根据临近Tiles设置海岸线
				for (int i = 0; i < sites.Length; ++i)
				{
					Site site = sites[i];
					int numOcean = 0;
					int numLand = 0;
					var adjacentSites = new NativeList<int>(Allocator.Temp);
					adjacentSites.Add(site.s0);
					adjacentSites.Add(site.s1);
					adjacentSites.Add(site.s2);
					adjacentSites.Add(site.s3);
					adjacentSites.Add(site.s4);
					adjacentSites.Add(site.s5);
					adjacentSites.Add(site.s6);
					adjacentSites.Add(site.s7);
					for (int j = 0; j < adjacentSites.Length; ++j)
					{
						int adjacentIndex = adjacentSites[j];
						if (adjacentIndex != -1)
						{
							Site adjacentSite = sites[adjacentIndex];
							numOcean += adjacentSite.biomeType == BiomeType.BT_Ocean ? 1 : 0;
							numLand += !adjacentSite.water ? 1 : 0;
						}
					}
					site.coast = (numOcean > 0) && (numLand > 0);
					sites[i] = site;
				}

				//根据Corner临近的Sites，设置Corner的海岸属性
				for (int i = 0; i < corners.Length; ++i)
				{
					int numOcean = 0;
					int numLand = 0;
					Corner corner = corners[i];
					var adjacentSites = new NativeList<int>(Allocator.Temp);
					adjacentSites.Add(corner.s0);
					adjacentSites.Add(corner.s1);
					adjacentSites.Add(corner.s2);
					adjacentSites.Add(corner.s3);
					for (int j = 0; j < adjacentSites.Length; ++j)
					{
						int adjacentIndex = adjacentSites[j];
						if (adjacentIndex != -1)
						{
							Site adjacentSite = sites[adjacentIndex];
							numOcean += adjacentSite.biomeType == BiomeType.BT_Ocean ? 1 : 0;
							numLand += !adjacentSite.water ? 1 : 0;
						}
						else
							numOcean += 1;
					}
					if(numOcean == 4)
						corner.biomeType = BiomeType.BT_Ocean;
					corner.coast = (numOcean > 0) && (numLand > 0);
					corner.water = (numLand != 4) && (!corner.coast);
					corners[i] = corner;
				}
			}

            /*
			 * 重新构建海拔，让低海拔变得比高海拔更普通，为此我们要从新排列Corners,并且为每一个corner设置期望海拔, 将x映射到1-x
			 */
            struct CornerElevationCompare : IComparer<Corner>
            {
                public int Compare(Corner x, Corner y)
                {
					return x.elevation.CompareTo(y.elevation);
                }
            }
            private void RedistributeCornersElevations()
			{
				NativeList<Corner> cornerList = new NativeList<Corner>(Allocator.Temp);
 				for (int i = 0; i < corners.Length; ++i)
				{
					Corner corner = corners[i];
					if ((!corner.coast) && (corner.biomeType != BiomeType.BT_Ocean)) //过滤非岸边与非海洋的Corner
						cornerList.Add(corner);
				}
				cornerList.Sort(new CornerElevationCompare());
				for (int i = 0; i < cornerList.Length; ++i)
				{
					Corner corner = cornerList[i];
					// y(x) = 1 - (1-x)^2.
					// 已知 y 求 x
					// y = 1 - (1 - 2x + x^2)
					// y = 2x - x^2
					// x^2 - 2x + y = 0 再将x换成1-x
					float y = (float)i / (cornerList.Length - 1);
					float x = math.sqrt(SCALE_FACTOR) - math.sqrt(SCALE_FACTOR * (1 - y));
					if (x > 1.0)
						x = 1.0f;  // 注意检查这个操作是否会破坏斜坡downslope
					corner.elevation = x;
					corners[corner.index] = corner;
				}

				// 为非陆地Corner赋值
				cornerList.Clear();
				for (int i = 0; i < corners.Length; ++i)
				{
					Corner corner = corners[i];
					if (corner.coast || corner.biomeType == BiomeType.BT_Ocean) //过滤岸边与海洋的Corner
						cornerList.Add(corner);
				}
				for (int i = 0; i < cornerList.Length; ++i)
				{
					Corner corner = cornerList[i];
					corner.elevation = 0;
					corners[corner.index] = corner;
				}
			}

			/*
			 * 计算陆地Regions的海拔，（也就是sites的海拔）
			 */
			private void AssignRegionsElevations()
			{
				// Regions的海拔为其四个Corner的平均值
				for (int i = 0; i < sites.Length; ++i)
				{
					Site site = sites[i];
					float sumElevation = 0.0f;
					var adjacentCorners = new NativeList<int>(Allocator.Temp);
					adjacentCorners.Add(site.c0);
					adjacentCorners.Add(site.c1);
					adjacentCorners.Add(site.c2);
					adjacentCorners.Add(site.c3);
					for (int j = 0; j < adjacentCorners.Length; ++j)
					{
						int adjacentIndex = adjacentCorners[j];
						Corner corner = corners[adjacentIndex];
						sumElevation += corner.elevation;
					}
					site.elevation = sumElevation / adjacentCorners.Length;
					sites[i] = site;
				}
			}

			/*
			 * 计算海洋Regions海拔，（也就是sites的海拔）
			 */
			private void AssignOceanRegionsElevations()
			{
				NativeQueue<Site> queue = new NativeQueue<Site>(Allocator.Temp);
				for (int i = 0; i < sites.Length; ++i)
				{
					Site site = sites[i];
					if (site.biomeType == BiomeType.BT_Ocean)
					{
						if (site.coast)
						{
							site.elevation = -0.2f;
							queue.Enqueue(site);
						}
						else
							site.elevation = float.NegativeInfinity;
						sites[i] = site;
					}	
				}

				while (!queue.IsEmpty())
				{
					var site = queue.Dequeue();
					var adjacentSites = new NativeList<int>(Allocator.Temp); //只取周边4个
					//adjacentSites.Add(site.s0);
					adjacentSites.Add(site.s1);
					//adjacentSites.Add(site.s2);
					adjacentSites.Add(site.s3);
					adjacentSites.Add(site.s4);
					//adjacentSites.Add(site.s5);
					adjacentSites.Add(site.s6);
					//adjacentSites.Add(site.s7);

					for (int i = 0; i < adjacentSites.Length; ++i)
					{
						int adjacentIndex = adjacentSites[i];
						if(adjacentIndex != -1)
						{
							Site adjacentSite = sites[adjacentIndex];
							if (adjacentSite.elevation < -1.0f)
							{
								float newElevation = math.max(site.elevation * SCALE_FACTOR, -1.0f);
								adjacentSite.elevation = newElevation;
								queue.Enqueue(adjacentSite);
								sites[adjacentIndex] = adjacentSite;
							}
						}
						
					}
				}
			}

			/*
			 * 计算Corners下游方向，每一个Corner指向其下游点，或者指向其本身，用于生成河流流域
			 */
			private void CalculateCornersDownslopes()
			{
				for (int i = 0; i < corners.Length; ++i)
				{
					Corner corner = corners[i];
					var adjacentCorners = new NativeList<int>(Allocator.Temp);
					adjacentCorners.Add(corner.c0);
					adjacentCorners.Add(corner.c1);
					adjacentCorners.Add(corner.c2);
					adjacentCorners.Add(corner.c3);
					float minelevation = corner.elevation;
					for (int j = 0; j < adjacentCorners.Length; ++j)
					{
						int adjacentIndex = adjacentCorners[j];
						if(adjacentIndex != -1)
                        {
							Corner adjacentCorner = corners[adjacentIndex];
							if (adjacentCorner.elevation <= minelevation)
							{
								minelevation = adjacentCorner.elevation;
								corner.downslope = adjacentIndex;
							}
						}
					}
					corners[i] = corner;
				}
			}

			/*
			 * 计算河流流域分水岭, watershed是下游图中最后一个下游点
			 */
			private void CalculateCornersWatersheds()
			{
				// 初始化最初的watershed
				for (int i = 0; i < corners.Length; ++i)
				{
					Corner corner = corners[i];
					corner.watershed = corner.index;
					if (corner.biomeType != BiomeType.BT_Ocean && !corner.coast)
						corner.watershed = corner.downslope;
					corners[i] = corner;
				}
			
				// 沿着下游图指向到达海岸，将迭代次数限制为100次，尽管大多数时间内当Corners=2000时，只需要20次迭代，因为大多数点离海岸不远
				for (int i = 0; i < 100; i++) //100是个魔数，出错后可以修改
				{
					bool changed = false;
					for (int j = 0; j < corners.Length; ++j)
					{
						Corner corner = corners[j];
						if(corner.watershed != -1)
						{
							Corner watershedCorner = corners[corner.watershed];

							if (corner.biomeType != BiomeType.BT_Ocean && corner.coast && !watershedCorner.coast)
							{
								if (corner.downslope != -1)
								{
									Corner downslopeCorner = corners[corner.downslope];
									if (downslopeCorner.watershed != -1)
									{
										Corner downslopeCornerWatershed = corners[downslopeCorner.watershed];
										if (downslopeCornerWatershed.biomeType != BiomeType.BT_Ocean)
										{
											corner.watershed = downslopeCorner.watershed;
											corners[j] = corner;
											changed = true;
										}
									}
								}
							}
						}
					}
					if (changed)
						break;
				}

				//计算流域水量
				for (int i = 0; i < corners.Length; ++i)
				{
					Corner corner = corners[i];
					if (corner.watershed != -1)
					{
						Corner watershedCorner = corners[corner.watershed];
						if (!watershedCorner.water)
							watershedCorner.watershed_size += 1;
						else
							watershedCorner.watershed_size = -1;

						corners[corner.watershed] = watershedCorner;
					}
				}
			}

			/*
			 * 沿着Edges创建河流，随机挑选corner点，沿着下游图标记edges与corners为河流
			 */
			private void CreateRivers()
			{
				
				for (var i = 0; i < (boundRect.xMax + boundRect.yMax) / 4; i++)
				{
					int randomIndex = rand.NextInt(0, corners.Length - 1);
					Corner corner = corners[randomIndex];
					if (corner.biomeType == BiomeType.BT_Ocean || corner.elevation < 0.3f || corner.elevation > 0.9f)
						continue;
					// 如果希望河往东南流添加:
					//if ((corners[corner.downslope].point.x > corner.point.x) || (corners[corner.downslope].point.y > corner.point.y))
					//	continue;
					while (!corner.coast)
					{
						if (corner.downslope == -1 || corner.index == corner.downslope)
							break;
						NativeList<int> adjacentEdges = new NativeList<int>(Allocator.Temp);
						adjacentEdges.Add(corner.e0);
						adjacentEdges.Add(corner.e1);
						adjacentEdges.Add(corner.e2);
						adjacentEdges.Add(corner.e3);
						int edgeIndex = -1;
						for (int j = 0; j < adjacentEdges.Length; j++)
						{
							int adjacentIndex = adjacentEdges[j];
							if (edges[adjacentIndex].c0 == corner.downslope || edges[adjacentIndex].c1 == corner.downslope)
								edgeIndex = edges[adjacentIndex].index;
						}

						if (edgeIndex != -1)
						{
							Edge edge = edges[edgeIndex];
							edge.river++;
							edges[edgeIndex] = edge;
						}
						Corner temp = corners[corner.index];
						temp.river++;
						corners[corner.index] = temp;
						Corner downslopCorner = corners[corner.downslope];
						//downslopCorner.river++;  // TODO: fix double count
						//corners[corner.downslope] = downslopCorner;
						corner = downslopCorner;
					}
				}
			}
		}
	}
}

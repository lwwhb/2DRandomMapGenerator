/*-----------------
 * File: MapGraph.cs
 * Description: 定义地图生成数据图
 * Author: haibo.wang@unity3d.com
 * Date: 2022-6-1
 */
using System;
using TinyFlare.RandomMapGenerator.MapDataStructures;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace TinyFlare
{
    namespace RandomMapGenerator
    {
        public class MapGraph
        {
            private TilesDiagram tilesDiagram = null;
            /*
             * 初始化
             */
            public bool Init(int width, int height, int numX, int numY)
            {
                if (tilesDiagram == null)
                {
                    tilesDiagram = new SimpleTilesDiagram();
                    if (!tilesDiagram.Init(width, height, numX, numY))
                    {
                        Debug.LogError("TilesDiagram init failed!");
                        return false;
                    }                    
                }
                return true;
            }
            /*
             * 销毁
             */
            public void Destroy()
            {
                if (tilesDiagram != null)
                {
                    tilesDiagram.Destroy();
                    tilesDiagram = null;
                }
            }

            public NativeArray<Site> Sites()
            {
                return tilesDiagram.Sites;
            }
            public NativeArray<Edge> Edges()
            {
                return tilesDiagram.Edges;
            }
            public NativeArray<Corner> Corners()
            {
                return tilesDiagram.Corners;
            }
            public int TileWidth()
            {
                return tilesDiagram.TileWidth;
            }
            public int TileHeight()
            {
                return tilesDiagram.TileHeight;
            }
        }
    }
}

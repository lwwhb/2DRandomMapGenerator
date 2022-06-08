/*-----------------
 * File: MapDataStructures.cs
 * Description: 定义地图必要的数据结构
 * Author: haibo.wang@unity3d.com
 * Date: 2022-5-30
 */
using Unity.Mathematics;

namespace TinyFlare
{ 
    namespace RandomMapGenerator
    {
        namespace MapDataStructures
        {
            /*
             * 地图大小类型
             */
            public enum MapSizeType
            {
                MST_Custom = 0,                // for debug
                MST_Minimum,
                MST_Small,
                MST_Medium,
                MST_Large,
                MST_Huge
            };

            /*
             * 陆地类型
             */
            public enum LandType
            {
                MT_ISLAND = 0,                  // 岛屿

            };

            /*
             * 地表生态区类型 
             */
            public enum BiomeType
            {
                BT_Cliff = 0,                   // 悬崖
                BT_Ocean,                       // 海洋
                BT_Coast,                       // 海岸线
                BT_LakeShore,                   // 湖岸
                BT_Marsh,                       // 沼泽湿地
                BT_Ice,                         // 冰原
                BT_Lake,                        // 湖
                BT_River,                       // 河水
                BT_Beach,                       // 海滩
                BT_Lava,                        // 熔岩
                BT_Snow,                        // 雪原
                BT_Tundra,                      // 苔原
                BT_Bare,                        // 荒原
                BT_Scorched,                    // 焦土
                BT_Taiga,                       // 针叶林
                BT_Shrubland,                   // 灌木林
                BT_TemperateDesert,             // 温带沙漠
                BT_TemperateRainForest,         // 温带雨林
                BT_TemperateDeciduousForest,    // 温带阔叶林
                BT_Grassland,                   // 草原
                BT_TropicalRainForest,          // 热带雨林
                BT_TropicalSeasonalForest,      // 热带季雨林
                BT_SubtropicalDesert            // 亚热带沙漠
            }

            /**
             * 地图上基本单位位置点
             */
            public struct Site
            {
                public int                  index;                      // 索引
                public float2               point;                      // 位置
                public bool                 water;                      // 是否是水面
                public bool                 island;                     // 是否为岛屿
                public bool                 coast;                      // 是否为岸边
                public bool                 border;                     // 是否为边界
                public BiomeType            biomeType;                  // 地表生态区类型
                public int                  s0,s1,s2,s3,s4,s5,s6,s7;    // 邻居节点
                public int                  e0,e1,e2,e3;                // 临近边
                public int                  c0,c1,c2,c3;                // 临近角
                public float                elevation;                  // 海拔 0.0-1.0
                public float                moisture;                   // 降雨量 0.0-1.0
                public float                flux;		                // 通量 0.0-1.0
            }

            /*
             * 地图基本单位的拐角点
             */
            public struct Corner
            {
                public int                  index;          // 索引
                public float2               point;          // 地图基本单位拐角点位置
                public bool                 water;          // 是否是水面
                public bool                 coast;          // 是否为岸边
                public bool                 border;         // 是否为边界
                public BiomeType            biomeType;      // 地表生态区类型
                public int                  s0,s1,s2,s3;    // 临近的基本单位位置点
                public int                  e0,e1,e2,e3;    // 所属的边
                public int                  c0,c1,c2,c3;    // 临近的拐角点
                public float                elevation;      // 海拔 0.0-1.0
                public float                moisture;       // 降雨量 0.0-1.0
                public float                flux;		    // 通量 0.0-1.0
            }

            /*
             * 地图基本单位的边
             */
            public struct Edge
            {
                public int                  index;      // 索引
                public float2               midpoint;   // 拐角点c0,c1构成边的中心点
                public bool                 border;     // 是否为边界
                public int                  s0, s1;     // 边两遍的基本单位位置点
                public int                  c0, c1;     // 拐角点c0,c1构成边
                public float                elevation;  // 海拔 0.0-1.0
                public float                moisture;   // 降雨量 0.0-1.0
                public float                flux;		// 通量 0.0-1.0
            }
        }
    }
}
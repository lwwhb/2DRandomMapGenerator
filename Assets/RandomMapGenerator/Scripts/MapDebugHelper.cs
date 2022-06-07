/*-----------------
 * File: MapDebugHelper.cs
 * Description: 地图调试辅助类
 * Author: haibo.wang@unity3d.com
 * Date: 2022-6-1
 */
using TinyFlare.RandomMapGenerator.MapDataStructures;
using Unity.Collections;
using UnityEngine;
namespace TinyFlare
{
    namespace RandomMapGenerator
    {
        public enum DebugInfoType
        {
            DIT_NONE = 0,
            DIT_DEBUG_INFO,                 //基础Debug信息，Site,Edge,Corner基础信息，包括位置、索引信息，Site是白色方块,索引为绿色；Corner是白色小菱形，索引为黄色；Edge是白线，索引为紫色；Border属性一律用红色复写，
            DIT_DEBUG_EXTRA_INFO,           //额外Debug信息，包括Site,Edge,Corner的临近Site,Edge,Corner的基础信息, 绿色是Site的额外信息，紫色是Edge的额外信息，黄色是Corner的额外信息
            DIT_CORNERS_GRAPH_INFO,         //Corners的海拔图
            DIT_BIOME_INFO
        };

        public enum DebugExtraInfoType
        {
            DEIT_SITE_EXTRA_INFO = 0,
            DEIT_CORNER_EXTRA_INFO,
            DEIT_EDGE_EXTRA_INFO
        };
        public static class MapDebugHelper
        {
            static Color HexToColor(string hex)
            {
                byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
                return new Color32(r, g, b, 255);
            }
            public static Color[] BiomeColor = new Color[23]
            {
                HexToColor("000000"),             // 悬崖
                HexToColor("44447a"),             // 海洋
                HexToColor("33335a"),             // 海岸线
                HexToColor("225588"),             // 湖岸
                HexToColor("2f6666"),             // 沼泽湿地
                HexToColor("99ffff"),             // 冰原
                HexToColor("336699"),             // 湖
                HexToColor("225588"),             // 河水
                HexToColor("a09077"),             // 海滩
                HexToColor("cc3333"),             // 熔岩
                HexToColor("ffffff"),             // 雪原
                HexToColor("bbbbaa"),             // 苔原
                HexToColor("888888"),             // 荒原
                HexToColor("555555"),             // 焦土
                HexToColor("99aa77"),             // 针叶林
                HexToColor("889977"),             // 灌木林
                HexToColor("c9d29b"),             // 温带沙漠
                HexToColor("448855"),             // 温带雨林
                HexToColor("679459"),             // 温带阔叶林
                HexToColor("88aa55"),             // 草原
                HexToColor("337755"),             // 热带雨林
                HexToColor("559944"),             // 热带季雨林
                HexToColor("d2b98b")              // 亚热带沙漠
            };

            private static DebugInfoType debugInfo = DebugInfoType.DIT_NONE;
            public static DebugInfoType DebugInfo
            {
                get { return debugInfo; }
                set
                {
                    if (debugInfo != value)
                    {
                        debugInfo = value;
                        if (debugInfo == DebugInfoType.DIT_DEBUG_INFO)
                        {
                            IMDraw.Flush();
                            showSites = true;
                            showRegions = true;
                            showEdges = true;
                            showCorners = true;
                            showIndex = true;
                            debugBorder = true;
                        }
                        else if (debugInfo == DebugInfoType.DIT_DEBUG_EXTRA_INFO)
                        {
                            IMDraw.Flush();
                            showDebugExtraInfo = DebugExtraInfoType.DEIT_SITE_EXTRA_INFO;
                        }
                        else if (debugInfo == DebugInfoType.DIT_CORNERS_GRAPH_INFO)
                        {
                            IMDraw.Flush();
                            showCornerElevations = true;
                            showCornerElevationNum = true;

                        }
                        else if (debugInfo == DebugInfoType.DIT_BIOME_INFO)
                        {
                            IMDraw.Flush();
                        }
                    }
                }
            }
            //---------Debug Info-----------------
            private static bool showSites = true;
            public static bool ShowSites
            {
                get { return showSites; }
                set
                {
                    if (debugInfo == DebugInfoType.DIT_DEBUG_INFO)
                    {
                        if (showSites != value)
                            showSites = value;
                    }
                }
            }
            private static bool showRegions = true;
            public static bool ShowRegions
            {
                get { return showRegions; }
                set
                {
                    if (debugInfo == DebugInfoType.DIT_DEBUG_INFO)
                    {
                        if (showRegions != value)
                            showRegions = value;
                    }
                }
            }
            private static bool showEdges = true;
            public static bool ShowEdges
            {
                get { return showEdges; }
                set
                {
                    if (debugInfo == DebugInfoType.DIT_DEBUG_INFO)
                    {
                        if (showEdges != value)
                            showEdges = value;
                    }
                }
            }
            private static bool showCorners = true;
            public static bool ShowCorners
            {
                get { return showCorners; }
                set
                {
                    if (debugInfo == DebugInfoType.DIT_DEBUG_INFO)
                    {
                        if (showCorners != value)
                            showCorners = value;
                    }
                }
            }
            private static bool showIndex = false;
            public static bool ShowIndex
            {
                get { return showIndex; }
                set
                {
                    if (debugInfo == DebugInfoType.DIT_DEBUG_INFO)
                    {
                        if (showIndex != value)
                            showIndex = value;
                    }
                }
            }
            private static bool debugBorder = false;
            public static bool DebugBorder
            {
                get { return debugBorder; }
                set
                {
                    if (debugInfo == DebugInfoType.DIT_DEBUG_INFO)
                    {
                        if (debugBorder != value)
                            debugBorder = value;
                    }
                }
            }
            
            private static void DrawSites(NativeArray<Site> sites, bool showRegions, int tileWidth, int tileHeight, bool showSiteIndex = true, bool debugBorder = false)
            {
                for (int i = 0; i < sites.Length; ++i)
                {
                    Site site = sites[i];
                    if (showRegions)
                        IMDraw.Quad3D(new Vector3(site.point.x, site.point.y, 20), Quaternion.Euler(0, 90, 0), tileWidth, tileHeight, IMDrawAxis.X, BiomeColor[(int)site.biomeType]);
                    if(debugBorder)
                        IMDraw.Label(new Vector3(site.point.x, site.point.y, 0), site.border ? Color.red : Color.white, LabelPivot.MIDDLE_CENTER, LabelAlignment.CENTER, "■");
                    else
                        IMDraw.Label(new Vector3(site.point.x, site.point.y, 0), Color.white, LabelPivot.MIDDLE_CENTER, LabelAlignment.CENTER, "■");
                    if (showSiteIndex)
                        IMDraw.Label(new Vector3(site.point.x, site.point.y, 0), Color.green, LabelPivot.LOWER_RIGHT, LabelAlignment.CENTER, site.index.ToString());
                }
            }
            
            private static void DrawCorners(NativeArray<Corner> corners, bool showCornerIndex = true, bool debugBorder = false)
            {
                for (int i = 0; i < corners.Length; ++i)
                {
                    Corner corner = corners[i];
                    if(debugBorder)
                        IMDraw.Label(new Vector3(corner.point.x, corner.point.y, 0), corner.border ? Color.red : Color.white, LabelPivot.MIDDLE_CENTER, LabelAlignment.CENTER, "◆");
                    else
                        IMDraw.Label(new Vector3(corner.point.x, corner.point.y, 0), Color.white, LabelPivot.MIDDLE_CENTER, LabelAlignment.CENTER, "◆");
                    IMDraw.Label(new Vector3(corner.point.x, corner.point.y, 0), Color.yellow, LabelPivot.LOWER_RIGHT, LabelAlignment.CENTER, corner.index.ToString());
                }
            }
            
            private static void DrawEdges(NativeArray<Edge> edges, NativeArray<Corner> corners, NativeArray<Site> sites, bool showEdgeIndex = true, bool debugBorder = false)
            {
                for (int i = 0; i < edges.Length; ++i)
                {
                    Edge edge = edges[i];
                    Corner c0 = corners[edge.c0];
                    Corner c1 = corners[edge.c1];
                    if(debugBorder)
                        IMDraw.Line3D(new Vector3(c0.point.x, c0.point.y, 15), new Vector3(c1.point.x, c1.point.y, 15), edge.border ? Color.red: Color.white);
                    else
                        IMDraw.Line3D(new Vector3(c0.point.x, c0.point.y, 15), new Vector3(c1.point.x, c1.point.y, 15), Color.white);
                    if (showEdgeIndex)
                        IMDraw.Label(new Vector3(edge.midpoint.x, edge.midpoint.y, 15), Color.magenta, LabelPivot.MIDDLE_CENTER, LabelAlignment.CENTER, edge.index.ToString());
                }
            }

            public static void DrawDebugInfo(MapGraph graph)
            {
                if (debugInfo == DebugInfoType.DIT_DEBUG_INFO)
                {
                    if (showSites)
                        DrawSites(graph.Sites(), showRegions, graph.TileWidth(), graph.TileHeight(), showIndex, debugBorder);
                    if (showEdges)
                        DrawEdges(graph.Edges(), graph.Corners(), graph.Sites(), showIndex, debugBorder);
                    if (showCorners)
                        DrawCorners(graph.Corners(), showIndex, debugBorder);
                }
            }
            //-----------------------------------------

            //---------Debug Extra Info---------------------
            private static DebugExtraInfoType showDebugExtraInfo = DebugExtraInfoType.DEIT_SITE_EXTRA_INFO;
            public static DebugExtraInfoType ShowDebugExtraInfo
            {
                get { return showDebugExtraInfo; }
                set
                {
                    if (debugInfo == DebugInfoType.DIT_DEBUG_EXTRA_INFO)
                    {
                        if (showDebugExtraInfo != value)
                            showDebugExtraInfo = value;
                    }
                }
            }
            private static void DrawSitesExtraInfo(NativeArray<Site> sites, NativeArray<Edge> edges, NativeArray<Corner> corners, int tileWidth, int tileHeight)
            {
                for (int i = 0; i < corners.Length; ++i)
                {
                    Corner corner = corners[i];
                    IMDraw.Label(new Vector3(corner.point.x, corner.point.y, 0), Color.white, LabelPivot.MIDDLE_CENTER, LabelAlignment.CENTER, "◆");
                    IMDraw.Label(new Vector3(corner.point.x, corner.point.y, 0), Color.yellow, LabelPivot.LOWER_RIGHT, LabelAlignment.CENTER, corner.index.ToString());
                }
                for (int i = 0; i < edges.Length; ++i)
                {
                    Edge edge = edges[i];
                    Corner c0 = corners[edge.c0];
                    Corner c1 = corners[edge.c1];
                    IMDraw.Line3D(new Vector3(c0.point.x, c0.point.y, 15), new Vector3(c1.point.x, c1.point.y, 15), Color.white);
                    IMDraw.Label(new Vector3(edge.midpoint.x, edge.midpoint.y, 15), Color.magenta, LabelPivot.MIDDLE_CENTER, LabelAlignment.CENTER, edge.index.ToString());

                }
                for (int i = 0; i < sites.Length; ++i)
                {
                    Site site = sites[i];
                    IMDraw.Label(new Vector3(site.point.x, site.point.y, 0), Color.white, LabelPivot.MIDDLE_CENTER, LabelAlignment.CENTER, "■");
                    IMDraw.Label(new Vector3(site.point.x, site.point.y, 0), Color.green, LabelPivot.LOWER_RIGHT, LabelAlignment.CENTER, site.index.ToString());
                    //显示临近Site
                    IMDraw.Label(new Vector3(site.point.x - tileWidth * 0.2f, site.point.y - tileHeight * 0.2f, 0), Color.green, LabelPivot.MIDDLE_CENTER, LabelAlignment.CENTER, site.n0.ToString());
                    IMDraw.Label(new Vector3(site.point.x , site.point.y - tileHeight * 0.2f, 0), Color.green, LabelPivot.MIDDLE_CENTER, LabelAlignment.CENTER, site.n1.ToString());
                    IMDraw.Label(new Vector3(site.point.x + tileWidth * 0.2f, site.point.y - tileHeight * 0.2f, 0), Color.green, LabelPivot.MIDDLE_CENTER, LabelAlignment.CENTER, site.n2.ToString());
                    IMDraw.Label(new Vector3(site.point.x - tileWidth * 0.2f, site.point.y, 0), Color.green, LabelPivot.MIDDLE_CENTER, LabelAlignment.CENTER, site.n3.ToString());
                    IMDraw.Label(new Vector3(site.point.x + tileWidth * 0.2f, site.point.y, 0), Color.green, LabelPivot.MIDDLE_CENTER, LabelAlignment.CENTER, site.n4.ToString());
                    IMDraw.Label(new Vector3(site.point.x - tileWidth * 0.2f, site.point.y + tileHeight * 0.2f, 0), Color.green, LabelPivot.MIDDLE_CENTER, LabelAlignment.CENTER, site.n5.ToString());
                    IMDraw.Label(new Vector3(site.point.x, site.point.y + tileHeight * 0.2f, 0), Color.green, LabelPivot.MIDDLE_CENTER, LabelAlignment.CENTER, site.n6.ToString());
                    IMDraw.Label(new Vector3(site.point.x + tileWidth * 0.2f, site.point.y + tileHeight * 0.2f, 0), Color.green, LabelPivot.MIDDLE_CENTER, LabelAlignment.CENTER, site.n7.ToString());
                    //显示临近Corner
                    IMDraw.Label(new Vector3(site.point.x - tileWidth * 0.4f, site.point.y - tileHeight * 0.4f, 0), Color.yellow, LabelPivot.MIDDLE_CENTER, LabelAlignment.CENTER, site.c0.ToString());
                    IMDraw.Label(new Vector3(site.point.x + tileWidth * 0.4f, site.point.y - tileHeight * 0.4f, 0), Color.yellow, LabelPivot.MIDDLE_CENTER, LabelAlignment.CENTER, site.c1.ToString());
                    IMDraw.Label(new Vector3(site.point.x - tileWidth * 0.4f, site.point.y + tileHeight * 0.4f, 0), Color.yellow, LabelPivot.MIDDLE_CENTER, LabelAlignment.CENTER, site.c2.ToString());
                    IMDraw.Label(new Vector3(site.point.x + tileWidth * 0.4f, site.point.y + tileHeight * 0.4f, 0), Color.yellow, LabelPivot.MIDDLE_CENTER, LabelAlignment.CENTER, site.c3.ToString());
                    //显示临近边
                    IMDraw.Label(new Vector3(site.point.x - tileWidth * 0.4f, site.point.y, 0), Color.magenta, LabelPivot.MIDDLE_CENTER, LabelAlignment.CENTER, site.e0.ToString());
                    IMDraw.Label(new Vector3(site.point.x + tileWidth * 0.4f, site.point.y, 0), Color.magenta, LabelPivot.MIDDLE_CENTER, LabelAlignment.CENTER, site.e1.ToString());
                    IMDraw.Label(new Vector3(site.point.x, site.point.y - tileHeight * 0.4f, 0), Color.magenta, LabelPivot.MIDDLE_CENTER, LabelAlignment.CENTER, site.e2.ToString());
                    IMDraw.Label(new Vector3(site.point.x, site.point.y + tileHeight * 0.4f, 0), Color.magenta, LabelPivot.MIDDLE_CENTER, LabelAlignment.CENTER, site.e3.ToString());
                }
            }
            private static void DrawCornersExtraInfo(NativeArray<Site> sites, NativeArray<Edge> edges, NativeArray<Corner> corners, int tileWidth, int tileHeight)
            {
                for (int i = 0; i < edges.Length; ++i)
                {
                    Edge edge = edges[i];
                    Corner c0 = corners[edge.c0];
                    Corner c1 = corners[edge.c1];
                    IMDraw.Line3D(new Vector3(c0.point.x, c0.point.y, 15), new Vector3(c1.point.x, c1.point.y, 15), Color.white);
                    IMDraw.Label(new Vector3(edge.midpoint.x, edge.midpoint.y, 15), Color.magenta, LabelPivot.MIDDLE_CENTER, LabelAlignment.CENTER, edge.index.ToString());

                }
                for (int i = 0; i < sites.Length; ++i)
                {
                    Site site = sites[i];
                    IMDraw.Label(new Vector3(site.point.x, site.point.y, 0), Color.white, LabelPivot.MIDDLE_CENTER, LabelAlignment.CENTER, "■");
                    IMDraw.Label(new Vector3(site.point.x, site.point.y, 0), Color.green, LabelPivot.LOWER_RIGHT, LabelAlignment.CENTER, site.index.ToString());
                }
                for (int i = 0; i < corners.Length; ++i)
                {
                    Corner corner = corners[i];
                    IMDraw.Label(new Vector3(corner.point.x, corner.point.y, 0), Color.white, LabelPivot.MIDDLE_CENTER, LabelAlignment.CENTER, "◆");
                    IMDraw.Label(new Vector3(corner.point.x, corner.point.y, 0), Color.yellow, LabelPivot.LOWER_RIGHT, LabelAlignment.CENTER, corner.index.ToString());
                    //显示临近Site
                    IMDraw.Label(new Vector3(corner.point.x - tileWidth * 0.2f, corner.point.y - tileHeight * 0.2f, 0), Color.green, LabelPivot.MIDDLE_CENTER, LabelAlignment.CENTER, corner.s0.ToString());
                    IMDraw.Label(new Vector3(corner.point.x + tileWidth * 0.2f, corner.point.y - tileHeight * 0.2f, 0), Color.green, LabelPivot.MIDDLE_CENTER, LabelAlignment.CENTER, corner.s1.ToString());
                    IMDraw.Label(new Vector3(corner.point.x - tileWidth * 0.2f, corner.point.y + tileHeight * 0.2f, 0), Color.green, LabelPivot.MIDDLE_CENTER, LabelAlignment.CENTER, corner.s2.ToString());
                    IMDraw.Label(new Vector3(corner.point.x + tileWidth * 0.2f, corner.point.y + tileHeight * 0.2f, 0), Color.green, LabelPivot.MIDDLE_CENTER, LabelAlignment.CENTER, corner.s3.ToString());
                    //显示临近Corner
                    IMDraw.Label(new Vector3(corner.point.x - tileWidth * 0.2f, corner.point.y, 0), Color.yellow, LabelPivot.MIDDLE_CENTER, LabelAlignment.CENTER, corner.c0.ToString());
                    IMDraw.Label(new Vector3(corner.point.x + tileWidth * 0.2f, corner.point.y, 0), Color.yellow, LabelPivot.MIDDLE_CENTER, LabelAlignment.CENTER, corner.c1.ToString());
                    IMDraw.Label(new Vector3(corner.point.x, corner.point.y - tileHeight * 0.2f, 0), Color.yellow, LabelPivot.MIDDLE_CENTER, LabelAlignment.CENTER, corner.c2.ToString());
                    IMDraw.Label(new Vector3(corner.point.x, corner.point.y + tileHeight * 0.2f, 0), Color.yellow, LabelPivot.MIDDLE_CENTER, LabelAlignment.CENTER, corner.c3.ToString());
                    //显示临近边
                    IMDraw.Label(new Vector3(corner.point.x - tileWidth * 0.3f, corner.point.y, 0), Color.magenta, LabelPivot.MIDDLE_CENTER, LabelAlignment.CENTER, corner.e0.ToString());
                    IMDraw.Label(new Vector3(corner.point.x + tileWidth * 0.3f, corner.point.y, 0), Color.magenta, LabelPivot.MIDDLE_CENTER, LabelAlignment.CENTER, corner.e1.ToString());
                    IMDraw.Label(new Vector3(corner.point.x, corner.point.y - tileHeight * 0.3f, 0), Color.magenta, LabelPivot.MIDDLE_CENTER, LabelAlignment.CENTER, corner.e2.ToString());
                    IMDraw.Label(new Vector3(corner.point.x, corner.point.y + tileHeight * 0.3f, 0), Color.magenta, LabelPivot.MIDDLE_CENTER, LabelAlignment.CENTER, corner.e3.ToString());
                }
            }
            private static void DrawEdgesExtraInfo(NativeArray<Site> sites, NativeArray<Edge> edges, NativeArray<Corner> corners, int tileWidth, int tileHeight)
            {
                for (int i = 0; i < edges.Length; ++i)
                {
                    Edge edge = edges[i];
                    if (edge.s0 != -1 && edge.s1 != -1)
                    {
                        Site s0 = sites[edge.s0];
                        Site s1 = sites[edge.s1];
                        IMDraw.Line3D(new Vector3(s0.point.x, s0.point.y, 15), new Vector3(s1.point.x, s1.point.y, 15), Color.magenta);
                    }
                }
            }
            public static void DrawDebugExtraInfo(MapGraph graph)
            {
                if (debugInfo == DebugInfoType.DIT_DEBUG_EXTRA_INFO)
                {
                    if (showDebugExtraInfo == DebugExtraInfoType.DEIT_SITE_EXTRA_INFO)
                        DrawSitesExtraInfo(graph.Sites(), graph.Edges(), graph.Corners(), graph.TileWidth(), graph.TileHeight());
                    else if (showDebugExtraInfo == DebugExtraInfoType.DEIT_EDGE_EXTRA_INFO)
                        DrawEdgesExtraInfo(graph.Sites(), graph.Edges(), graph.Corners(), graph.TileWidth(), graph.TileHeight());
                    else if (showDebugExtraInfo == DebugExtraInfoType.DEIT_CORNER_EXTRA_INFO)
                        DrawCornersExtraInfo(graph.Sites(), graph.Edges(), graph.Corners(), graph.TileWidth(), graph.TileHeight());
                }
            }
            //----------------------------------------

            //---------Corners GraphInfo---------------------
            private static bool showCornerElevations = false;
            public static bool ShowCornerElevations
            {
                get { return showCornerElevations; }
                set
                {
                    if (debugInfo == DebugInfoType.DIT_CORNERS_GRAPH_INFO)
                    {
                        if (showCornerElevations != value)
                            showCornerElevations = value;
                    }
                }
            }
            private static bool showCornerElevationNum = false;
            public static bool ShowCornerElevationNum
            {
                get { return showCornerElevationNum; }
                set
                {
                    if (debugInfo == DebugInfoType.DIT_CORNERS_GRAPH_INFO)
                    {
                        if (showCornerElevationNum != value)
                            showCornerElevationNum = value;
                    }
                }
            }

            public static void DrawCornersGraphInfo(MapGraph graph)
            {
                if (debugInfo == DebugInfoType.DIT_CORNERS_GRAPH_INFO)
                {
                    if (showCornerElevations)
                        DrawCornerElevations(graph.Corners(), showCornerElevationNum);
                    

                }
            }
            private static void DrawCornerElevations(NativeArray<Corner> corners, bool showElevationNum = true)
            {
                for (int i = 0; i < corners.Length; ++i)
                {
                    Corner corner = corners[i];
                    Color color = new Color(corner.elevation / 255, corner.elevation / 255, corner.elevation / 255, 1.0f);
                    IMDraw.Label(new Vector3(corner.point.x, corner.point.y, 0), color, LabelPivot.MIDDLE_CENTER, LabelAlignment.CENTER, "◆");
                    IMDraw.Label(new Vector3(corner.point.x, corner.point.y, 0), Color.yellow, LabelPivot.LOWER_RIGHT, LabelAlignment.CENTER, corner.elevation.ToString());
                }
            }
            //------------------------------------------
        }
    }
}

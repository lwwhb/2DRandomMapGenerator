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
            DIT_CORNERS_GRAPH_INFO,         //Corners的信息图
            DIT_SITES_GRAPH_INFO,           //Sites的信息图
            DIT_EDGES_GRAPH_INFO,           //Edges的信息图
            DIT_BIOME_INFO
        };

        public enum DebugExtraInfoType
        {
            DEIT_SITE_EXTRA_INFO = 0,
            DEIT_CORNER_EXTRA_INFO,
            DEIT_EDGE_EXTRA_INFO
        };

        public enum CornersGraphInfoType
        {
            CGIT_ELEVATION = 0,
            CGIT_WATER_AND_COAST,
            CGIT_WATERSHEDS_AND_RIVER,
            CGIT_FLUXES,
            CGIT_MOISTURES
        };
        public enum SitesGraphInfoType
        {
            SGIT_ELEVATION = 0,
            SGIT_WATER_AND_COAST,
            SGIT_FLUXES,
            SGIT_MOISTURES
        };
        public enum EdgesGraphInfoType
        {
            EGIT_RIVER = 0
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
                            showEdges = false;
                            showCorners = false;
                            showIndex = false;
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
                            cornersGraphInfoType = CornersGraphInfoType.CGIT_MOISTURES;
                            showCornerElevationNum = false;
                            showCornerDownslope = false;
                            showCornerWatersheds = false;
                            showCornerWatershedSizeNum = true;
                        }
                        else if (debugInfo == DebugInfoType.DIT_SITES_GRAPH_INFO)
                        {
                            IMDraw.Flush();
                            sitesGraphInfoType = SitesGraphInfoType.SGIT_ELEVATION;
                            showSiteElevationNum = true;
                        }
                        else if (debugInfo == DebugInfoType.DIT_EDGES_GRAPH_INFO)
                        {
                            IMDraw.Flush();
                            edgesGraphInfoType = EdgesGraphInfoType.EGIT_RIVER;
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
                        IMDraw.Label(new Vector3(site.point.x, site.point.y, 0), site.water? Color.blue : Color.white, LabelPivot.MIDDLE_CENTER, LabelAlignment.CENTER, "■");
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
                    IMDraw.Label(new Vector3(site.point.x - tileWidth * 0.2f, site.point.y - tileHeight * 0.2f, 0), Color.green, LabelPivot.MIDDLE_CENTER, LabelAlignment.CENTER, site.s0.ToString());
                    IMDraw.Label(new Vector3(site.point.x , site.point.y - tileHeight * 0.2f, 0), Color.green, LabelPivot.MIDDLE_CENTER, LabelAlignment.CENTER, site.s1.ToString());
                    IMDraw.Label(new Vector3(site.point.x + tileWidth * 0.2f, site.point.y - tileHeight * 0.2f, 0), Color.green, LabelPivot.MIDDLE_CENTER, LabelAlignment.CENTER, site.s2.ToString());
                    IMDraw.Label(new Vector3(site.point.x - tileWidth * 0.2f, site.point.y, 0), Color.green, LabelPivot.MIDDLE_CENTER, LabelAlignment.CENTER, site.s3.ToString());
                    IMDraw.Label(new Vector3(site.point.x + tileWidth * 0.2f, site.point.y, 0), Color.green, LabelPivot.MIDDLE_CENTER, LabelAlignment.CENTER, site.s4.ToString());
                    IMDraw.Label(new Vector3(site.point.x - tileWidth * 0.2f, site.point.y + tileHeight * 0.2f, 0), Color.green, LabelPivot.MIDDLE_CENTER, LabelAlignment.CENTER, site.s5.ToString());
                    IMDraw.Label(new Vector3(site.point.x, site.point.y + tileHeight * 0.2f, 0), Color.green, LabelPivot.MIDDLE_CENTER, LabelAlignment.CENTER, site.s6.ToString());
                    IMDraw.Label(new Vector3(site.point.x + tileWidth * 0.2f, site.point.y + tileHeight * 0.2f, 0), Color.green, LabelPivot.MIDDLE_CENTER, LabelAlignment.CENTER, site.s7.ToString());
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
            private static CornersGraphInfoType cornersGraphInfoType = CornersGraphInfoType.CGIT_ELEVATION;
            public static CornersGraphInfoType CornersGraphInfoType
            {
                get { return cornersGraphInfoType; }
                set
                {
                    if (debugInfo == DebugInfoType.DIT_CORNERS_GRAPH_INFO)
                    {
                        if (cornersGraphInfoType != value)
                            cornersGraphInfoType = value;
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
            private static bool showCornerDownslope = false;
            public static bool ShowCornerDownslope
            {
                get { return showCornerDownslope; }
                set
                {
                    if (debugInfo == DebugInfoType.DIT_CORNERS_GRAPH_INFO)
                    {
                        if (showCornerDownslope != value)
                            showCornerDownslope = value;
                    }
                }
            }
            private static bool showCornerWatersheds = false;
            public static bool ShowCornerWatersheds
            {
                get { return showCornerWatersheds; }
                set
                {
                    if (debugInfo == DebugInfoType.DIT_CORNERS_GRAPH_INFO)
                    {
                        if (showCornerWatersheds != value)
                            showCornerWatersheds = value;
                    }
                }
            }
            private static bool showCornerWatershedSizeNum = false;
            public static bool ShowCornerWatershedsSizeNum
            {
                get { return showCornerWatershedSizeNum; }
                set
                {
                    if (debugInfo == DebugInfoType.DIT_CORNERS_GRAPH_INFO)
                    {
                        if (showCornerWatershedSizeNum != value)
                            showCornerWatershedSizeNum = value;
                    }
                }
            }

            private static bool showCornerFluxesNum = false;
            public static bool ShowCornerFluxesNum
            {
                get { return showCornerFluxesNum; }
                set
                {
                    if (debugInfo == DebugInfoType.DIT_CORNERS_GRAPH_INFO)
                    {
                        if (showCornerFluxesNum != value)
                            showCornerFluxesNum = value;
                    }
                }
            }

            private static bool showCornerMoisturesNum = false;
            public static bool ShowCornerMoisturesNum
            {
                get { return showCornerMoisturesNum; }
                set
                {
                    if (debugInfo == DebugInfoType.DIT_CORNERS_GRAPH_INFO)
                    {
                        if (showCornerMoisturesNum != value)
                            showCornerMoisturesNum = value;
                    }
                }
            }

            public static void DrawCornersGraphInfo(MapGraph graph)
            {
                if (debugInfo == DebugInfoType.DIT_CORNERS_GRAPH_INFO)
                {
                    if (cornersGraphInfoType == CornersGraphInfoType.CGIT_ELEVATION)
                        DrawCornersElevations(graph.Corners());
                    else if (cornersGraphInfoType == CornersGraphInfoType.CGIT_WATER_AND_COAST)
                        DrawCornersWaterAndCoastInfo(graph.Corners());
                    else if (cornersGraphInfoType == CornersGraphInfoType.CGIT_WATERSHEDS_AND_RIVER)
                        DrawCornersWatershedsAndRiver(graph.Corners(), graph.Sites(), graph.TileWidth(), graph.TileHeight());
                    else if (cornersGraphInfoType == CornersGraphInfoType.CGIT_FLUXES)
                        DrawCornerFluxes(graph.Corners(), graph.Sites(), graph.TileWidth(), graph.TileHeight());
                    else if (cornersGraphInfoType == CornersGraphInfoType.CGIT_MOISTURES)
                        DrawCornerMoistures(graph.Corners(), graph.Sites(), graph.TileWidth(), graph.TileHeight());
                }
            }
            private static void DrawCornersElevations(NativeArray<Corner> corners)
            {
                for (int i = 0; i < corners.Length; ++i)
                {
                    Corner corner = corners[i];
                    Color color = new Color(corner.elevation, corner.elevation, corner.elevation, 1.0f);
                    IMDraw.Label(new Vector3(corner.point.x, corner.point.y, 0), color, LabelPivot.MIDDLE_CENTER, LabelAlignment.CENTER, "◆");
                    if(showCornerElevationNum)
                        IMDraw.Label(new Vector3(corner.point.x, corner.point.y, 0), Color.yellow, LabelPivot.LOWER_RIGHT, LabelAlignment.CENTER, corner.elevation.ToString());
                }
            }
            private static void DrawCornersWaterAndCoastInfo(NativeArray<Corner> corners)
            {
                for (int i = 0; i < corners.Length; ++i)
                {
                    Corner corner = corners[i];
                    if (corner.water)
                        IMDraw.Label(new Vector3(corner.point.x, corner.point.y, 0), Color.blue, LabelPivot.MIDDLE_CENTER, LabelAlignment.CENTER, "◆");
                    else
                    {
                        if (corner.coast)
                            IMDraw.Label(new Vector3(corner.point.x, corner.point.y, 0), Color.cyan, LabelPivot.MIDDLE_CENTER, LabelAlignment.CENTER, "◆");
                        else
                            IMDraw.Label(new Vector3(corner.point.x, corner.point.y, 0), Color.yellow, LabelPivot.MIDDLE_CENTER, LabelAlignment.CENTER, "◆");
                    }
                }
            }
            private static void DrawCornersWatershedsAndRiver(NativeArray<Corner> corners, NativeArray<Site> sites, int tileWidth, int tileHeight)
            {
                for (int i = 0; i < sites.Length; ++i)
                {
                    Site site = sites[i];
                    IMDraw.Quad3D(new Vector3(site.point.x, site.point.y, 20), Quaternion.Euler(0, 90, 0), tileWidth, tileHeight, IMDrawAxis.X, new Color(site.elevation, site.elevation, site.elevation, 0.5f));
                }
                for (int i = 0; i < corners.Length; ++i)
                {
                    Corner corner = corners[i];
                    IMDraw.Label(new Vector3(corner.point.x, corner.point.y, 0), corner.water? Color.blue : Color.yellow, LabelPivot.MIDDLE_CENTER, LabelAlignment.CENTER, "◆");
                    IMDraw.Label(new Vector3(corner.point.x, corner.point.y, 0), Color.yellow, LabelPivot.UPPER_LEFT, LabelAlignment.CENTER, corner.river.ToString());
                    if (showCornerDownslope)
                    {
                        if (!corner.border && !corner.water)
                        {
                            if (corner.downslope == corner.c0)
                                IMDraw.Label(new Vector3(corner.point.x - tileWidth * 0.2f, corner.point.y, 0), Color.yellow, LabelPivot.MIDDLE_CENTER, LabelAlignment.CENTER, "←");
                            if (corner.downslope == corner.c1)
                                IMDraw.Label(new Vector3(corner.point.x + tileWidth * 0.2f, corner.point.y, 0), Color.yellow, LabelPivot.MIDDLE_CENTER, LabelAlignment.CENTER, "→");
                            if (corner.downslope == corner.c2)
                                IMDraw.Label(new Vector3(corner.point.x, corner.point.y - tileHeight * 0.2f, 0), Color.yellow, LabelPivot.MIDDLE_CENTER, LabelAlignment.CENTER, "↓");
                            if (corner.downslope == corner.c3)
                                IMDraw.Label(new Vector3(corner.point.x, corner.point.y + tileHeight * 0.2f, 0), Color.yellow, LabelPivot.MIDDLE_CENTER, LabelAlignment.CENTER, "↑");
                        }
                    }
                    if (showCornerWatersheds)
                    {
                        if (!corner.border && !corner.water)
                        {
                            if (corner.watershed == corner.c0)
                                IMDraw.Label(new Vector3(corner.point.x - tileWidth * 0.2f, corner.point.y, 0), Color.yellow, LabelPivot.MIDDLE_CENTER, LabelAlignment.CENTER, "←");
                            if (corner.watershed == corner.c1)
                                IMDraw.Label(new Vector3(corner.point.x + tileWidth * 0.2f, corner.point.y, 0), Color.yellow, LabelPivot.MIDDLE_CENTER, LabelAlignment.CENTER, "→");
                            if (corner.watershed == corner.c2)
                                IMDraw.Label(new Vector3(corner.point.x, corner.point.y - tileHeight * 0.2f, 0), Color.yellow, LabelPivot.MIDDLE_CENTER, LabelAlignment.CENTER, "↓");
                            if (corner.watershed == corner.c3)
                                IMDraw.Label(new Vector3(corner.point.x, corner.point.y + tileHeight * 0.2f, 0), Color.yellow, LabelPivot.MIDDLE_CENTER, LabelAlignment.CENTER, "↑");
                        }
                    }
                    if (showCornerWatershedSizeNum)
                        IMDraw.Label(new Vector3(corner.point.x, corner.point.y, 0), Color.white, LabelPivot.LOWER_RIGHT, LabelAlignment.CENTER, corner.watershed_size.ToString());
                }
            }
            private static void DrawCornerFluxes(NativeArray<Corner> corners, NativeArray<Site> sites, int tileWidth, int tileHeight)
            {
                for (int i = 0; i < sites.Length; ++i)
                {
                    Site site = sites[i];
                    IMDraw.Quad3D(new Vector3(site.point.x, site.point.y, 20), Quaternion.Euler(0, 90, 0), tileWidth, tileHeight, IMDrawAxis.X, new Color(site.elevation, site.elevation, site.elevation, 0.5f));
                }
                for (int i = 0; i < corners.Length; ++i)
                {
                    Corner corner = corners[i];
                    IMDraw.Label(new Vector3(corner.point.x, corner.point.y, 0), corner.water ? Color.blue : Color.yellow, LabelPivot.MIDDLE_CENTER, LabelAlignment.CENTER, "◆");
                    if(showCornerFluxesNum)
                        IMDraw.Label(new Vector3(corner.point.x, corner.point.y, 0), Color.yellow, LabelPivot.UPPER_LEFT, LabelAlignment.CENTER, corner.flux.ToString());
                }
            }
            private static void DrawCornerMoistures(NativeArray<Corner> corners, NativeArray<Site> sites, int tileWidth, int tileHeight)
            {
                for (int i = 0; i < sites.Length; ++i)
                {
                    Site site = sites[i];
                    IMDraw.Quad3D(new Vector3(site.point.x, site.point.y, 20), Quaternion.Euler(0, 90, 0), tileWidth, tileHeight, IMDrawAxis.X, new Color(site.moisture, site.moisture, site.moisture, 0.5f));
                }
                for (int i = 0; i < corners.Length; ++i)
                {
                    Corner corner = corners[i];
                    IMDraw.Label(new Vector3(corner.point.x, corner.point.y, 0), corner.water ? Color.blue : Color.yellow, LabelPivot.MIDDLE_CENTER, LabelAlignment.CENTER, "◆");
                    if(showCornerMoisturesNum)
                        IMDraw.Label(new Vector3(corner.point.x, corner.point.y, 0), Color.yellow, LabelPivot.UPPER_LEFT, LabelAlignment.CENTER, corner.moisture.ToString());
                }
            }
            //------------------------------------------

            //---------Sites GraphInfo---------------------
            private static SitesGraphInfoType sitesGraphInfoType = SitesGraphInfoType.SGIT_WATER_AND_COAST;
            public static SitesGraphInfoType SitesGraphInfoType
            {
                get { return sitesGraphInfoType; }
                set
                {
                    if (debugInfo == DebugInfoType.DIT_SITES_GRAPH_INFO)
                    {
                        if (sitesGraphInfoType != value)
                            sitesGraphInfoType = value;
                    }
                }
            }

            private static bool showSiteElevationNum = false;
            public static bool ShowSiteElevationNum
            {
                get { return showSiteElevationNum; }
                set
                {
                    if (debugInfo == DebugInfoType.DIT_SITES_GRAPH_INFO)
                    {
                        if (showSiteElevationNum != value)
                            showSiteElevationNum = value;
                    }
                }
            }

            private static bool showSiteFluxesNum = false;
            public static bool ShowSiteFluxesNum
            {
                get { return showSiteFluxesNum; }
                set
                {
                    if (debugInfo == DebugInfoType.DIT_SITES_GRAPH_INFO)
                    {
                        if (showSiteFluxesNum != value)
                            showSiteFluxesNum = value;
                    }
                }
            }

            private static bool showSiteMoisturesNum = false;
            public static bool ShowSiteMoisturesNum
            {
                get { return showSiteMoisturesNum; }
                set
                {
                    if (debugInfo == DebugInfoType.DIT_SITES_GRAPH_INFO)
                    {
                        if (showSiteMoisturesNum != value)
                            showSiteMoisturesNum = value;
                    }
                }
            }

            public static void DrawSitesGraphInfo(MapGraph graph)
            {
                if (debugInfo == DebugInfoType.DIT_SITES_GRAPH_INFO)
                {
                    if (sitesGraphInfoType == SitesGraphInfoType.SGIT_ELEVATION)
                        DrawSitesElevations(graph.Sites(), graph.TileWidth(), graph.TileHeight());
                    else if (sitesGraphInfoType == SitesGraphInfoType.SGIT_WATER_AND_COAST)
                        DrawSitesWaterAndCoastInfo(graph.Sites(), graph.TileWidth(), graph.TileHeight());
                    else if (sitesGraphInfoType == SitesGraphInfoType.SGIT_FLUXES)
                        DrawSitesFluxes(graph.Sites(), graph.TileWidth(), graph.TileHeight());
                    else if(sitesGraphInfoType == SitesGraphInfoType.SGIT_MOISTURES)
                        DrawSitesMoistures(graph.Sites(), graph.TileWidth(), graph.TileHeight());
                }
            }
            private static void DrawSitesElevations(NativeArray<Site> sites, int tileWidth, int tileHeight)
            {
                for (int i = 0; i < sites.Length; ++i)
                {
                    Site site = sites[i];
                    if(showSiteElevationNum)
                    {
                        IMDraw.Label(new Vector3(site.point.x, site.point.y, 0), site.biomeType == BiomeType.BT_Ocean ? Color.blue : Color.white, LabelPivot.MIDDLE_CENTER, LabelAlignment.CENTER, "■");
                        IMDraw.Label(new Vector3(site.point.x, site.point.y, 0), Color.green, LabelPivot.LOWER_RIGHT, LabelAlignment.CENTER, site.elevation.ToString());
                    }
                    IMDraw.Quad3D(new Vector3(site.point.x, site.point.y, 20), Quaternion.Euler(0, 90, 0), tileWidth, tileHeight, IMDrawAxis.X, new Color(site.elevation, site.elevation, site.elevation, 0.5f));
                }
            }
            private static void DrawSitesWaterAndCoastInfo(NativeArray<Site> sites, int tileWidth, int tileHeight)
            {
                for (int i = 0; i < sites.Length; ++i)
                {
                    Site site = sites[i];
                    if (site.water)
                    {
                        IMDraw.Label(new Vector3(site.point.x, site.point.y, 0), Color.blue, LabelPivot.MIDDLE_CENTER, LabelAlignment.CENTER, "■");
                        IMDraw.Quad3D(new Vector3(site.point.x, site.point.y, 20), Quaternion.Euler(0, 90, 0), tileWidth, tileHeight, IMDrawAxis.X, new Color(0, 0, 1, 0.5f));
                    }
                    else
                    {
                        if (site.coast)
                        {
                            IMDraw.Label(new Vector3(site.point.x, site.point.y, 0), Color.cyan, LabelPivot.MIDDLE_CENTER, LabelAlignment.CENTER, "■");
                            IMDraw.Quad3D(new Vector3(site.point.x, site.point.y, 20), Quaternion.Euler(0, 90, 0), tileWidth, tileHeight, IMDrawAxis.X, new Color(0, 1, 1, 0.5f));
                        }
                        else
                        {
                            IMDraw.Label(new Vector3(site.point.x, site.point.y, 0), Color.green, LabelPivot.MIDDLE_CENTER, LabelAlignment.CENTER, "■");
                            IMDraw.Quad3D(new Vector3(site.point.x, site.point.y, 20), Quaternion.Euler(0, 90, 0), tileWidth, tileHeight, IMDrawAxis.X, new Color(0, 1, 0, 0.5f));
                        }
                    }
                }
            }
            private static void DrawSitesFluxes(NativeArray<Site> sites, int tileWidth, int tileHeight)
            {
                for (int i = 0; i < sites.Length; ++i)
                {
                    Site site = sites[i];
                    IMDraw.Quad3D(new Vector3(site.point.x, site.point.y, 20), Quaternion.Euler(0, 90, 0), tileWidth, tileHeight, IMDrawAxis.X, new Color(site.flux, site.flux, site.flux, 0.5f));
                    if(showSiteFluxesNum)
                        IMDraw.Label(new Vector3(site.point.x, site.point.y, 0), Color.white, LabelPivot.LOWER_RIGHT, LabelAlignment.CENTER, site.flux.ToString());
                }
            }
            private static void DrawSitesMoistures(NativeArray<Site> sites, int tileWidth, int tileHeight)
            {
                for (int i = 0; i < sites.Length; ++i)
                {
                    Site site = sites[i];
                    IMDraw.Quad3D(new Vector3(site.point.x, site.point.y, 20), Quaternion.Euler(0, 90, 0), tileWidth, tileHeight, IMDrawAxis.X, new Color(site.moisture, site.moisture, site.moisture, 0.5f));
                    if(showSiteMoisturesNum)
                        IMDraw.Label(new Vector3(site.point.x, site.point.y, 0), Color.white, LabelPivot.LOWER_RIGHT, LabelAlignment.CENTER, site.moisture.ToString());
                }
            }
            //------------------------------------------------

            //---------Edges GraphInfo------------------------
            private static EdgesGraphInfoType edgesGraphInfoType = EdgesGraphInfoType.EGIT_RIVER;
            public static EdgesGraphInfoType EdgesGraphInfoType
            {
                get { return edgesGraphInfoType; }
                set
                {
                    if (debugInfo == DebugInfoType.DIT_EDGES_GRAPH_INFO)
                    {
                        if (edgesGraphInfoType != value)
                            edgesGraphInfoType = value;
                    }
                }
            }
            public static void DrawEdgesGraphInfo(MapGraph graph)
            {
                if (debugInfo == DebugInfoType.DIT_EDGES_GRAPH_INFO)
                {
                    if (edgesGraphInfoType == EdgesGraphInfoType.EGIT_RIVER)
                        DrawEdgesRivers(graph.Edges(), graph.Corners(), graph.Sites(), graph.TileWidth(), graph.TileHeight());
                }
            }
            private static void DrawEdgesRivers(NativeArray<Edge> edges, NativeArray<Corner> corners, NativeArray<Site> sites, int tileWidth, int tileHeight)
            {
                for (int i = 0; i < sites.Length; ++i)
                {
                    Site site = sites[i];
                    IMDraw.Quad3D(new Vector3(site.point.x, site.point.y, 20), Quaternion.Euler(0, 90, 0), tileWidth, tileHeight, IMDrawAxis.X, new Color(site.elevation, site.elevation, site.elevation, 0.5f));
                }
                for (int i = 0; i < edges.Length; ++i)
                {
                    Edge edge = edges[i];
                    if (edge.river > 0)
                    {
                        Corner c0 = corners[edge.c0];
                        Corner c1 = corners[edge.c1];
                        IMDraw.Line3D(new Vector3(c0.point.x, c0.point.y, 15), new Vector3(c1.point.x, c1.point.y, 15), Color.blue);
                        IMDraw.Label(new Vector3(edge.midpoint.x, edge.midpoint.y, 15), Color.white, LabelPivot.MIDDLE_CENTER, LabelAlignment.CENTER, edge.river.ToString());
                    }
                }
            }
            //------------------------------------------------

            //---------Biome Info-----------------------------
            public static void DrawBiomeInfo(MapGraph graph)
            {
                NativeArray<Site> sites = graph.Sites();
                if (debugInfo == DebugInfoType.DIT_BIOME_INFO)
                {
                    for (int i = 0; i < sites.Length; ++i)
                    {
                        Site site = sites[i];
                        IMDraw.Quad3D(new Vector3(site.point.x, site.point.y, 20), Quaternion.Euler(0, 90, 0), graph.TileWidth(), graph.TileHeight(), IMDrawAxis.X, BiomeColor[(int)site.biomeType]);
                    }
                }
            }
            //------------------------------------------------
        }
    }
}

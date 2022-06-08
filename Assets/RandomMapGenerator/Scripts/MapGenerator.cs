/*-----------------
 * File: MapGenerator.cs
 * Description: 地图生成器
 * Author: haibo.wang@unity3d.com
 * Date: 2022-6-1
 */
using UnityEngine;
namespace TinyFlare
{
    namespace RandomMapGenerator
    {
        public class MapGenerator : MonoBehaviour
        {
            private MapGraph graph = new MapGraph();
            private bool hasInit = false;
            public int width = 800;
            public int height = 600;
            public int numX = 1;
            public int numY = 1;
            private int maxOrthographicSize;
            private int initOrthographicSize;
            private int minOrthographicSize;

            // Start is called before the first frame update
            void Start()
            {
                //初始化Map Graph
                if (graph != null)
                {
                    if (!graph.Init(width, height, numX, numY))
                        Debug.LogError("Map Graph init failed!");
                    
                }
                ///调整所有相机属性
                maxOrthographicSize = (int)(height *0.5 + height * 0.5f / numY);
                initOrthographicSize = (int)(height * 0.25f);
                minOrthographicSize = (int)(height * 0.1f);
                for (int i = 0; i < Camera.allCameras.Length; ++i)
                {
                    Camera cam = Camera.allCameras[i];
                    if (cam)
                    {
                        cam.orthographicSize = maxOrthographicSize;
                        cam.transform.Translate(new Vector3(width / 2 - width * 0.5f / numX, height / 2 - height * 0.5f / numY, 0));
                    }
                }
                ///初始化调试信息
                MapDebugHelper.DebugInfo = DebugInfoType.DIT_SITES_GRAPH_INFO;
                hasInit = true;
            }

            // Update is called once per frame
            void Update()
            {
                if (!hasInit)
                    return;
                switch (MapDebugHelper.DebugInfo)
                {
                    case DebugInfoType.DIT_DEBUG_INFO:
                        MapDebugHelper.DrawDebugInfo(graph);
                        break;
                    case DebugInfoType.DIT_DEBUG_EXTRA_INFO:
                        MapDebugHelper.DrawDebugExtraInfo(graph);
                        break;
                    case DebugInfoType.DIT_CORNERS_GRAPH_INFO:
                        MapDebugHelper.DrawCornersGraphInfo(graph);
                        break;
                    case DebugInfoType.DIT_SITES_GRAPH_INFO:
                        MapDebugHelper.DrawSitesGraphInfo(graph);
                        break;
                    default:
                        break;
                }
                
            }
        }
    }
}

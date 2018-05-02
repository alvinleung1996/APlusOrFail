using UnityEngine;
using UnityEngine.SceneManagement;

namespace APlusOrFail
{
    public static class SceneBuildIndex
    {
        public static readonly int setup = GetSceneBuildIndex("Setup");
        public static readonly int map0 = GetSceneBuildIndex("Map0");

        private static int GetSceneBuildIndex(string path)
        {
            int index = SceneUtility.GetBuildIndexByScenePath(path);
            if (index < 0) Debug.LogErrorFormat($"Cannot find scene path {path}");
            return index;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APlusOrFail.Setup
{
    public static class MapSelectionRegistry
    {
        public static IMapSelectionRegistry instance { get; private set; }

        public static IMapSelectionHandler Schedule(int sceneId, int priority) => instance.Schedule(sceneId, priority);
        public static void UpdateSchedule(IMapSelectionHandler handler, int sceneId, int priority) => instance.UpdateSchedule(handler, sceneId, priority);
        public static void Unschedule(IMapSelectionHandler handler) => instance.Unschedule(handler);

        public static bool Register(this IMapSelectionRegistry registry)
        {
            if (instance == null)
            {
                instance = registry;
                return true;
            }
            return false;
        }

        public static void Unregister(this IMapSelectionRegistry registry)
        {
            if (instance == registry)
            {
                instance = null;
            }
        }
    }

    public interface IMapSelectionRegistry
    {
        IMapSelectionHandler Schedule(int sceneId, int priority);
        void UpdateSchedule(IMapSelectionHandler handler, int sceneId, int priority);
        void Unschedule(IMapSelectionHandler handler);
    }

    public interface IMapSelectionHandler
    {
        int sceneIndex { get; }
        int priority { get; }
    }
}

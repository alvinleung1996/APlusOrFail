namespace APlusOrFail.Maps
{
    public static class MapManager
    {
        public static IMapManager instance { get; private set; }

        public static IMapStat mapStat => instance?.stat;

        public static bool Register(this IMapManager manager)
        {
            if (instance == null)
            {
                instance = manager;
                return true;
            }
            return false;
        }

        public static bool Unregister(this IMapManager manager)
        {
            if (ReferenceEquals(instance, manager))
            {
                instance = null;
                return true;
            }
            return false;
        }
    }

    public interface IMapManager
    {
        IMapStat stat { get; }
    }
}

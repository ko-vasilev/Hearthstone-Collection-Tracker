using System.Collections.Generic;
using System.Linq;

namespace Hearthstone_Collection_Tracker.Internal.DataUpdaters
{
    public static class DefaultDataUpdater
    {
        private static IEnumerable<IDataUpdater> _updaters = new List<IDataUpdater>()
        {
            new DataUpdaterV02(),
            new DataUpdaterV021(),
            new DataUpdaterV022(),
            new DataUpdaterV03(),
            new DataUpdaterV031(),
            new DataUpdaterV032(),
            new DataUpdaterV04(),
            new DataUpdaterV041()
        };

        /// <summary>
        /// A list of update steps that are required to keep data files up-to-date
        /// </summary>
        public static IEnumerable<IDataUpdater> Updaters
        {
            get
            {
                return _updaters;
            }
        }

        public static void PerformUpdates()
        {
            foreach(var updater in Updaters.OrderBy(u => u.Version))
            {
                if (!updater.RequiresUpdate)
                    continue;
                updater.PerformUpdate();
            }
        }
    }
}

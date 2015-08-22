using System;

namespace Hearthstone_Collection_Tracker.Internal.DataUpdaters
{
    public interface IDataUpdater
    {
        Version Version { get; }

        bool RequiresUpdate { get; }

        void PerformUpdate();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hearthstone_Collection_Tracker.Internal.DataUpdaters
{
    public interface IDataUpdater
    {
        Version Version { get; }

        bool RequiresUpdate { get; }

        void PerformUpdate();
    }
}

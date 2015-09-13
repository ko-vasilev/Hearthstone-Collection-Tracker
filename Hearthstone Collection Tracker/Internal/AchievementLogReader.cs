using Hearthstone_Deck_Tracker.LogReader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hearthstone_Collection_Tracker.Internal
{
    public class AchievementLogReader : LogReaderInfo
    {
        public AchievementLogReader()
        {
            this.Name = "Achievements";
            this.ContainsFilters = new[] { "NotifyOfCardGained" };
        }
    }
}

using System;

namespace Hearthstone_Collection_Tracker.Internal.DataUpdaters
{
    public class DataUpdaterV041 : BaseUpdaterByVersion
    {
        private static readonly Version _version = new Version(0, 4, 1);

        public override Version Version
        {
            get
            {
                return _version;
            }
        }
    }
}

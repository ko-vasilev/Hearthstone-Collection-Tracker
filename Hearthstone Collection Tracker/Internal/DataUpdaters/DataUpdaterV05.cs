using System;

namespace Hearthstone_Collection_Tracker.Internal.DataUpdaters
{
    public class DataUpdaterV05 : BaseUpdaterByVersion
    {
        private static readonly Version _version = new Version(0, 5, 0);

        public override Version Version
        {
            get
            {
                return _version;
            }
        }
    }
}

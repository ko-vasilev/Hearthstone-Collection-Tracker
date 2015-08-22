using System.Collections.Generic;

namespace Hearthstone_Collection_Tracker.ViewModels
{
    // used for serialization
    public class BasicSetCollectionInfo
    {
        public string SetName { get; set; }

        public List<CardInCollection> Cards { get; set; }
    }
}

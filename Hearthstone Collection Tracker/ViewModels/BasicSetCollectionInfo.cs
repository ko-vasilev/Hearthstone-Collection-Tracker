using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hearthstone_Collection_Tracker.ViewModels
{
    // used for serialization
    public class BasicSetCollectionInfo
    {
        public string SetName { get; set; }

        public List<CardInCollection> Cards { get; set; }
    }
}

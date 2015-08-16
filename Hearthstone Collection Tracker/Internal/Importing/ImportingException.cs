using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hearthstone_Collection_Tracker.Internal.Importing
{
    internal class ImportingException : Exception
    {
        public ImportingException()
        {
        }

        public ImportingException(string message)
            :base(message)
        {
        }

        public ImportingException(string message, Exception innerException)
            :base(message, innerException)
        {
        }
    }
}

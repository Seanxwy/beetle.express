using System;
using System.Collections.Generic;

using System.Text;

namespace Beetle.Express
{

    public class ChannelReceiveEventArgs : ChannelEventArgs
    {

        public IReceiveData Data
        {
            get;
            internal set;
        }
    }
}

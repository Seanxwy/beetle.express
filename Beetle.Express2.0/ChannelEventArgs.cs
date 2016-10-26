using System;
using System.Collections.Generic;

using System.Text;

namespace Beetle.Express
{

    public class ChannelEventArgs : EventArgs
    {

        public IChannel Channel
        {
            get;
            internal set;
        }
    }
}

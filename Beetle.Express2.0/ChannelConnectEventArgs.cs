using System;
using System.Collections.Generic;

using System.Text;

namespace Beetle.Express
{

    public class ChannelConnectEventArgs : ChannelEventArgs
    {

        public bool Cancel
        {
            get;
            set;
        }
    }

}

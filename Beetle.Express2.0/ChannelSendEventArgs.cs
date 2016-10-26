using System;
using System.Collections.Generic;

using System.Text;

namespace Beetle.Express
{

    public class ChannelSendEventArgs : ChannelEventArgs
    {

        public IData Data
        {
            get;
            set;
        }

        public SendStatus Status
        {
            get;
            set;
        }
    }
}

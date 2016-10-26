using System;
using System.Collections.Generic;

using System.Text;

namespace Beetle.Express
{

    public class ErrorEventArgs : ChannelEventArgs
    {

        public Exception Error
        {
            get;
            internal set;
        }

        public string Tag
        {
            get;
            set;
        }
    }
}

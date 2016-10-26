using System;
using System.Collections.Generic;

using System.Text;

namespace Beetle.Express
{

    public class OnlineSegment
    {

        public OnlineSegment(int count)
        {
            mChannels = new IChannel[count];
        }

        private IChannel[] mChannels;

        public IChannel[] Channels
        {
            get
            {
                return mChannels;
            }
        }

        public long Version
        {
            get;
            set;
        }

        public int Count
        {
            get;
            internal set;
        }

    }
}

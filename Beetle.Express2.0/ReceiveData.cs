using System;
using System.Collections.Generic;

using System.Text;

namespace Beetle.Express
{

    class ReceiveData : IReceiveData
    {
        public ReceiveData(int length)
        {
            Array = new byte[length];
            CREA = new ChannelReceiveEventArgs();
            CREA.Data = this;
        }

        public byte[] Array
        {
            get;
            internal set;
        }

        public int Offset
        {
            get;
            set;
        }

        public int Count
        {
            get;
            set;
        }

        public void Dispose()
        {
            lock (this)
            {
                if (CREA != null)
                {
                    CREA.Data = null;
                    CREA = null;
                }
            }
        }

        internal ChannelReceiveEventArgs CREA
        {
            get;
            set;
        }

        internal ReceiveDataPool Pool
        {
            get;
            set;
        }

        internal IChannel Channel { get; set; }

        internal void Import(byte[] data, int offset, int count)
        {
            Buffer.BlockCopy(data, offset, Array, offset, count);
            Offset = offset;
            Count = count;
        }

        internal void Exit()
        {
            Channel = null;
            if (Pool != null)
            {
                Pool.Push(this);
            }
        }

        public string ToString(Encoding coding)
        {
            return coding.GetString(Array, Offset, Count);
        }
    }
}

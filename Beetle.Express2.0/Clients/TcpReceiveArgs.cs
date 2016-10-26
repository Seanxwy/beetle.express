using System;
using System.Collections.Generic;
//********************************************************
// 	Copyright © henryfan 2013		 
//	Email:		henryfan@msn.com	
//	HomePage:	http://www.ikende.com		
//	CreateTime:	2013/6/15 14:55:21
//********************************************************	 
using System.Text;

namespace Beetle.Express.Clients
{

    public class TcpReceiveArgs : EventArgs
    {

        public TcpClient Client
        {
            get;
            internal set;
        }

        public byte[] Data
        {
            get;
            internal set;
        }

        public int Offset
        {
            get;
            internal set;
        }

        public int Count
        {
            get;
            internal set;
        }

        public byte[] ToArray()
        {
            byte[] result = new byte[Count];
            Buffer.BlockCopy(Data, Offset, result, 0, Count);
            return result;
        }
    
        public void CopyTo(byte[] data, int start = 0)
        {
            Buffer.BlockCopy(Data, Offset, data, start, Count);
        }
    }
}

using System;
using System.Collections.Generic;
//********************************************************
// 	Copyright © henryfan 2013		 
//	Email:		henryfan@msn.com	
//	HomePage:	http://www.ikende.com		
//	CreateTime:	2013/6/10 22:54:28
//********************************************************	 
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace Beetle.Express.Clients
{

    public class UdpClient
    {

        public UdpClient(string host, int port)
        {
            mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            if (string.IsNullOrEmpty(host))
                mSocket.Bind(new IPEndPoint(IPAddress.Any, port));
            else
                mSocket.Bind(new IPEndPoint(IPAddress.Parse(host), port));
            mReceiveSAEA.Completed += OnReceiveCompleted;
            mReceiveSAEA.SetBuffer(new byte[1024 * 64], 0, 1024 * 64);
            BeginReceive();
        }

        private Exception mLastError;

        private SocketAsyncEventArgs mReceiveSAEA = new SocketAsyncEventArgs();

        private Socket mSocket;

        private void OnReceiveCompleted(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                if (e.SocketError == SocketError.Success && e.BytesTransferred > 0)
                {
                    UdpReceiveArgs ura = new UdpReceiveArgs();
                    ura.EndPoint = e.RemoteEndPoint;
                    ura.Data = e.Buffer;
                    ura.Offset = 0;
                    ura.Count = e.BytesTransferred;
                    OnReceive(ura);
                }
            }
            catch (Exception e_)
            {
                mLastError = e_;
            }
            finally
            {

                BeginReceive();
            }
        }

        private void BeginReceive()
        {
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, 0);

            mReceiveSAEA.RemoteEndPoint = endpoint;
            if (!mSocket.ReceiveFromAsync(mReceiveSAEA))
            {
                OnReceiveCompleted(this, mReceiveSAEA);
            }
        }

        protected virtual void OnReceive(UdpReceiveArgs e)
        {
            if (Receive != null)
                Receive(this, e);
        }

        public Exception LastError
        {
            get
            {
                return mLastError;
            }
        }

        public void Send(string data, string host, int port)
        {
            Send(data, new IPEndPoint(IPAddress.Parse(host), port));
        }

        public void Send(byte[] data, string host, int port)
        {
            Send(data, new IPEndPoint(IPAddress.Parse(host), port));
        }

        public void Send(byte[] data, EndPoint point)
        {
            Send(data, 0, data.Length, point);
        }
 
        public void Send(byte[] data, int offset, int count, EndPoint point)
        {
            while (count > 0)
            {
                int sends = mSocket.SendTo(data, offset, count, SocketFlags.None, point);
                count -= sends;
                offset += sends;
            }
        }

        public void Send(string data, EndPoint point)
        {
            Send(Encoding.UTF8.GetBytes(data), point);
        }

        public event EventHandler<UdpReceiveArgs> Receive;

    }



   
}

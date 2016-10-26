using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace Beetle.Express.Clients
{

    public class TcpClient
    {

        public TcpClient()
        {
            mSAEA.SetBuffer(new byte[1024 * 8], 0, 1024 * 8);
            mSAEA.Completed += Receive_Completed;
        }

        public TcpClient(IPackage package)
        {
            Package = package;
            mSAEA.SetBuffer(new byte[1024 * 8], 0, 1024 * 8);
            mSAEA.Completed += Receive_Completed;
        }

        public IPackage Package
        {
            get;
            private set;
        }

        private bool mConnected = false;

        private Socket mSocket;

        private Exception mLastError;

        private SocketAsyncEventArgs mSAEA = new SocketAsyncEventArgs();

        public void DisConnect()
        {
            mConnected = false;
            try
            {
                if (mSocket != null)
                {
                    mSocket.Close();

                }
            }
            catch
            {
            }
            mSocket = null;
        }

        private void Receive_Completed(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                if (e.SocketError == SocketError.Success && e.BytesTransferred > 0)
                {
                    TcpReceiveArgs tra = new TcpReceiveArgs();
                    tra.Data = e.Buffer;
                    tra.Offset = 0;
                    tra.Count = e.BytesTransferred;
                    OnReceive(tra);
                    BeginReceive();
                }
                else
                {
                    mLastError = new Exception(string.Format("socket recieve {0} error {1}", e.BytesTransferred, e.SocketError.ToString()));
                    DisConnect();
                }
            }
            catch (Exception e_)
            {
                mLastError = e_;
            }

        }

        private void BeginReceive()
        {
            try
            {

                if (!mSocket.ReceiveAsync(mSAEA))
                {
                    Receive_Completed(this, mSAEA);
                }
            }
            catch (Exception e_)
            {
                DisConnect();
                mLastError = e_;
            }


        }

        protected virtual void OnReceive(TcpReceiveArgs e)
        {
            e.Client = this;
            if (Package != null)
            {
                Package.Import(e.Data, e.Offset, e.Count);
            }
            else
            {
                if (Receive != null)
                    Receive(this, e);
            }
        }

        public event EventHandler<TcpReceiveArgs> Receive;

        public Exception LastError
        {
            get
            {
                return mLastError;
            }
        }

        public Socket Socket
        {
            get
            {
                return mSocket;
            }

        }

        public bool Connected
        {
            get
            {
                return mConnected;
            }
        }

        public void Connect(string host, int port)
        {
            IPAddress[] ips = Dns.GetHostAddresses(host);
            if (ips.Length == 0)
                throw new Exception("get host's IPAddress error");
            var address = ips[0];
            try
            {
                mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                mSocket.Connect(address, port);
                mConnected = true;
                BeginReceive();
            }
            catch (Exception e_)
            {
                DisConnect();
                mLastError = e_;
                throw e_;
            }
        }

        public bool SendMessage(object message)
        {
            IData data;
            if (message is IData)
            {
                data = (IData)message;

            }
            else
            {
                data = Package.GetMessageData(message);

            }
            bool result= Send(data.Array, data.Offset, data.Count);
            Package.Recover(data);
            return result;
        }

        public bool Send(string value)
        {
            return Send(value, Encoding.UTF8);
        }

        public bool Send(string value, Encoding coding)
        {
            return Send(coding.GetBytes(value));
        }

        public bool Send(byte[] data)
        {
            return Send(data, 0, data.Length);
        }
        public bool Send(byte[] data, int offset, int count)
        {
            try
            {
                lock (this)
                {

                    while (count > 0)
                    {
                        int sends = mSocket.Send(data, offset, count, SocketFlags.None);
                        count -= sends;
                        offset += sends;
                    }
                }
                return true;
            }
            catch (Exception e_)
            {
                DisConnect();
                mLastError = e_;
                return false;
            }

        }
        public bool Send(ArraySegment<byte> data)
        {
            return Send(data.Array, data.Offset, data.Count);

        }

    }
}

using System;
using System.Collections.Generic;

using System.Text;

namespace Beetle.Express
{

    class UdpChannel : IChannel
    {

        public UdpChannel(System.Net.Sockets.Socket socket, IServer server, System.Net.EndPoint remotEndPoint)
        {
            mID = Guid.NewGuid().ToString("N");
            mServer = server;
            mSocket = socket;
            mEndPoint = remotEndPoint;
            mSendSAEA = new System.Net.Sockets.SocketAsyncEventArgs();
            mSendSAEA.SetBuffer(new byte[server.SendBufferSize], 0,server.SendBufferSize);
            mSendSAEA.RemoteEndPoint = mEndPoint;
            mSendSAEA.Completed += OnSend;
            Status = ChannelStatus.None;
            
        }

        private System.Net.Sockets.Socket mSocket;

        private string mID;

        private System.Collections.Hashtable mProperties = new System.Collections.Hashtable();

        private System.Net.EndPoint mEndPoint;

        private IServer mServer;

        private bool mIsDisposed = false;

        internal bool Sending = false;

        private System.Net.Sockets.SocketAsyncEventArgs mSendSAEA;

        internal void OnSend(object sender, System.Net.Sockets.SocketAsyncEventArgs e)
        {
            if (e.SocketError == System.Net.Sockets.SocketError.Success && e.BytesTransferred > 0)
            {

                Sending = false;
                AddSendData(null);
                OnSendEvent((IData)e.UserToken, SendStatus.Success);
                e.UserToken = null;

            }
            else
            {
                Sending = false;
                Dispose();
            }
        }
     
        public string ID
        {
            get { return mID; }
        }

        public System.Net.EndPoint EndPoint
        {
            get
            {
                return mEndPoint;
            }
        }

        public string Name
        {
            get;
            set;
        }

        public object this[string name]
        {
            get
            {
                return mProperties[name];
            }
            set
            {
                mProperties[name] = value;
            }
        }

        public System.Net.Sockets.Socket Socket
        {
            get
            {
                return mSocket;
            }
        }

        public IServer Server
        {
            get { return mServer; }
        }

        public bool IsDisposed
        {
            get { return mIsDisposed; }
        }

        private void OnDispose()
        {
          
            try
            {
                
                mSocket = null;
                mSendSAEA.Completed -= OnSend;
                while (mDatas.Count > 0)
                {
                    SendData(GetSendData());
                }
                if (ChannelDisposed != null)
                {
                    ChannelDisposed(Server, new ChannelEventArgs { Channel = this });
                }
            }
            finally
            {
                mProperties.Clear();

                mServer = null;
            }


        }

        public void Dispose()
        {
            lock (this)
            {
                if (!mIsDisposed)
                {
                    mIsDisposed = true;
                    OnDispose();
                }
            }
        }

        internal EventChannelDisposed ChannelDisposed;

        internal EventChannelError ChannelError;

        internal EventChannelReceive ChannelReceive;

        internal EventChannelSend ChannelSend;

        private Queue<object> mDatas = new Queue<object>();

        internal void AddSendData(object data)
        {
            lock (mDatas)
            {
                if (data != null)
                {

                    mDatas.Enqueue(data);
                }
                if (!Sending && mDatas.Count > 0)
                {
                    Server.InvokeSend(this);
                    Sending = true;
                }

            }
        }

        internal IData GetSendData()
        {
            lock (mDatas)
            {
                if (mDatas.Count > 0)
                {
                    object result= mDatas.Dequeue();
                    if (result is IData)
                        return (IData)result;
                    return Package.GetMessageData(result);
                }
                return null;
            }
        }

        public void InvokeSend()
        {
            IData data = GetSendData();
            if (data != null)
            {
                SendData(data);
            }
        }

        private void SendData(IData data)
        {
            try
            {
                if (IsDisposed)
                {
                    OnSendEvent(data, SendStatus.ChannelDisposed);
                    return;
                }
                if (Package != null)
                    Package.Channel = null;
                Buffer.BlockCopy(data.Array, 0, mSendSAEA.Buffer, 0, data.Count);
                mSendSAEA.SetBuffer(0, data.Count);
                mSendSAEA.UserToken = data;
                if (!mSocket.SendToAsync(mSendSAEA))
                {
                    OnSend(this, mSendSAEA);
                }
            }
            catch (Exception e_)
            {
                Sending = false;
                InvokeError(e_);
                OnSendEvent(data, SendStatus.Error);
            }
        }

        private void OnSendEvent(IData data, SendStatus status)
        {
            try
            {
                data.Decrement();
                if (ChannelSend != null)
                {
                    ChannelSend(Server, new ChannelSendEventArgs { Channel = this, Data = data, Status = status });
                }
            }
            catch
            {
            }
        }

        public void InvokeReceive(IReceiveData data)
        {
            if (ChannelReceive != null)
            {
                ReceiveData rd = (ReceiveData)data;
                rd.CREA.Channel = rd.Channel;
                ChannelReceive(Server, rd.CREA);
            }
        }

        public void InvokeError(Exception e)
        {
            if (!mIsDisposed && (e is ObjectDisposedException || e is System.Net.Sockets.SocketException))
            {
                Dispose();
            }
            System.Threading.ThreadPool.QueueUserWorkItem(
                OnError, new ErrorEventArgs { Channel = this, Error = e });
        }

        private void OnError(object e)
        {
            try
            {
                ErrorEventArgs ceea = (ErrorEventArgs)e;
                if (ChannelError != null)
                    ChannelError(Server, ceea);
            }
            catch
            {
            }

        }

        public string ChannelType
        {
            get { return "UDP"; }
        }

     

        public void TimeOut()
        {
            Dispose();
        }

        public ChannelStatus Status
        {
            get;
            set;
        }


        public string CloseStatus
        {
            get;
            set;
        }


        public object Tag
        {
            get;
            set;
        }

        public IPackage Package
        {
            get;
            set;
        }
    }
}

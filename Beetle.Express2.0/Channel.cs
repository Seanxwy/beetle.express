using System;
using System.Collections.Generic;

using System.Text;

namespace Beetle.Express
{
    class Channel : IChannel
    {
        public Channel(TcpServer server, System.Net.Sockets.Socket socket)
        {
            mID = Guid.NewGuid().ToString("N");
            mServer = server;
            mSendSAEA = new System.Net.Sockets.SocketAsyncEventArgs();
            mReceiveSAEA = new System.Net.Sockets.SocketAsyncEventArgs();
            mSendSAEA.SetBuffer(new byte[Server.SendBufferSize], 0, server.SendBufferSize);
            mReceiveSAEA.SetBuffer(new byte[server.ReceiveBufferSize], 0, server.ReceiveBufferSize);
            mSendSAEA.Completed += OnSend;
           mReceiveSAEA.Completed += OnReceive;
            mSocket = socket;
            mEndPoint = socket.RemoteEndPoint;
            Status = ChannelStatus.None;

        }

        internal ReceiveDispatch ReceiveDispatch;

        private System.Net.Sockets.Socket mSocket;
    
        private string mID;
  
        private System.Collections.Hashtable mProperties = new System.Collections.Hashtable();

        private System.Net.Sockets.SocketAsyncEventArgs mSendSAEA;

        private System.Net.Sockets.SocketAsyncEventArgs mReceiveSAEA;
   
        private System.Net.EndPoint mEndPoint;
 
        private TcpServer mServer;

        private bool mIsDisposed = false;
   
        internal bool Sending = false;
     
        private SendUserToken mSendUserToken = new SendUserToken();

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
            TcpServer.DisposeSocket(mSocket);
            try
            {
                if (Package != null)
                    Package.Channel = null;
               
                mSocket = null;
                mSendSAEA.SetBuffer(null, 0, 0);
                mReceiveSAEA.SetBuffer(null, 0, 0);
                mSendSAEA.Completed -= OnSend;
                mReceiveSAEA.Completed -= OnReceive;
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
    

        internal void BeginReceive()
        {
            try
            {
                if (!mSocket.ReceiveAsync(mReceiveSAEA))
                {
                    OnReceive(this, mReceiveSAEA);
                }
            }
            catch (Exception e_)
            {
                InvokeError(e_);
            }
        }


        private void OnReceive(object sender, System.Net.Sockets.SocketAsyncEventArgs e)
        {
            if (e.SocketError == System.Net.Sockets.SocketError.Success && e.BytesTransferred > 0)
            {
                //数据接收成功
                ReceiveData rd = (ReceiveData)Server.PopData();
                rd.Import(e.Buffer, 0, e.BytesTransferred);
                rd.Channel = this;
                Server.InvokeReceive(rd);
                BeginReceive();
            }
            else
            {
                CloseStatus = "receive error " + e.SocketError.ToString();
                Dispose();
            }
        }
        

        private void OnSend(object sender, System.Net.Sockets.SocketAsyncEventArgs e)
        {
            if (e.SocketError == System.Net.Sockets.SocketError.Success && e.BytesTransferred > 0)
            {

                if (e.BytesTransferred != e.Count)
                {
                   
                    e.SetBuffer(e.Offset + e.BytesTransferred, e.Count - e.BytesTransferred);
                    if (!Socket.SendAsync(e))
                        OnSend(this, e);
                }
                else
                {

                    if (mSendUserToken.Count > 0)
                    {
                       
                        SendData(mSendUserToken.Data);
                    }
                    else
                    {
                        
                        Sending = false;
                        AddSendData(null);
                        OnSendEvent(mSendUserToken.Data, SendStatus.Success);

                    }
                }
            }
            else
            {
                
                Sending = false;
                OnSendEvent(mSendUserToken.Data, SendStatus.Error);
                CloseStatus = "send error " + e.SocketError.ToString();
                Dispose();
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
                if (mSendUserToken.Count > 0)
                {
                    CopyToBuffer();
                    if (!mSocket.SendAsync(mSendSAEA))
                    {
                        OnSend(this, mSendSAEA);
                    }
                }


            }
            catch (Exception e_)
            {
                Sending = false;
                InvokeError(e_);
                OnSendEvent(data, SendStatus.Error);
            }
        }
         
       
        private void CopyToBuffer()
        {
            if (mSendUserToken.Count > mSendSAEA.Buffer.Length)
            {
                Buffer.BlockCopy(mSendUserToken.Array, mSendUserToken.Offset, mSendSAEA.Buffer, 0, mSendSAEA.Buffer.Length);
                mSendSAEA.SetBuffer(0, mSendSAEA.Buffer.Length);
                mSendUserToken.Offset += mSendSAEA.Buffer.Length;
                mSendUserToken.Count -= mSendSAEA.Buffer.Length;

            }
            else
            {
                Buffer.BlockCopy(mSendUserToken.Array, mSendUserToken.Offset, mSendSAEA.Buffer, 0, mSendUserToken.Count);
                mSendSAEA.SetBuffer(0, mSendUserToken.Count);
                mSendUserToken.Count = 0;

            }
        }


        private Queue<object> mDatas = new Queue<object>();

        internal void AddSendData(object data)
        {
            lock (mDatas)
            {
                if (data != null)
                {

                    mDatas.Enqueue(data);
                }
                if (!Sending && mDatas.Count > 0 && !IsDisposed)
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
                    object msg = mDatas.Dequeue();
                    if (msg is IData)
                        return (IData)msg;
                    return Package.GetMessageData(msg);
                }
                return null;
            }
        }

        public void InvokeSend()
        {
            IData data = GetSendData();
            if (data != null)
            {

                mSendUserToken.Data = data;
                mSendUserToken.Array = data.Array;
                mSendUserToken.Offset = data.Offset;
                mSendUserToken.Count = data.Count;
                SendData(data);
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
            get { return "TCP"; }
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

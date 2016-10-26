using System;
using System.Collections.Generic;

using System.Text;
using System.Net;
using System.Net.Sockets;

namespace Beetle.Express
{

    class UdpServer : IServer
    {
        private string mHost = null;

        private int mPort = 8088;

        private System.Net.Sockets.Socket mSocket;

        private int mSendBufferSize = 1024 * 4;

        private int mReceiveDataPoolSize = 1024;

        private int mReceiveBufferSize = 1024 * 4;


        private Stack<SocketAsyncEventArgs> mSeaes = new Stack<SocketAsyncEventArgs>();


        internal ReceiveDataPool mReceiveDataPool;

        private Dictionary<string, IChannel> mClients = new Dictionary<string, IChannel>(5000);

        private SendDispatch mSendDispatch;

        private ReceiveDispatch mReceiveDispatch;

        private bool mIsDisposed = false;

        private ChannelConnectEventArgs mConnectEA = new ChannelConnectEventArgs();


        private SocketAsyncEventArgs GetSeae()
        {
            SocketAsyncEventArgs seae = new SocketAsyncEventArgs();

            seae.SetBuffer(new byte[ReceiveBufferSize], 0, ReceiveBufferSize);
            seae.Completed += OnSeaeReceive;
            return seae;
        }

        private SocketAsyncEventArgs Pop()
        {
            lock (mSeaes)
            {
                if (mSeaes.Count > 0)
                    return mSeaes.Pop();
                return GetSeae();
            }
        }

        private void Push(SocketAsyncEventArgs e)
        {
            lock (mSeaes)
            {
                mSeaes.Push(e);
            }
        }

        private void OnInit()
        {
            try
            {
                for (int i = 0; i < 100; i++)
                {
                    Push(GetSeae());
                }
                mReceiveDataPool = new ReceiveDataPool(ReceiveDataPoolSize, ReceiveBufferSize);
                mReceiveDispatch = new ReceiveDispatch();
                mReceiveDispatch.Run();
                mSendDispatch = new SendDispatch();
                mSendDispatch.Run();

            }
            catch (Exception e_)
            {
                OnChannelError(this, new ErrorEventArgs { Error = e_, Tag = "Server Init" });
            }
        }

        private void OnListen()
        {
            try
            {

                mSocket = new System.Net.Sockets.Socket(System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Dgram, System.Net.Sockets.ProtocolType.Udp);
                System.Net.IPEndPoint point = new System.Net.IPEndPoint(System.Net.IPAddress.Any, mPort);
                if (!string.IsNullOrEmpty(Host))
                {
                    point = new System.Net.IPEndPoint(System.Net.IPAddress.Parse(Host), Port);

                }
                mSocket.Bind(point);
                System.Threading.ThreadPool.QueueUserWorkItem(OnUdpReceive);
                System.Threading.ThreadPool.QueueUserWorkItem(OnUdpReceive);
                System.Threading.ThreadPool.QueueUserWorkItem(OnUdpReceive);
                System.Threading.ThreadPool.QueueUserWorkItem(OnUdpReceive);
            }
            catch (Exception e_)
            {
                OnChannelError(this, new ErrorEventArgs { Error = e_, Tag = "Server Udp Listen" });
            }
        }

        private void ChannelReceiveData(SocketAsyncEventArgs e)
        {
            Beetle.Express.ReceiveData rd = (Beetle.Express.ReceiveData)PopData();
            Buffer.BlockCopy(e.Buffer, 0, rd.Array, 0, e.BytesTransferred);
            rd.Offset = 0;
            rd.Count = e.BytesTransferred;
            string key = e.RemoteEndPoint.ToString();
            IChannel channel = null;
            if (!mClients.TryGetValue(key, out channel))
            {
                lock (mClients)
                {
                    if (!mClients.TryGetValue(key, out channel))
                    {
                        channel = AddClient(mSocket, e.RemoteEndPoint, key);
                    }
                }
            }
            if (channel != null)
            {
                rd.Channel = channel;
                InvokeReceive(rd);
            }
        }

        private void OnSeaeReceive(object sender, System.Net.Sockets.SocketAsyncEventArgs e)
        {
           
            try
            {
                if (e.SocketError == System.Net.Sockets.SocketError.Success && e.BytesTransferred > 0)
                {
                    ChannelReceiveData(e);
                }
            }
            catch (System.Net.Sockets.SocketException se)
            {
                OnChannelError(this, new ErrorEventArgs { Error = se, Tag = "Server Udp Receive" });
                Dispose();

            }
            catch (Exception e_)
            {
                OnChannelError(this, new ErrorEventArgs { Error = e_, Tag = "Server Udp Receive" });
            }
            finally
            {
                Push(e);
            }
            OnUdpReceive(null);

        }

        private void OnUdpReceive(object state)
        {

            try
            {
                SocketAsyncEventArgs seae = Pop();
                seae.RemoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

                if (!mSocket.ReceiveFromAsync(seae))
                {
                    OnSeaeReceive(this, seae);
                }
            }
            catch (System.Net.Sockets.SocketException se)
            {
                OnChannelError(this, new ErrorEventArgs { Error = se, Tag = "Server Udp Receive" });
                Dispose();

            }
            catch (Exception e)
            {
                OnChannelError(this, new ErrorEventArgs { Error = e, Tag = "Server Udp Receive" });
            }

        }

        private UdpChannel AddClient(System.Net.Sockets.Socket socket, System.Net.EndPoint remotpoint, string key)
        {
            UdpChannel channel = new UdpChannel(mSocket, this, remotpoint);
            OnConnected(channel);
            if (mConnectEA.Cancel)
            {
                channel.Dispose();
                return null;
            }
            else
            {
                channel.ChannelDisposed = OnChannelDispose;
                channel.ChannelError = OnChannelError;
                channel.ChannelReceive = OnChannelReceive;
                channel.ChannelSend = OnChannelSend;
                lock (mClients)
                {
                    mClients.Add(key, channel);

                    Version++;
                }
            }
            return channel;

        }

        private void RemoveClient(IChannel channel)
        {
            lock (mClients)
            {
                mClients.Remove(channel.EndPoint.ToString());
                Version++;
            }
        }

        private void OnConnected(IChannel channel)
        {
            mConnectEA.Cancel = false;
            mConnectEA.Channel = channel;
            if (Handler != null)
            {
                Handler.Connect(this, mConnectEA);
            }
        }


        public void Open()
        {
            OnInit();
            OnListen();
        }

        public void Open(Listen listen)
        {

            Host = listen.Host;
            Port = listen.Port;
            mSendBufferSize = listen.SendBufferSize;
            mReceiveBufferSize = listen.ReceiveBufferSize;
            mReceiveDataPoolSize = listen.ReceiveDataPoolSize;
            Name = listen.Name;
            if (!string.IsNullOrEmpty(listen.Handler))
            {
                Handler = (IServerHandler)Activator.CreateInstance(Type.GetType(listen.Handler));
            }
            OnInit();
            OnListen();
        }


        public int Port
        {
            get
            {
                return mPort;
            }
            set
            {
                mPort = value;
            }
        }


        public string Host
        {
            get
            {
                return mHost;
            }
            set
            {
                mHost = value;
            }
        }


        public int SendBufferSize
        {
            get
            {
                return mSendBufferSize;
            }
            set
            {
                mSendBufferSize = value;
            }
        }

        public int ReceiveBufferSize
        {
            get
            {
                return mReceiveBufferSize;
            }
            set
            {
                mReceiveBufferSize = value;
            }
        }


        public int ReceiveDataPoolSize
        {
            get
            {
                return mReceiveDataPoolSize;
            }
            set
            {
                mReceiveDataPoolSize = value;
            }
        }


        public void Send(object data, params IChannel[] channels)
        {
            if (channels != null)
            {
                if(data is IData)
                    ((IData)data).ExChange(channels.Length);
                for (int i = 0; i < channels.Length; i++)
                {
                    AddSend(data, channels[i]);
                }
            }
        }

        private void AddSend(object data, IChannel channel)
        {

            ((UdpChannel)channel).AddSendData(data);
        }

        public void InvokeSend(IChannel channel)
        {
            mSendDispatch.Add(channel);
        }

        public void InvokeReceive(IReceiveData rd)
        {

            mReceiveDispatch.Add((ReceiveData)rd);
        }

        public void Send(object data, IList<IChannel> channels)
        {
            if (channels != null)
            {
                if(data is IData)
                    ((IData)data).ExChange(channels.Count);
                for (int i = 0; i < channels.Count; i++)
                {
                    AddSend(data, channels[i]);
                }
            }
        }

        internal void OnChannelReceive(object sender, ChannelReceiveEventArgs e)
        {
            if (Handler != null)
                Handler.Receive(this, e);
        }

        internal void OnChannelError(object sender, ErrorEventArgs e)
        {
            try
            {
                if (Handler != null)
                    Handler.Error(this, e);
            }
            catch
            {
            }

        }

        internal void OnChannelDispose(object sender, ChannelEventArgs e)
        {
            try
            {
                RemoveClient(e.Channel);
                if (Handler != null)
                    Handler.Disposed(this, e);
            }
            catch
            {
            }
        }

        internal void OnChannelSend(object sender, ChannelSendEventArgs e)
        {
            try
            {
                if (Handler != null)
                    Handler.SendCompleted(this, e);
            }
            catch
            {
            }
        }


        public IServerHandler Handler
        {
            get;
            set;
        }

        public void Dispose()
        {
            lock (this)
            {
                if (!mIsDisposed)
                {
                    mIsDisposed = true;
                    try
                    {

                        IChannel[] items = GetOnlines();
                        foreach (IChannel item in items)
                        {
                            item.Dispose();
                        }
                        mSendDispatch.Dispose();
                        mReceiveDispatch.Dispose();

                        TcpServer.DisposeSocket(mSocket);
                    }
                    catch (Exception e_)
                    {
                        OnChannelError(this, new ErrorEventArgs { Error = e_, Tag = "Server disposed" });
                    }
                }
            }
        }

        public IReceiveData PopData()
        {
            return mReceiveDataPool.Pop();
        }

        public int Version
        {
            get;
            set;
        }

        public IChannel[] GetOnlines()
        {
            lock (mClients)
            {
                IChannel[] result = new IChannel[mClients.Count];
                mClients.Values.CopyTo(result, 0);
                return result;
            }
        }

        public ArraySegment<IChannel> GetOnlines(IChannel[] array)
        {
            lock (mClients)
            {
                mClients.Values.CopyTo(array, 0);
                return new ArraySegment<IChannel>(array, 0, mClients.Count);
            }
        }

        public IChannel GetChannel(string id)
        {
            IChannel channel = null;
            mClients.TryGetValue(id, out channel);
            return channel;
        }




        public string Name
        {
            get;
            private set;
        }


        public void GetOnlines(OnlineSegment segment)
        {
            if (segment.Version != this.Version)
            {
                lock (mClients)
                {
                    mClients.Values.CopyTo(segment.Channels, 0);
                    segment.Count = mClients.Values.Count;
                    segment.Version = this.Version;
                }
            }
        }


        public int Dispatchs
        {
            get;
            set;
        }


        public int Count
        {
            get { return mClients.Count; }
        }


        public int GetSendQueues()
        {
            return mSendDispatch.Count;
        }

        public int GetReceiveQuues()
        {
            return mReceiveDispatch.Count;
        }
    }
}

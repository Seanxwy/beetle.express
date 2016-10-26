using System;
using System.Collections.Generic;

using System.Text;

namespace Beetle.Express
{

    class TcpServer :IServer
    {
        public TcpServer()
        {
            Dispatchs = 3;
        }

        private string mHost = null;

        private int mPort = 8088;


        private System.Net.Sockets.Socket mSocket;


        private int mSendBufferSize = 1024 * 4;


        private int mReceiveDataPoolSize = 1024;


        private int mReceiveBufferSize = 1024 * 4;


        internal ReceiveDataPool mReceiveDataPool;


        private Dictionary<string, IChannel> mClients = new Dictionary<string, IChannel>(5000);


        private SendDispatch mSendDispatch;

        private IList<ReceiveDispatch> ReceiveDispatchs = new List<ReceiveDispatch>();

        private bool mIsDisposed = false;

        private ChannelConnectEventArgs mConnectEA = new ChannelConnectEventArgs();

        public int Dispatchs
        {
            get;
            set;
        }

        private void OnInit()
        {
            try
            {
                mReceiveDataPool = new ReceiveDataPool(ReceiveDataPoolSize, ReceiveBufferSize);

                for (int i = 0; i < Dispatchs; i++)
                {
                    ReceiveDispatch rd = new ReceiveDispatch();
                    rd.Run();
                    ReceiveDispatchs.Add(rd);
                }
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

                mSocket = new System.Net.Sockets.Socket(System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);
                System.Net.IPEndPoint point = new System.Net.IPEndPoint(System.Net.IPAddress.Any, mPort);
                if (!string.IsNullOrEmpty(Host))
                {
                    point = new System.Net.IPEndPoint(System.Net.IPAddress.Parse(Host), Port);

                }
                mSocket.Bind(point);
                mSocket.Listen(100);
                System.Threading.ThreadPool.QueueUserWorkItem(OnAccept);
            }
            catch (Exception e_)
            {
                OnChannelError(this, new ErrorEventArgs { Error = e_, Tag = "Server Listen" });
            }
        }

        private void OnAccept(object state)
        {
            while (!mIsDisposed)
            {
                try
                {
                    System.Net.Sockets.Socket asocket = mSocket.Accept();
                    AddClient(asocket);
                }
                catch (System.Net.Sockets.SocketException se)
                {
                    OnChannelError(this, new ErrorEventArgs { Error = se, Tag = "Server Accept" });
                    Dispose();
                    break;
                }
                catch (Exception e)
                {
                    OnChannelError(this, new ErrorEventArgs { Error = e, Tag = "Server Accept" });
                }
            }
        }

        private void AddClient(System.Net.Sockets.Socket socket)
        {
            Channel channel = new Channel(this, socket);
            OnConnected(channel);
            if (mConnectEA.Cancel)
            {
                channel.Dispose();
            }
            else
            {
               
                channel.ChannelDisposed = OnChannelDispose;
                channel.ChannelError = OnChannelError;
                channel.ChannelReceive = OnChannelReceive;
                channel.ChannelSend = OnChannelSend;
                lock (mClients)
                {
                    mClients.Add(channel.ID, channel);
                    channel.ReceiveDispatch = ReceiveDispatchs[mClients.Count % ReceiveDispatchs.Count];
                    Version++;
                }
                channel.BeginReceive();
            }

        }

        private void RemoveClient(IChannel channel)
        {
            lock (mClients)
            {
                mClients.Remove(channel.ID);
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
            Dispatchs = listen.Dispatchs;
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

            ((Channel)channel).AddSendData(data);
        }

        public void InvokeSend(IChannel channel)
        {
            mSendDispatch.Add((Channel)channel);
        }

        public void InvokeReceive(IReceiveData rd)
        {
            ReceiveData data = (ReceiveData)rd;
            Channel channel = (Channel)data.Channel;
            channel.ReceiveDispatch.Add(data);
         
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
                        foreach (ReceiveDispatch item in ReceiveDispatchs)
                        {
                            item.Dispose();
                        }
                       // mReceiveDispatch.Dispose();
                        DisposeSocket(mSocket);
                    }
                    catch (Exception e_)
                    {
                        OnChannelError(this, new ErrorEventArgs { Error = e_, Tag = "Server disposed" });
                    }
                }
            }
        }

        internal static void DisposeSocket(System.Net.Sockets.Socket socket)
        {
            try
            {
                socket.Shutdown(System.Net.Sockets.SocketShutdown.Both);
            }
            catch
            {
            }
            try
            {
                socket.Close();

            }
            catch
            {
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
                mClients.Values.CopyTo(array,0);
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
        public int Count
        {
            get
            {
                return mClients.Count;
            }
        }


        public int GetSendQueues()
        {
            return mSendDispatch.Count;
        }

        public int GetReceiveQuues()
        {
            int result=0;
            for (int i = 0; i < ReceiveDispatchs.Count; i++)
            {
                result += ReceiveDispatchs[i].Count;
            }
            return result;
            
        }
    }
}

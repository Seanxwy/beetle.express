using System;
using System.Collections.Generic;

using System.Text;

namespace Beetle.Express
{

    public  class ServerFactory:IDisposable
    {

        public ServerFactory(string config)
        {

            ServerSection section = (ServerSection)System.Configuration.ConfigurationManager.GetSection(config);
            Init(section.Listens);
        }

        private IList<IServer> mServers = new List<IServer>();


        public IList<IServer> Servers
        {
            get
            {
                return mServers;
            }
        }

        private void Init(ListenCollection listens)
        {
            IServer server;
            foreach (Listen item in listens)
            {
                if (item.Type == "TCP")
                {
                    server = new TcpServer();
                }
                else
                {
                    server = new UdpServer();
                }
                mServers.Add(server);
                server.Open(item);
            }
        }

        public IServer CreateTCP()
        {
            return new TcpServer();
        }
        public IServer CreateUDP()
        {
            return new UdpServer();
        }

        private bool mIsDisposed = false;

        public void Dispose()
        {
            lock (this)
            {
                if (!mIsDisposed)
                {
                    mIsDisposed = true;
                    foreach (IServer item in mServers)
                    {
                        item.Dispose();
                    }
                    mServers.Clear();
                }
            }
        }
    }
}

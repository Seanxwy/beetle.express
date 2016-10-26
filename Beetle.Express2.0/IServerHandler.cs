using System;
using System.Collections.Generic;

using System.Text;

namespace Beetle.Express
{

    public interface IServerHandler
    {

        void Connect(IServer server, ChannelConnectEventArgs e);

        void Disposed(IServer server, ChannelEventArgs e);

        void Error(IServer server, ErrorEventArgs e);

        void Receive(IServer server, ChannelReceiveEventArgs e);

        void SendCompleted(IServer server, ChannelSendEventArgs e);

       
    }
}

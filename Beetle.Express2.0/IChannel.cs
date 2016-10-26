using System;
using System.Collections.Generic;

using System.Text;

namespace Beetle.Express
{
 
    public interface IChannel : IDisposable
    {

        ChannelStatus Status
        {
            get;
            set;
        }

        string ID { get; }


        string Name { get; set; }


        object this[string name] { get; set; }


        IServer Server { get; }

 
        bool IsDisposed { get; }


        System.Net.EndPoint EndPoint
        {
            get;
        }


        System.Net.Sockets.Socket Socket
        { get; }


        void InvokeSend();


        void InvokeError(Exception e);


        void InvokeReceive(IReceiveData data);

        string ChannelType
        {
            get;

        }

        object Tag
        {
            get;
            set;
        }
 
        IPackage Package
        {
            get;
            set;
        }

        string CloseStatus
        {
            get;
            set;
        }
       
    }

}

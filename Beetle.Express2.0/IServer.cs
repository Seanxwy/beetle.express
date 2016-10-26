using System;
using System.Collections.Generic;

using System.Text;

namespace Beetle.Express
{

    public interface IServer : IDisposable
    {

        void Send(object data, params IChannel[] channels);

        void Send(object data, IList<IChannel> channels);

        IServerHandler Handler
        {
            get;
            set;
        }
        int Dispatchs
        {
            get;
            set;
        }

        void InvokeReceive(IReceiveData rd);
        void InvokeSend(IChannel channel);

        void Open();
        void Open(Listen listen);

        int Count
        {
            get;
        }


        string Name
        {
            get;
        }


        IReceiveData PopData();

        int Port
        {
            get;
            set;
        }


        string Host
        {
            get;
            set;
        }


        int SendBufferSize
        {
            get;
            set;
        }


        int ReceiveBufferSize
        {
            get;
            set;
        }


        int ReceiveDataPoolSize
        {
            get;
            set;
        }

        int Version { get; set; }

        IChannel[] GetOnlines();

        ArraySegment<IChannel> GetOnlines(IChannel[] array);


        IChannel GetChannel(string id);


        void GetOnlines(OnlineSegment segment);

        int GetSendQueues();

        int GetReceiveQuues();

    }
}

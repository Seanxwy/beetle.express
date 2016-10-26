using System;
using System.Collections.Generic;
using System.Text;

namespace Beetle.Express
{

    public interface IPackage
    {
 
        IData GetMessageData(object message);

    
        object GetMessage(System.IO.Stream stream);
  
        IChannel Channel
        {
            get;
            set;
        }
 
        EventPackageReceive Receive
        {
            get;
            set;
        }

        void Import(byte[] data, int start, int count);
 
        void Recover(IData data);
    }

   
    public delegate void EventPackageReceive(object sender,PackageReceiveArgs e);

  
    public class PackageReceiveArgs:EventArgs
    {
      
        public IChannel Channel
        {
            get;set;
        }

        public Object Message
        {
            get;
            set;
        }
    }

}

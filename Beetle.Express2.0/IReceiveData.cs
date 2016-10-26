using System;
using System.Collections.Generic;

using System.Text;

namespace Beetle.Express
{
 
    public interface IReceiveData : IDisposable
    {

        byte[] Array { get; }

        int Offset { get; }

        int Count { get; }

        string ToString(Encoding coding);
    }

}

using System;
using System.Collections.Generic;

using System.Text;

namespace Beetle.Express
{

    public interface IData
    {

        byte[] Array { get; set; }
     
        int Offset { get; set; }

        int Count { get; set; }

        object Tag { get; set; }

        int Counter { get; }

        void ExChange(int value);
      

        void Decrement();

        bool UsePool
        {
            get;
            set;
        }
    }
}

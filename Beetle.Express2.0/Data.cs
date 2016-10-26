using System;
using System.Collections.Generic;

using System.Text;

namespace Beetle.Express
{

    public class Data : IData
    {

        public Data()
        {
            UsePool = false;
        }

        public Data(byte[] array, int length)
        {
            Array = array;
            Count = length;
        }

        public Data(int length)
        {
            Array = new byte[length];
        }

        public byte[] Array
        {
            get;
            set;
        }

        private int mCounter;

       
        public int Offset
        {
            get;
            set;
        }

        public int Count
        {
            get;
            set;
        }

        public object Tag
        {
            get;
            set;
        }

        public void Write(byte[] data, int offset, int count)
        {
            Buffer.BlockCopy(data, offset, Array, 0, count);
            Offset = 0;
            Count = count;
        }

        public void Write(byte[] data)
        {
            Buffer.BlockCopy(data, 0, Array, 0, data.Length);
            Offset = 0;
            Count = data.Length;
        }

        public void Write(string value, Encoding coding)
        {
            Write(coding.GetBytes(value));
        }

        public void Write(System.IO.Stream stream)
        {
            Count = stream.Read(Array, 0, Array.Length);
            
        }

        public void Write(byte[] data, int count)
        {
            Buffer.BlockCopy(data, 0, Array, 0, count);
            Offset = 0;
            Count = count;
        }

        public void SetBuffer(byte[] data, int offset, int count)
        {
            Array = data;
            Offset = offset;
            Count = count;
        }
  
        public void SetBuffer(byte[] data, int count)
        {
            SetBuffer(data, 0, count);
        }

        public int Counter
        {
            get
            {
                return mCounter;
            }
        }

        public void ExChange(int value)
        {
            mCounter = value;
        }

        public void Decrement()
        {
            System.Threading.Interlocked.Decrement(ref mCounter);
        }

        public bool UsePool
        {
            get;
            set;
        }
    }
}


using System;
using System.Collections.Generic;

using System.Text;

namespace Beetle.Express
{

    class ReceiveDataPool : IDisposable
    {
        private Stack<ReceiveData> mDatas = null;

        private bool mIsDisposed = false;

        private int mDataLength;

        private ReceiveData createData()
        {
            ReceiveData rd = new ReceiveData(mDataLength);
            rd.Pool = this;
            return rd;
        }

        public ReceiveDataPool(int count, int dataLength)
        {
            mDatas = new Stack<ReceiveData>(count);
            mDataLength = dataLength;
            for (int i = 0; i < count; i++)
            {
                mDatas.Push(createData());
            }
        }

        public ReceiveData Pop()
        {
            lock (this)
            {
                if (mDatas.Count > 0)
                    return mDatas.Pop();
                return createData();
            }
        }

        public void Push(ReceiveData e)
        {
            lock (this)
            {

                mDatas.Push(e);
            }


        }

        protected virtual void OnDispose()
        {

            while (mDatas.Count > 0)
            {
                ReceiveData rd = mDatas.Pop();
                rd.Dispose();
            }
        }

        public void Dispose()
        {
            lock (this)
            {
                if (!mIsDisposed)
                {

                    mIsDisposed = true;
                    OnDispose();
                }
            }
        }
    }
}

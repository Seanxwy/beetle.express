using System;
using System.Collections.Generic;

using System.Text;

namespace Beetle.Express
{
    class ReceiveDispatch : IDisposable
    {
        private Queue<ReceiveData> mDatas = new Queue<ReceiveData>(1024);

        private bool mIsDisposed = false;

        public void Add(ReceiveData item)
        {
            lock (this)
            {
                mDatas.Enqueue(item);
            }
        }
        public int Count
        {
            get
            {
                return mDatas.Count;
            }
        }
        private ReceiveData Get()
        {
            lock (this)
            {
                if (mDatas.Count > 0)
                    return mDatas.Dequeue();
                return null;
            }
        }

        public void Run()
        {
            System.Threading.ThreadPool.QueueUserWorkItem(OnRun);
        }

        private void OnRun(object state)
        {
            while (!mIsDisposed)
            {
                ReceiveData data = Get();
                if (data != null)
                {
                    IChannel channel = data.Channel;
                    try
                    {
                        channel.InvokeReceive(data);
                    }
                    catch (Exception e_)
                    {
                        channel.InvokeError(e_);
                    }
                    finally
                    {
                        data.Exit();
                    }

                }
                else
                    System.Threading.Thread.Sleep(1);
            }
        }

        public void Dispose()
        {
            lock (this)
            {
                if (!mIsDisposed)
                {
                    mIsDisposed = true;
                    mDatas.Clear();
                }
            }
        }
    }
}

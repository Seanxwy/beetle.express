using System;
using System.Collections.Generic;

using System.Text;

namespace Beetle.Express
{
    class SendDispatch : IDisposable
    {
        private Queue<IChannel> mDatas = new Queue<IChannel>(1024);

        private bool mIsDisposed = false;

        public void Add(IChannel item)
        {
            lock (this)
            {
                mDatas.Enqueue(item);
            }
        }

        private IChannel Get()
        {
            lock (this)
            {
                if (mDatas.Count > 0)
                    return mDatas.Dequeue();
                return null;
            }
        }
        public int Count
        {
            get
            {
                return mDatas.Count;
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
                IChannel channel = Get();
                if (channel != null)
                {                   
                    try
                    {
                        channel.InvokeSend();
                    }
                    catch (Exception e_)
                    {
                        channel.InvokeError(e_);
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

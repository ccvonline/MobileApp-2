using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

namespace Rock.Mobile.Threading
{
    /// <summary>
    /// Usage: On a worker thread, processes any IJobRequest added via PushRequest.
    /// This allows serialized background work. Ideal for file loads, or any kind of cpu
    /// intense operation.
    /// 
    /// Simply implement the IJobRequest interface on whatever job class you want executed.
    /// </summary>
    public class AsyncTaskScheduler
    {
        public interface IJobRequest
        {
            void ProcessRequest( );
        }

        static AsyncTaskScheduler _Instance = new AsyncTaskScheduler( );
        public static AsyncTaskScheduler Instance { get { return _Instance; } }

        /// The queue of job requests that need to be executed
        /// </summary>
        ConcurrentQueue<IJobRequest> RequestQueue { get; set; }

        /// <summary>
        /// Pointer to the worker thread
        /// </summary>
        System.Threading.Thread JobThreadHandle { get; set; }

        /// <summary>
        /// Simply lets the worker thread sleep while the queue is empty.
        /// </summary>
        EventWaitHandle QueueProcessHandle { get; set; }

        public AsyncTaskScheduler( )
        {
            // create our queue and fire up the worker thread
            RequestQueue = new ConcurrentQueue<IJobRequest>( );

            JobThreadHandle = new System.Threading.Thread( ThreadProc );
            JobThreadHandle.Start( );

            QueueProcessHandle = new EventWaitHandle( false, EventResetMode.AutoReset );
        }

        /// <summary>
        /// The one entry point, this is where requests should be sent
        /// </summary>
        public void AddJob( IJobRequest jobRequestObj )
        {
            // queue the object and notify the threadProc that there's work to do
            RequestQueue.Enqueue( jobRequestObj );

            QueueProcessHandle.Set( );
        }

        void ThreadProc( )
        {
            while( true )
            {
                QueueProcessHandle.WaitOne( );

                // while there are requests pending, process them
                while ( RequestQueue.IsEmpty == false )
                {
                    // get the web request out of the queue
                    IJobRequest requestObj = null;
                    RequestQueue.TryDequeue( out requestObj ); //yank it out

                    if( requestObj != null )
                    {
                        // execute it
                        requestObj.ProcessRequest( );
                    }
                }
            }
        }
    }
}

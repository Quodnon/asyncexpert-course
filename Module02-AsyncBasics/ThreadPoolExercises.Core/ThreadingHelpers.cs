using System;
using System.Threading;

namespace ThreadPoolExercises.Core
{
    public class ThreadingHelpers
    {
        public static void ExecuteOnThread(Action action, int repeats, CancellationToken token = default, Action<Exception>? errorAction = null)
        {
            // * Create a thread and execute there `action` given number of `repeats` - waiting for the execution!
            //   HINT: you may use `Join` to wait until created Thread finishes
            // * In a loop, check whether `token` is not cancelled
            // * If an `action` throws and exception (or token has been cancelled) - `errorAction` should be invoked (if provided)
            var thread = new Thread(ThreadStart) {IsBackground = true};
            thread.Start();
            thread.Join();

            void ThreadStart()
            {
                try
                {
                    for (var i = 0; i < repeats; i++)
                    {
                        token.ThrowIfCancellationRequested();
                        action();
                    }
                }
                catch (Exception e)
                {
                    errorAction?.Invoke(e);
                }
            }


        }

        public static void ExecuteOnThreadPool(Action action, int repeats, CancellationToken token = default, Action<Exception>? errorAction = null)
        {
            // * Queue work item to a thread pool that executes `action` given number of `repeats` - waiting for the execution!
            //   HINT: you may use `AutoResetEvent` to wait until the queued work item finishes
            // * In a loop, check whether `token` is not cancelled
            // * If an `action` throws and exception (or token has been cancelled) - `errorAction` should be invoked (if provided)
            
            var autoResetEvent = new AutoResetEvent(false);
            
            var args = Tuple.Create(action, repeats, token, errorAction, autoResetEvent);
            
            ThreadPool.QueueUserWorkItem(state => ThreadPoolWaitCallback(state!), args);
            autoResetEvent.WaitOne();


        }
        static void ThreadPoolWaitCallback(object args)
        {
            var (action, repeats, token, errorAction, autoResetEvent) =
                (Tuple<Action, int, CancellationToken, Action<Exception>?, AutoResetEvent>) args;

            try
            {
                for (var i = 0; i < repeats; i++)
                {
                    token.ThrowIfCancellationRequested();
                    action();
                }
            }
            catch (Exception e)
            {
                errorAction?.Invoke(e);
            }
            finally
            {
                autoResetEvent?.Set();
            }
        }
    }
}

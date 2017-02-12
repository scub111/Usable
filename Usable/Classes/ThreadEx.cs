using System;
using System.Threading;
using System.Threading.Tasks;

namespace Usable
{
    public class ThreadEx
    {
        static void ActualMethodWrapper(Action method, Action callBackMethod)
        {
            try
            {
                method.Invoke();
            }
            catch (ThreadAbortException)
            {
                //Console.WriteLine("Method aborted early");
            }
            finally
            {
                if (callBackMethod != null)
                    callBackMethod.Invoke();
            }
        }

        public static void CallTimedOutMethodAsync(Action method, int milliseconds, Action callBackMethod = null)
        {
            new Thread(new ThreadStart(() =>
            {
                Thread actionThread = new Thread(new ThreadStart(() =>
                {
                    ActualMethodWrapper(method, callBackMethod);
                }));

                actionThread.Start();
                Thread.Sleep(milliseconds);
                if (actionThread.IsAlive)
                {
                    actionThread.Abort();
                }
            })).Start();
        }

        public static bool CallTimedOutMethodSync(Action method, int milliseconds)
        {
            bool isGood = true;
            var tokenSource = new CancellationTokenSource();
            CancellationToken token = tokenSource.Token;

            var task = Task.Factory.StartNew(method);

            if (!task.Wait(milliseconds, token))
                isGood = false;

            return isGood;
        }
    }
}

using System;
using System.Threading;
using System.Threading.Tasks;

namespace RealtimeFramework.Messaging.Ext
{
    public delegate void TimerCallback(object state);

    public sealed class Timer
    {
        /// <summary>
        /// Timer Param
        /// </summary>
        public object State { get; protected set; }
        /// <summary>
        /// Delay
        /// </summary>
        public int Period { get; protected set; }
        /// <summary>
        /// Is Started
        /// </summary>
        public bool IsRunning { get; protected set; }

        private bool _repeat = true;
        /// <summary>
        /// Repeat
        /// </summary>
        public bool Repeat
        {
            get { return _repeat; }
            set { _repeat = value; }
        }


        TimerCallback Callback { get; set; }
        TimerInternal Internal;

        public Timer(TimerCallback callback, int period, object state = null)
        {
            Callback = callback;
            State = state;
            Period = period;
        }

        /// <summary>
        /// Starts the Timer
        /// </summary>
        public void Start()
        {
            if (IsRunning)
                Stop();

            Internal = new TimerInternal(HandlerInternal, State, Period);
            IsRunning = true;
        }

        /// <summary>
        /// Stops the timer
        /// </summary>
        public void Stop()
        {
            if (Internal != null)
                Internal.Dispose();
            Internal = null;
            IsRunning = false;
        }

        void HandlerInternal(object state)
        {
            if (IsRunning)
                Callback(state);

            if (Repeat && IsRunning)
            {
                Internal = new TimerInternal(HandlerInternal, State, Period);
            }
            else
            {
                IsRunning = false;
                if (Internal != null)
                    Internal.Dispose();
                Internal = null;
            }
        }
    }

    internal sealed class TimerInternal : CancellationTokenSource
    {

        internal TimerInternal(TimerCallback callback, object state, int period)
        {
            Task.Delay(period, Token).ContinueWith((t, s) =>
                {
                    var tuple = (Tuple<TimerCallback, object>)s;
                    tuple.Item1(tuple.Item2);
                }, Tuple.Create(callback, state), CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnRanToCompletion,
                TaskScheduler.Default);
        }

        protected override void Dispose(bool disposing)
        {
            Cancel();
            base.Dispose(disposing);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Threading;

namespace JiME.Views
{
    public static class WpfUtils
    {
        /// <summary>
        /// This needs to be set before using DeferExecution e.g. from a WPF Window constructor that will call it. 
        /// Can be overwritten safely as long as always overwritten with the MAIN thread dispatcher so calls go back to UI thread.
        /// </summary>
        public static Dispatcher MainThreadDispatcher { get; set; }

        private static Dictionary<string, Timer> m_deferredTimers = new Dictionary<string, Timer>();

        /// <summary>
        /// Defers execution based on a string identifier for some milliseconds before executing it in the MainThreadDispatcher.
        /// Time resets whenever this is called with the same identifier -- useful if we need to make sure that certain action
        /// happens just once even if it is triggered multiple times in a short period of time.
        /// </summary>
        public static void DeferExecution(string identifier, int milliseconds, Action action)
        {
            if (!m_deferredTimers.ContainsKey(identifier))
            {
                // The timer wasn't running for this identifier
                var timer = new System.Timers.Timer();
                m_deferredTimers.Add(identifier, timer);
                timer.AutoReset = false;
                timer.Interval = milliseconds;
                timer.Elapsed += (object sender, ElapsedEventArgs e) =>
                {
                    m_deferredTimers.Remove(identifier);
                    MainThreadDispatcher.BeginInvoke(action);
                };
                timer.Start();
            }
            else
            {
                // Timer was already running --> restart it
                var timer = m_deferredTimers[identifier];
                timer.Stop();
                timer.Start();
            }
        }
    }
}

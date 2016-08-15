using System.Collections;
using System.Threading;

namespace UnitySpeechToText.Utilities
{
    /// <summary>
    /// Base class for a job that runs on its own thread.
    /// This is derived from a ThreadedJob script which can be found here: 
    /// http://answers.unity3d.com/questions/357033/unity3d-and-c-coroutines-vs-threading.html
    /// </summary>
    public abstract class ThreadedJob
    {
        /// <summary>
        /// Thread on which the conversion process runs
        /// </summary>
        Thread m_Thread;
        /// <summary>
        /// Whether the conversion process is done
        /// </summary>
        bool m_IsDone;

        /// <summary>
        /// Creates and starts the thread on which the conversion process runs.
        /// </summary>
        public void Start()
        {
            m_Thread = new Thread(Run);
            m_Thread.Start();
        }

        /// <summary>
        /// Waits for the conversion process to finish.
        /// </summary>
        public IEnumerator WaitFor()
        {
            while (!m_IsDone)
            {
                yield return null;
            }
        }

        /// <summary>
        /// Runs the job.
        /// </summary>
        void Run()
        {
            ThreadFunction();
            m_IsDone = true;
        }

        /// <summary>
        /// Specific thread function to run.
        /// </summary>
        protected abstract void ThreadFunction();
    }
}

using FiveDChessDataInterface.MemoryHelpers;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace FiveDChessDataInterface.Util
{
    internal class SuspendGameProcessLock
    {
        private readonly object lockObject = new object();
        private readonly IntPtr gameHandle;
        private int lockCnt = 0;

        internal SuspendGameProcessLock(IntPtr gameHandle)
        {
            this.gameHandle = gameHandle;
        }

        /// <summary>
        /// Acquires a lock, and suspends the game process while this lock is held.
        /// Threadsafe.
        /// </summary>
        /// <param name="a">The inner action to be executed, while the process is suspended.</param>
        [DebuggerHidden]
        internal void Lock(Action a)
        {
            lock (this.lockObject)
            {
                this.lockCnt++;
                try
                {
                    if (this.lockCnt == 1) // only suspend if this is the first time the lock has been acquired recursively
                    {
                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        {
                            KernelMethods.NtSuspendProcess(this.gameHandle);
                        }
                        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                        {
                            KernelMethods.kill(this.gameHandle.ToInt32(), 19 /* SIGSTOP */);
                        }
                    }

                    a.Invoke();
                }
                finally
                {
                    if (this.lockCnt == 1)  // only resume if this is the first time the lock has been acquired recursively
                    {
                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        {
                            KernelMethods.NtResumeProcess(this.gameHandle);
                        }
                        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                        {
                            KernelMethods.kill(this.gameHandle.ToInt32(), 18 /* SIGCONT */);
                        }
                    }

                    this.lockCnt--;
                }
            }
        }
    }
}

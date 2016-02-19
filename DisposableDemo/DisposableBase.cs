using System;
using System.Threading;

namespace DisposableDemo
{
    /// <summary>
    /// Provides a base class for thread-safe disposible.
    /// </summary>
    public abstract class DisposableBase
        : IDisposable
    {
        private volatile bool _IsDisposeExactlyInvoked = false;
        /// <summary>
        /// Determine if the Dispose method is explicitly invoked.
        /// </summary>
        protected bool IsDisposeExactlyInvoked => _IsDisposeExactlyInvoked;

        private volatile int _IsDisposeTriggered = 0;
        /// <summary>
        /// Determine if the disposing process is triggered by any means.
        /// </summary>
        protected bool IsDisposeTriggered => _IsDisposeTriggered != 0;

        private volatile bool _IsDisposed = false;
        /// <summary>
        /// Determine if the object is already disposed.
        /// </summary>
        public bool IsDisposed => _IsDisposed;

        /// <summary>
        /// The method which implements the actual disposing logic.
        /// </summary>
        protected abstract void Dispose(bool disposing);

        private void FireDispose(bool disposing)
        {
            if (disposing)
                _IsDisposeExactlyInvoked = true;

#pragma warning disable 0420
            if (Interlocked.Exchange(ref _IsDisposeTriggered, 1) != 0)
                return;
#pragma warning restore 0420

            Dispose(disposing);

            _IsDisposed = true;

            if (disposing)
                GC.SuppressFinalize(this);
        }

        protected void Dispose()
            => FireDispose(true);

        void IDisposable.Dispose()
            => Dispose();

#pragma warning disable 1591
        ~DisposableBase()
        {
            FireDispose(false);
        }
    }
}
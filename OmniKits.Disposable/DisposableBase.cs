using System;
using System.Collections.Generic;
using System.Threading;

namespace OmniKits
{
    /// <summary>
    /// Provides a base class for thread-safe disposible.
    /// </summary>
    public abstract class DisposableBase
        : IDisposable
    {
        ReaderWriterLockZone _LockZone;
        private bool? _UseReaderWriterLock;
        /// <summary>
        /// Determine if the object is using a form of lock to guarantee clean disposing.
        /// </summary>
        protected bool UseLockForDisposing => _UseReaderWriterLock.HasValue;

        public DisposableBase(bool? useReaderWriterLockForDisposing)
        {
            _UseReaderWriterLock = useReaderWriterLockForDisposing;
            if (UseLockForDisposing)
                _LockZone = ReaderWriterLockZone.Spawn();
        }
        public DisposableBase()
            : this(null)
        { }

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

        private LinkedList<IDisposable> _DisposableList = null;
        private void AlsoDisposeCore(IDisposable target)
        {
            if (_DisposableList == null)
                _DisposableList = new LinkedList<IDisposable>();
            _DisposableList.AddFirst(target);
        }
        /// <summary>
        /// Add a depended IDisposable object to FILO recursive disposing list.
        /// </summary>
        protected void AlsoDispose(IDisposable target)
        {
            if (!UseLockForDisposing)
                AlsoDisposeCore(target);
            else
            {
                using (_LockZone.WriterLocking())
                    AlsoDisposeCore(target);
            }
        }

        private void RecursiveDispose(bool disposing)
        {
            if (_DisposableList != null)
            {
                foreach (var item in _DisposableList)
                    item.Dispose();
            }
            Dispose(disposing);
        }

        private void FireDispose(bool disposing)
        {
            if (disposing)
                _IsDisposeExactlyInvoked = true;

#pragma warning disable 0420
            if (Interlocked.Exchange(ref _IsDisposeTriggered, 1) != 0)
                return;
#pragma warning restore 0420

            if (!UseLockForDisposing)
                RecursiveDispose(disposing);
            else
            {
                using (_LockZone.WriterLocking())
                    RecursiveDispose(disposing);
            }

            _IsDisposed = true;

            if (disposing)
                GC.SuppressFinalize(this);
        }

        /// <summary>
        /// The base Dispose() to call.
        /// </summary>
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
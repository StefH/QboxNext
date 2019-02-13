using System;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace QboxNext.Storage.Qbx
{
    /// <summary>
    /// This is a thread-safe wrapper around a FileStream.  While it is not a Stream itself, it can be cast to
    /// one (keep in mind that this might throw an exception).
    /// The SafeFileStream will create a mutex in Global to signal the opening of a specific file path for writing.
    /// This will allow other file streams to wait for the mutex to unlock before opening the same file. 
    /// </summary>
    internal class SafeFileStream : IDisposable
    {
        #region Private Members
        private Stream _mStream;
        private readonly ILogger<SafeFileStream> _logger;
        private readonly string _mPath;
        private readonly FileMode _mFileMode;
        private readonly FileAccess _mFileAccess;
        private readonly FileShare _mFileShare;
        #endregion//Private Members

        #region Constructors

        /// <summary>
        /// Constructor for the SaveFileStream creates the resources the stream depends upon
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="path">The path including the file name for the file to open</param>
        /// <param name="mode">The mode for the file open.</param>
        /// <param name="access">The required access type</param>
        /// <param name="share">The type of share that is allowed between streams opening the same file</param>
        public SafeFileStream(ILogger<SafeFileStream> logger, string path, FileMode mode, FileAccess access, FileShare share)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mPath = path;
            _mFileMode = mode;
            _mFileAccess = access;
            _mFileShare = share;
        }

        #endregion//Constructors

        #region Properties

        /// <summary>
        /// The underlying stream for this wrapper. This helps in enabling casting directly to a stream.
        /// </summary>
        public Stream UnderlyingStream
        {
            get
            {
                if (!IsOpen)
                    throw new InvalidOperationException("The underlying stream does not exist - try opening this stream.");
                return _mStream;
            }
        }

        /// <summary>
        /// Returns true if the stream is created and therefore active 
        /// </summary>
        public bool IsOpen
        {
            get { return _mStream != null; }
        }
        #endregion//Properties

        #region Functions
        /// <summary>
        /// Opens the stream when it is not locked.  If the file is locked, then will wait for it to unlock.
        /// </summary>
        public void Open()
        {
            if (_mStream != null)
            {
                throw new InvalidOperationException();
            }
            do
            {
                try
                {
                    _mStream = System.IO.File.Open(_mPath, _mFileMode, _mFileAccess, _mFileShare);
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"Can't open file: {ex.Message}\nWill retry after one second.");
                    Thread.Sleep(1000);
                }
            }
            while (true);
        }

        /// <summary>
        /// Try Open will try to open the stream and will wait for a lock to unlock for the given time span.
        /// The procedure catches any exceptions and logs it to allow procedural flow in the calling class 
        /// to continue (using if statement).
        /// </summary>
        /// <param name="span">The period of time to wait for the file to unlock</param>
        /// <returns>True if the file was opened and a stream is attached. Otherwise returns false</returns>
        public bool TryOpen(TimeSpan span)
        {
            if (_mStream != null)
            {
                throw new InvalidOperationException();
            }

            DateTime deadline = DateTime.Now + span;
            do
            {
                try
                {
                    _mStream = System.IO.File.Open(_mPath, _mFileMode, _mFileAccess, _mFileShare);
                    return true;
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, $"Can't open file {_mPath}: {e.Message}\nWill retry after one second.");
                    Thread.Sleep(1000);
                }
            }
            while (DateTime.Now < deadline);

            _logger.LogWarning($"Could not open file {_mPath} for {_mFileMode}/{_mFileAccess}/{_mFileShare} after {span.TotalMilliseconds} milliseconds");
            return false;
        }

        /// <summary>
        /// Part of the IDisposable pattern to release resources
        /// </summary>
        /// <param name="disposing">True if the call to Close was made from Dispose</param>
        public void Close(bool disposing)
        {
            if (_mStream != null)
            {
                _mStream.Close();
                _mStream = null;
            }
        }

        /// <summary>
        /// Implementation for the IDisposable interface
        /// It will close the file and release the mutex by calling close and suppress the finalizer
        /// to be run from the Garbage collection.
        /// </summary>
        public void Dispose()
        {
            Close(true);
            GC.SuppressFinalize(this);
        }

        ~SafeFileStream()
        {
            Close(false);
        }

        /// <summary>
        /// Enables the direct casting of this wrapper to a stream instance
        /// </summary>
        /// <param name="sfs"></param>
        /// <returns></returns>
        public static explicit operator Stream(SafeFileStream sfs)
        {
            return sfs.UnderlyingStream;
        }

        #endregion//Functions
    }
}

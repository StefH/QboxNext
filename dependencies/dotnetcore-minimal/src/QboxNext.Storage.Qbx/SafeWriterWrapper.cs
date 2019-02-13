using System;
using System.IO;
using Microsoft.Extensions.Logging;

namespace QboxNext.Storage.Qbx
{
    /// <summary>
    /// Convenience class that wraps the SafeFileStream for use in this particular class
    /// Builds the correct file and binary writer combination from the FilePath only (see ctor)
    /// </summary>
    internal class SafeWriterWrapper : IDisposable
    {
        private readonly ILogger<SafeWriterWrapper> _logger;

        protected SafeFileStream Stream { get; set; }

        /// <summary>
        /// The writer used from the Storage Provider
        /// </summary>
        public BinaryWriter BinaryWriter { get; set; }

        /// <summary>
        /// Contructor for the class to assemble all elements needed from the file path only.
        /// </summary>
        /// <param name="loggerFactory"></param>
        /// <param name="filePath"></param>
        public SafeWriterWrapper(ILoggerFactory loggerFactory, string filePath)
        {
            _logger = loggerFactory?.CreateLogger<SafeWriterWrapper>() ?? throw new ArgumentNullException(nameof(loggerFactory));
            _logger.LogTrace("Enter");

            Stream = new SafeFileStream(loggerFactory.CreateLogger<SafeFileStream>(), filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
            if (!Stream.TryOpen(new TimeSpan(30000)))
                _logger.LogError("Cannot open file: {0}", filePath);
            BinaryWriter = new BinaryWriter(Stream.UnderlyingStream);

            _logger.LogTrace("Exit");
        }

        #region IDisposable

        /// <summary>
        /// Dispoze implementation for the IDisposable pattern and interface.
        /// Calls Dispose to signal that the class and it's resources are no longer required.
        /// Then suppresses the finalizer in the garbage collector.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes of the resources that are no longer needed.
        /// </summary>
        /// <param name="disposing"></param>
        public void Dispose(bool disposing)
        {
            _logger.LogTrace("Enter");

            if (BinaryWriter != null)
                BinaryWriter.Dispose();
            if (Stream != null)
                Stream.Dispose();

            _logger.LogTrace("Exit");
        }

        /// <summary>
        /// If for any reason the dispose was not called we will handle this in the finalizer.
        /// The finalizer is called by the garbage collector but we do not know when and we do not
        /// know in which order. So some of the resources could be finalized already.
        /// </summary>
        ~SafeWriterWrapper()
        {
            Dispose(false);
        }

        #endregion
    }
}
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Qboxes.Classes;
using QboxNext.Core.Utils;
using QboxNext.Extensions.Interfaces.Public;
using System;

namespace QboxNext.Extensions.Implementations
{
    internal class QboxNextDataHandlerFactory : IQboxNextDataHandlerFactory
    {
        private readonly IServiceProvider _provider;

        /// <summary>
        /// Initializes a new instance of the <see cref="QboxNextDataHandlerFactory"/> class.
        /// </summary>
        /// <param name="provider">The provider.</param>
        public QboxNextDataHandlerFactory([NotNull] IServiceProvider provider)
        {
            Guard.IsNotNull(provider, nameof(provider));

            _provider = provider;
        }

        /// <inheritdoc cref="IQboxNextDataHandlerFactory.Create(string, QboxDataDumpContext)"/>
        public IQboxNextDataHandler Create(string correlationId, QboxDataDumpContext context)
        {
            Guard.IsNotNullOrEmpty(correlationId, nameof(correlationId));
            Guard.IsNotNull(context, nameof(context));

            return ActivatorUtilities.CreateInstance<QboxNextDataHandler>(_provider, correlationId, context);
        }
    }
}
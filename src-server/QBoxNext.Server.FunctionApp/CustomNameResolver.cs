﻿using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using QboxNext.Server.Common.Validation;

namespace QBoxNext.Server.FunctionApp
{
    internal class CustomNameResolver : INameResolver
    {
        private readonly IConfigurationRoot _configuration;

        public CustomNameResolver(IConfigurationRoot configuration)
        {
            Guard.NotNull(configuration, nameof(configuration));
            _configuration = configuration;
        }

        public string Resolve(string name)
        {
            Guard.NotNull(name, nameof(name));

            string value = _configuration.GetValue<string>(name, null);
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException(
                    $"The key '{name}' in the configuration file does not exist or has a null or empty value.", name);
            }

            return value;
        }
    }
}
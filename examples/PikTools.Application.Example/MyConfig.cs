﻿namespace PikTools.Application.Example
{
    using System.IO;
    using Di;
    using Logs;
    using Microsoft.Extensions.Configuration;
    using SimpleInjector;

    /// <inheritdoc />
    public class MyConfig : IApplicationConfiguration
    {
        /// <inheritdoc />
        public void Configure(Container container)
        {
            container.Register<IService, Service>();

            container.AddConfiguration(builder => builder
                .SetBasePath(Path.GetDirectoryName(GetType().Assembly.Location))
                .AddJsonFile("application.settings.json")
                .Build());

            container.AddLogs();
        }
    }
}
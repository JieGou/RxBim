﻿namespace RxBim.Application.Menu.Fluent.Revit.Sample
{
    using Application.Revit;
    using Autodesk.Revit.UI;
    using Shared;

    /// <inheritdoc />
    public class App : RxBimApplication
    {
        /// <summary>
        /// Start application.
        /// </summary>
        public PluginResult Start()
        {
            TaskDialog.Show(GetType().Namespace, "App started");
            return PluginResult.Succeeded;
        }

        /// <summary>
        /// Shutdown application.
        /// </summary>
        public PluginResult Shutdown()
        {
            TaskDialog.Show(GetType().Namespace, "App finished");
            return PluginResult.Succeeded;
        }
    }
}
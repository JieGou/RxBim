﻿namespace RxBim.Application.Ui.Revit.Api.Models
{
    using System;
    using System.Linq;
    using Autodesk.Revit.UI;
    using Autodesk.Windows;
    using Di;
    using RxBim.Application.Ui.Api.Abstractions;
    using RxBim.Application.Ui.Api.Models;
    using UIFramework;

    /// <summary>
    /// Ribbon wrapper
    /// </summary>
    public class Ribbon : RibbonBase
    {
        private readonly RibbonControl _ribbonControl;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="application">UIControlledApplication</param>
        /// <param name="container"><see cref="IContainer"/></param>
        public Ribbon(UIControlledApplication application, IContainer container)
            : base(container)
        {
            Application = application;

            _ribbonControl = RevitRibbonControl.RibbonControl;
            if (_ribbonControl == null)
                throw new NotSupportedException("Could not initialize Revit ribbon control");
        }

        /// <summary>
        /// UIControlledApplication
        /// </summary>
        public UIControlledApplication Application { get; }

        /// <inheritdoc />
        protected override bool TabIsExists(string tabTitle)
        {
            return _ribbonControl.Tabs.Any(t => t.Title.Equals(tabTitle));
        }

        /// <inheritdoc />
        protected override void CreateTabAndAddToRibbon(string tabTitle)
        {
            Application.CreateRibbonTab(tabTitle);
        }

        /// <inheritdoc />
        protected override ITab GetTab(string title, IContainer container)
        {
            return new Tab(this, title, container);
        }
    }
}
﻿namespace RxBim.Application.Ribbon.Autocad.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Windows.Controls;
    using Autodesk.AutoCAD.ApplicationServices.Core;
    using Autodesk.Private.Windows;
    using Autodesk.Windows;
    using Extensions;
    using GalaSoft.MvvmLight.CommandWpf;
    using Models;
    using Models.Configurations;
    using Ribbon.Abstractions;
    using Ribbon.Services;
    using Ribbon.Services.ConfigurationBuilders;
    using Button = Models.Configurations.Button;

    /// <summary>
    /// Implementation of <see cref="IRibbonMenuBuilder"/> for AutoCAD
    /// </summary>
    public class AutocadRibbonMenuBuilder : RibbonMenuBuilderBase<RibbonTab, RibbonPanel>
    {
        private const string ThemeVariableName = "COLORTHEME";
        private readonly List<(RibbonButton, Button)> _createdButtons = new ();

        /// <inheritdoc />
        public AutocadRibbonMenuBuilder(Assembly menuAssembly)
            : base(menuAssembly)
        {
            Application.SystemVariableChanged += (_, args) =>
            {
                if (!args.Name.Equals(ThemeVariableName, StringComparison.OrdinalIgnoreCase))
                    return;
                var theme = GetCurrentTheme();
                _createdButtons.ForEach(x => SetButtonImages(x.Item1, x.Item2, theme));
            };
        }

        /// <inheritdoc />
        protected override void PreBuildActions()
        {
            base.PreBuildActions();
            _createdButtons.Clear();
        }

        /// <inheritdoc />
        protected override bool CheckRibbonCondition()
        {
            return ComponentManager.Ribbon != null;
        }

        /// <inheritdoc />
        protected override RibbonTab GetOrCreateTab(string tabName)
        {
            var acRibbonTab = ComponentManager.Ribbon.Tabs.FirstOrDefault(x =>
                x.IsVisible &&
                x.Title != null &&
                x.Title.Equals(tabName, StringComparison.OrdinalIgnoreCase));

            if (acRibbonTab is null)
            {
                acRibbonTab = new RibbonTab
                    { Title = tabName, Id = $"TAB_{tabName.GetHashCode():0}" };
                ComponentManager.Ribbon.Tabs.Add(acRibbonTab);
            }

            return acRibbonTab;
        }

        /// <inheritdoc />
        protected override RibbonPanel GetOrCreatePanel(RibbonTab acRibbonTab, string panelName)
        {
            var acRibbonPanel = acRibbonTab.Panels.FirstOrDefault(x =>
                x.Source.Name != null &&
                x.Source.Name.Equals(panelName, StringComparison.OrdinalIgnoreCase));
            if (acRibbonPanel is null)
            {
                acRibbonPanel = new RibbonPanel
                {
                    Source = new RibbonPanelSource
                    {
                        Name = panelName,
                        Title = panelName,
                        Id = $"{acRibbonTab.Id}_PANEL_{panelName.GetHashCode():0}"
                    },
                };

                acRibbonTab.Panels.Add(acRibbonPanel);
            }

            if (acRibbonPanel.GetCurrentRowOrNull() is null)
                acRibbonPanel.AddNewRow();

            return acRibbonPanel;
        }

        /// <inheritdoc />
        protected override void CreateAboutButton(RibbonPanel panel, AboutButton aboutButton)
        {
            var orientation = aboutButton.GetSingleLargeButtonOrientation();
            panel.AddToCurrentRow(CreateAboutButtonInternal(aboutButton, RibbonItemSize.Large, orientation));
        }

        /// <inheritdoc />
        protected override void CreateCommandButton(RibbonPanel panel, CommandButton cmdButton)
        {
            var orientation = cmdButton.GetSingleLargeButtonOrientation();
            panel.AddToCurrentRow(CreateCommandButtonInternal(cmdButton, RibbonItemSize.Large, orientation));
        }

        /// <inheritdoc />
        protected override void CreatePullDownButton(RibbonPanel panel, PullDownButton pullDownButton)
        {
            var orientation = pullDownButton.GetSingleLargeButtonOrientation();
            panel.AddToCurrentRow(CreatePullDownButtonInternal(pullDownButton,
                RibbonItemSize.Large,
                orientation));
        }

        /// <inheritdoc />
        protected override void AddSeparator(RibbonPanel panel)
        {
            panel.AddToCurrentRow(new RibbonSeparator());
        }

        /// <inheritdoc />
        protected override void AddSlideOut(RibbonPanel panel)
        {
            if (panel.HasSlideOut())
                return;

            panel.Source.Items.Add(new RibbonPanelBreak());
            panel.AddNewRow();
        }

        /// <inheritdoc />
        protected override void CreateStackedItems(RibbonPanel panel, StackedItems stackedItems)
        {
            var stackSize = stackedItems.StackedButtons.Count;
            var stackedItemsRow = new RibbonRowPanel();
            var size = stackSize == StackedItemsBuilder.MaxStackSize
                ? RibbonItemSize.Standard
                : RibbonItemSize.Large;

            panel.AddToCurrentRow(stackedItemsRow);

            for (var i = 0; i < stackSize; i++)
            {
                if (i > 0)
                {
                    stackedItemsRow.Items.Add(new RibbonRowBreak());
                }

                var buttonConfig = stackedItems.StackedButtons[i];
                var buttonItem = buttonConfig switch
                {
                    AboutButton aboutButton => CreateAboutButtonInternal(aboutButton, size, Orientation.Horizontal),
                    CommandButton cmdButton => CreateCommandButtonInternal(cmdButton, size, Orientation.Horizontal),
                    PullDownButton pullDownButton => CreatePullDownButtonInternal(pullDownButton,
                        size,
                        Orientation.Horizontal),
                    _ => throw new ArgumentOutOfRangeException($"Unknown button type: {buttonConfig.GetType().Name}")
                };

                stackedItemsRow.Items.Add(buttonItem);
            }
        }

        private T CreateNewButton<T>(
            Button buttonConfig,
            RibbonItemSize size,
            Orientation orientation,
            bool forceTextSettings = false)
            where T : RibbonButton, new()
        {
            var ribbonButton = new T();
            ribbonButton.SetButtonProperties(buttonConfig, size, orientation, forceTextSettings);
            SetButtonImages(ribbonButton, buttonConfig, GetCurrentTheme());
            _createdButtons.Add((ribbonButton, buttonConfig));
            return ribbonButton;
        }

        private void SetButtonImages(RibbonButton button, Button buttonConfig, ThemeType themeType)
        {
            if (themeType is ThemeType.Light)
            {
                button.Image = GetIconImage(buttonConfig.SmallImageLight ?? buttonConfig.SmallImage);
                button.LargeImage = GetIconImage(buttonConfig.LargeImageLight ?? buttonConfig.LargeImage);
            }
            else
            {
                button.Image = GetIconImage(buttonConfig.SmallImage);
                button.LargeImage = GetIconImage(buttonConfig.LargeImage);
            }
        }

        private ThemeType GetCurrentTheme()
        {
            var themeTypeValue = (short)Application.GetSystemVariable(ThemeVariableName);
            return themeTypeValue == 0 ? ThemeType.Dark : ThemeType.Light;
        }

        private RibbonButton CreateAboutButtonInternal(
            AboutButton aboutButtonConfig,
            RibbonItemSize size,
            Orientation orientation)
        {
            var button = CreateNewButton<RibbonButton>(aboutButtonConfig, size, orientation);
            button.SetTooltipForNonCommandButton(aboutButtonConfig);
            button.CommandHandler = new RelayCommand(() =>
                {
                    if (TryShowAboutWindow(aboutButtonConfig.Content))
                        return;

                    Application.ShowAlertDialog(aboutButtonConfig.Content.ToString());
                },
                true);
            return button;
        }

        private RibbonButton CreateCommandButtonInternal(
            CommandButton buttonConfig,
            RibbonItemSize size,
            Orientation orientation)
        {
            var button = CreateNewButton<RibbonButton>(buttonConfig, size, orientation);
            button.SetTooltipForCommandButton(buttonConfig);
            if (!string.IsNullOrWhiteSpace(buttonConfig.CommandType))
            {
                var commandName = GetCommandName(buttonConfig.CommandType!);
                button.CommandHandler = new RelayCommand(() =>
                    {
                        Application.DocumentManager.MdiActiveDocument?
                            .SendStringToExecute($"{commandName} ", false, false, true);
                    },
                    true);
            }

            return button;
        }

        private RibbonButton CreatePullDownButtonInternal(
            PullDownButton pullDownButtonConfig,
            RibbonItemSize size,
            Orientation orientation)
        {
            var forceTextSettings =
                pullDownButtonConfig.CommandButtonsList.Any(x => !string.IsNullOrWhiteSpace(x.Text));
            var splitButton =
                CreateNewButton<RibbonSplitButton>(pullDownButtonConfig, size, orientation, forceTextSettings);

            splitButton.ListStyle = RibbonSplitButtonListStyle.List;
            splitButton.ListButtonStyle = RibbonListButtonStyle.SplitButton;
            splitButton.ListImageSize =
                size == RibbonItemSize.Standard ? RibbonImageSize.Standard : RibbonImageSize.Large;

            foreach (var commandButtonConfig in pullDownButtonConfig.CommandButtonsList)
            {
                splitButton.Items.Add(CreateCommandButtonInternal(commandButtonConfig, size, orientation));
            }

            return splitButton;
        }

        private string GetCommandName(string commandTypeName)
        {
            var commandType = GetCommandType(commandTypeName);

            const string cmdNameProperty = "CommandName";
            var attributes = Attribute.GetCustomAttributes(commandType);

            foreach (var attribute in attributes)
            {
                try
                {
                    var cmdProperty = attribute.GetType()
                        .GetProperty(cmdNameProperty,
                            BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);

                    if (cmdProperty is null)
                        continue;

                    return cmdProperty.GetValue(attribute).ToString();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw new InvalidOperationException("Failed to retrieve command name!", e);
                }
            }

            return commandType.Name;
        }
    }
}
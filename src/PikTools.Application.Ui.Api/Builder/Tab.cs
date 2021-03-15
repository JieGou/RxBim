﻿namespace PikTools.Application.Ui.Api.Builder
{
    using System;
    using Autodesk.Windows;

    /// <summary>
    /// Закладка панели
    /// </summary>
    public class Tab : RibbonBuilder
    {
        private readonly string _tabName;

        private bool _isAddAboutButton;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="ribbon">панель</param>
        public Tab(Ribbon ribbon)
            : base(ribbon)
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="ribbon">панель</param>
        /// <param name="tabName">имя закладки</param>
        public Tab(Ribbon ribbon, string tabName)
            : base(ribbon)
        {
            _tabName = tabName;
        }

        /// <summary>
        /// Создает панель на закладке
        /// </summary>
        /// <param name="panelTitle">имя панели</param>
        public Panel Panel(string panelTitle)
        {
            var ribbonPanel = string.IsNullOrEmpty(_tabName)
                ? Ribbon.Application.CreateRibbonPanel(panelTitle)
                : Ribbon.Application.CreateRibbonPanel(_tabName, panelTitle);

            return new Panel(Ribbon, this, ribbonPanel);
        }

        /// <summary>
        /// Создает кнопку о программе
        /// </summary>
        /// <param name="panelTitle">имя панели</param>
        /// <param name="name">Название кнопки</param>
        /// <param name="text">Тест описания</param>
        /// <param name="action">Дополнительны действия для кнопки о программе</param>
        public Tab AboutButton(
            string panelTitle, string name, string text, Action<AboutButton> action)
        {
            if (_isAddAboutButton)
                return this;

            _ = string.IsNullOrEmpty(_tabName)
                ? Ribbon.Application.CreateRibbonPanel(panelTitle)
                : Ribbon.Application.CreateRibbonPanel(_tabName, panelTitle);

            var ribbon = ComponentManager.Ribbon;
            foreach (RibbonTab tab in ribbon.Tabs)
            {
                if (tab.Title.Equals(_tabName))
                {
                    foreach (RibbonPanel panel in tab.Panels)
                    {
                        if (panel.Source.Title.Equals(panelTitle))
                        {
                            var button = new AboutButton(name, text, $"PIK_ABOUT_{_tabName?.GetHashCode()}");
                            action?.Invoke(button);

                            var buttonData = button.BuildButton();

                            panel.Source.Items.Add(buttonData);
                            break;
                        }
                    }

                    break;
                }
            }

            _isAddAboutButton = true;
            return this;
        }
    }
}
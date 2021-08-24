﻿namespace RxBim.Application.Ui.Api.Abstractions
{
    using Di;

    /// <summary>
    /// Фабрика ленты
    /// </summary>
    public interface IRibbonFactory
    {
        /// <summary>
        /// Создаёт и возвращает ленту
        /// </summary>
        /// <param name="container">Контейнер</param>
        IRibbon Create(IContainer container);
    }
}
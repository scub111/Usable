using System;

namespace Usable
{
    /// <summary>
    /// Класс для реализации событий.
    /// </summary>
    public class EventEx
    {
        public delegate void EventHandlerEx<T, U>(T sender, U args) where U : EventArgs;
    }

    // Пример реализации
    // public event EventEx.EventHandlerEx<object, Event.CurrentObjectEventArgs> CurrentObjectChanged = delegate { };
}

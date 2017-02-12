using System;
using System.Collections.Generic;

namespace Usable
{
    /// <summary>
    /// Класс для симуляции триггера
    /// </summary>
    public class TriggerT<Tp1> where Tp1 : new()
    {
        /// <summary>
        /// Конструктор по-умолчанию. <see cref="TriggerT&lt;Tp1&gt;"/> class.
        /// </summary>
	    public TriggerT()
        {
        }

        /// <summary>
        /// Вычисление выходного значения.
        /// </summary>
        public bool CalculateRet(Tp1 Value)
        {
            if (_ValueLast != null && _ValueLast.Equals(Value)) return false;
            _ValueLast = Value;
            return true;
        }

        /// <summary>
        /// Вычисление выходного значения с сохранением предыдующего значения.
        /// </summary>
        public bool CalculateWithLastRet(Tp1 Value)
        {
            if (_ValueLast != null && _ValueLast.Equals(Value)) return false;
            _ValueLastLast = _ValueLast;
            _ValueLast = Value;
            return true;
        }

        /// <summary>
        /// Произошло ли изменение значения.
        /// </summary>
	    public bool IsChanged(Tp1 Value)
        {
            return _ValueLast.Equals(Value);
        }

        private Tp1 _ValueLastLast;
        /// <summary>
        /// Значение на предыдущем шаге.
        /// </summary>
        public Tp1 ValueLastLast
        {
            get { return _ValueLastLast; }
            set { _ValueLastLast = value; }
        }

        private Tp1 _ValueLast;
        /// <summary>
        /// Значение на предыдущем шаге, перезаписывающееся на текущее.
        /// </summary>
        public Tp1 ValueLast
        {
            get { return _ValueLast; }
            set { _ValueLast = value; }
        }
    }
}

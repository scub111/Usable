using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Usable
{
    /// <summary>
    /// Класс для проведения автоматических тестов, проверки условий и выведения диагностической информации.
    /// </summary>
    public class TestConditions
    {
        public class TestPair
        {
            public TestPair(Func<bool> logic, Func<string> massage = null)
            {
                Logic = logic;
                Massage = massage;
            }
            /// <summary>
            /// Логика.
            /// </summary>
            public Func<bool> Logic { get;  private set; }

            /// <summary>
            /// Сообщение при невыполнении
            /// </summary>
            public Func<string> Massage { get; private set; }
        }
        /// <summary>
        /// Конструктор.
        /// </summary>
        public TestConditions()
        {
            conditions = new Collection<TestPair>();
        }

        /// <summary>
        /// Условия.
        /// </summary>
        private Collection<TestPair> conditions;

        public void Add(Func<bool> logic, Func<string> massage = null)
        {
            conditions.Add(new TestPair(logic, massage));
        }

        /// <summary>
        /// Расчет выходного значения условий.
        /// </summary>
        /// <returns></returns>
        public bool Calculate()
        {
            foreach (TestPair item in conditions)
                if (!item.Logic())
                    return false;
            return true;
        }

        /// <summary>
        /// Вывод диагностики.
        /// </summary>
        public void Print()
        {
            int trouble = 0;
            foreach (TestPair item in conditions)
                if (!item.Logic())
                {
                    trouble++;
                    if (trouble == 1)
                        Console.WriteLine("------Troubles------");
                    Console.WriteLine(string.Format("[{0}] -> {1}", trouble, item.Massage()));
                }
        }
    }
}

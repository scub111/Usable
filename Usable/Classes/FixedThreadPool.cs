using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Usable
{
    /// <summary>
    /// Приоритет потока.
    /// </summary>
    public enum TaskPriorityEx
    {
        
        HIGH = 3,   //Высокий.
        NORMAL = 1, //Нормальный.
        LOW = 0     //Низкий.
    }

    /// <summary>
    /// Класс задач.
    /// </summary>
    public class TaskEx
    {
        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="action">Задача для выполнения.</param>
        /// <param name="priority">Приоритет задачи.</param>
        public TaskEx(Action action)
        {
            this.action = action;
            this.priority  = TaskPriorityEx.LOW;
            Executing = false;
            Tried = false;
        }

        /// <summary>
        /// Действие.
        /// </summary>
        Action action;

        /// <summary>
        /// Приоритет.
        /// </summary>
        TaskPriorityEx priority;

        /// <summary>
        /// Выполняется ли задача.
        /// </summary>
        public bool Executing { get; private set; }

        /// <summary>
        /// Пытались ли выполнить задачу. 
        /// </summary>
        public bool Tried { get; set; }

        /// <summary>
        /// Выполнить задачу.
        /// </summary>
        public void Execute()
        {
            lock (this)
            {
                Executing = true;
            }
            action();
        }
    }

    /// <summary>
    /// Класс пула потоков.
    /// </summary>
    public class FixedThreadPool
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="threadCount">Количество потоков</param>
        public FixedThreadPool(int threadCount)
        {
            this.threadCount = threadCount;
            pool = new Thread[threadCount];
            waitHandle = new ManualResetEvent(false);
            tasks = new List<TaskEx>();
            lockFind = new object();
            for (int i = 0; i < threadCount; i++)
            {
                pool[i] = new Thread(ThreadEngine) { Name = string.Format("Pool thread #{0}", i), IsBackground = true };
                pool[i].Start();
            }
        }

        /// <summary>
        /// Лок на поиск очередной задачи для выполнения.
        /// </summary>
        private object lockFind;

        /// <summary>
        /// Пул выполняющих потоков.
        /// </summary>
        private Thread[] pool;

        /// <summary>
        /// Количество потоков в пуле.
        /// </summary>
        private int threadCount;

        /// <summary>
        /// Список задач на выполнение.
        /// </summary>
        private List<TaskEx> tasks;

        EventWaitHandle waitHandle;

        /// <summary>
        /// Механизм выполнения задач.
        /// </summary>
        private void ThreadEngine()
        {
            while (true)
            {
                waitHandle.WaitOne();
                Console.WriteLine(string.Format("Thread Id={0} cycling...", Thread.CurrentThread.ManagedThreadId));
                TaskEx task = FindNewTask();
                if (task != null)
                {
                    task.Execute();
                    Console.WriteLine(string.Format("Thread Id={0} executed", Thread.CurrentThread.ManagedThreadId));
                }
                else
                    waitHandle.Reset();
            }
        }

        /// <summary>
        /// Поиск невыполненной задачи.
        /// </summary>
        private TaskEx FindNewTask()
        {
            lock (lockFind)
            {
                IEnumerable<TaskEx> searchTasks = tasks.Where(i => !i.Tried);

                if (searchTasks != null && searchTasks.Count() > 0)
                {
                    TaskEx searchTasksFirst = searchTasks.First();
                    searchTasksFirst.Tried = true;
                    return searchTasksFirst;
                }
                else
                    return null;
            }
        }

        public void Execute(TaskEx taskEx, TaskPriorityEx priority = TaskPriorityEx.LOW)
        {
            tasks.Add(taskEx);
            waitHandle.Set();
        }
    }
}

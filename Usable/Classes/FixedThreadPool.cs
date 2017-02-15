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
        public TaskEx(Action action, TaskPriorityEx priority = TaskPriorityEx.LOW)
        {
            this.action = action;
            Priority  = priority;
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
        public TaskPriorityEx Priority { get; private set; }

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
            highCount = 0;
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

        /// <summary>
        /// Объект синхронизации потоков.
        /// </summary>
        private EventWaitHandle waitHandle;

        /// <summary>
        /// Счетчик выполненных задач с высоким приоритетом.
        /// </summary>
        private int highCount;

        /// <summary>
        /// Механизм выполнения задач.
        /// </summary>
        private void ThreadEngine()
        {
            while (true)
            {
                waitHandle.WaitOne();
                //Console.WriteLine(string.Format("Thread Id={0} cycling...", Thread.CurrentThread.ManagedThreadId));
                TaskEx task = FindNewTask();
                if (task != null)
                {
                    task.Execute();
                    //Console.WriteLine(string.Format("Thread Id={0} executed", Thread.CurrentThread.ManagedThreadId));
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
                    IEnumerable<TaskEx> highTasks = searchTasks.Where(i => i.Priority == TaskPriorityEx.HIGH);
                    IEnumerable<TaskEx> normalTasks = searchTasks.Where(i => i.Priority == TaskPriorityEx.NORMAL);

                    TaskEx searchTasksFirst = searchTasks.First();

                    if (normalTasks.Count() > 0)
                       searchTasksFirst = normalTasks.First();

                    if (highTasks.Count() > 0)
                    {
                        highCount++;
                        searchTasksFirst = highTasks.First();
                        if (highCount >= (int)TaskPriorityEx.HIGH + 1)
                            highCount = (int)TaskPriorityEx.HIGH + 1;

                        if (normalTasks.Count() > 0 &&
                            highCount == (int)TaskPriorityEx.HIGH + 1)
                        {
                            highCount = 0;
                            searchTasksFirst = normalTasks.First();
                        }
                    }

                    searchTasksFirst.Tried = true;
                    return searchTasksFirst;
                }
                else
                    return null;
            }
        }


        public void Execute(Action action, TaskPriorityEx priority = TaskPriorityEx.LOW)
        {
            TaskEx taskEx = new TaskEx(action, priority);
            tasks.Add(taskEx);
            waitHandle.Set();
        }
    }
}

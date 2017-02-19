using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Usable
{
    /// <summary>
    /// Приоритет задачи.
    /// </summary>
    public enum TaskPriorityEx
    {
        
        HIGH = 3,   //Высокий.
        NORMAL = 1, //Нормальный.
        LOW = 0     //Низкий.
    }

    /// <summary>
    /// Статус выполнения задачи.
    /// </summary>
    public enum TaskStatus
    {
        Whaiting,   //В ожидании
        Running,    //Запущен.            
        Finished,   //Завершен.
        Aborted     //Прекращен.
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
            Status = TaskStatus.Whaiting;
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
        /// Статус задача.
        /// </summary>
        public TaskStatus Status { get; private set; }

        /// <summary>
        /// Пытались ли выполнить задачу. 
        /// </summary>
        public bool Tried { get; set; }

        /// <summary>
        /// Выполнить задачу.
        /// </summary>
        public void Execute()
        {
            Status = TaskStatus.Running;
            try
            {
                action();
                Status = TaskStatus.Finished;
            }
            catch
            {
                Status = TaskStatus.Aborted;
            }
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
            Stoping = false;
            Cleared = false;
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
        /// Останавливает работу пул потоков.
        /// </summary>
        public bool Stoping { get; private set; }

        /// <summary>
        /// Общее количество задач.
        /// </summary>
        public int TaskCount  { get { return tasks.Count(); } }

        /// <summary>
        /// Собыите на завершение обработки.
        /// </summary>
        public event EventHandler Finished = delegate { };

        /// <summary>
        /// Очищен ли список.
        /// </summary>
        private bool Cleared; 

        /// <summary>
        /// Количество выполненных задач.
        /// </summary>
        public int ExecutedCount
        {
            get
            {
                return tasks.Where(item => item.Status == TaskStatus.Finished || 
                                            item.Status == TaskStatus.Aborted).Count();
            }
        }

        /// <summary>
        /// Механизм выполнения задач.
        /// </summary>
        private void ThreadEngine()
        {
            while (true)
            {
                Stoping = false;
                waitHandle.WaitOne();
                TaskEx task = FindTask();
                if (task != null)
                    task.Execute();
                else
                    waitHandle.Reset();

                if (ExecutedCount == TaskCount)
                {
                    Finished(this, EventArgs.Empty);
                    Cleared = false;
                }
            }
        }

        /// <summary>
        /// Поиск невыполненной задачи.
        /// </summary>
        private TaskEx FindTask()
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

        public void Stop()
        {
            Stoping = true;
            ClearTasks(true);
            waitHandle.Reset();
        }

        /// <summary>
        /// Очистака списка задач.
        /// </summary>
        /// <param name="force"></param>
        private void ClearTasks(bool force = false)
        {
            lock (lockFind)
            {
                if (!Cleared || force)
                    if (ExecutedCount == TaskCount || force)
                    {
                        tasks.Clear();
                        Cleared = true;
                    }
            }
        }

        /// <summary>
        /// Выполнение то или иной задачи.
        /// </summary>
        /// <param name="action">Задача</param>
        /// <param name="priority">Приоритет</param>
        /// <returns>Поставлена задача в очередь</returns>
        public bool Execute(Action action, TaskPriorityEx priority = TaskPriorityEx.LOW)
        {
            if (!Stoping)
            {
                ClearTasks();

                TaskEx taskEx = new TaskEx(action, priority);
                tasks.Add(taskEx);
                waitHandle.Set();
                return true;
            }
            else
                return false;
        }
    }
}

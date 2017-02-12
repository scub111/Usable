using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

namespace Usable
{
    public class ThreadTimer
    {
        public ThreadTimer()
        {
            Period = 1000;
            Delay = 100;
            TimeError = 50;
            WorkCount = 0;
            Worker = new BackgroundWorker();
            Worker.WorkerReportsProgress = true;
            Worker.WorkerSupportsCancellation = true;
            Worker.DoWork += WorkerDoWork;
            Worker.ProgressChanged += WorkerProgressChanged;
            lockInterface = new object();
        }

        int _Perion;
        /// <summary>
        /// Период таймера в мс.
        /// </summary>
        public int Period 
        {
            get
            {
                return _Perion;
            }
            set
            {
                float K = _Perion / (float)value;
                _Perion = value;
                WorkCount = (int)(WorkCount * K);
            }
        }

        /// <summary>
        /// Задержки в цикле в мс.
        /// </summary>
        public int Delay { get; set; }

        /// <summary>
        /// Ошибка в точности таймера.
        /// </summary>
        public int TimeError { get; set; }

        /// <summary>
        /// Время запуска таймера.
        /// </summary>
        DateTime StartTime { get; set; }

        /// <summary>
        /// Счетчик выполнения.
        /// </summary>
        public int WorkCount { get; set; }

        /// <summary>
        /// Счетчик внутреннего выполнения .
        /// </summary>
        int InternalWorkCount { get; set; }

        /// <summary>
        /// Счетчик циклов.
        /// </summary>
        public int CycleCount { get; set; }

        /// <summary>
        /// Разница во времени.
        /// </summary>
        TimeSpan TimeDiff;

        /// <summary>
        /// Время выполнения одного цикла.
        /// </summary>
        public TimeSpan CycleSpan { get; set; }

        /// <summary>
        /// Перепенная для расчет времени выполнения одного цикла.
        /// </summary>
        DateTime T0;

        /// <summary>
        /// Рабочее событие.
        /// </summary>
        public event EventHandler WorkChanged = delegate { };

        /// <summary>
        /// Событие на обновление интерфейса.
        /// </summary>
        public event EventHandler InterfaceChanged = delegate { };

        BackgroundWorker Worker { get; set; }

        /// <summary>
        /// Объект блокировки обработчика интерфейсного потока.
        /// </summary>
        object lockInterface;

        /// <summary>
        /// Запуск таймера.
        /// </summary>
        public void Run()
        {
            if (!Worker.IsBusy)
            {
                Worker.RunWorkerAsync();
                Reset();
            }
        }

        /// <summary>
        /// Стоп таймера.
        /// </summary>
        public void Stop()
        {
            Worker.CancelAsync();
        }

        /// <summary>
        /// Обнуление переменных.
        /// </summary>
        void Reset()
        {
            StartTime = DateTime.Now;
            InternalWorkCount = 0;
        }

        /// <summary>
        /// Ускорение просчетов.
        /// </summary>
        public void Force()
        {
            Reset();
        }

        /// <summary>
        /// Выполняется ли поток.
        /// </summary>
        public bool IsBusy
        {
            get
            {
                return Worker.IsBusy;
            }
        }

        void WorkerDoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                CycleCount++;

                //Подсчет разницы во времени.
                TimeDiff = DateTime.Now - StartTime;

                if (TimeDiff.TotalMilliseconds >= InternalWorkCount * Period)
                {
                    WorkCount++;
                    InternalWorkCount++;

                    //На случай получения команды останова потока.
                    if (Worker.CancellationPending)
                    {
                        e.Cancel = true;
                        break;
                    }       

                    T0 = DateTime.Now;
                    WorkChanged(this, EventArgs.Empty);
                    Worker.ReportProgress(0);
                    CycleSpan = DateTime.Now - T0;

                    if (CycleSpan.TotalMilliseconds > Period + TimeError)
                        Reset();
                }
                Thread.Sleep(Delay);
            }
        }

        void WorkerProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //Необходим был для избежания периодической остановки потоки. 
            //Скорей всего связанного с тем, что выполнения освного потока занимало большего времени, чем планировалось.
           lock (lockInterface)
                InterfaceChanged(this, EventArgs.Empty);
        }
    }
}

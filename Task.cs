using System;
using System.Threading;
using System.Collections.Generic;

namespace Miscellaneous
{
    /// <summary>
    /// Статусы задач
    /// </summary>
    public enum TaskState
    {
        /// <summary>
        /// Не начата
        /// </summary>
        UnStarted,

        /// <summary>
        /// Работает
        /// </summary>
        Running,

        /// <summary>
        /// Завершена
        /// </summary>
        Complited,

        /// <summary>
        /// Отменена
        /// </summary>
        Aborted
    }

	public sealed class TaskManager
	{
		private object _lock = new object();
		private TaskManager _instance;
		private int _poolsize = 8;
		private int _runtaskcount;
		private List<BaseTask> _pool = new List<BaseTask>();

		private TaskManager()
		{
		}

		public TaskManager GetInstance()
		{
			if (_instance == null)
			{
				lock (_lock)
				{
					if (_instance == null)
						_instance = new TaskManager();
				}
			}
			return _instance;
		}

		public int PoolSize
		{
			get
			{
				return _poolsize;
			}
			set
			{
				_poolsize = value;
			}
		}

		public void Add(BaseTask task)
		{
			task.OnEnd += (s, e) =>
			{
				_runtaskcount--;
				_pool.Remove(task);
			};
			_pool.Add(task);
		}
	}

    /// <summary>
    /// Класс задачи
    /// Выполняет метод в отдельном потоке.
    /// </summary>
    public abstract class BaseTask
    {
		/// <summary>
		/// Событие окончания работы задачи
		/// </summary>
		public event EventHandler OnEnd;

		private TaskState _state;
        /// <summary>
        /// Текущий статус задачи
        /// </summary>
		public TaskState State
		{
			get
			{
				return _state;
			}
			protected set
			{
				_state = value;
				if (value == TaskState.Complited || value == TaskState.Aborted)
				{
					if (OnEnd != null)
						OnEnd(this, EventArgs.Empty);
				}
			}
		}

        /// <summary>
        /// Поток в котором выполняется задача
        /// </summary>
        protected Thread TaskThread;

        /// <summary>
        /// Фоновое задание?
        /// </summary>
        public bool IsBackground
        {
            get { return TaskThread.IsBackground; }
            set { TaskThread.IsBackground = value; }
        }

        /// <summary>
        /// Приоритет
        /// </summary>
        public ThreadPriority Priority
        {
            get { return TaskThread.Priority; }
            set { TaskThread.Priority = value; }
        }

        /// <summary>
        /// Имя
        /// </summary>
        public string Name
        {
            get { return TaskThread.Name; }
            set { TaskThread.Name = value; }
        }

        /// <summary>
        /// Создает отдельный поток и выполняет в нем метод переданный через конструктор
        /// </summary>
        public void Start()
        {
            State = TaskState.Running;
            TaskThread.Start();
            
        }

        /// <summary>
        /// Прерывает выполнение
        /// </summary>
        public void Abort()
        {
            TaskThread.Abort();
            State = TaskState.Aborted;
        }

        /// <summary>
        /// Ожидает завершение выполнения задачи
        /// </summary>
        public void Wait()
        {
            while (true)
            {
                if (State == TaskState.UnStarted)
                    throw new Exception("Task not start");
                if (State == TaskState.Complited || State == TaskState.Aborted)
                    return;
            }
        }
    }

    #region Task Action

    /// <summary>
    /// Класс задачи
    /// Выполняет метод в отдельном потоке.
    /// </summary>
    public class Task : BaseTask
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="action">Метод</param>
        public Task(Action action)
        {
            if (action == null)
                throw new ArgumentNullException("action");
            TaskThread = new Thread(() =>
            {
                action();
                State = TaskState.Complited;
            })
            {
                IsBackground = true
            };
        }

        /// <summary>
        /// Запускает выполнение метода в отдельном потоке
        /// </summary>
        /// <param name="action">Метод для выполнения</param>
        public static void Run(Action action)
        {
            if (action == null)
                throw new ArgumentNullException("action");
            new Task(action).Start();
        }

        /// <summary>
        /// Запускает выполнение метода и возвращает объект Task выполниющий переданный метод
        /// </summary>
        /// <param name="action">Метод для выполнения</param>
        /// <returns>Объект Task выполняющий метод</returns>
        public static Task RunNew(Action action)
        {
            if (action == null)
                throw new ArgumentNullException("action");
            var task = new Task(action);
            task.Start();
            return task;
        }
    }

    /// <summary>
    /// Класс задачи
    /// Выполняет метод в отдельном потоке.
    /// </summary>
    public class Task<T> : BaseTask
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="action">Метод</param>
        /// <param name="arg"></param>
        public Task(Action<T> action, T arg)
        {
            if (action == null)
                throw new ArgumentNullException("action");
            TaskThread = new Thread(() =>
            {
                action(arg);
                State = TaskState.Complited;
            })
            {
                IsBackground = true
            };
        }

        /// <summary>
        /// Запускает выполнение метода в отдельном потоке
        /// </summary>
        /// <typeparam name="TY">Тип аргумента</typeparam>
        /// <param name="action">Метод для выполнения</param>
        /// <param name="arg">Аргумент</param>
        public static void Run<TY>(Action<TY> action, TY arg)
        {
            if (action == null)
                throw new ArgumentNullException("action");
            new Task<TY>(action, arg).Start();
        }

        /// <summary>
        /// Запускает выполнение метода и возвращает объект Task выполниющий переданный метод
        /// </summary>
        /// <typeparam name="TY">Тип аргумента</typeparam>
        /// <param name="action">Метод для выполнения</param>
        /// <returns>Объект Task выполняющий метод</returns>
        /// <param name="arg">Аргумент</param>
        public static Task<TY> RunNew<TY>(Action<TY> action, TY arg)
        {
            if (action == null)
                throw new ArgumentNullException("action");
            var task = new Task<TY>(action, arg);
            task.Start();
            return task;
        }
    }

    /// <summary>
    /// Класс задачи
    /// Выполняет метод в отдельном потоке.
    /// </summary>
    public class Task<T1, T2> : BaseTask
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="action">Метод</param>
        /// <param name="arg1">Аргумент 1</param>
        /// <param name="arg2">Аргумент 2</param>
        public Task(Action<T1, T2> action, T1 arg1, T2 arg2)
        {
            if (action == null)
                throw new ArgumentNullException("action");
            TaskThread = new Thread(() =>
            {
                action(arg1, arg2);
                State = TaskState.Complited;
            })
            {
                IsBackground = true
            };
        }

        /// <summary>
        /// Запускает выполнение метода в отдельном потоке
        /// </summary>
        /// <typeparam name="TY1">Тип аргумента 1</typeparam>
        /// <typeparam name="TY2">Тип аргумента 2</typeparam>
        /// <param name="action">Метод для выполнения</param>
        /// <param name="arg1">Аргумент 1</param>
        /// <param name="arg2">Аргумент 2</param>
        public static void Run<TY1, TY2>(Action<TY1, TY2> action, TY1 arg1, TY2 arg2)
        {
            if (action == null)
                throw new ArgumentNullException("action");
            new Task<TY1, TY2>(action, arg1, arg2).Start();
        }

        /// <summary>
        /// Запускает выполнение метода и возвращает объект Task выполниющий переданный метод
        /// </summary>
        /// <typeparam name="TY1">Тип аргумента 1</typeparam>
        /// <typeparam name="TY2">Тип аргумента 2</typeparam>
        /// <param name="action">Метод для выполнения</param>
        /// <returns>Объект Task выполняющий метод</returns>
        /// <param name="arg1">Аргумент 1</param>
        /// <param name="arg2">Аргумент 2</param>
        public static Task<TY1, TY2> RunNew<TY1, TY2>(Action<TY1, TY2> action, TY1 arg1, TY2 arg2)
        {
            if (action == null)
                throw new ArgumentNullException("action");
            var task = new Task<TY1, TY2>(action, arg1, arg2);
            task.Start();
            return task;
        }
    }

    /// <summary>
    /// Класс задачи
    /// Выполняет метод в отдельном потоке.
    /// </summary>
    public class Task<T1, T2, T3> : BaseTask
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="action">Метод</param>
        /// <param name="arg1">Аргумент 1</param>
        /// <param name="arg2">Аргумент 2</param>
        /// <param name="arg3">Аргумент 3</param>
        public Task(Action<T1, T2, T3> action, T1 arg1, T2 arg2, T3 arg3)
        {
            if (action == null)
                throw new ArgumentNullException("action");
            TaskThread = new Thread(() =>
            {
                action(arg1, arg2, arg3);
                State = TaskState.Complited;
            })
            {
                IsBackground = true
            };
        }

        /// <summary>
        /// Запускает выполнение метода в отдельном потоке
        /// </summary>
        /// <typeparam name="TY1">Тип аргумента 1</typeparam>
        /// <typeparam name="TY2">Тип аргумента 2</typeparam>
        /// <typeparam name="TY3">Тип аргумента 3</typeparam>
        /// <param name="action">Метод для выполнения</param>
        /// <param name="arg1">Аргумент 1</param>
        /// <param name="arg2">Аргумент 2</param>
        /// <param name="arg3">Аргумент 3</param>
        public static void Run<TY1, TY2, TY3>(Action<TY1, TY2, TY3> action, TY1 arg1, TY2 arg2, TY3 arg3)
        {
            if (action == null)
                throw new ArgumentNullException("action");
            new Task<TY1, TY2, TY3>(action, arg1, arg2, arg3).Start();
        }

        /// <summary>
        /// Запускает выполнение метода и возвращает объект Task выполниющий переданный метод
        /// </summary>
        /// <typeparam name="TY1">Тип аргумента 1</typeparam>
        /// <typeparam name="TY2">Тип аргумента 2</typeparam>
        /// <typeparam name="TY3">Тип аргумента 3</typeparam>
        /// <param name="action">Метод для выполнения</param>
        /// <returns>Объект Task выполняющий метод</returns>
        /// <param name="arg1">Аргумент 1</param>
        /// <param name="arg2">Аргумент 2</param>
        /// <param name="arg3">Аргумент 3</param>
        public static Task<TY1, TY2, TY3> RunNew<TY1, TY2, TY3>(Action<TY1, TY2, TY3> action, TY1 arg1, TY2 arg2,
            TY3 arg3)
        {
            if (action == null)
                throw new ArgumentNullException("action");
            var task = new Task<TY1, TY2, TY3>(action, arg1, arg2, arg3);
            task.Start();
            return task;
        }
    }

    /// <summary>
    /// Класс задачи
    /// Выполняет метод в отдельном потоке.
    /// </summary>
    public class Task<T1, T2, T3, T4> : BaseTask
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="action">Метод</param>
        /// <param name="arg1">Аргумент 1</param>
        /// <param name="arg2">Аргумент 2</param>
        /// <param name="arg3">Аргумент 3</param>
        /// <param name="arg4">Аргумент 4</param>
        public Task(Action<T1, T2, T3, T4> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            if (action == null)
                throw new ArgumentNullException("action");
            TaskThread = new Thread(() =>
            {
                action(arg1, arg2, arg3, arg4);
                State = TaskState.Complited;
            })
            {
                IsBackground = true
            };
        }

        /// <summary>
        /// Запускает выполнение метода в отдельном потоке
        /// </summary>
        /// <typeparam name="TY1">Тип аргумента 1</typeparam>
        /// <typeparam name="TY2">Тип аргумента 2</typeparam>
        /// <typeparam name="TY3">Тип аргумента 3</typeparam>
        /// <typeparam name="TY4">Тип аргумента 4</typeparam>
        /// <param name="action">Метод для выполнения</param>
        /// <param name="arg1">Аргумент 1</param>
        /// <param name="arg2">Аргумент 2</param>
        /// <param name="arg3">Аргумент 3</param>
        /// <param name="arg4">Аргумент 4</param>
        public static void Run<TY1, TY2, TY3, TY4>(Action<TY1, TY2, TY3, TY4> action, TY1 arg1, TY2 arg2, TY3 arg3,
            TY4 arg4)
        {
            if (action == null)
                throw new ArgumentNullException("action");
            new Task<TY1, TY2, TY3, TY4>(action, arg1, arg2, arg3, arg4).Start();
        }

        /// <summary>
        /// Запускает выполнение метода и возвращает объект Task выполниющий переданный метод
        /// </summary>
        /// <typeparam name="TY1">Тип аргумента 1</typeparam>
        /// <typeparam name="TY2">Тип аргумента 2</typeparam>
        /// <typeparam name="TY3">Тип аргумента 3</typeparam>
        /// <typeparam name="TY4">Тип аргумента 4</typeparam>
        /// <param name="action">Метод для выполнения</param>
        /// <returns>Объект Task выполняющий метод</returns>
        /// <param name="arg1">Аргумент 1</param>
        /// <param name="arg2">Аргумент 2</param>
        /// <param name="arg3">Аргумент 3</param>
        /// <param name="arg4">Аргумент 4</param>
        public static Task<TY1, TY2, TY3, TY4> RunNew<TY1, TY2, TY3, TY4>(Action<TY1, TY2, TY3, TY4> action, TY1 arg1,
            TY2 arg2, TY3 arg3,
            TY4 arg4)
        {
            if (action == null)
                throw new ArgumentNullException("action");
            var task = new Task<TY1, TY2, TY3, TY4>(action, arg1, arg2, arg3, arg4);
            task.Start();
            return task;
        }
    }

    #endregion

    #region Task Func

    public abstract class TaskResultBase<TResult> : BaseTask where TResult : new()
    {
        /// <summary>
        /// Результат работы функции
        /// </summary>
        public TResult Result { get; protected set; }

        /// <summary>
        /// Событие окончания работы функции
        /// </summary>
        /// <remarks>ВНИМАНИЕ! Событие будет вызвано из потока задачи</remarks>
        public event Action OnComplited;

        /// <summary>
        /// Генерирует событие окончания работы функции
        /// </summary>
        protected void OnComplitedEvent()
        {
            State = TaskState.Complited;
            if (OnComplited != null)
                OnComplited();
        }
    }

    /// <summary>
    /// Задача возвращающая результат
    /// </summary>
    /// <typeparam name="TResult">Тип результата</typeparam>
    public class TaskResult<TResult> : TaskResultBase<TResult> where TResult : new()
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <typeparam name="TResult">Тип результата</typeparam>
        /// <param name="func">Функция</param>
        public TaskResult(Func<TResult> func)
        {
            if (func == null)
                throw new ArgumentNullException("func");
            TaskThread = new Thread(() =>
            {
                Result = func();
                OnComplitedEvent();
            });
        }

        /// <summary>
        /// Запускает выполнение функции и возвращает объект задачи выполняющий функцию
        /// </summary>
        /// <param name="func">Метод для выполнения</param>
        /// <returns>Объект задачи</returns>
        public static TaskResult<TResult> RunNew(Func<TResult> func)
        {
            if (func == null)
                throw new ArgumentNullException("func");
            var task = new TaskResult<TResult>(func);
            task.Start();
            return task;
        }
    }

    /// <summary>
    /// Задача возвращающая результат
    /// </summary>
    /// <typeparam name="TResult">Тип результата</typeparam>
    /// <typeparam name="T1">Тип аргумента 1</typeparam>
    public class TaskResult<T1, TResult> : TaskResultBase<TResult> where TResult : new()
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <typeparam name="TResult">Тип результата</typeparam>
        /// <param name="func">Функция</param>
        /// <param name="arg1">Аргумент 1</param>
        public TaskResult(Func<T1, TResult> func, T1 arg1)
        {
            if (func == null)
                throw new ArgumentNullException("func");
            TaskThread = new Thread(() =>
            {
                Result = func(arg1);
                OnComplitedEvent();
            });
        }

        /// <summary>
        /// Запускает выполнение функции и возвращает объект задачи выполняющий функцию
        /// </summary>
        /// <typeparam name="TYResult">Тип результата</typeparam>
        /// <typeparam name="TY1">Тип аргумента 1</typeparam>
        /// <param name="func">Метод для выполнения</param>
        /// <returns>Объект задачи</returns>
        /// <param name="arg1">Аргумент 1</param>
        public static TaskResult<TY1, TYResult> RunNew<TY1, TYResult>(Func<TY1, TYResult> func, TY1 arg1)
            where TYResult : new()
        {
            if (func == null)
                throw new ArgumentNullException("func");
            var task = new TaskResult<TY1, TYResult>(func, arg1);
            task.Start();
            return task;
        }
    }

    /// <summary>
    /// Задача возвращающая результат
    /// </summary>
    /// <typeparam name="TResult">Тип результата</typeparam>
    /// <typeparam name="T1">Тип аргумента 1</typeparam>
    /// <typeparam name="T2">Тип аргумента 2</typeparam>
    public class TaskResult<T1, T2, TResult> : TaskResultBase<TResult> where TResult : new()
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <typeparam name="TResult">Тип результата</typeparam>
        /// <param name="func">Функция</param>
        /// <param name="arg1">Аргумент 1</param>
        /// <param name="arg2">Аргумент 2</param>
        public TaskResult(Func<T1, T2, TResult> func, T1 arg1, T2 arg2)
        {
            if (func == null)
                throw new ArgumentNullException("func");
            TaskThread = new Thread(() =>
            {
                Result = func(arg1, arg2);
                OnComplitedEvent();
            });
        }

        /// <summary>
        /// Запускает выполнение функции и возвращает объект задачи выполняющий функцию
        /// </summary>
        /// <typeparam name="TYResult">Тип результата</typeparam>
        /// <typeparam name="TY1">Тип аргумента 1</typeparam>
        /// <typeparam name="TY2">Тип аргумента 2</typeparam>
        /// <param name="func">Метод для выполнения</param>
        /// <returns>Объект задачи</returns>
        /// <param name="arg1">Аргумент 1</param>
        /// <param name="arg2">Аргумент 2</param>
        public static TaskResult<TY1, TY2, TYResult> RunNew<TY1, TY2, TYResult>(Func<TY1, TY2, TYResult> func, TY1 arg1,
            TY2 arg2) where TYResult : new()
        {
            if (func == null)
                throw new ArgumentNullException("func");
            var task = new TaskResult<TY1, TY2, TYResult>(func, arg1, arg2);
            task.Start();
            return task;
        }
    }

    /// <summary>
    /// Задача возвращающая результат
    /// </summary>
    /// <typeparam name="TResult">Тип результата</typeparam>
    /// <typeparam name="T1">Тип аргумента 1</typeparam>
    /// <typeparam name="T2">Тип аргумента 2</typeparam>
    /// <typeparam name="T3">Тип аргумента 3</typeparam>
    public class TaskResult<T1, T2, T3, TResult> : TaskResultBase<TResult> where TResult : new()
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <typeparam name="TResult">Тип результата</typeparam>
        /// <param name="func">Функция</param>
        /// <param name="arg1">Аргумент 1</param>
        /// <param name="arg2">Аргумент 2</param>
        /// <param name="arg3">Аргумент 3</param>
        public TaskResult(Func<T1, T2, T3, TResult> func, T1 arg1, T2 arg2, T3 arg3)
        {
            if (func == null)
                throw new ArgumentNullException("func");
            TaskThread = new Thread(() =>
            {
                Result = func(arg1, arg2, arg3);
                OnComplitedEvent();
            });
        }

        /// <summary>
        /// Запускает выполнение функции и возвращает объект задачи выполняющий функцию
        /// </summary>
        /// <typeparam name="TYResult">Тип результата</typeparam>
        /// <typeparam name="TY1">Тип аргумента 1</typeparam>
        /// <typeparam name="TY2">Тип аргумента 2</typeparam>
        /// <typeparam name="TY3">Тип аргумента 3</typeparam>
        /// <param name="func">Метод для выполнения</param>
        /// <returns>Объект задачи</returns>
        /// <param name="arg1">Аргумент 1</param>
        /// <param name="arg2">Аргумент 2</param>
        /// <param name="arg3">Аргумент 3</param>
        public static TaskResult<TY1, TY2, TY3, TYResult> RunNew<TY1, TY2, TY3, TYResult>(
            Func<TY1, TY2, TY3, TYResult> func, TY1 arg1, TY2 arg2, TY3 arg3) where TYResult : new()
        {
            if (func == null)
                throw new ArgumentNullException("func");
            var task = new TaskResult<TY1, TY2, TY3, TYResult>(func, arg1, arg2, arg3);
            task.Start();
            return task;
        }
    }

    /// <summary>
    /// Задача возвращающая результат
    /// </summary>
    /// <typeparam name="TResult">Тип результата</typeparam>
    /// <typeparam name="T1">Тип аргумента 1</typeparam>
    /// <typeparam name="T2">Тип аргумента 2</typeparam>
    /// <typeparam name="T3">Тип аргумента 3</typeparam>
    /// <typeparam name="T4">Тип аргумента 4</typeparam>
    public class TaskResult<T1, T2, T3, T4, TResult> : TaskResultBase<TResult> where TResult : new()
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <typeparam name="TResult">Тип результата</typeparam>
        /// <param name="func">Функция</param>
        /// <param name="arg1">Аргумент 1</param>
        /// <param name="arg2">Аргумент 2</param>
        /// <param name="arg3">Аргумент 3</param>
        /// <param name="arg4">Аргумент 4</param>
        public TaskResult(Func<T1, T2, T3, T4, TResult> func, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            if (func == null)
                throw new ArgumentNullException("func");
            TaskThread = new Thread(() =>
            {
                Result = func(arg1, arg2, arg3, arg4);
                OnComplitedEvent();
            });
        }

        /// <summary>
        /// Запускает выполнение функции и возвращает объект задачи выполняющий функцию
        /// </summary>
        /// <typeparam name="TYResult">Тип результата</typeparam>
        /// <typeparam name="TY1">Тип аргумента 1</typeparam>
        /// <typeparam name="TY2">Тип аргумента 2</typeparam>
        /// <typeparam name="TY3">Тип аргумента 3</typeparam>
        /// <typeparam name="TY4">Тип аргумента 4</typeparam>
        /// <param name="func">Метод для выполнения</param>
        /// <returns>Объект задачи</returns>
        /// <param name="arg1">Аргумент 1</param>
        /// <param name="arg2">Аргумент 2</param>
        /// <param name="arg3">Аргумент 3</param>
        /// <param name="arg4">Аргумент 4</param>
        public static TaskResult<TY1, TY2, TY3, TY4, TYResult> RunNew<TY1, TY2, TY3, TY4, TYResult>(
            Func<TY1, TY2, TY3, TY4, TYResult> func, TY1 arg1, TY2 arg2, TY3 arg3, TY4 arg4) where TYResult : new()
        {
            if (func == null)
                throw new ArgumentNullException("func");
            var task = new TaskResult<TY1, TY2, TY3, TY4, TYResult>(func, arg1, arg2, arg3, arg4);
            task.Start();
            return task;
        }
    }

    #endregion

}
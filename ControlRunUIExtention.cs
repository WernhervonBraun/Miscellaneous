using System;
using System.Diagnostics;

namespace Miscellaneous
{
    /// <summary>
    /// Класс расширения для выполнения кода к главном потоке
    /// </summary>
    public static class ControlRunUIExtention
    {
        /// <summary>
        /// Выполняет метод в главном потоке
        /// </summary>
        /// <param name="context">Контекст потока</param>
        /// <param name="action">Метод для выполнения</param>
        public static void RunUIContext(this System.Windows.Forms.Control context, Action action)
        {
            if (action == null)
                throw new ArgumentNullException("action");
            if (context == null)
                throw new ArgumentNullException("context");
            if (context.InvokeRequired)
	            try
	            {
		            context.Invoke(action);
	            }
	            catch (ObjectDisposedException e)
	            {
		            Debug.WriteLine(e.Message);
	            }
            else
                action();
        }

        /// <summary>
        /// Выполняет метод в главном потоке передавая ему аргумент
        /// </summary>
        /// <typeparam name="T">Тип аргумента</typeparam>
        /// <param name="context">Контекст потока</param>
        /// <param name="action">Метод</param>
        /// <param name="arg">Аргумент</param>
        public static void RunUIContext<T>(this System.Windows.Forms.Control context, Action<T> action, T arg)
        {
            if (action == null)
                throw new ArgumentNullException("action");
            if (context == null)
                throw new ArgumentNullException("context");
            if (context.InvokeRequired)
                context.Invoke(action, arg);
            else
                action(arg);
        }

        /// <summary>
        /// Выполняет метод в главном потоке передавая ему аргумент
        /// </summary>
        /// <typeparam name="T1">Тип аргумента 1</typeparam>
        /// <typeparam name="T2">Тип аргумента 2</typeparam>
        /// <param name="context">Контекст потока</param>
        /// <param name="action">Метод</param>
        /// <param name="arg1">Аргумент 1</param>
        /// <param name="arg2">Аргумент 2</param>
        public static void RunUIContext<T1, T2>(this System.Windows.Forms.Control context, Action<T1, T2> action, T1 arg1, T2 arg2)
        {
            if (action == null)
                throw new ArgumentNullException("action");
            if (context == null)
                throw new ArgumentNullException("context");
            if (context.InvokeRequired)
                context.Invoke(action, arg1, arg2);
            else
                action(arg1, arg2);
        }

        /// <summary>
        /// Выполняет метод в главном потоке передавая ему аргумент
        /// </summary>
        /// <typeparam name="T1">Тип аргумента 1</typeparam>
        /// <typeparam name="T2">Тип аргумента 2</typeparam>
        /// <typeparam name="T3">Тип аргумента 3</typeparam>
        /// <param name="context">Контекст потока</param>
        /// <param name="action">Метод</param>
        /// <param name="arg1">Аргумент 1</param>
        /// <param name="arg2">Аргумент 2</param>
        /// <param name="arg3">Аргумент 3</param>
        public static void RunUIContext<T1, T2, T3>(this System.Windows.Forms.Control context, Action<T1, T2, T3> action, T1 arg1, T2 arg2, T3 arg3)
        {
            if (action == null)
                throw new ArgumentNullException("action");
            if (context == null)
                throw new ArgumentNullException("context");
            if (context.InvokeRequired)
                context.Invoke(action, arg1, arg2, arg3);
            else
                action(arg1, arg2, arg3);
        }

        /// <summary>
        /// Выполняет метод в главном потоке передавая ему аргумент
        /// </summary>
        /// <typeparam name="T1">Тип аргумента 1</typeparam>
        /// <typeparam name="T2">Тип аргумента 2</typeparam>
        /// <typeparam name="T3">Тип аргумента 3</typeparam>
        /// <typeparam name="T4">Тип аргумента 4</typeparam>
        /// <param name="context">Контекст потока</param>
        /// <param name="action">Метод</param>
        /// <param name="arg1">Аргумент 1</param>
        /// <param name="arg2">Аргумент 2</param>
        /// <param name="arg3">Аргумент 3</param>
        /// <param name="arg4">Аргумент 4</param>
        public static void RunUIContext<T1, T2, T3, T4>(this System.Windows.Forms.Control context, Action<T1, T2, T3, T4> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            if (action == null)
                throw new ArgumentNullException("action");
            if (context == null)
                throw new ArgumentNullException("context");
            if (context.InvokeRequired)
                context.Invoke(action, arg1, arg2, arg3, arg4);
            else
                action(arg1, arg2, arg3, arg4);
        }
    }
}
using System;
using System.Collections.Generic;

namespace Assets.Scripts.UI.WindowsManagerSystem
{
    /// <summary>
    /// Класс который следит за очередью для открытия окон. 2 варианта использования:
    /// 1) Очередь на окно когда все окна закрыты. Окно будет открыто после того как остальные закроются.
    ///     Пример: начало игры, окно "У нас появилось!" со своей цепочкой окон, потом еще окно "Новый ивент!"
    /// 2) Очередь на конкретное окно. Когда конкретное окно станет топовым откроется окно из очереди для него.
    ///     Пример: есть окно карты, при заходе на него открывается окно "открыта новая локация А", сразу же в очереди
    ///     окно "открыта локация Б" которое откроется после закрытия окна А
    /// </summary>
    class WindowsQueueManager 
    {
        private Queue<(Window, Action<Window>)> emptyQueue = new Queue<(Window, Action<Window>)>();
        private Dictionary<Window, Queue<(Window, Action<Window>)>> queueForWindow = new Dictionary<Window, Queue<(Window, Action<Window>)>>();

        // Окно будет открыто когда закроется окно "target"
        // Если target == null то окно откроется при закрытии всех остальных
        public void EnqueueForWindow(Window prefab, Action<Window> action, Window target = null)
        {
            if (target == null)
            {
                emptyQueue.Enqueue((prefab, action));
                return;
            }

            if (!queueForWindow.ContainsKey(target))
                queueForWindow.Add(target, new Queue<(Window, Action<Window>)>());

            queueForWindow[target].Enqueue((prefab, action));
        }

        // Достаем окно из очереди на закрытие всех окон
        public (Window wnd, Action<Window> action) DequeueForEmpty()
        {
            return Dequeue(emptyQueue);
        }

        // Достаем окно для конкретного окна, если 
        // если currentTopWindow == null вызывается DequeueForEmpty
        public (Window wnd, Action<Window> action) DequeueForWindow(Window currentTopWindow)
        {
            if (currentTopWindow == null)
                return DequeueForEmpty();

            var hasWindow = queueForWindow.ContainsKey(currentTopWindow);
            if (!hasWindow)
                return (null, null);

            var queue = queueForWindow[currentTopWindow];
            var res = Dequeue(queue);
            if (queue.Count == 0)
                queueForWindow.Remove(currentTopWindow);

            return res;
        }
        private (Window, Action<Window>) Dequeue(Queue<(Window, Action<Window>)> queue)
        {
            if (queue.Count == 0)
                return (null, null);

            var res = queue.Dequeue();
            while (res.Item1 == null && queue.Count > 0)
            {
                res = queue.Dequeue();
            }
            return res;
        }

        public bool IsQueueEmpty => emptyQueue.Count == 0;
    }
}
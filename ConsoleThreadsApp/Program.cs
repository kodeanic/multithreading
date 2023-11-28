using System.Collections.Concurrent;

namespace ConsoleThreadsApp;

class MyThreadPool : IDisposable
{
    private readonly Thread[] _threads;
    private readonly Queue<Action> _actions;

    private readonly object _syncRoot = new object();

    public MyThreadPool(int maxThread = 4)
    {
        _threads = new Thread[maxThread];
        _actions = new Queue<Action>();

        for (int i = 0; i < _threads.Length; i++)
        {
            _threads[i] = new Thread(ThreadProc)
            {
                IsBackground = true,
                Name = $"MyThreadPool Thread {i}"
            };
            _threads[i].Start();
        }
    }

    public void Queue(Action action)
    {
        Monitor.Enter(_syncRoot);
        try
        {
            _actions.Enqueue(action);
            if (_actions.Count == 1)
            {
                Monitor.Pulse(_syncRoot);
            }
        }
        finally
        {
            Monitor.Exit(_syncRoot);
        }
    }

    private void ThreadProc()
    {
        while (true)
        {
            Action action;
            Monitor.Enter(_syncRoot);
            try
            {
                if (IsDisposed)
                {
                    return;
                }

                if (_actions.Count > 0)
                {
                    action = _actions.Dequeue();
                }
                else
                {
                    Monitor.Wait(_syncRoot);
                    continue;
                }
            }
            finally
            {
                Monitor.Exit(_syncRoot);
            }

            action();
        }
    }

    public bool IsDisposed { get; private set; }

    public void Dispose()
    {
        bool isDisposing = false;
        if (!IsDisposed)
        {
            Monitor.Enter(_syncRoot);
            try
            {
                if (!IsDisposed)
                {
                    IsDisposed = true;
                    Monitor.PulseAll(_syncRoot);
                    isDisposing = true;
                }
            }
            finally
            {
                Monitor.Exit(_syncRoot);
            }
        }

        if (isDisposing)
        {
            for (int i = 0; i < _threads.Length; i++)
            {
                _threads[i].Join();
            }
        }
    }
}

class NeuroNet
{
    public void Process(byte[] data)
    {
        // do something
    }
}

class NeuroNetPool
{
    // Реализация кэша
    private readonly ConcurrentBag<NeuroNet> _cache;

    public NeuroNetPool()
    {
        _cache = new ConcurrentBag<NeuroNet>();
    }

    private NeuroNet GetNet()
    {
        if (!_cache.TryTake(out var neuroNet))
        {
            neuroNet = new NeuroNet();
        }

        return neuroNet;
    }

    public void Process(byte[] data)
    {
        var nn = GetNet();
        try
        {
            nn.Process(data);
        }
        finally
        {
            _cache.Add(nn);
        }
    }
}

static class Program
{
    static void Main(string[] args)
    {
        // Запуск программы в единственном экземпляре
        using (new Mutex(true, "MyMutex.48484993", out var createdNew))
        {
            if (!createdNew) return;
            using (var pool = new MyThreadPool())
            {
                pool.Queue(() => Console.WriteLine("From Thread Pool"));
            }

            Console.ReadKey();
        }

        // System.Threading
        var mutex = new Mutex(); // аналогично lock, критическая секция
        var sem = new Semaphore(initialCount: 5, maximumCount: 5); // Mutex для нескольких потоков
        var mevt = new ManualResetEvent(initialState: false);
        var aevt = new AutoResetEvent(initialState: false);

        // Примеры возможных взаимодействий:
        // mevt.Set();
        // mevt.Reset();
        // mevt.WaitOne();
        // WaitHandle.WaitAll(new WaitHandle[] { mutex, sem, mevt, aevt });
        // Необходимо уничтожать
        //mutex.Dispose();
    }
}
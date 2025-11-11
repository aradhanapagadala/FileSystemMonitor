using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.ComponentModel;
using System.Diagnostics;

namespace TestBed
{
    public enum FileSystemMonitorEventType
    {
        Create,
        Change,
        Rename,
        Delete,
        Error
    }

    public class FileSystemMonitor : IDisposable
    {
        public String Path { get; private set; }
        public EventHandler<FileSystemMonitorEvent> OnEvent { get; set; } = null!;
        private readonly object queueLock = new object();
        public Boolean EventInQueue
        {
            get
            {
                lock (queueLock)
                {
                    if (_Events.Count != 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }

        private FileSystemWatcher _Watcher;
        private Queue<FileSystemMonitorEvent> _Events = [];

        private bool useQueues;

        public FileSystemMonitor(String path, String filter = "*", Boolean recursive = true, bool UseQueues = false)
        {
            Path = path;
            _Watcher = new FileSystemWatcher(path, filter);
            _Watcher.IncludeSubdirectories = true;
            useQueues = UseQueues;

            _Watcher.NotifyFilter = NotifyFilters.Attributes
                                | NotifyFilters.CreationTime
                                | NotifyFilters.DirectoryName
                                | NotifyFilters.FileName
                                | NotifyFilters.LastAccess
                                | NotifyFilters.LastWrite
                                | NotifyFilters.Security
                                | NotifyFilters.Size;
        }

        private void HandleCreated(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine($"Create: {e}");

            var monitoring = new FileSystemMonitorEvent();
            monitoring.Path = e.FullPath;
            monitoring.EventType = FileSystemMonitorEventType.Create;

            OnEvent?.Invoke(this, monitoring);

            if (useQueues == true)
            {
                lock (queueLock)
                {
                    _Events.Enqueue(monitoring);
                }
            }
        }

        private void HandleDeleted(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine($"Delete: {e.FullPath}");

            var monitoring = new FileSystemMonitorEvent();
            monitoring.Path = e.FullPath;
            monitoring.EventType = FileSystemMonitorEventType.Delete;

            OnEvent?.Invoke(this, monitoring);

            if (useQueues == true)
            {
                lock (queueLock)
                {
                    _Events.Enqueue(monitoring);
                }
            }
        }

        private void HandleChanged(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine($"Change: {Enum.GetName(e.ChangeType)} {e.FullPath}");

            var monitoring = new FileSystemMonitorEvent();
            monitoring.Path = e.FullPath;
            monitoring.EventType = FileSystemMonitorEventType.Change;

            OnEvent?.Invoke(this, monitoring);

            if (useQueues == true)
            {
                lock (queueLock)
                {
                    _Events.Enqueue(monitoring);
                }
            }
        }

        private void HandleRenamed(object sender, RenamedEventArgs e)
        {
            Console.WriteLine($"Rename: {e.OldName} => {e.Name}");

            var monitoring = new FileSystemMonitorEvent();
            monitoring.Path = e.FullPath;
            monitoring.EventType = FileSystemMonitorEventType.Rename;

            OnEvent?.Invoke(this, monitoring);

            if (useQueues == true)
            {
                lock (queueLock)
                {
                    _Events.Enqueue(monitoring);
                }
            }
        }

        private void HandleError(object sender, ErrorEventArgs e)
        {
            Console.WriteLine($"Error: {e.GetException().Message}");

            var monitoring = new FileSystemMonitorEvent();
            monitoring.Path = Path;
            monitoring.EventType = FileSystemMonitorEventType.Error;

            OnEvent?.Invoke(this, monitoring);

            if (useQueues == true)
            {
                lock (queueLock)
                {
                    _Events.Enqueue(monitoring);
                }
            }
        }


        /// <summary>
        /// Starts the monitoring of the path
        /// </summary>
        public void Start()
        {
            _Watcher.Created += HandleCreated;
            _Watcher.Deleted += HandleDeleted;
            _Watcher.Changed += HandleChanged;
            _Watcher.Renamed += HandleRenamed;
            _Watcher.Error += HandleError;

            _Watcher.EnableRaisingEvents = true;


        }

        /// <summary>
        /// Stops all monitoring on the path
        /// </summary>
        public void Stop()
        {
            _Watcher.Created -= HandleCreated;
            _Watcher.Deleted -= HandleDeleted;
            _Watcher.Changed -= HandleChanged;
            _Watcher.Renamed -= HandleRenamed;
            _Watcher.Error -= HandleError;

            _Watcher.EnableRaisingEvents = false;

        }

        /// <summary>
        /// Returns the next event in the queue or null if the queue is empty
        /// </summary>
        /// <returns>The file system event</returns>
        public FileSystemMonitorEvent? Dequeue()
        {
            lock (queueLock)
            {
                if (_Events.Count > 0)
                {
                    return _Events.Dequeue();
                }
                else
                {
                    return null;
                }
            }
        }

        bool disposed;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    Stop();
                }

            }

            _Watcher.Dispose();
            disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);

        }
    }

    public class FileSystemMonitorEvent
    {
        public String Path { get; set; } = default!;
        public FileSystemMonitorEventType EventType { get; set; }
    }
}

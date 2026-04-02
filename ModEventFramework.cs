using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace AssetExporter
{
    public interface IModEvent
    {
        DateTime OccurredAtUtc { get; }
    }

    public sealed class ModEventHub
    {
        private readonly ConcurrentDictionary<Type, List<Delegate>> _handlers = new ConcurrentDictionary<Type, List<Delegate>>();
        private readonly object _gate = new object();

        public IDisposable Subscribe<TEvent>(Action<TEvent> handler) where TEvent : IModEvent
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            var eventType = typeof(TEvent);
            lock (_gate)
            {
                if (!_handlers.TryGetValue(eventType, out var delegates))
                {
                    delegates = new List<Delegate>();
                    _handlers[eventType] = delegates;
                }

                delegates.Add(handler);
            }

            return new Subscription(() => Unsubscribe(handler));
        }

        public void Publish<TEvent>(TEvent evt) where TEvent : IModEvent
        {
            if (evt == null) return;

            List<Delegate> snapshot;
            lock (_gate)
            {
                if (!_handlers.TryGetValue(typeof(TEvent), out var delegates) || delegates.Count == 0)
                    return;

                snapshot = new List<Delegate>(delegates);
            }

            foreach (var handler in snapshot)
            {
                if (handler is Action<TEvent> typedHandler)
                    typedHandler(evt);
            }
        }

        public void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : IModEvent
        {
            if (handler == null) return;

            lock (_gate)
            {
                if (!_handlers.TryGetValue(typeof(TEvent), out var delegates))
                    return;

                delegates.Remove(handler);
                if (delegates.Count == 0)
                    _handlers.TryRemove(typeof(TEvent), out _);
            }
        }

        private sealed class Subscription : IDisposable
        {
            private readonly Action _disposeAction;
            private bool _disposed;

            public Subscription(Action disposeAction)
            {
                _disposeAction = disposeAction;
            }

            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;
                _disposeAction?.Invoke();
            }
        }
    }

    public static class ModFramework
    {
        public static ModEventHub Events { get; } = new ModEventHub();
    }

    public sealed class ModInitializedEvent : IModEvent
    {
        public ModInitializedEvent(DateTime occurredAtUtc, string version)
        {
            OccurredAtUtc = occurredAtUtc;
            Version = version;
        }

        public DateTime OccurredAtUtc { get; }
        public string Version { get; }
    }

    public sealed class ModTickEvent : IModEvent
    {
        public ModTickEvent(DateTime occurredAtUtc)
        {
            OccurredAtUtc = occurredAtUtc;
        }

        public DateTime OccurredAtUtc { get; }
    }

    public sealed class ExportStartedEvent : IModEvent
    {
        public ExportStartedEvent(DateTime occurredAtUtc, string exportPath)
        {
            OccurredAtUtc = occurredAtUtc;
            ExportPath = exportPath;
        }

        public DateTime OccurredAtUtc { get; }
        public string ExportPath { get; }
    }

    public sealed class ExportCompletedEvent : IModEvent
    {
        public ExportCompletedEvent(DateTime occurredAtUtc, string exportPath, int objectsScanned)
        {
            OccurredAtUtc = occurredAtUtc;
            ExportPath = exportPath;
            ObjectsScanned = objectsScanned;
        }

        public DateTime OccurredAtUtc { get; }
        public string ExportPath { get; }
        public int ObjectsScanned { get; }
    }

    public sealed class ToggleChangedEvent : IModEvent
    {
        public ToggleChangedEvent(DateTime occurredAtUtc, string toggleName, bool enabled)
        {
            OccurredAtUtc = occurredAtUtc;
            ToggleName = toggleName;
            Enabled = enabled;
        }

        public DateTime OccurredAtUtc { get; }
        public string ToggleName { get; }
        public bool Enabled { get; }
    }

    public sealed class ModErrorEvent : IModEvent
    {
        public ModErrorEvent(DateTime occurredAtUtc, string context, string message)
        {
            OccurredAtUtc = occurredAtUtc;
            Context = context;
            Message = message;
        }

        public DateTime OccurredAtUtc { get; }
        public string Context { get; }
        public string Message { get; }
    }
}

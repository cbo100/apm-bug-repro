using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Most implementations copied from Elastic.Apm
/// </summary>
namespace IDiagLib
{
    internal static class Logger
    {
        internal static void Log(string message)
        {
            // Console.WriteLine(message);
            Debug.WriteLine(message);
        }
    }

    public interface IDiagnosticListener : IObserver<KeyValuePair<string, object>>
    {
        /// <summary>
        /// Represents the component associated with the event.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; }
    }

    public class HttpDiagnosticListenerImplBase<TRequest, TResponse> : IDiagnosticListener
        where TRequest : class
        where TResponse : class
    {
        public string Name => "System.Net.Http.Desktop";
        internal string StartEventKey => "System.Net.Http.Desktop.HttpRequestOut.Start";
        internal string StopEventKey => "System.Net.Http.Desktop.HttpRequestOut.Stop";

        public void OnCompleted()
        {
            //
        }

        public void OnError(Exception error)
        {
            Logger.Log($"ERR: {error.Message}");
        }

        public void OnNext(KeyValuePair<string, object> value)
        {
            var url = "[-]";
            var requestObject = value.Value.GetType().GetTypeInfo().GetDeclaredProperty("Request")?.GetValue(value.Value);
            if (!(requestObject is null) && requestObject is HttpWebRequest)
            {
                var req = requestObject as HttpWebRequest;
                //req.Headers.Add("traceparent", "00-1234");
                url = $"{req.Method} {req.Host}";
            }


            Logger.Log($"INF: {value.Key} - {url}");
        }
    }


    public class DiagnosticInitializer : IObserver<DiagnosticListener>, IDisposable
    {
        private readonly IEnumerable<IDiagnosticListener> _listeners;

        public DiagnosticInitializer(IEnumerable<IDiagnosticListener> listeners)
        {
            _listeners = listeners;
        }

        private IDisposable _sourceSubscription;

        public void OnCompleted() { }

        public void OnError(Exception error) { }

        public void OnNext(DiagnosticListener value)
        {
            var subscribedAny = false;
            foreach (var listener in _listeners)
            {
                if (value.Name == listener.Name)
                {
                    _sourceSubscription = value.Subscribe(listener);
                    Logger.Log($"Subscribed {listener.GetType().FullName} to `{value.Name}' events source");
                    subscribedAny = true;
                }
            }

            if (!subscribedAny)
            {
                Logger.Log($"There are no listeners in the current batch ({string.Join(", ", _listeners.Select(listener => listener.GetType().FullName))}) that would like to subscribe to `{value.Name}' events source");
            }
        }

        public void Dispose() => _sourceSubscription?.Dispose();
    }


    public class CompositeDisposable : IDisposable
    {
        private readonly List<IDisposable> _disposables = new List<IDisposable>();
        private readonly object _lock = new object();

        private bool _isDisposed;

        public CompositeDisposable Add(IDisposable disposable)
        {
            if (_isDisposed) throw new ObjectDisposedException(nameof(CompositeDisposable));

            _disposables.Add(disposable);
            return this;
        }

        public void Dispose()
        {
            if (_isDisposed) return;

            lock (_lock)
            {
                if (_isDisposed) return;

                _isDisposed = true;
                foreach (var d in _disposables) d.Dispose();
            }
        }
    }
}

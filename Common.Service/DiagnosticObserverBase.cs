using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace VH.MiniService.Common.Service
{
    public abstract class DiagnosticObserverBase : IObserver<DiagnosticListener>
    {
        private readonly List<IDisposable> _subscriptions = new();

        protected abstract bool IsMatch(string name);

        void IObserver<DiagnosticListener>.OnNext(DiagnosticListener diagnosticListener)
        {
            if (!IsMatch(diagnosticListener.Name)) return;

            var subscription = diagnosticListener.SubscribeWithAdapter(this);
            _subscriptions.Add(subscription);
        }

        void IObserver<DiagnosticListener>.OnError(Exception error)
        { }

        void IObserver<DiagnosticListener>.OnCompleted()
        {
            _subscriptions.ForEach(x => x.Dispose());
            _subscriptions.Clear();
        }
    }
}

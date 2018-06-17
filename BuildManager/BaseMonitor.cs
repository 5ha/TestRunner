using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildManager
{
    public abstract class BaseMonitor<T> : IObservable<T> where T : class
    {
        private List<IObserver<T>> _observers;

        public BaseMonitor()
        {
            _observers = new List<IObserver<T>>();
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            if (!_observers.Contains(observer)) _observers.Add(observer);

            return new Unsubscriber<T>(_observers, observer);
        }

        internal void notifyNext(T mess)
        {
            foreach (var observer in _observers)
            {
                observer.OnNext(mess);
            }
        }

        internal void notifyComplete()
        {
            foreach (var observer in _observers)
            {
                observer.OnCompleted();
            }
        }

        internal void notifyError(Exception err)
        {
            foreach (var observer in _observers)
            {
                observer.OnError(err);
            }
        }

        private class Unsubscriber<T> : IDisposable
        {
            private readonly List<IObserver<T>> _observers;
            private readonly IObserver<T> _observer;

            public Unsubscriber(List<IObserver<T>> observers, IObserver<T> observer)
            {
                _observers = observers;
                _observer = observer;
            }

            public void Dispose()
            {
                if (!(_observer == null)) _observers.Remove(_observer);
            }
        }
    }
}

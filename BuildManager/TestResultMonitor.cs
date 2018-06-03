using BuildManager.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildManager
{
    public class TestResultMonitor : IObservable<TestResult>
    {
        public TestResultMonitor()
        {
            _observers = new List<IObserver<TestResult>>();
        }

        private List<IObserver<TestResult>> _observers;

        public IDisposable Subscribe(IObserver<TestResult> observer)
        {
            if (!_observers.Contains(observer)) _observers.Add(observer);

            return new Unsubscriber(_observers, observer);
        }

        internal void notifyNext(TestResult testResult)
        {
            foreach(var observer in _observers)
            {
                observer.OnNext(testResult);
            }
        }

        internal void notifyComplete()
        {
            foreach(var observer in _observers)
            {
                observer.OnCompleted();
            }
        }

        private class Unsubscriber : IDisposable
        {
            private readonly List<IObserver<TestResult>> _observers;
            private readonly IObserver<TestResult> _observer;

            public Unsubscriber(List<IObserver<TestResult>> observers, IObserver<TestResult> observer)
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

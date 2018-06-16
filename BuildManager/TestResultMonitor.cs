using BuildManager.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildManager
{
    public class TestResultMonitor : IObservable<TestExecutionResult>
    {
        public TestResultMonitor()
        {
            _observers = new List<IObserver<TestExecutionResult>>();
        }

        private List<IObserver<TestExecutionResult>> _observers;

        public IDisposable Subscribe(IObserver<TestExecutionResult> observer)
        {
            if (!_observers.Contains(observer)) _observers.Add(observer);

            return new Unsubscriber(_observers, observer);
        }

        internal void notifyNext(TestExecutionResult testResult)
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
            private readonly List<IObserver<TestExecutionResult>> _observers;
            private readonly IObserver<TestExecutionResult> _observer;

            public Unsubscriber(List<IObserver<TestExecutionResult>> observers, IObserver<TestExecutionResult> observer)
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

using BuildManager.Model;
using MessageModels;
using System;

namespace Common
{
    public interface ITestRunObserver : IObserver<TestExecutionResult>, IObserver<StatusMessage>
    {
    }
}

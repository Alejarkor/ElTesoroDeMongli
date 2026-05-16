using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Networking;

namespace ElTesoroDeMongli.API
{
    public static class UnityWebRequestAwaiterExtensions
    {
        public static UnityWebRequestAsyncOperationAwaiter GetAwaiter(this UnityWebRequestAsyncOperation asyncOperation)
        {
            return new UnityWebRequestAsyncOperationAwaiter(asyncOperation);
        }
    }

    public class UnityWebRequestAsyncOperationAwaiter : INotifyCompletion
    {
        private readonly UnityWebRequestAsyncOperation asyncOperation;
        private Action continuation;

        public UnityWebRequestAsyncOperationAwaiter(UnityWebRequestAsyncOperation asyncOperation)
        {
            this.asyncOperation = asyncOperation;
            asyncOperation.completed += OnCompleted;
        }

        public bool IsCompleted => asyncOperation.isDone;

        public void GetResult()
        {
        }

        public void OnCompleted(Action continuation)
        {
            this.continuation = continuation;
        }

        private void OnCompleted(AsyncOperation operation)
        {
            continuation?.Invoke();
        }
    }
}

## Overview

- `TaskCompletionSource<T>` is a powerful class in .NET used to create and control `Task` objects manually. It allows you to create a `Task` that can be completed, faulted, or canceled externally, giving you more fine-grained control over asynchronous operations.
- Using `Task` generally we have to wait for the asynchronous operation to Finish . But if we return `Task` from consumer side using  `TaskCompletionSource<T>` we can manually return it by using inbuilt methods of `TaskCompletionSource<T>` .
- `TaskCompletionSource<T>` class represents the producer side of a Task<TResult> unbound to a delegate, providing access to the consumer side through the Task property.
## Use Cases

### 1. Bridging Asynchronous Patterns

`TaskCompletionSource<T>` is often used to bridge older asynchronous patterns with the newer `Task`-based asynchronous pattern (`async/await`). For example:

```csharp
public Task<string> GetDataAsync()
{
    var tcs = new TaskCompletionSource<string>();
    result = "Sample Data";
        try
        {
            string data = result;
            tcs.SetResult(data);
        }
        catch (Exception ex)
        {
            tcs.SetException(ex);
        }

    return tcs.Task;
}
```

### 2. Manually Completing Tasks

In scenarios where the completion of a task depends on **external events or signals**, `TaskCompletionSource<T>` allows you to **manually** complete a task when those events occur.

Example: You might be waiting for a callback from an external service to complete a task.

```csharp
public Task<string> WaitForCallbackAsync()
{
    var tcs = new TaskCompletionSource<string>();

    SomeService.OnCallback += (data) =>
    {
        tcs.SetResult(data);
    };

    return tcs.Task;
}
```

### 3. Creating Task-based APIs

You can create task-based APIs for operations that might not naturally be task-based. This is useful when you want to expose an asynchronous API but need more control over when the task is considered completed.

Example: Creating a method that waits for a condition to be met:

```csharp
public Task WaitForConditionAsync(Func<bool> condition)
{
    var tcs = new TaskCompletionSource<object>();

    Task.Run(async () =>
    {
        while (!condition())
        {
            await Task.Delay(100);
        }

        tcs.SetResult(null); ///ON COMPLETION
    });

    return tcs.Task;
}
```

### 4. Handling Timeouts

You can use `TaskCompletionSource<T>` to handle timeouts in asynchronous operations by setting a result, exception, or cancellation if the operation exceeds a specified time limit.

```csharp
public async Task<string> GetDataWithTimeoutAsync(TimeSpan timeout)
{
    var tcs = new TaskCompletionSource<string>();

    // Simulate async work
    var workTask = Task.Run(async () =>
    {
        await Task.Delay(5000); // Simulating long-running task
        tcs.TrySetResult("Data retrieved");
    });

    if (await Task.WhenAny(workTask, Task.Delay(timeout)) == workTask)
    {
        return await tcs.Task; // The workTask completed within timeout
    }
    else
    {
        tcs.TrySetCanceled(); // Timeout occurred
        throw new TimeoutException("The operation timed out.");
    }
}
```

## Notes

- **Thread Safety**: `TaskCompletionSource<T>` is thread-safe, meaning you can safely complete a task from different threads.
  
- **Setting Task State**: The task created by `TaskCompletionSource<T>` can be completed, faulted, or canceled using the `SetResult`, `SetException`, or `SetCanceled` methods, respectively.

- **Avoiding Deadlocks**: Be cautious with synchronization contexts, particularly in UI applications. If you're completing a `TaskCompletionSource` on the UI thread, make sure it doesn't lead to deadlocks.

- **Exception Handling**: Always handle exceptions in your asynchronous operations. You can set the exception on the `TaskCompletionSource<T>` to propagate it to the awaiting code.

- **Cancellation Tokens**: Although `TaskCompletionSource<T>` itself doesn't support cancellation tokens directly, you can handle cancellation manually by linking a `CancellationToken` to the task's completion state.


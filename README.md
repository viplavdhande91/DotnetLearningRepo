# Dependency Injection & Service Lifetimes in .NET

Simple learning notes for ASP.NET Core (.NET 8 and later).

## Contents

1. [What is Dependency Injection?](#1-what-is-dependency-injection)
2. [How the DI Container Works](#2-how-the-di-container-works)
3. [Ways to Receive a Service](#3-ways-to-receive-a-service)
4. [Service Lifetimes](#4-service-lifetimes)
5. [See Lifetimes in Action](#5-see-lifetimes-in-action)
6. [The Captive Dependency Trap](#6-the-captive-dependency-trap)
7. [Handy Registration Tricks](#7-handy-registration-tricks)
8. [More Concepts Worth Knowing](#8-more-concepts-worth-knowing)
9. [Best Practices and Common Mistakes](#9-best-practices-and-common-mistakes)
10. [Cheat Sheet](#10-cheat-sheet)
11. [Practice Exercises](#11-practice-exercises)

---

## 1. What is Dependency Injection?

A **dependency** is any object that your class needs to do its work.

Example: `OrderService` needs something to send messages. The beginner way is to create it inside the class:

```csharp
public class OrderService
{
    private readonly EmailSender _sender = new EmailSender();   // created here

    public void PlaceOrder() => _sender.Send("Order placed");
}
```

This causes three problems:

- **Hard to change.** To use SMS instead of email, you must edit `OrderService`.
- **Hard to test.** You cannot swap in a fake sender, so tests send real emails.
- **Hidden needs.** You must read the whole class to see what it depends on.

**Dependency Injection (DI)** fixes this. The class does not create its dependencies. It simply asks for them in the constructor, and someone else provides them.

```csharp
public interface IMessageSender
{
    void Send(string message);
}

public class EmailSender : IMessageSender
{
    public void Send(string message) => Console.WriteLine($"Email: {message}");
}

public class OrderService
{
    private readonly IMessageSender _sender;

    public OrderService(IMessageSender sender)   // "please give me a sender"
    {
        _sender = sender;
    }

    public void PlaceOrder() => _sender.Send("Order placed");
}
```

```text
WITHOUT DI                                 WITH DI

 OrderService                               OrderService
      |                                          |
      | creates it itself with "new"             | only knows the interface
      v                                          v
 EmailSender                                IMessageSender
 (stuck with it)                              ^          ^
                                              |          |
                                        EmailSender   SmsSender
                                        (swap with one line of config)
```

### Words you will hear

| Word | Simple meaning |
|---|---|
| **Dependency** | An object another class needs |
| **Inversion of Control (IoC)** | The idea that a class should not build its own dependencies |
| **Dependency Injection** | The technique that applies IoC: dependencies are passed in |
| **DI Container** | The built-in .NET tool that creates and delivers the objects |
| **Service** | A class you have registered in the container |

---

## 2. How the DI Container Works

The container does three jobs:

```text
 1. REGISTER   You tell it: "when someone asks for IMessageSender,
                give them EmailSender."

 2. RESOLVE    When a class needs something, it builds that object
                (and everything that object needs, too).

 3. DISPOSE    When the work is over, it cleans up the objects it created.
```

You do the registering in `Program.cs`:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IMessageSender, EmailSender>();   // interface -> class
builder.Services.AddScoped<OrderService>();                  // class only

var app = builder.Build();
app.Run();
```

What happens when a request comes in:

```text
Request arrives
   |
   v
Container needs OrderController
   -> it needs OrderService
        -> it needs IMessageSender
             -> container creates EmailSender
        <- container gives EmailSender to OrderService
   <- container gives OrderService to OrderController
   |
   v
Controller runs, response is sent, container cleans up
```

You never write `new` for these classes. The container builds the whole chain for you.

---

## 3. Ways to Receive a Service

**Constructor injection (use this most of the time):**

```csharp
public class OrderController : ControllerBase
{
    private readonly OrderService _orders;

    public OrderController(OrderService orders) => _orders = orders;
}

// C# 12 shortcut (primary constructor):
public class OrderController(OrderService orders) : ControllerBase { }
```

**Minimal API (parameters are filled in automatically):**

```csharp
app.MapGet("/order", (OrderService orders) => orders.PlaceOrder());
```

**Only one action needs it (`[FromServices]`):**

```csharp
public IActionResult Get([FromServices] IMessageSender sender) => Ok();
```

> Avoid asking the container directly with `serviceProvider.GetService<T>()` in normal code. It hides what your class needs. Use it only in special infrastructure code.

---

## 4. Service Lifetimes

The lifetime answers one question: **how long does an object live, and who shares it?**

Think of a tea shop:

| Lifetime | Tea shop idea | Real meaning | Register with |
|---|---|---|---|
| **Transient** | A new paper cup every time you ask | A new object **every time** it is requested | `AddTransient` |
| **Scoped** | One cup per customer visit | One object **per HTTP request** | `AddScoped` |
| **Singleton** | One big kettle for the whole shop | One object for the **whole app** | `AddSingleton` |

### Who gets which object?

Two requests come in. In each one, `ServiceA` and `ServiceB` both ask for the same service.

```text
                  Request 1               Request 2
                 A         B             A         B

Transient       [#1]      [#2]          [#3]      [#4]        4 objects

Scoped          [#1]------[#1]          [#2]------[#2]        2 objects

Singleton       [#1]------[#1]----------[#1]------[#1]        1 object
```

- **Transient:** everyone gets their own new object.
- **Scoped:** shared inside one request, new for the next request.
- **Singleton:** shared by everyone, always.

### When to use which

| Lifetime | Use it for | Examples |
|---|---|---|
| **Transient** | Small, stateless helpers | Validators, mappers, formatters |
| **Scoped** | Work that belongs to one request | `DbContext`, repositories, current user |
| **Singleton** | Shared, thread-safe data or expensive objects | Cache, configuration, clock |

How to pick quickly:

```text
Needs to be shared by the whole app (and is thread-safe)?  --> Singleton
Uses DbContext or holds per-request data?                  --> Scoped
Small and stateless?                                       --> Transient
Not sure?                                                  --> Scoped
```

### When are objects cleaned up?

- **Transient and Scoped:** when the request (scope) ends.
- **Singleton:** when the app shuts down.

The container disposes what it creates. Do not call `Dispose()` on an injected service yourself.

---

## 5. See Lifetimes in Action

Give each object a unique ID, then print it.

```csharp
public interface ITransientService { Guid Id { get; } }
public interface IScopedService    { Guid Id { get; } }
public interface ISingletonService { Guid Id { get; } }

public class OperationService : ITransientService, IScopedService, ISingletonService
{
    public Guid Id { get; } = Guid.NewGuid();   // new ID for every new object
}
```

```csharp
// Program.cs
builder.Services.AddTransient<ITransientService, OperationService>();
builder.Services.AddScoped<IScopedService, OperationService>();
builder.Services.AddSingleton<ISingletonService, OperationService>();

app.MapGet("/lifetimes", (
    ITransientService t1, ITransientService t2,
    IScopedService s1,    IScopedService s2,
    ISingletonService g1, ISingletonService g2) => new
{
    Transient = new[] { t1.Id, t2.Id },
    Scoped    = new[] { s1.Id, s2.Id },
    Singleton = new[] { g1.Id, g2.Id }
});
```

Open `/lifetimes`, then refresh the page. The IDs behave like this:

| | First visit | After refresh |
|---|---|---|
| **Transient** (2 IDs) | A, B (different) | C, D (all new) |
| **Scoped** (2 IDs) | E, E (same) | F, F (new, but same pair) |
| **Singleton** (2 IDs) | G, G (same) | G, G (never changes) |

---

## 6. The Captive Dependency Trap

### The rule

> A service should only depend on services that live **as long or longer** than itself.

```text
Short life  <  Medium life  <  Long life
Transient   <     Scoped    <  Singleton

OK:   Scoped    uses  Singleton
OK:   Transient uses  Scoped or Singleton
BAD:  Singleton uses  Scoped or Transient
```

### Why it is bad

A Singleton lives forever, so anything it holds also lives forever. The short-lived object gets "captured".

```text
+--------------------------+              +--------------------------+
| SINGLETON: ReportCache   |  holds on to | SCOPED: AppDbContext     |
| lives for the whole app  | -----------> | should end after request |
+--------------------------+              +--------------------------+
                    The DbContext is stuck and never released.
```

```csharp
builder.Services.AddScoped<AppDbContext>();
builder.Services.AddSingleton<ReportCache>();

public class ReportCache
{
    private readonly AppDbContext _db;                    // BAD: captured forever
    public ReportCache(AppDbContext db) => _db = db;
}
```

Result: stale data, leaked connections, and random errors, because `DbContext` is not thread-safe.

### The fix: open a short scope when needed

```csharp
public class ReportCache
{
    private readonly IServiceScopeFactory _scopeFactory;

    public ReportCache(IServiceScopeFactory scopeFactory) => _scopeFactory = scopeFactory;

    public async Task RefreshAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        // use db here
    }   // db is cleaned up here
}
```

### Let .NET warn you

In the Development environment, ASP.NET Core checks this automatically and throws:

```text
Cannot consume scoped service 'AppDbContext' from singleton 'ReportCache'.
```

In other apps (like a console app), turn it on yourself:

```csharp
var provider = services.BuildServiceProvider(new ServiceProviderOptions
{
    ValidateScopes  = true,   // catches captive dependencies
    ValidateOnBuild = true    // catches missing registrations
});
```

---

## 7. Handy Registration Tricks

**Many classes for one interface.** The last registered one wins for a normal request. Ask for `IEnumerable<T>` to get all of them.

```csharp
builder.Services.AddScoped<IMessageSender, EmailSender>();
builder.Services.AddScoped<IMessageSender, SmsSender>();

public class Notifier(IEnumerable<IMessageSender> senders) { }   // gets both
```

**Keyed services (.NET 8+).** Choose one by name.

```csharp
builder.Services.AddKeyedScoped<IMessageSender, EmailSender>("email");
builder.Services.AddKeyedScoped<IMessageSender, SmsSender>("sms");

public class Alerts([FromKeyedServices("sms")] IMessageSender sender) { }
```

**Factory.** Use it when building the object needs custom logic.

```csharp
builder.Services.AddScoped<IMessageSender>(sp =>
    sp.GetRequiredService<IConfiguration>()["Sender"] == "sms"
        ? new SmsSender()
        : new EmailSender());
```

**`TryAdd`.** Register only if nothing is registered yet (handy for libraries).

```csharp
builder.Services.TryAddScoped<IMessageSender, EmailSender>();
```

**Group your registrations** in an extension method to keep `Program.cs` tidy:

```csharp
public static IServiceCollection AddOrderModule(this IServiceCollection services)
{
    services.AddScoped<IOrderRepository, OrderRepository>();
    services.AddScoped<OrderService>();
    return services;
}

// Program.cs
builder.Services.AddOrderModule();
```

---

## 8. More Concepts Worth Knowing

### 8.1 DI in a console app (no ASP.NET Core)

DI is not only for web apps. Without a web host, you build the container yourself.

```csharp
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddScoped<IMessageSender, EmailSender>();
services.AddScoped<OrderService>();

using var provider = services.BuildServiceProvider();

// There is no HTTP request, so YOU decide where a scope starts and ends.
using (var scope = provider.CreateScope())
{
    var orders = scope.ServiceProvider.GetRequiredService<OrderService>();
    orders.PlaceOrder();
}   // scoped services are disposed here
```

In a web app, ASP.NET Core opens one scope for every HTTP request. In a console app, a "scope" is whatever you create with `CreateScope()`.

### 8.2 Scoped services in middleware and background services

Two places where people often get stuck:

**Middleware** is created **once**, like a Singleton. So never put a Scoped service in its constructor. Put it in the `InvokeAsync` method instead.

```csharp
public class LoggingMiddleware
{
    private readonly RequestDelegate _next;

    public LoggingMiddleware(RequestDelegate next) => _next = next;   // OK: Singleton-like

    public async Task InvokeAsync(HttpContext context, IScopedService scoped)   // OK: scoped here
    {
        // use scoped here
        await _next(context);
    }
}
```

**`BackgroundService`** is registered as a Singleton. If it needs a Scoped service (like `DbContext`), open a scope each time it works:

```csharp
public class CleanupWorker(IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            // do the work with db
            await Task.Delay(TimeSpan.FromMinutes(5), token);
        }
    }
}
```

### 8.3 Singletons must be thread-safe

Many requests run at the same time, and they all share the same Singleton. Changing its data without protection causes bugs that appear only now and then.

```csharp
// BAD: two requests can read and write at the same moment
public class VisitCounter
{
    private int _count;
    public int Increase() => ++_count;
}

// GOOD: the increase is atomic
public class VisitCounter
{
    private int _count;
    public int Increase() => Interlocked.Increment(ref _count);
}
```

Safe options: keep the Singleton **read-only**, use `lock`, use `Interlocked`, or use thread-safe collections such as `ConcurrentDictionary`.

### 8.4 Transient inside a Singleton is also captive

Section 6 showed Scoped inside Singleton. A Transient has the same problem. The Singleton gets **one** Transient object when it is created and keeps it forever, so it stops behaving like a Transient.

```text
Singleton (created once)
    |
    +--> Transient (created once too, then stuck inside the Singleton)
```

If the Singleton needs a fresh object each time, inject a factory (`Func<T>`) or use `IServiceScopeFactory`.

### 8.5 Do not use `new HttpClient()`; use `IHttpClientFactory`

Creating `HttpClient` yourself for every call can use up all the network sockets. The factory reuses connections safely.

```csharp
builder.Services.AddHttpClient<WeatherClient>(client =>
{
    client.BaseAddress = new Uri("https://api.example.com/");
});

public class WeatherClient(HttpClient http)    // injected, ready to use
{
    public Task<string> GetAsync() => http.GetStringAsync("forecast");
}
```

### 8.6 Typed settings with the Options pattern

Instead of reading `IConfiguration` everywhere, map a section of `appsettings.json` to a class.

```csharp
public class EmailSettings
{
    public string Host { get; set; } = "";
    public int Port { get; set; }
}

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("Email"));

public class EmailSender(IOptions<EmailSettings> options)
{
    private readonly EmailSettings _settings = options.Value;
}
```

| Interface | Lifetime | Picks up changes in the file? |
|---|---|---|
| `IOptions<T>` | Singleton | No (read once) |
| `IOptionsSnapshot<T>` | Scoped | Yes, on each new request |
| `IOptionsMonitor<T>` | Singleton | Yes, right away |

### 8.7 DI makes testing easy

Because `OrderService` asks for an `IMessageSender`, a test can hand it a fake one. No real email is sent.

```csharp
public class FakeSender : IMessageSender
{
    public List<string> Sent { get; } = new();
    public void Send(string message) => Sent.Add(message);
}

[Fact]
public void PlaceOrder_SendsOneMessage()
{
    var fake = new FakeSender();
    var service = new OrderService(fake);     // no container needed

    service.PlaceOrder();

    Assert.Single(fake.Sent);
}
```

### 8.8 Who disposes what?

- Objects the **container creates** are disposed by the container (`IDisposable` and `IAsyncDisposable`).
- Objects **you create** with `new` and give to the container as a ready-made instance are **not** disposed by it. You must dispose them.
- Transient objects are disposed when their **scope** ends, not right after use. If you create many Transient `IDisposable` objects inside a long-running scope (like a Singleton or a loop), they pile up in memory until the scope ends.

### 8.9 Constructor injection and the dependency graph

The container builds a whole **tree** of objects, not just one.

```text
OrderController
   |
   +--> OrderService
           |
           +--> IOrderRepository --> AppDbContext
           |
           +--> IMessageSender   --> EmailSender --> IOptions<EmailSettings>
```

Two things follow from this:

- If one object in the tree is missing, the whole request fails with "Unable to resolve service".
- A deep or wide tree is a warning sign. If a class needs too many things, split it into smaller classes.

---

## 9. Best Practices and Common Mistakes

### Do

- Depend on **interfaces**, not concrete classes.
- Prefer **constructor injection**.
- Keep constructors small. Five or more parameters usually means the class does too much.
- Register `DbContext` as **Scoped**.
- Make Singletons **thread-safe** and avoid changing shared data.
- Choose the **shortest lifetime** that works.
- Turn on `ValidateScopes` and `ValidateOnBuild` while developing.
- Use `IHttpClientFactory` instead of `new HttpClient()`.

### Common mistakes

| Mistake | What happens | Fix |
|---|---|---|
| Scoped service inside a Singleton | Captive dependency, stale data | Use `IServiceScopeFactory`, or change a lifetime |
| Forgetting to register a service | `Unable to resolve service for type ...` | Add it in `Program.cs` |
| Circular dependency (A needs B, B needs A) | Error when resolving | Split the shared logic into a third class |
| Using `new` for a service | Skips DI, hard to test | Inject it instead |
| Registering the class, injecting the interface | Cannot resolve | Register as `AddScoped<IFoo, Foo>()` |

A circular dependency looks like this:

```text
ServiceA ---> ServiceB ---> ServiceC
   ^                           |
   |___________________________|     (endless loop, so the container fails)
```

How to read the most common error:

```text
Unable to resolve service for type 'IMessageSender'
while attempting to activate 'OrderService'.
```

It means `OrderService` asked for `IMessageSender`, but you never registered it.

---

## 10. Cheat Sheet

```text
Register:   builder.Services.AddXxx<IService, Implementation>();
Receive:    public MyClass(IService service) { ... }

Transient   new object every time           small, stateless helpers
Scoped      one object per HTTP request     DbContext, repositories
Singleton   one object for the whole app    cache, configuration

Golden rule:    depend on services that live as long or longer
Never:          Singleton -> Scoped
Escape hatch:   IServiceScopeFactory.CreateScope()
```

| I want to... | Use |
|---|---|
| Get a fresh object each time | `AddTransient` |
| Share one object inside a request | `AddScoped` |
| Share one object across the whole app | `AddSingleton` |
| Get all implementations | Inject `IEnumerable<T>` |
| Pick an implementation by name | `AddKeyedXxx` + `[FromKeyedServices]` |
| Use a scoped service inside a Singleton | `IServiceScopeFactory` |
| Use a scoped service in middleware | Inject it into `InvokeAsync`, not the constructor |
| Use DI in a console app | `ServiceCollection` + `BuildServiceProvider()` |
| Read typed settings | `IOptions<T>` / `IOptionsSnapshot<T>` / `IOptionsMonitor<T>` |

---

## 11. Practice Exercises

1. Build the `/lifetimes` endpoint from section 5 and refresh it a few times. Watch which IDs change.
2. Create `IMessageSender` with `EmailSender` and `SmsSender`. Switch between them by changing one line in `Program.cs`.
3. Break it on purpose: make a Singleton depend on a Scoped service, read the error, then fix it with `IServiceScopeFactory`.
4. Write a unit test for `OrderService` using a fake `IMessageSender`.
5. Make two services depend on each other, read the error, then remove the loop.
6. Build a small console app that uses `ServiceCollection` and `BuildServiceProvider` (section 8.1). Resolve a service from two different scopes and compare IDs.
7. Write a middleware that needs a Scoped service. First inject it in the constructor and watch it fail, then move it to `InvokeAsync` (section 8.2).
8. Create a Singleton counter. Increase it from many threads with a plain `int`, then with `Interlocked.Increment`, and compare the results (section 8.3).

---

## Further Reading

- Microsoft Docs: *Dependency injection in ASP.NET Core*
- Microsoft Docs: *Dependency injection guidelines*

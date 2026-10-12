# Dependency Injection & Service Lifetimes in .NET

> Learning notes for ASP.NET Core / .NET 8+ (concepts apply to all modern .NET versions)

---

## Table of Contents

1. [What Problem Does DI Solve?](#1-what-problem-does-di-solve)
2. [Key Terms](#2-key-terms)
3. [Tight Coupling vs Dependency Injection](#3-tight-coupling-vs-dependency-injection)
4. [The Built-in DI Container](#4-the-built-in-di-container)
5. [Registering Services](#5-registering-services)
6. [Ways to Inject Services](#6-ways-to-inject-services)
7. [Service Lifetimes](#7-service-lifetimes)
8. [Lifetime Demo (See It Yourself)](#8-lifetime-demo-see-it-yourself)
9. [Captive Dependency (Biggest Trap)](#9-captive-dependency-the-biggest-trap)
10. [Advanced Registration Patterns](#10-advanced-registration-patterns)
11. [Disposal of Services](#11-disposal-of-services)
12. [Best Practices](#12-best-practices)
13. [Common Mistakes](#13-common-mistakes)
14. [Interview Questions](#14-interview-questions)
15. [Cheat Sheet](#15-cheat-sheet)
16. [Practice Exercises](#16-practice-exercises)

---

## 1. What Problem Does DI Solve?

Imagine an `OrderService` that needs to send emails. The naive way is to create the email sender inside the class:

```csharp
public class OrderService
{
    private readonly EmailSender _email = new EmailSender(); // hard-coded!

    public void PlaceOrder() => _email.Send("Order placed");
}
```

Problems with this approach:

| Problem | Why it hurts |
|---|---|
| **Tight coupling** | `OrderService` is locked to `EmailSender`. Switching to SMS means editing `OrderService`. |
| **Hard to test** | You cannot replace `EmailSender` with a fake, so unit tests send real emails. |
| **Hidden dependencies** | You must read the whole class to know what it needs. |
| **Lifetime control** | Every class decides when to create/destroy objects, so resources get wasted or leaked. |

**Dependency Injection** fixes this by letting someone else **supply** the dependencies from the outside.

---

## 2. Key Terms

| Term | Meaning | Analogy |
|---|---|---|
| **Dependency** | An object that another class needs to do its job. | Ingredients for a recipe |
| **Inversion of Control (IoC)** | A principle: a class does not create its dependencies, control is handed to an outside party. | The chef doesn't go shopping; groceries are delivered |
| **Dependency Injection (DI)** | A technique that *implements* IoC by passing dependencies in (usually via the constructor). | The delivery itself |
| **DI Container / IoC Container** | The framework component that creates, injects and disposes objects. | The delivery company |
| **Service** | Any class registered in the container. | An item in the catalogue |
| **Service Lifetime** | How long a created instance lives and who shares it. | Rent a car per trip vs per day vs buy it |

> **Remember:** IoC is the *idea*, DI is the *technique*, the container is the *tool*.

---

## 3. Tight Coupling vs Dependency Injection

```text
WITHOUT DI

  +--------------+   creates with "new"   +-------------+
  | OrderService | ---------------------> | EmailSender |
  +--------------+                        +-------------+
  (locked to EmailSender, hard to change or test)
```

```text
WITH DI

  +--------------+   depends on   +----------------+
  | OrderService | -------------> | IMessageSender |  (interface)
  +--------------+                +----------------+
         ^                          ^            ^
         |                          |            |
         |                 +-------------+  +-----------+
         |                 | EmailSender |  | SmsSender |
         |                 +-------------+  +-----------+
         |
  +----------------+
  |  DI Container  |  picks the implementation and injects it
  +----------------+
```

### The same code with DI

```csharp
// 1. Abstraction (contract)
public interface IMessageSender
{
    void Send(string message);
}

// 2. Implementations
public class EmailSender : IMessageSender
{
    public void Send(string message) => Console.WriteLine($"Email: {message}");
}

public class SmsSender : IMessageSender
{
    public void Send(string message) => Console.WriteLine($"SMS: {message}");
}

// 3. Consumer: it only knows the interface
public class OrderService
{
    private readonly IMessageSender _sender;

    public OrderService(IMessageSender sender)   // injected by the container
    {
        _sender = sender;
    }

    public void PlaceOrder() => _sender.Send("Order placed");
}
```

Benefits: **loose coupling**, **easy testing** (inject a mock), **flexible** (swap implementations in one line), **clear dependencies** (visible in the constructor).

---

## 4. The Built-in DI Container

ASP.NET Core ships with a DI container (`Microsoft.Extensions.DependencyInjection`). You do **not** need a third-party library for most apps.

The container does three jobs:

```
 ┌────────────────────────────────────────────────────────────┐
 │                       DI CONTAINER                         │
 │                                                            │
 │   1. REGISTER   →  "When someone asks for IMessageSender,  │
 │                     give them EmailSender"                 │
 │                                                            │
 │   2. RESOLVE    →  Builds the object and all of its        │
 │                     dependencies (the whole object graph)  │
 │                                                            │
 │   3. DISPOSE    →  Cleans up IDisposable objects it        │
 │                     created, at the right time             │
 └────────────────────────────────────────────────────────────┘
```

### The lifecycle of a request

```text
Step-by-step: what happens when a request arrives

 1. Program.cs registers services     -->  DI Container
 2. Program.cs calls Build()          -->  DI Container is ready

        ... an HTTP request arrives ...

 3. Container needs a Controller
 4.   Controller needs OrderService
 5.     OrderService needs IMessageSender
 6.       Container creates EmailSender
 7.     Container injects EmailSender into OrderService
 8.   Container injects OrderService into Controller
 9. Controller runs and sends the response

        ... request ends ...

10. Container disposes the scoped services
```

---

## 5. Registering Services

All registration happens in `Program.cs` using `builder.Services`.

```csharp
var builder = WebApplication.CreateBuilder(args);

// Interface -> Implementation
builder.Services.AddScoped<IMessageSender, EmailSender>();

// Concrete class only (no interface)
builder.Services.AddTransient<ReportGenerator>();

// Singleton
builder.Services.AddSingleton<ICacheService, MemoryCacheService>();

var app = builder.Build();
app.Run();
```

### Registration methods at a glance

| Method | Meaning |
|---|---|
| `AddTransient<TService, TImpl>()` | New instance **every time** it is requested |
| `AddScoped<TService, TImpl>()` | One instance **per scope** (per user /session HTTP request in web apps) |
| `AddSingleton<TService, TImpl>()` | **One instance** for the each and every HTTP request |

Framework-provided helpers such as `AddControllers()`, `AddDbContext<>()`, `AddHttpClient()` and `AddLogging()` are all just bundles of registrations.

---

## 6. Ways to Inject Services

### 6.1 Constructor injection (preferred)

```csharp
public class OrderController : ControllerBase
{
    private readonly OrderService _orders;

    public OrderController(OrderService orders) => _orders = orders;
}
```

Since C# 12 you can use a **primary constructor**:

```csharp
public class OrderController(OrderService orders) : ControllerBase
{
    [HttpPost]
    public IActionResult Post() { orders.PlaceOrder(); return Ok(); }
}
```

### 6.2 Action injection with `[FromServices]`

Use when only **one** action needs the service.

```csharp
public IActionResult Get([FromServices] IMessageSender sender)
{
    sender.Send("Hello");
    return Ok();
}
```

### 6.3 Minimal API injection

Parameters are resolved automatically.

```csharp
app.MapGet("/order", (OrderService orders) =>
{
    orders.PlaceOrder();
    return Results.Ok();
});
```

### 6.4 Service Locator (`IServiceProvider`) , avoid when possible

```csharp
var sender = serviceProvider.GetRequiredService<IMessageSender>();
```

This hides dependencies and is considered an **anti-pattern** in normal business code. Use it only in infrastructure code (middleware factories, background services creating scopes, etc.).

---

## 7. Service Lifetimes

This is the **most important part** of DI. The lifetime decides **when an instance is created, who shares it, and when it is disposed**.

```text
Shortest life                                              Longest life
   |                                                            |
   v                                                            v
+-----------+        +-----------+                    +-------------+
| TRANSIENT | -----> |  SCOPED   | -----------------> |  SINGLETON  |
+-----------+        +-----------+                    +-------------+
 new object           one object                       one object
 every time           per HTTP request                 for the whole app
```

### 7.1 Transient

> A **new instance is created every time** the service is requested.

```csharp
builder.Services.AddTransient<IEmailBuilder, EmailBuilder>();
```

Use for: lightweight, **stateless** services (formatters, mappers, validators, builders).

### 7.2 Scoped

> **One instance per scope.** In ASP.NET Core, a scope = **one HTTP request**.

```csharp
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
```

Use for: per-request work, **`DbContext`** (the default for `AddDbContext`), unit-of-work, repositories, current-user info.

### 7.3 Singleton

> **Created once** (first request or at startup) and **shared everywhere** until the app stops.

```csharp
builder.Services.AddSingleton<ICacheService, MemoryCacheService>();
```

Use for: configuration wrappers, in-memory caches, expensive-to-create shared objects, stateless helpers. Must be **thread-safe**.

---

### 7.4 Visual illustration: who gets which instance?

Suppose `ServiceA` and `ServiceB` both depend on `IMyService`, and **two HTTP requests** (Request 1 and Request 2) come in.

```
                    TRANSIENT                SCOPED                 SINGLETON
              ┌─────────────────────┬─────────────────────┬─────────────────────┐
 REQUEST 1    │                     │                     │                     │
  ServiceA ──►│  Instance #1        │  Instance #1  ◄──┐  │  Instance #1  ◄──┐  │
  ServiceB ──►│  Instance #2        │  Instance #1  ◄──┘  │  Instance #1  ◄──┤  │
              ├─────────────────────┼─────────────────────┼──────────────────│──┤
 REQUEST 2    │                     │                     │                  │  │
  ServiceA ──►│  Instance #3        │  Instance #2  ◄──┐  │  Instance #1  ◄──┤  │
  ServiceB ──►│  Instance #4        │  Instance #2  ◄──┘  │  Instance #1  ◄──┘  │
              └─────────────────────┴─────────────────────┴─────────────────────┘
               4 instances            2 instances           1 instance
```

**Reading the picture:**

- **Transient**: every injection gets a brand-new object (4 objects).
- **Scoped**: objects are shared *inside* a request, but each request gets its own (2 objects).
- **Singleton**: everyone, in every request, shares the same object (1 object).

### 7.5 Same idea as a timeline

```text
Time ------------------------------------------------------------------>

                     |<---- Request 1 ---->|   |<---- Request 2 ---->|

Singleton            [=================== Instance 1 ==============================]

Scoped               [==== Instance 1 ====]   [==== Instance 2 ====]

Transient (A, Req 1) [T1]
Transient (B, Req 1)      [T2]
Transient (A, Req 2)                          [T3]
Transient (B, Req 2)                               [T4]
```

### 7.6 Comparison table

| Feature | Transient | Scoped | Singleton |
|---|---|---|---|
| Instances created | Every injection | Once per scope/request | Once per app |
| Shared within a request? | No | **Yes** | **Yes** |
| Shared across requests? | No | No | **Yes** |
| Memory / GC pressure | Highest | Medium | Lowest |
| Must be thread-safe? | No | Usually no | **Yes** |
| Can hold state? | Only briefly | Per-request state | Global state (careful!) |
| Typical examples | Validators, mappers | `DbContext`, repositories | Cache, config, `HttpClient` factory |

### 7.7 Which lifetime should I choose? (Decision flow)

```text
                 START: I have a new service
                              |
                              v
        Does it hold state shared by the WHOLE app?
                  /                        \
                YES                         NO
                 |                           |
          Is it thread-safe?        Does it hold per-request state
           /          \             or use DbContext?
         YES          NO                /          \
          |            |              YES           NO
      SINGLETON    Make it             |             |
                   thread-safe      SCOPED     Is it light and stateless?
                   or use SCOPED                    /          \
                                                  YES          NO
                                                   |            |
                                               TRANSIENT      SCOPED
```

---

## 8. Lifetime Demo (See It Yourself)

The best way to understand lifetimes is to print a unique ID from each instance.

```csharp
// Services/Guids.cs
public interface ITransientService { Guid Id { get; } }
public interface IScopedService    { Guid Id { get; } }
public interface ISingletonService { Guid Id { get; } }

public class OperationService : ITransientService, IScopedService, ISingletonService
{
    public Guid Id { get; } = Guid.NewGuid();   // unique per instance
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
    Transient1 = t1.Id, Transient2 = t2.Id,   // different
    Scoped1    = s1.Id, Scoped2    = s2.Id,   // same in this request
    Singleton1 = g1.Id, Singleton2 = g2.Id    // same always
});
```

### Expected output

**Request 1**

```json
{
  "transient1": "a1111111-...",
  "transient2": "b2222222-...",   // different from transient1
  "scoped1":    "c3333333-...",
  "scoped2":    "c3333333-...",   // same as scoped1
  "singleton1": "d4444444-...",
  "singleton2": "d4444444-..."    // same as singleton1
}
```

**Request 2** (refresh the page)

```json
{
  "transient1": "e5555555-...",   // new
  "transient2": "f6666666-...",   // new
  "scoped1":    "07777777-...",   // new (new request = new scope)
  "scoped2":    "07777777-...",
  "singleton1": "d4444444-...",   // UNCHANGED, same as Request 1
  "singleton2": "d4444444-..."
}
```

> **What you should notice:** Transient IDs always differ. Scoped IDs match inside a request but change on the next request. Singleton IDs never change.

---

## 9. Captive Dependency (The Biggest Trap)

A **captive dependency** happens when a **longer-lived** service holds a reference to a **shorter-lived** one. The short-lived service gets "captured" and lives far longer than intended.

### The rule

> A service must only depend on services with the **same or longer** lifetime.

```
 Lifetime length:   Transient  <  Scoped  <  Singleton

 ✅ Singleton  → Singleton
 ✅ Scoped     → Scoped, Singleton, Transient
 ✅ Transient  → Transient, Scoped, Singleton

 ❌ Singleton  → Scoped      (captive dependency!)
 ❌ Singleton  → Transient   (transient becomes effectively singleton)
 ❌ Scoped     → (nothing wrong going to Transient, but see note below)
```

### Visual

```text
+----------------------------------+        +------------------------------+
| SINGLETON: ReportCache           |        | SCOPED: AppDbContext         |
| (lives for the whole app)        | -----> | (should die after request 1) |
+----------------------------------+  holds +------------------------------+
                                  reference
                                  FOREVER   <-- the DbContext is "captured"
                                               and never released (BAD)
```

### Bad example

```csharp
builder.Services.AddScoped<AppDbContext>();                 // short-lived
builder.Services.AddSingleton<ReportCache>();               // long-lived

public class ReportCache
{
    private readonly AppDbContext _db;                      // ❌ captured forever!
    public ReportCache(AppDbContext db) => _db = db;
}
```

What goes wrong:

- `DbContext` is **not thread-safe**, but a Singleton is used by many threads, causing random exceptions.
- The `DbContext` is never disposed, so connections leak and **stale data** is returned.

### The fix: create a scope manually

```csharp
public class ReportCache
{
    private readonly IServiceScopeFactory _scopeFactory;

    public ReportCache(IServiceScopeFactory scopeFactory)
        => _scopeFactory = scopeFactory;

    public async Task RefreshAsync()
    {
        using var scope = _scopeFactory.CreateScope();      // new short scope
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        // use db here...
    }                                                       // db disposed here
}
```

### Let .NET catch it for you

In **Development**, ASP.NET Core enables scope validation automatically and throws:

```
System.InvalidOperationException: Cannot consume scoped service 'AppDbContext'
from singleton 'ReportCache'.
```

To enable it manually (for example in a console app):

```csharp
var provider = services.BuildServiceProvider(new ServiceProviderOptions
{
    ValidateScopes  = true,
    ValidateOnBuild = true   // also checks that all dependencies are registered
});
```

---

## 10. Advanced Registration Patterns

### 10.1 Factory registration

When construction needs custom logic.

```csharp
builder.Services.AddScoped<IMessageSender>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    return config["Sender"] == "sms" ? new SmsSender() : new EmailSender();
});
```

### 10.2 Register an existing instance (Singleton only)

```csharp
builder.Services.AddSingleton<IClock>(new SystemClock());
```

### 10.3 Multiple implementations of one interface

```csharp
builder.Services.AddScoped<IMessageSender, EmailSender>();
builder.Services.AddScoped<IMessageSender, SmsSender>();
```

- Injecting `IMessageSender` gives the **last registered** one (`SmsSender`).
- Injecting `IEnumerable<IMessageSender>` gives **all** of them.

```csharp
public class Notifier(IEnumerable<IMessageSender> senders)
{
    public void NotifyAll(string msg)
    {
        foreach (var s in senders) s.Send(msg);
    }
}
```

### 10.4 Keyed services (.NET 8+)

Pick a specific implementation by key.

```csharp
builder.Services.AddKeyedScoped<IMessageSender, EmailSender>("email");
builder.Services.AddKeyedScoped<IMessageSender, SmsSender>("sms");

public class Alerts([FromKeyedServices("sms")] IMessageSender sender) { }
```

### 10.5 `TryAdd` (do not overwrite)

```csharp
using Microsoft.Extensions.DependencyInjection.Extensions;

builder.Services.TryAddScoped<IMessageSender, EmailSender>(); // only if not already registered
```

Great for libraries so they do not override the app's own registrations.

### 10.6 Open generics

```csharp
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
// IRepository<Order>, IRepository<Customer> ... all resolved automatically
```

### 10.7 Options pattern (typed configuration)

```csharp
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("Email"));

public class EmailSender(IOptions<EmailSettings> options)
{
    private readonly EmailSettings _settings = options.Value;
}
```

| Interface | Lifetime | Reloads on change? |
|---|---|---|
| `IOptions<T>` | Singleton | No |
| `IOptionsSnapshot<T>` | Scoped | Yes, per request |
| `IOptionsMonitor<T>` | Singleton | Yes, with change notifications |

---

## 11. Disposal of Services

The container **owns and disposes** what it creates (if the type implements `IDisposable` / `IAsyncDisposable`).

| Lifetime | Disposed when |
|---|---|
| Transient | The **scope** that created it ends (not immediately!) |
| Scoped | The **scope** ends (request finishes) |
| Singleton | The **application shuts down** |

Important rules:

- If **you** create the object with `new` and register it as an instance, **you** are responsible for disposing it.
- If you register using a factory, the container **does** dispose the returned object.
- Never call `Dispose()` on a service you received through injection, because the container will do it.
- Transient `IDisposable` objects resolved from the **root** provider are held until app shutdown, which is a memory-leak risk.

---

## 12. Best Practices

1. **Depend on abstractions** (interfaces), not concrete classes.
2. **Prefer constructor injection**, so dependencies are explicit and the class cannot exist in an invalid state.
3. **Keep constructors small.** More than 4 or 5 parameters usually means the class does too much (Single Responsibility Principle violated).
4. **Register `DbContext` as Scoped** (the default) and never as Singleton.
5. **Make Singletons thread-safe** and avoid mutable shared state.
6. **Avoid the service locator pattern** in business code.
7. **Do not inject `IServiceProvider` everywhere.** If you need it, ask why.
8. **Choose the shortest lifetime that works.** Default to Transient or Scoped, and use Singleton only when sharing is the point.
9. **Turn on `ValidateScopes` and `ValidateOnBuild`** during development.
10. **Use `IHttpClientFactory`** instead of `new HttpClient()` to avoid socket exhaustion.
11. **Keep registration organised** with extension methods:

```csharp
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOrderModule(this IServiceCollection services)
    {
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<OrderService>();
        return services;
    }
}

// Program.cs
builder.Services.AddOrderModule();
```

---

## 13. Common Mistakes

| Mistake | Result | Fix |
|---|---|---|
| Injecting Scoped into Singleton | Captive dependency, stale data, thread-safety bugs | Use `IServiceScopeFactory` or change lifetimes |
| Forgetting to register a service | `InvalidOperationException: Unable to resolve service for type ...` | Add registration in `Program.cs` |
| Circular dependency (A needs B, B needs A) | Exception at resolve time | Redesign, extract a third class, or use an interface/event |
| Using `new` for services | Bypasses DI, hard to test | Inject the service |
| Singleton with mutable state, no locking | Race conditions | Use `lock`, `ConcurrentDictionary`, or make it immutable |
| Resolving scoped service from root provider | Exception (with validation on) | Create a scope first |
| Registering concrete class but injecting interface | Cannot resolve | Register `AddScoped<IFoo, Foo>()` |
| Too many constructor parameters | "Constructor over-injection" smell | Split the class |

### Circular dependency illustration

```text
  +----------+       +----------+       +----------+
  | ServiceA | ----> | ServiceB | ----> | ServiceC |
  +----------+       +----------+       +----------+
        ^                                     |
        |_____________________________________|
          C needs A  -->  endless loop (ERROR)
```

The container cannot build `A` because it needs `C`, which needs `B`, which needs `A`.

### Common error message decoded

```
Unable to resolve service for type 'IMessageSender'
while attempting to activate 'OrderService'.
```

Meaning: `OrderService` asked for `IMessageSender`, but you never registered it. Add `builder.Services.AddScoped<IMessageSender, EmailSender>();`.

---

## 14. Interview Questions

**Q1. What is Dependency Injection?**
A technique where a class receives its dependencies from the outside instead of creating them itself, which gives loose coupling and testability.

**Q2. Difference between IoC and DI?**
IoC is the broad principle of handing control of object creation to an external party. DI is one specific way to achieve it.

**Q3. Explain the three service lifetimes.**
Transient creates a new instance per request for the service. Scoped creates one per scope (per HTTP request). Singleton creates one for the whole application.

**Q4. What is a captive dependency?**
When a longer-lived service (Singleton) holds a shorter-lived one (Scoped/Transient), keeping it alive far longer than intended.

**Q5. Why should `DbContext` be Scoped?**
It is not thread-safe, it tracks entities per unit of work, and it should be disposed after each request.

**Q6. How do you use a Scoped service inside a Singleton or `BackgroundService`?**
Inject `IServiceScopeFactory`, create a scope, and resolve the service from `scope.ServiceProvider`.

**Q7. What happens if you register the same interface multiple times?**
The last registration wins for single injection. `IEnumerable<T>` returns all of them.

**Q8. What is constructor injection and why is it preferred?**
Dependencies are passed via the constructor. It makes requirements explicit, supports immutability (`readonly` fields) and prevents half-initialised objects.

**Q9. Difference between `AddTransient`, `AddScoped` and `AddSingleton` in terms of disposal?**
Transient and Scoped are disposed when their scope ends. Singleton is disposed when the app shuts down.

**Q10. What are keyed services?**
A .NET 8 feature that lets you register and resolve multiple implementations of the same interface using a key.

---

## 15. Cheat Sheet

```
┌──────────────────────────────────────────────────────────────────┐
│                  DI & LIFETIMES, QUICK REFERENCE                 │
├──────────────────────────────────────────────────────────────────┤
│  Register   builder.Services.AddXxx<IService, Impl>();           │
│  Inject     public MyClass(IService svc) { ... }                 │
│                                                                  │
│  TRANSIENT  new object every time        stateless, light        │
│  SCOPED     one object per HTTP request  DbContext, repositories │
│  SINGLETON  one object for whole app     cache, config, helpers  │
│                                                                  │
│  Golden rule:  depend on same-or-LONGER lifetimes only           │
│  Never:        Singleton → Scoped                                │
│  Escape hatch: IServiceScopeFactory.CreateScope()                │
│  Safety net:   ValidateScopes = true, ValidateOnBuild = true     │
└──────────────────────────────────────────────────────────────────┘
```

| I want to... | Use |
|---|---|
| Share one object app-wide | `AddSingleton` |
| Share within a request only | `AddScoped` |
| Get a fresh object each time | `AddTransient` |
| Choose implementation by name | `AddKeyedXxx` + `[FromKeyedServices]` |
| Get all implementations | Inject `IEnumerable<T>` |
| Register only if missing | `TryAddXxx` |
| Use scoped service in singleton | `IServiceScopeFactory` |

---

## 16. Practice Exercises

1. **Lifetime demo:** Build the `/lifetimes` endpoint from section 8. Refresh the page several times and record which GUIDs change.
2. **Swap implementations:** Create `IMessageSender` with `EmailSender` and `SmsSender`. Switch between them by changing only one line in `Program.cs`.
3. **Break it on purpose:** Make a Singleton that depends on a Scoped service. Run in Development and read the exception. Then fix it with `IServiceScopeFactory`.
4. **Unit test:** Write a unit test for `OrderService` using a fake `IMessageSender` (or Moq) without sending real messages.
5. **Keyed services:** Register two senders with keys `"email"` and `"sms"` and inject each into different endpoints.
6. **Extension method:** Move all your registrations into an `AddMyAppServices()` extension method.
7. **Circular dependency:** Create two services that depend on each other, observe the error, then refactor to remove the cycle.

---

## Further Reading

- Microsoft Docs: *Dependency injection in ASP.NET Core*
- Microsoft Docs: *Dependency injection guidelines* (.NET fundamentals)
- Book: *Dependency Injection Principles, Practices, and Patterns* by Mark Seemann & Steven van Deursen

---

*Happy learning! Understanding lifetimes well will save you from some of the nastiest bugs in .NET.*

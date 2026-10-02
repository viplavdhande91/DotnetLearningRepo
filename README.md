# 🚀 Routing in ASP.NET Core (.NET 6 / 7 / 8)

This guide covers complete routing concepts in modern ASP.NET Core using the **Minimal Hosting Model**.

It includes:

- Routing Basics
- Endpoint Routing Internals
- Route Templates & Constraints
- Metadata
- URL Generation
- Route Groups
- Performance Guidance
- Senior-Level Concepts

---

# 📌 1️⃣ What is Routing?

Routing matches incoming HTTP requests to executable endpoints.

An endpoint can be:
- Controller action
- Minimal API handler
- Razor Page
- SignalR Hub
- gRPC Service
- Health Check

Example request:

```
GET /api/users/5
```

Matched to:

```
UsersController → GetById(int id)
```

---

# 📌 2️⃣ Minimal Hosting Model (.NET 6+)

In .NET 6+, routing is automatically configured.

You DO NOT manually call:

- `UseRouting()`
- `UseEndpoints()`

Basic setup:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
```

`MapControllers()` wires endpoint routing automatically.

---

# 📌 3️⃣ Endpoint Execution Flow (VERY IMPORTANT)

```text
1. Custom middleware (before routing)
2. UseRouting → Endpoint matching happens
3. Middleware between UseRouting & UseEndpoints:
   - Authentication
   - Authorization
   - CORS
   - Custom middleware inspecting metadata
4. UseEndpoints → Endpoint executes (terminal)
5. Middleware after UseEndpoints runs only if no match
```

---

# 📌 4️⃣ Types of Routing

---

## ✅ 1. Attribute Routing (Recommended for APIs)

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll() => Ok("All Users");

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id) => Ok(id);

    [HttpPost]
    public IActionResult Create([FromBody] string name)
        => Created("", name);
}
```

Routes:

```
GET    /api/users
GET    /api/users/5
POST   /api/users
```

---

## ✅ 2. Conventional Routing (MVC)

```csharp
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
```

Pattern tokens:

| Token | Meaning |
|--------|----------|
| {controller} | Controller name |
| {action} | Action method |
| {id?} | Optional parameter |

---

## ✅ 3. Minimal API Routing

```csharp
app.MapGet("/users", () => "All Users");

app.MapGet("/users/{id}", (int id) =>
{
    return Results.Ok($"User {id}");
});
```

---

# 📌 5️⃣ Route Templates

Common patterns:

| Pattern | Meaning |
|----------|----------|
| `{id}` | Required |
| `{id?}` | Optional |
| `{id:int}` | Constraint |
| `{id:guid}` | GUID only |

Example:

```csharp
[HttpGet("{id:int:min(1)}")]
```

---

# 📌 6️⃣ Route Constraints

Constraints run AFTER route match but BEFORE execution.

Example:

```csharp
[HttpGet("{id:int:min(1)}")]
```

Common constraints:

- int
- guid
- bool
- datetime
- decimal
- minlength(x)
- maxlength(x)
- range(min,max)
- alpha
- regex(...)

⚠ Do NOT use constraints for input validation.  
Invalid input should return 400, not 404.

---


# 📌 7️⃣ Route Precedence (How .NET Chooses Between Multiple Routes)

## 🔎 What Problem Does This Solve?

When multiple endpoints can match the same URL, how does ASP.NET Core determine which endpoint should execute?

ASP.NET Core uses **endpoint routing** to match the request against route patterns and select the best matching endpoint.

---

## 🧠 Key Concept: Route Specificity

Route precedence is primarily about **how specific a route pattern is**.

A more specific route pattern takes precedence over a more general pattern.

For example:

```csharp
app.MapGet("/hello", () => "Hello literal");

app.MapGet("/{message}", (string message) => $"Message: {message}");
```

Request:

```text
GET /hello
```

Both endpoints can match:

* `/hello` → literal match ✅
* `/{message}` → parameter match, `message = "hello"` ✅

### Which one runs?

👉 `/hello` runs.

### Why?

Because a **literal segment is more specific than a parameter segment**.

---

## 🏆 Route Precedence Rules

For interview purposes, remember the general specificity order:

1. **Literal segment**
2. **Constrained parameter**
3. **Unconstrained parameter**
4. **Catch-all parameter**

In other words:

```text
Literal
   ↓
Constrained parameter
   ↓
Unconstrained parameter
   ↓
Catch-all
```

### Example

```csharp
app.MapGet("/products/list", () => "List");

app.MapGet("/products/{id:int}", (int id) => $"Product {id}");

app.MapGet("/products/{name}", (string name) => $"Name {name}");

app.MapGet("/products/{*path}", (string path) => $"Catch-all: {path}");
```

For:

```text
GET /products/list
```

Potential matches:

```text
/products/list       → Literal
/products/{id:int}   → Does NOT match ("list" is not an int)
/products/{name}     → Parameter match
/products/{*path}    → Catch-all match
```

Therefore:

```text
/products/list
```

is selected because the literal route is more specific.

---

## ✅ Example 2 – Constrained vs Unconstrained Parameter

```csharp
app.MapGet("/users/{id:int}", (int id) => $"User ID: {id}");

app.MapGet("/users/{value}", (string value) => $"Value: {value}");
```

Request:

```text
GET /users/123
```

Both routes can match:

```text
/users/{id:int}   → id = 123
/users/{value}    → value = "123"
```

The constrained route is more specific:

```text
/users/{id:int}
```

Therefore, the `int`-constrained endpoint takes precedence over the unconstrained parameter endpoint.

---

## ⚠️ Important: "More Segments = Higher Priority" Is NOT a General Rule

Do **not** memorize this:

```text
More segments → Higher priority
```

That is an oversimplification.

For example:

```text
/{id}
```

and:

```text
/products/{id}
```

have different route shapes and normally do not compete for the same URL.

The important interview concept is:

> **Route precedence is based on route-pattern specificity, not simply on the number of segments.**

---

## ⚠️ What If Two Routes Have the Same Precedence?

If multiple endpoints match and routing cannot determine a single best endpoint, ASP.NET Core can report an **ambiguous match** rather than arbitrarily choosing one.

Example:

```csharp
app.MapGet("/users/{id}", () => "Route 1");

app.MapGet("/users/{name}", () => "Route 2");
```

Request:

```text
GET /users/123
```

Both patterns have essentially the same precedence.

There is no meaningful distinction between `{id}` and `{name}` for routing purposes.

This can result in an:

```text
AmbiguousMatchException
```

---

## 🧠 Interview Mental Model

Think of route matching like this:

```text
Incoming Request
       ↓
Match route patterns
       ↓
Apply route constraints
       ↓
Apply routing policies
       ↓
Choose the best/specific endpoint
       ↓
Execute endpoint
```

### Remember:

```text
Literal
   >
Constrained parameter
   >
Unconstrained parameter
   >
Catch-all
```

### ⭐ Interview Answer

> **ASP.NET Core endpoint routing can have multiple endpoints matching the same URL. Route precedence favors more specific route patterns: literal segments are more specific than constrained parameters, constrained parameters are more specific than unconstrained parameters, and catch-all parameters have lower precedence. If multiple endpoints remain equally valid, endpoint selection can result in an ambiguous match rather than arbitrarily choosing one.**


# 📌 8️⃣ URL Matching Phases

Routing processes in phases:

1. Match URL against all templates
2. Remove those failing constraints
3. Apply MatcherPolicy
4. EndpointSelector chooses best match

If multiple endpoints have same priority → Ambiguous match exception.

---

# 📌 9️⃣ Endpoint Metadata

## 🔎 What is Endpoint Metadata?

Every endpoint contains:

- RequestDelegate (code to execute)
- Metadata (extra information attached to endpoint)

Metadata can include:

- Authorization
- CORS
- Filters
- Custom attributes

---

## ✅ Example – Authorization Metadata

```csharp
app.MapGet("/secure", () => "Secret Data")
   .RequireAuthorization();
```

The `/secure` endpoint now has Authorization metadata attached.

---

## 🔍 How Middleware Uses Metadata

```csharp
app.Use(async (context, next) =>
{
    var endpoint = context.GetEndpoint();

    if (endpoint?.Metadata.GetMetadata<IAuthorizeData>() != null)
    {
        Console.WriteLine("This endpoint requires authorization.");
    }

    await next();
});
```

### Flow:

1. Routing selects endpoint.
2. Middleware reads metadata.
3. Middleware applies behavior (auth, cors, etc.).

---

## 🧠 Key Idea

Routing is metadata-driven.

Middleware between routing and endpoint execution can inspect endpoint metadata and apply policies.

---

# 📌 🔟 URL Generation (LinkGenerator)

Modern URL generation API:

```csharp
public class MyService
{
    private readonly LinkGenerator _linkGenerator;

    public MyService(LinkGenerator linkGenerator)
    {
        _linkGenerator = linkGenerator;
    }

    public string Generate()
    {
        return _linkGenerator.GetPathByAction(
            "GetById",
            "Users",
            new { id = 10 });
    }
}
```

Methods:

| Method | Returns |
|----------|----------|
| GetPathByAction | Relative path |
| GetUriByAction | Absolute URI |
| GetPathByPage | Razor page path |
| GetUriByPage | Absolute URI |

---

# 📌 1️⃣1️⃣ Ambient vs Explicit Route Values

Ambient values = current request route values.  
Explicit values = values passed to LinkGenerator.

Rule:

If a left-side parameter changes,  
all right-side ambient values are invalidated.

Example template:

```
{controller}/{action}/{id?}
```

If controller changes → id is invalidated.

---

# 📌 1️⃣2️⃣ Route Groups (.NET 7+)

## 🔎 Problem Without Groups

```csharp
app.MapGet("/admin/users", GetUsers).RequireAuthorization();
app.MapPost("/admin/users", CreateUser).RequireAuthorization();
app.MapDelete("/admin/users/{id}", DeleteUser).RequireAuthorization();
```

Repeated prefix + repeated authorization.

---

## ✅ Using Route Groups

```csharp
var adminGroup = app.MapGroup("/admin")
                    .RequireAuthorization();

adminGroup.MapGet("/users", GetUsers);
adminGroup.MapPost("/users", CreateUser);
adminGroup.MapDelete("/users/{id}", DeleteUser);
```

Now:

- All routes start with `/admin`
- All require authorization
- Cleaner and maintainable

---

## 🔥 Example – Multi-Tenant API

```csharp
app.MapGroup("/tenant/{tenantId}")
   .RequireAuthorization()
   .MapGet("/users", (string tenantId) =>
   {
       return $"Users for tenant {tenantId}";
   });
```

Request:

```
GET /tenant/abc/users
```

Response:

```
Users for tenant abc
```

---

## 🧠 Mental Model

Route Group = Folder for endpoints with shared configuration.

---

# 📌 1️⃣3️⃣ ShortCircuit()

## 🔎 What Problem Does This Solve?

Normally request goes through full middleware pipeline:

- Logging
- CORS
- Authentication
- Authorization
- Custom middleware
- Endpoint

Sometimes we want to execute endpoint immediately.

---

## ✅ Example

```csharp
app.MapGet("/fast", () => "Fast response")
   .ShortCircuit();
```

Now:

- `/fast` executes immediately
- Skips remaining middleware
- Improves performance

---

## ✅ Example – Ignore Bots

```csharp
app.MapShortCircuit(404, "robots.txt", "favicon.ico");
```

Requests to:

```
/robots.txt
/favicon.ico
```

Immediately return 404 without running full pipeline.

---

## ⚠ Important

Cannot use `.ShortCircuit()` with:

- RequireAuthorization
- RequireCors

Because those rely on middleware execution.

---

# 📌 1️⃣4️⃣ Catch-All Parameters

## 🔎 Problem

Normal parameter:

```csharp
app.MapGet("/files/{name}", (string name) => name);
```

Matches:

```
/files/test
```

But NOT:

```
/files/folder1/test
```

---

## ✅ Catch-All Parameter

```csharp
app.MapGet("/files/{*path}", (string path) => path);
```

Now matches:

```
/files/folder1/test
```

Value of `path`:

```
folder1/test
```

---

## ✅ Double Asterisk Version

```csharp
app.MapGet("/files/{**path}", (string path) => path);
```

Used when generating URLs and preserving slashes.

---

## 🔥 Real Use Case – Static File Style Routing

```csharp
app.MapGet("/static/{**filepath}", (string filepath) =>
{
    return $"Requested file: {filepath}";
});
```

Request:

```
/static/images/logo.png
```

Response:

```
Requested file: images/logo.png
```

---

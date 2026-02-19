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

When multiple routes match the same URL, how does ASP.NET Core decide which one to execute?

---

## ✅ Example 1 – Literal vs Parameter

```csharp
app.MapGet("/hello", () => "Hello literal");
app.MapGet("/{message}", (string message) => $"Message: {message}");
```

Request:

```
GET /hello
```

Both routes technically match:

- `/hello` ✅
- `/{message}` where message = "hello" ✅

### Which one runs?

👉 `/hello` runs.

### Why?

Because literal segments have higher precedence than parameter segments.

---

## ✅ Example 2 – Literal vs Constrained Parameter

```csharp
app.MapGet("/users/list", () => "List of users");
app.MapGet("/users/{id:int}", (int id) => $"User {id}");
```

Request:

```
GET /users/list
```

- `/users/list` matches literal route ✅
- `/users/{id:int}` does NOT match because "list" is not int ❌

Literal wins again.

---

## 🏆 Route Precedence Rules

Priority order:

1. More segments → Higher priority
2. Literal segment > Parameter segment
3. Parameter with constraint > Without constraint
4. Catch-all is lowest priority

---

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

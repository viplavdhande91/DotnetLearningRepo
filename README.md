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

# 📌 7️⃣ Route Precedence Rules (Interview Critical)

Routing chooses the most specific match.

Priority:

1. More segments → Higher priority
2. Literal segment > Parameter segment
3. Parameter with constraint > Without constraint
4. Catch-all is lowest priority

Example:

```
/hello
/{message}
```

`/hello` wins.

---

# 📌 8️⃣ URL Matching Phases

Routing processes in phases:

1. Match URL against all templates
2. Remove those failing constraints
3. Apply MatcherPolicy
4. EndpointSelector chooses best match

If multiple endpoints have same priority → Ambiguous match exception.

---

# 📌 9️⃣ Endpoint Metadata (VERY IMPORTANT)

Each endpoint contains:

- RequestDelegate (what executes)
- Metadata collection (authorization, filters, CORS, etc.)

Example:

```csharp
app.MapGet("/health", () => "OK")
   .RequireAuthorization()
   .WithMetadata(new MyCustomMetadata());
```

Access metadata:

```csharp
app.Use(async (context, next) =>
{
    var endpoint = context.GetEndpoint();

    if (endpoint?.Metadata.GetMetadata<RequiresAuditAttribute>() != null)
    {
        Console.WriteLine("Audit required.");
    }

    await next();
});
```

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

Group endpoints with shared prefix and metadata.

```csharp
app.MapGroup("/admin")
   .RequireAuthorization()
   .MapGet("/users", () => "Admin Users");
```

Benefits:

- Cleaner organization
- Shared auth
- Shared filters
- Shared metadata

---

# 📌 1️⃣3️⃣ ShortCircuit()

Executes endpoint immediately without running remaining middleware.

```csharp
app.MapGet("/fast", () => "Fast")
   .ShortCircuit();
```

Useful for:
- Health checks
- robots.txt
- favicon.ico

---

# 📌 1️⃣4️⃣ Catch-All Parameters

```csharp
blog/{**slug}
```

- Matches everything after blog/
- Can include slashes

Single `*` escapes slashes  
Double `**` preserves slashes

---

# 📌 1️⃣5️⃣ Custom Route Constraints

```csharp
public class NoZeroesRouteConstraint : IRouteConstraint
{
    public bool Match(...)
    {
        return !values[routeKey].ToString().Contains("0");
    }
}
```

Register:

```csharp
builder.Services.AddRouting(options =>
{
    options.ConstraintMap.Add("noZeroes", typeof(NoZeroesRouteConstraint));
});
```

---

# 📌 1️⃣6️⃣ Performance Guidance

Potentially expensive features:

- Complex regex
- Complex segments ({x}-{y}-{z})
- Large route tables with early parameters
- Synchronous DB access during routing

Best practices:

- Use constraints
- Move parameters to later segments
- Avoid `{param}/literal` in large route tables

Routing is highly optimized and rarely the bottleneck.

---

# 📌 1️⃣7️⃣ Debugging Routing

Enable detailed logs:

```json
{
  "Logging": {
    "LogLevel": {
      "Microsoft": "Debug"
    }
  }
}
```

Inspect endpoint:

```csharp
var endpoint = context.GetEndpoint();
```

---

# 📌 1️⃣8️⃣ REST Best Practices

Good:

```
GET    /api/users
POST   /api/users
GET    /api/users/5
DELETE /api/users/5
```

Avoid:

```
GET /api/getUserById
POST /api/createUser
```

---

# 📌 1️⃣9️⃣ Senior-Level Summary

✔ Endpoint routing is metadata-driven  
✔ Routing is separated into matching & execution  
✔ Middleware can inspect endpoints  
✔ Route precedence determines best match  
✔ LinkGenerator handles URL creation  
✔ Route groups simplify organization  
✔ ShortCircuit improves performance  

---

# 🎯 Final Recommendation for Production APIs

Use:

- Attribute Routing
- Route constraints
- API versioning
- Route groups
- LinkGenerator
- Metadata-driven policies

Avoid:

- Verb-based URLs
- Deeply nested routes
- Custom terminal middleware when routing can solve it

---


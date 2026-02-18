# 🚀 Routing in .NET 6+ (ASP.NET Core)

This guide covers complete routing concepts in **ASP.NET Core (.NET 6, 7, 8)** using the **Minimal Hosting Model**.

---

# 📌 1️⃣ What is Routing?

Routing is the process of matching an incoming HTTP request to a specific endpoint  
(Controller action, Razor Page, or Minimal API handler).

### Example

Request:

```
GET /api/users/5
```

Matched to:

```
UsersController → GetById(int id)
```

---

# 📌 2️⃣ Routing in .NET 6+ (Minimal Hosting Model)

In .NET 6+, routing is automatically configured.

We **DO NOT manually use**:

- `app.UseRouting()`
- `app.UseEndpoints()`

Instead, we use:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Register services
builder.Services.AddControllers();

var app = builder.Build();

// Middleware
app.UseAuthentication();
app.UseAuthorization();

// Map endpoints
app.MapControllers();

app.Run();
```

👉 `MapControllers()` internally handles endpoint routing.

---

# 📌 3️⃣ Types of Routing in .NET 6+

---

## 1️⃣ Attribute Routing (✅ Recommended for APIs)

Defined directly on controller and action methods.

```csharp
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    // GET: api/users
    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok("All Users");
    }

    // GET: api/users/5
    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        return Ok($"User {id}");
    }

    // POST: api/users
    [HttpPost]
    public IActionResult Create([FromBody] string name)
    {
        return Created("", name);
    }
}
```

### Generated Routes

| HTTP Method | URL              |
|------------|------------------|
| GET        | /api/users       |
| GET        | /api/users/5     |
| POST       | /api/users       |

---

## 2️⃣ Conventional Routing (MVC Style)

Usually used in MVC apps (with Views).

```csharp
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
```

### Pattern Breakdown

| Token         | Meaning |
|--------------|----------|
| `{controller}` | Controller name |
| `{action}`     | Action method |
| `{id?}`        | Optional parameter |

Example:

```
/Home/Index/5
```

---

## 3️⃣ Minimal API Routing

Used without controllers.

```csharp
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/users", () =>
{
    return Results.Ok("All Users");
});

app.MapGet("/users/{id}", (int id) =>
{
    return Results.Ok($"User {id}");
});

app.MapPost("/users", (string name) =>
{
    return Results.Created($"/users/{name}", name);
});

app.Run();
```

✔ Lightweight  
✔ Faster startup  
✔ Great for microservices  

---

# 📌 4️⃣ Route Templates

Common route patterns:

| Pattern        | Meaning |
|---------------|----------|
| `{id}`        | Required parameter |
| `{id?}`       | Optional parameter |
| `{id:int}`    | Must be integer |
| `{id:guid}`   | Must be GUID |

Example:

```csharp
[HttpGet("{id:int}")]
public IActionResult GetById(int id)
{
    return Ok(id);
}
```

---

# 📌 5️⃣ Route Constraints

Restrict route matching to specific formats.

```csharp
[HttpGet("{id:int:min(1)}")]
public IActionResult GetPositiveId(int id)
{
    return Ok(id);
}
```

### Common Constraints

- `int`
- `guid`
- `bool`
- `datetime`
- `min(x)`
- `max(x)`
- `minlength(x)`
- `maxlength(x)`
- `regex(...)`

Example:

```csharp
[HttpGet("{email:regex(^\\S+@\\S+\\.\\S+$)}")]
public IActionResult GetByEmail(string email)
{
    return Ok(email);
}
```

---

# 📌 6️⃣ Route Prefixing

Controller-level prefix:

```csharp
[Route("api/v1/[controller]")]
public class UsersController : ControllerBase
{
    [HttpGet("active")]
    public IActionResult GetActive()
    {
        return Ok("Active Users");
    }
}
```

### Final URL

```
GET /api/v1/users/active
```

---

# 📌 7️⃣ API Versioning (Interview Important)

### Simple Versioning via Route

```csharp
[Route("api/v1/[controller]")]
```

### Better Approach (Package-Based)

Install:

```
Microsoft.AspNetCore.Mvc.Versioning
```

Then configure:

```csharp
builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
});
```

Example:

```
GET /api/v1/users
GET /api/v2/users
```

---

# 📌 8️⃣ How Routing Works Internally

1. Request enters middleware pipeline  
2. Endpoint Routing matches URL pattern  
3. Matched endpoint stored in `HttpContext`  
4. Authentication runs  
5. Authorization runs  
6. Controller action executes  

---

# 📌 9️⃣ Difference: UseRouting vs MapControllers

| Old (.NET Core 3.1) | Modern (.NET 6+) |
|---------------------|------------------|
| `app.UseRouting()` | Automatic |
| `app.UseEndpoints()` | Not needed |
| `endpoints.MapControllers()` | `app.MapControllers()` |

.NET 6+ simplifies the routing configuration.

---

# 📌 🔟 RESTful Routing Best Practices

✅ Use nouns, not verbs  
✅ Keep URLs resource-based  
✅ Use proper HTTP methods  
✅ Apply constraints  
✅ Version APIs  

### ✅ Good

```
GET    /api/users
POST   /api/users
GET    /api/users/5
DELETE /api/users/5
```

### ❌ Avoid

```
GET /api/getUserById
POST /api/createUser
```

---

# 📌 1️⃣1️⃣ Common Interview Questions

### Q: What is Endpoint Routing?

Endpoint Routing separates route matching from execution and integrates with the middleware pipeline.

---

### Q: Difference between Conventional & Attribute Routing?

- Conventional → Centralized route pattern
- Attribute → Defined directly on controllers (preferred for APIs)

---

### Q: When does routing happen?

Routing happens early in middleware pipeline, before authorization executes.

---

# 📌 1️⃣2️⃣ Summary

✔ .NET 6+ uses Minimal Hosting Model  
✔ Routing is automatically configured  
✔ Prefer Attribute Routing for APIs  
✔ Use `app.MapControllers()`  
✔ Minimal APIs use `MapGet`, `MapPost`, etc.  
✔ Route constraints improve reliability  

---

# 🎯 Final Recommendation

For modern Web APIs (.NET 6/7/8):

### ✅ Use
- Attribute Routing
- `app.MapControllers()`
- Versioned APIs
- Route constraints

### ❌ Avoid
- Manual `UseEndpoints`
- Verb-based URLs
- Deeply nested route hierarchies

---

Happy Coding 🚀



````markdown
# 🚀 Routing in .NET 6+ (ASP.NET Core)

This guide covers complete routing concepts in ASP.NET Core (.NET 6, 7, 8) using the **Minimal Hosting Model**.

---

# 📌 1. What is Routing?

Routing is the process of matching an incoming HTTP request to a specific endpoint (Controller action, Minimal API handler, etc.).

Example:

GET /api/users/5  
➡️ Matched to: UsersController → GetById(int id)

---

# 📌 2. Routing in .NET 6+ (Minimal Hosting Model)

In .NET 6+, routing is automatically configured.

We DO NOT manually use:

- app.UseRouting()
- app.UseEndpoints()

Instead, we use:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
```

`MapControllers()` internally handles endpoint routing.

---

# 📌 3. Types of Routing in .NET 6+

## 1️⃣ Attribute Routing (Recommended for APIs)

Defined directly on controller & action.

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok("All Users");
    }

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        return Ok($"User {id}");
    }
}
```

Generated Routes:

GET /api/users  
GET /api/users/5  

---

## 2️⃣ Conventional Routing (MVC style)

Usually used in MVC apps.

```csharp
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
```

Pattern breakdown:

{controller} → Controller name  
{action} → Action method  
{id?} → Optional parameter  

---

## 3️⃣ Minimal API Routing

Used without controllers.

```csharp
app.MapGet("/users", () => "All Users");

app.MapGet("/users/{id}", (int id) => $"User {id}");
```

Very lightweight and fast.

---

# 📌 4. Route Templates

Common route patterns:

| Pattern | Meaning |
|---------|----------|
| {id} | Required parameter |
| {id?} | Optional parameter |
| {id:int} | Constraint: must be int |
| {id:guid} | Constraint: must be GUID |

Example:

```csharp
[HttpGet("{id:int}")]
public IActionResult GetById(int id)
```

---

# 📌 5. Route Constraints

Used to restrict route matching.

```csharp
[HttpGet("{id:int:min(1)}")]
```

Common constraints:

- int
- guid
- bool
- datetime
- minlength(x)
- maxlength(x)
- regex(...)

---

# 📌 6. Route Prefixing

Controller-level prefix:

```csharp
[Route("api/v1/[controller]")]
```

Action-level override:

```csharp
[HttpGet("active")]
```

Final URL:

GET /api/v1/users/active

---

# 📌 7. API Versioning (Common Interview Topic)

Simple versioning via route:

```csharp
[Route("api/v1/[controller]")]
```

Better approach: Use Microsoft.AspNetCore.Mvc.Versioning package.

Example:

GET /api/v1/users  
GET /api/v2/users  

---

# 📌 8. How Routing Works Internally

1. Request enters middleware pipeline
2. Endpoint Routing matches URL pattern
3. Matched endpoint stored in HttpContext
4. Authentication runs
5. Authorization runs
6. Controller action executes

---

# 📌 9. Difference: UseRouting vs MapControllers

| Old (.NET Core 3.1) | Modern (.NET 6+) |
|----------------------|------------------|
| app.UseRouting() | Automatic |
| app.UseEndpoints() | Not needed |
| endpoints.MapControllers() | app.MapControllers() |

In .NET 6+, routing is simplified.

---

# 📌 10. Best Practices for Production APIs

✅ Use Attribute Routing  
✅ Use Route Constraints  
✅ Keep route names RESTful  
✅ Version your APIs  
✅ Avoid deeply nested routes  
✅ Keep URLs resource-based, not verb-based  

Good:

GET /api/users  
POST /api/users  
GET /api/users/5  

Avoid:

GET /api/getUserById  

---

# 📌 11. Common Interview Questions

Q: What is Endpoint Routing?  
A: It separates route matching from execution and integrates with middleware pipeline.

Q: Difference between Conventional & Attribute routing?  
A: Conventional uses centralized route templates; Attribute routing defines routes directly on controllers.

Q: When does routing happen in middleware?  
A: Before authentication & authorization execution but after pipeline begins.

---

# 📌 12. Summary

✔ .NET 6+ uses Minimal Hosting Model  
✔ Routing is automatic  
✔ Prefer Attribute Routing for APIs  
✔ Use MapControllers()  
✔ Minimal APIs use MapGet/MapPost  
✔ Route constraints improve reliability  

---

# 🎯 Final Recommendation

For modern Web APIs (.NET 6/7/8):

Use:

- Attribute Routing
- app.MapControllers()
- Versioned APIs
- Route constraints

Avoid:

- Manual UseEndpoints
- Conventional routing for APIs (unless MVC app)

---



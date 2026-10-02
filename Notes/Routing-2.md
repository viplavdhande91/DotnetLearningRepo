# 📌 3️⃣ Endpoint Execution Flow (VERY IMPORTANT)

In ASP.NET Core, routing is part of the middleware pipeline. With the Minimal Hosting Model, WebApplication automatically wires routing when needed.

A useful mental model is:

1. Middleware before routing
        ↓
2. Routing / endpoint matching
        ↓
3. Middleware that runs after routing
   - Authentication
   - Authorization
   - CORS
   - Custom middleware inspecting endpoint metadata
        ↓
4. Endpoint execution
        ↓
5. Middleware unwinds and the response travels back

✅ Typical Middleware Setup
```csharp

var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
```
🧠 Important Concept: UseRouting()
In the traditional middleware model:
```csharp

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});
```
UseRouting() selects the endpoint and makes it available through:
```csharp

HttpContext.GetEndpoint();
```
With the modern WebApplication model, explicit UseRouting() and UseEndpoints() are normally unnecessary.

⚠️ Important Interview Correction
Do NOT say:

"Middleware after UseEndpoints() runs only when there is no route."

Endpoint execution does not mean the middleware pipeline is simply abandoned. Middleware executes as a nested pipeline and can continue during the response/unwinding phase.

⭐ Interview Mental Model
Routing selects the endpoint. Middleware can then use the selected endpoint's metadata before execution, the endpoint executes, and the middleware pipeline unwinds while the response travels back.

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

### Pattern tokens

| Token          | Meaning            |
| -------------- | ------------------ |
| `{controller}` | Controller name    |
| `{action}`     | Action method      |
| `{id?}`        | Optional parameter |


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

### Common patterns

| Pattern     | Meaning            |
| ----------- | ------------------ |
| `{id}`      | Required parameter |
| `{id?}`     | Optional parameter |
| `{id:int}`  | Integer constraint |
| `{id:guid}` | GUID constraint    |


Example:

```csharp

[HttpGet("{id:int:min(1)}")]

```

---

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

# 📌 6️⃣ Route Constraints

Route constraints restrict which route values are considered valid during endpoint matching.

They participate in candidate selection before the final endpoint is selected and executed.

Example:
```csharp

[HttpGet("{id:int:min(1)}")]
public IActionResult GetById(int id)
{
    return Ok(id);
}
```
This route requires:

id to be an integer
id to be at least 1

### Common Constraints

| Constraint       | Meaning                                           |
| ---------------- | ------------------------------------------------- |
| `int`            | Integer value                                     |
| `long`           | Long integer value                                |
| `guid`           | GUID value                                        |
| `bool`           | Boolean value                                     |
| `datetime`       | Date and time value                               |
| `decimal`        | Decimal value                                     |
| `double`         | Double-precision floating-point value             |
| `minlength(x)`   | Minimum string length                             |
| `maxlength(x)`   | Maximum string length                             |
| `length(x)`      | Exact string length                               |
| `min(x)`         | Minimum numeric value                             |
| `max(x)`         | Maximum numeric value                             |
| `range(min,max)` | Value must be within the specified range          |
| `alpha`          | Alphabetic characters only                        |
| `regex(...)`     | Value must match the specified regular expression |


🔎 Example
```csharp
app.MapGet("/users/{id:int}", (int id) =>
{
    return $"User {id}";
});
```
Request:
```
GET /users/123
```
Matches the route:

id = 123

But:
```
GET /users/abc
```
does not satisfy the int constraint, so this endpoint is not selected.

⚠️ Constraints Are Not General Input Validation
Do not use route constraints as a replacement for application/business validation.

For example:
```csharp
[HttpGet("{id:int:min(1)}")]
```
is useful for deciding whether the request matches the route.

But checking whether the user is allowed to access that ID, whether the resource exists, or whether the supplied data satisfies business rules belongs elsewhere.

404 vs 400 — Important Interview Point
A route constraint failure can result in the endpoint not matching, which can lead to a 404 Not Found.

For example:
```
GET /users/abc
```
against:
```
/users/{id:int}
```
does not match that endpoint.

This is different from validation performed after an endpoint has been selected, which may produce a 400 Bad Request.

⭐ Interview Answer
Route constraints participate in endpoint matching by eliminating route candidates whose route values don't satisfy the constraint. They are useful for routing decisions, but they should not be treated as a replacement for input or business validation.

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

🔎 What is Endpoint Metadata?
An ASP.NET Core endpoint contains information that describes what should execute and how the endpoint should be treated.

An endpoint commonly has:

RequestDelegate → code that executes the endpoint
Metadata → information attached to the endpoint
Metadata can describe things such as:

Authorization requirements
CORS requirements
HTTP methods
Endpoint names
Custom application metadata
Framework-specific endpoint information
✅ Example – Authorization Metadata
app.MapGet("/secure", () => "Secret Data")
   .RequireAuthorization();

The endpoint receives authorization-related metadata.

Authorization middleware can use this metadata to determine that the endpoint requires authorization.

🔍 Reading Endpoint Metadata
Middleware can inspect the selected endpoint:

app.Use(async (context, next) =>
{
    var endpoint = context.GetEndpoint();

    if (endpoint?.Metadata.GetMetadata<IAuthorizeData>() != null)
    {
        Console.WriteLine("This endpoint requires authorization.");
    }

    await next();
});

⚠️ Important
GetEndpoint() returns the endpoint selected by routing.

Therefore, middleware that wants to inspect endpoint metadata must run after endpoint routing has selected an endpoint.

Flow
HTTP Request
     ↓
Routing selects endpoint
     ↓
HttpContext.GetEndpoint()
     ↓
Middleware reads endpoint metadata
     ↓
Middleware/framework applies relevant policies
     ↓
Endpoint executes

🧠 Key Idea
Endpoint metadata is declarative information attached to an endpoint. Routing selects the endpoint, and middleware/framework components can inspect its metadata to apply behavior or policies.

⭐ Interview Example
If asked:

"How does authorization know that /secure requires authorization?"

A good answer is:

RequireAuthorization() adds authorization metadata to the endpoint. After routing selects that endpoint, the authorization middleware reads the endpoint's metadata and performs authorization.

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
### LinkGenerator Methods

| Method            | Returns         |
| ----------------- | --------------- |
| `GetPathByAction` | Relative path   |
| `GetUriByAction`  | Absolute URI    |
| `GetPathByPage`   | Razor Page path |
| `GetUriByPage`    | Absolute URI    |


---

Here is the **entire Point 11**, properly formatted as Markdown and optimized for interview preparation:

# 📌 1️⃣1️⃣ Ambient vs Explicit Route Values

When ASP.NET Core generates URLs, it can use route values from two sources:

---

## 🔹 Ambient Values

**Ambient values** are route values already associated with the current request.

For example, if the current route is:

```text
/products/details/10
````

The current route values may contain:

```text
controller = Products
action     = Details
id         = 10
```

These values can be reused during URL generation when appropriate.

---

## 🔹 Explicit Values

**Explicit values** are values supplied directly to the URL-generation API.

Example:

```csharp
_linkGenerator.GetPathByAction(
    "Details",
    "Products",
    new { id = 20 });
```

Here:

```text
controller = Products
action     = Details
id         = 20
```

are explicitly supplied for URL generation.

---

## 🧠 Ambient Value Invalidation

A useful rule for interview purposes:

> **When generating a URL, changing a value earlier in the route can invalidate dependent ambient values that appear later in the route.**

Example template:

```text
{controller}/{action}/{id?}
```

Suppose the current request has:

```text
controller = Products
action     = Details
id         = 10
```

If URL generation changes:

```text
controller = Orders
```

the existing ambient:

```text
id = 10
```

should not automatically be assumed to belong to the new controller/action combination.

This prevents unrelated route values from being carried forward incorrectly.

---

## ✅ Example

```csharp
var path = linkGenerator.GetPathByAction(
    httpContext,
    action: "Details",
    controller: "Orders",
    values: null);
```

The URL-generation system considers both:

* Explicit route values
* Compatible ambient route values

when finding a route.

---

## 🧠 Simple Mental Model

```text
Current Request
      ↓
Ambient Route Values
      ↓
+ Explicit Route Values
      ↓
URL Generation
      ↓
Generated URL
```

---

## ⭐ Interview Answer

> **Ambient route values come from the current request and can be reused during URL generation. Explicit route values are values supplied by the caller. When explicit values change earlier route parameters, dependent ambient values may no longer be valid and aren't blindly carried forward.**


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

Here is the **entire Point 13**, properly formatted as Markdown and optimized for interview preparation:

# 📌 1️⃣3️⃣ ShortCircuit()

## 🔎 What Problem Does This Solve?

Normally, after routing selects an endpoint, the request can continue through middleware before the endpoint executes.

For endpoints that need to execute immediately, ASP.NET Core provides `ShortCircuit()`.

---

## ✅ Example

```csharp
app.MapGet("/fast", () => "Fast response")
   .ShortCircuit();
````

With `ShortCircuit()`:

* The endpoint is executed immediately after routing.
* Middleware that would normally execute **after routing and before the endpoint** is bypassed.
* Middleware that already ran **before routing** is not undone.
* The request completes without continuing through the normal post-routing middleware pipeline.

---

## 🧠 Mental Model

### Normal

```text
Request
   ↓
Middleware
   ↓
Routing
   ↓
Authentication / Authorization / Other Middleware
   ↓
Endpoint
```

### With `ShortCircuit()`

```text
Request
   ↓
Middleware before routing
   ↓
Routing
   ↓
Short-circuited Endpoint

---

## ✅ Example – Short-Circuiting Known Paths

```csharp
app.MapShortCircuit(404, "robots.txt", "favicon.ico");
```

Requests to:

```text
/robots.txt
/favicon.ico
```

can be short-circuited with a `404` response.

---

## ⚠️ Important

Be careful when short-circuiting endpoints that depend on middleware-based behavior.

For example, if an endpoint requires:

```csharp
.RequireAuthorization()
```

or:

```csharp
.RequireCors(...)
```

you should not assume those middleware-based policies will execute normally when the endpoint is short-circuited.

---

## ⭐ Interview Answer

> **`ShortCircuit()` tells ASP.NET Core to execute the matched endpoint immediately after routing, bypassing middleware that would normally run after routing. Middleware that already executed before routing is unaffected.**

```
```


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


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

**Endpoint metadata** is information attached to an endpoint that tells ASP.NET Core how that endpoint should be treated.

An endpoint commonly contains:

- `RequestDelegate` → code that executes
- `Metadata` → information about the endpoint

Metadata can include:

- Authorization requirements
- CORS requirements
- HTTP methods
- Endpoint names
- Custom application metadata

---

## ✅ Example – Authorization Metadata

```csharp
app.MapGet("/secure", () => "Secret Data")
   .RequireAuthorization();
   
   ```
---
Ah, understood. You mean **you haven't understood Point 10 — URL Generation / `LinkGenerator`**. Let's learn that from the beginning, very simply.

# 📌 10️⃣ URL Generation — `LinkGenerator`

The basic idea is:

> **Routing is not only about receiving URLs. ASP.NET Core can also generate URLs for you.**

For example, suppose you have this API:

```csharp
[HttpGet("users/{id}")]
public IActionResult GetUser(int id)
{
    return Ok();
}
```

The route is:

```text
/users/10
```

Instead of manually writing:

```csharp
"/users/10"
```

ASP.NET Core can **generate the URL for you** using routing information.

---

## 🔹 What is `LinkGenerator`?

`LinkGenerator` is an ASP.NET Core service used to **generate URLs from endpoints/routes**.

Think:

```text
Route Definition
       ↓
LinkGenerator
       ↓
Generated URL
```

For example:

```text
Route: /users/{id}

id = 10

       ↓

/users/10
```

---

# 🔹 Why do we need it?

Imagine your application has:

```text
/api/users/{id}
```

Today it might be:

```text
/api/users/10
```

Tomorrow you change the route to:

```text
/api/v2/users/{id}
```

If your code contains:

```csharp
"/api/users/10"
```

you have to manually update it.

But if you generate the URL using routing:

```csharp
LinkGenerator
```

ASP.NET Core can use the route definition to generate the correct URL.

### 🧠 Simple idea

**Don't hard-code URLs when ASP.NET Core can generate them from routing information.**

---

# 🔹 Example

Suppose you have a controller:

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        return Ok(id);
    }
}
```

The route is:

```text
/api/users/{id}
```

We want to generate:

```text
/api/users/10
```

Using:

```csharp
LinkGenerator
```

---

# 🔹 `GetPathByAction()`

One common method is:

```csharp
_linkGenerator.GetPathByAction(
    "GetById",
    "Users",
    new { id = 10 });
```

Here:

```text
"GetById"
     ↓
Action

"Users"
     ↓
Controller

new { id = 10 }
     ↓
Route value
```

ASP.NET Core uses this information to find the appropriate route and generate a path.

Conceptually:

```text
Controller = Users
Action     = GetById
id         = 10

        ↓

LinkGenerator

        ↓

/api/users/10
```

---

# 🔹 Path vs URI

This is very important for interviews.

### `GetPath...`

Returns a **path**:

```text
/api/users/10
```

### `GetUri...`

Returns an **absolute URI**:

```text
https://example.com/api/users/10
```

So:

| Method            | Returns         |
| ----------------- | --------------- |
| `GetPathByAction` | Relative path   |
| `GetUriByAction`  | Absolute URI    |
| `GetPathByPage`   | Razor Page path |
| `GetUriByPage`    | Absolute URI    |

---

# 🔹 `GetPathByAction` vs `GetPathByName`

There's another important concept for interviews.

You can generate URLs using an **action/controller**:

```csharp
GetPathByAction(...)
```

or using a **named endpoint**:

```csharp
GetPathByName(...)
```

For example:

```csharp
app.MapGet("/users/{id}", (int id) => ...)
   .WithName("GetUser");
```

Now you can generate its URL by name:

```csharp
var path = linkGenerator.GetPathByName(
    httpContext,
    "GetUser",
    new { id = 10 });
```

Conceptually:

```text
Endpoint name = GetUser
id = 10

       ↓

/users/10
```

This is often very useful because the URL structure can change without requiring callers to know the route template.

---

# 🧠 The easiest way to remember Point 10

Think about routing in **two directions**:

### Incoming request

```text
URL
 ↓
Routing
 ↓
Endpoint
```

Example:

```text
GET /users/10
       ↓
GetById(10)
```

### URL generation

```text
Endpoint + Route Values
 ↓
LinkGenerator
 ↓
URL
```

Example:

```text
GetUser + id=10
       ↓
LinkGenerator
       ↓
/users/10
```

So:

> **Routing matches URLs to endpoints. `LinkGenerator` does the reverse: it generates URLs from endpoint routing information.**

### ⭐ Interview answer

> **`LinkGenerator` is an ASP.NET Core service used to generate URLs based on the application's endpoint routing configuration. It can generate paths or absolute URIs using actions, Razor Pages, or named endpoints together with route values.**

That is the **core concept you need to understand for Point 10**.


# 📌 1️⃣1️⃣ Ambient vs Explicit Route Values

When ASP.NET Core generates a URL, route values can come from:

- **Ambient values** → values from the current request
- **Explicit values** → values provided by the caller

---

## 🔹 Ambient Values

Ambient values come from the current route.

For example:

```text
/products/details/10
````

Current route values:

```text
controller = Products
action     = Details
id         = 10
```

ASP.NET Core can reuse these values during URL generation when appropriate.

---

## 🔹 Explicit Values

Explicit values are provided directly when generating the URL.

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

are explicit values.

---

## 🧠 Important Rule

> **Changing an earlier route value can invalidate later ambient values.**

Example:

```text
{controller}/{action}/{id?}
```

Current values:

```text
controller = Products
action     = Details
id         = 10
```

If we change:

```text
controller = Orders
```

the existing ambient `id = 10` should not automatically be carried over.

This prevents unrelated route values from being reused.

---

## ✅ Example

```csharp
var path = linkGenerator.GetPathByAction(
    httpContext,
    action: "Details",
    controller: "Orders",
    values: null);
```

ASP.NET Core considers:

* Explicit values
* Compatible ambient values

to generate the URL.

---

## 🧠 Simple Mental Model

```text
Current Request
      ↓
Ambient Values
      +
Explicit Values
      ↓
URL Generation
      ↓
Generated URL
```

---

## ⭐ Interview Answer

> **Ambient route values come from the current request, while explicit route values are provided by the caller. ASP.NET Core can reuse compatible ambient values during URL generation, but changing an earlier route value can invalidate later ambient values.**





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

Users for tenant abc


---

## 🧠 Mental Model

Route Group = Folder for endpoints with shared configuration.


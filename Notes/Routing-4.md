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


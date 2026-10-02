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
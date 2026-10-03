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

# 📌 1️⃣0️⃣ Route Groups (.NET 7+)

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


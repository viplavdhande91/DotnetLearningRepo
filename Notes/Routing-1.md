# 🚀 Routing in .NET (.NET 6 / 7 / 8)

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
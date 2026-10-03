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

# 📌 1️⃣1️⃣ ShortCircuit()

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

# 📌 1️⃣2️⃣ Catch-All Parameters

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


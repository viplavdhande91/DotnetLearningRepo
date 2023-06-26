

## Dependency Injection

**Singleton** : creates a single instance once and reuses the same object in all calls. Use Singletons where you need to maintain application wide state, for example, application configuration, logging service, caching of data, etc.
```bash
  * Singletons are memory efficient as they are created once and reused everywhere.
  * Memory leaks in these services will build up over time.

```


**Scoped** : lifetime services are created once per request. For example, in MVC it creates one instance for each HTTP request, but it uses the same instance in the other calls within the same web request.
```bash
  * Scoped is a good option when you want to maintain state within a request.

```


**Transient** : lifetime services are created each time they are requested. This lifetime works best for lightweight, stateless services. Since they are created every time, they will use more memory & resources and can have negative impact on performance.
```bash
  * Transient is good for lightweight services with little or no state.

```

### Notes

- As far as I know, the Singleton is normally used for a **global single instance**. For example, you will have an **image store service** you could have a service to load images from a given location and keeps them in memory for future use.

- A scoped lifetime indicates that services are created once per client request. Normally we will use this for **SQL connection. It means it will create and dispose the sql connection per request.**

- A transient lifetime services are created each time they're requested from the service container. For example, during one request you use **httpclient service to call other web api request multiple times**, but the web api endpoint is different. At that time you will register the httpclient service as transient. That means each time when you call the httpclient service it will create a new httpclient to send the request not used the same one .

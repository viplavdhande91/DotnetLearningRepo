
# Project Title

A brief description of what this project does and who it's for



1. Explanation: In C#, IEnumerable is an interface, and it is useful to enable an iteration over non-generic collections, and it is available with System.Collections namespace.



## Depenedency Injection

**Singleton** : creates a single instance once and reuses the same object in all calls. Use Singletons where you need to maintain application wide state, for example, application configuration, logging service, caching of data, etc.
```bash
  * Singletons are memory efficient as they are created once and reused everywhere.
  * Memory leaks in these services will build up over time.

```


**Scoped ** : lifetime services are created once per request. For example, in MVC it creates one instance for each HTTP request, but it uses the same instance in the other calls within the same web request.
```bash
  * Scoped is a good option when you want to maintain state within a request.

```


**Transient ** : lifetime services are created each time they are requested. This lifetime works best for lightweight, stateless services. Since they are created every time, they will use more memory & resources and can have negative impact on performance.
```bash
  * Transient is good for lightweight services with little or no state.
  
```


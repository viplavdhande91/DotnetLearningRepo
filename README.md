# ASP.NET Core Filter Notes
1. Filters are a powerful feature in ASP.NET Core that allow you to add cross-cutting concerns and perform certain actions at various stages of the request processing pipeline. 
2. They help in implementing functionalities such as logging, exception handling, authorization, caching, and more.
3. Before request hits to Controller we can process our Filters also on result PreProcessing we can execute our filter implemented.

## Table of Contents
- [Overview](#overview)
- [Types of Filters](#types-of-filters)
- [Usage](#usage)
- [Creating Custom Filters](#creating-custom-filters)
- [Registering Filters](#registering-filters)
- [Ordering Filters](#ordering-filters)
- [Filter Execution](#filter-execution)
- [Resources](#resources)

## Overview
ASP.NET Core filters are categorized into the following types:
1. **Authorization filters**: Determine whether the current user is authorized to access a resource.
2. **Resource filters**: Handle resource-related concerns, such as caching.
3. **Action filters**: Perform actions before and after an action method executes.
5. **Exception filters**: Handle exceptions thrown during the execution of an action method.

4. **Result filters**: Perform actions before and after the execution of the action result.

# ARAER
## Types of Filters[Scope]
1. **Global Filters**: Applied to all controllers and actions by default. Configured during application startup.
2. **Controller Filters**: Applied to all actions within a specific controller or controller hierarchy.
3. **Action Filters**: Applied to specific action methods.


## Usage
To use filters in ASP.NET Core, you can apply them at **various levels** depending on your requirements. You can use **attributes** to apply filters directly to controllers or action methods. Additionally, you can register filters globally during application startup using the `AddMvc` or `AddControllers` methods.

For example, to apply an action filter to a specific action method, we can use the action atrribute as Action Filter:

```csharp
[MyActionFilter("ActionLevel")]
public IActionResult MyAction()
{
    // Action code
}
```

## Creating Custom Filters
You can create custom filters by implementing one of the filter interfaces provided by ASP.NET Core. For example, to create an action filter, you can implement the `IActionFilter` or `IAsyncActionFilter` interfaces.

```csharp
public class MyActionFilterAttribute : Attribute,IActionFilter
{
    private readonly string _name;

    public MyActionFilterAttribute(string name){
        _name = name;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        // Code to be executed before the action method
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // Code to be executed after the action method
    }
}
```

## Registering Filters
To register filters globally, you can use the `AddMvc` or `AddControllers` methods in the `Startup.cs` file.

```csharp
services.AddControllers(options =>
{
    options.Filters.Add(new MyActionFilterAttribute());
});
```

## Ordering Filters
Filters can be ordered to control the execution order. The execution order is determined by the filter scope and the filter type. Global filters execute first, followed by controller filters, and then action filters. Within each scope, the order is determined by the filter's `Order` property [As of now we have not implemented that].

## Filter Execution
The **order of execution[As per Microsoft Official Documentation]** for filters is as follows:
1. Authorization filters
2. **Resource filters (OnResourceExecuting)**
3. ****Action filters (OnActionExecuting)****
4. Result filters (OnResultExecuting)


5. ****Action filters (OnActionExecuted)****
6. Result filters (OnResultExecuted)
7. **Resource filters (OnResourceExecuted)**
8. Exception filters (OnException)



## Resources
- [Official ASP.NET Core Documentation on Filters for images](https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/filters)

- [Rahul nath video-1](https://www.youtube.com/watch?v=mKM6FbxMGI8&list=PL59L9XrzUa-nqfCHIKazYMFRKapPNI4sP&index=25&pp=iAQB&ab_channel=RahulNath)

- [Rahul nath video-2](https://www.youtube.com/watch?v=kqwjrJ4kb9Q&list=PL59L9XrzUa-nqfCHIKazYMFRKapPNI4sP&index=26&ab_channel=RahulNath)

- [Notes ppt] (https://drive.google.com/drive/folders/1tm1091K5-R370SmGFC9qVIe-GsCkY2S-?usp=sharing)



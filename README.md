
# Entity framework core
 


#### 1. Creation and Installation of Packages[Using CLI]
```
Microsoft.EntityFrameworkCore.Design

Microsoft.EntityFrameworkCore.SqlServer

Microsoft.EntityFrameworkCore.Tools
```




## 2. Migrations

#### Add Migration
```
Add-Migration InitialCreate

```

#### Update Database
```
Update-Database
```

## 3. Stored Procedure

#### With Parameters
```
   var result = await _context.Persons_.FromSqlRaw("EXEC SelectSpecificPerson {0}", personId.ToString()).ToListAsync();
             
    return Ok(result);
```

#### Without Parameters
```
    var result = await _context.Persons_.FromSqlRaw("SelectAllPersons").ToListAsync();

    return Ok(result);
```




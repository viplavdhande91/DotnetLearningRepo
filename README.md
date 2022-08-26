
# Entity framework core
 


## A)Creation and Installation of Packages

#### 1.Add  packages and  {Command 3 :Verify Installation Command} for dotnet-ef}
```
D:\dotnet-project>dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 3.1.3

D:\dotnet-project>dotnet tool install --global dotnet-ef --version 3.1.28

D:\dotnet-project>dotnet ef

D:\dotnet-project> dotnet add package Microsoft.EntityFrameworkCore.Design --version 3.1.28
```

#### 2.Add Db ts SQL Server and table then Run Command
```
dotnet ef dbcontext scaffold "Server=localhost\SQLEXPRESS;Database=gamedb;Trusted_Connection=True;" Microsoft.EntityFrameworkCore.SqlServer -o Models 

```


## B)Migrations

#### 1.Command for Updation Model [Note: --force flag we have to add]
```
dotnet ef dbcontext scaffold "Server=localhost\SQLEXPRESS;Database=gamedb;Trusted_Connection=True;" Microsoft.EntityFrameworkCore.SqlServer --force -o Models 

```


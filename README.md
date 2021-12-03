# ExtendedWebApi

## What it is

Is a library of abstract classes aimed to provide applications 
common access to a datatable in sql server by using EF core tools.
This access consists of 

- Get
  - By Id
  - By Id Details 
  - By Search Query with Paging
- Insert
  - Insert Child Rows
- Update
  - Update Child Rows
- Delete

The table must have a defined pk named id that is not composite. 
Otherwise BySearchQuery does not throw an error.
Encapsulation is possible but modifying the data throws an error.

## What its not

It's not compatible with all types of dbs or tables. Currently sql server is good.

## How to use it

The classes are tightly coupled with EF. In order to work, code needs to be applied, yo.

- inherit db table model class.
- inherit models repo class
- inherit models ApiController class.
- add repo to inject at startup

### Models

Modify models resulting from the ef scaffolding command
```
dotnet ef dbcontext scaffold "your_connection_string" -t tableName
```

so they inherit IEntity. 

```
public class Example : IEntity<T> // where T is the data type of your id column in the database
{
    override public T Id {get; set;};
    ...
} 
```

### Repo

```
public class ExampleRepository : EFCoreRepository<Example, ExampleRepository, T> 
{

}

```

### Controllers

```
public class ExampleController : BaseRestAPIController<Example, ExampleRepository, T> 
{
    // override any route here
}
```

### Startup

Add `app.AddScoped<ExampleRepository>` to ur startup ConfigureServices function.




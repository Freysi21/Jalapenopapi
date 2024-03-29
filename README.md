## WIP
This copy is outdated. I've updated to dotnet6. I've added a bunch of new features. Just need to give myself the time to move all development from my work environment to here. 
So to try it out you have to checkout the repo and add it as referenced project in solution or build the project and reference the artifact in your proj.

# JalapenopAPI 🌶🥵
- [JalapenopAPI🌶️🥵](#jalapenopapi️)
  - [What it is](#what-it-is)
    - [Prerequirements](#prerequirements)
      - [Entity Framework](#entity-framework)
      - [dotnet5.0](#dotnet50)
      - [Data pattern](#data-pattern)
        - [Surrogate key vs natural key](#surrogate-key-vs-natural-key)
      - [Tools](#tools)
  - [What its not](#what-its-not)
  - [Why](#why)
  - [How to use it](#how-to-use-it)
    - [Models](#models)
      - [ModelAttributes](#modelattributes)
      - [Circular Reference](#circular-reference)
    - [Repo](#repo)
    - [Controllers](#controllers)
    - [Startup](#startup)
  -  [Thanks to our lord and saviour Jon Skeet](#jon-skeet)
  
## What it is

Is a library of abstract classes aimed to provide applications 
common access to a table or view in sql server by using EF core tools.
This access consists of 

- Get
  - By Id
  - By Id Details 
  - By Search Query with Paging
- Insert (POST)
  - Insert Child Rows
- Update (PUT)
  - Update Child Rows
- Delete (DELETE)

By Details means any foreign key relations 
in other tables will be returned as nested property of the body response.
Search Query supports primitive types such as strings, guids and numbers. It also 
supports date range queries. It will always default to creating AND clauses for
each search criteria and selecting one method for each data type. Strings will 
always be compared with StartsWith, guids and numbers are an exact match. Dates
support an exact match or for range queries less than and greater than. Also
selecting ordering by one column is supported. Insert Child Rows and Update Child 
rows means if the nested table properties from a By Details response is modified 
and returned to Insert, without ids, would create new rows in the related tables 
as well while Update would update existing rows.  

### Prerequirements
#### Entity Framework
The tables model class needs to be generated by running the cli command 
```
dotnet ef dbcontext scaffold "your_connection_string" -t tableName
```
#### dotnet5.0

Currently it runs on dotnet5.0. There exists a dotnet 3.1 version as well if anyone
is interested. 

#### Data pattern

The table must have a defined pk named id that is not composite. 
Otherwise all functions fail except BySearchQuery.
Encapsulation is possible but modifying the data throws an error.
View models work the same as a table without a pk.

##### Surrogate key vs natural key

I went about this the first time with composite key support. Shits hard. I love natural keys but
I have resulted to surrogates just because of this package. I still apply unique constraints on
natural keys. It's easier with the tools. Not that EF is not handling it. My implementation 
just hasn't gotten there.  

#### Tools

Under Expressions sql builders are available and can be used with linq IQueryable. 
PagedList also adds skip take tokens to queries. 

## What its not

It's not compatible with all types of dbs. Currently sql server is good.

## Why

After working with databases and webservices in Windows world for 5 years I found that I did some things
repeatedly. Creating table definitions. Scaffolding models and implementing the web CRUD stuff
along with some search parameter thingies. I used asp-dotnet-generator for the crud stuff but
the filtering/searching wasn't a part of that + the details part. When using different tech stacks
like elastic or mongodb, The webservicing part has less overhead in most cases. This is my way
of getting to live in both worlds and saving some time on the ApiController stuff. 

## How to use it

Some code is required to function.

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
public class Example : IEntity<T> // where T is the data type of the id column in the database
{
    override public T Id {get; set;};
    ...
} 
```

#### ModelAttributes

Scaffolded models need to be decorated with attributes. one to- relation should be decorated with SingleNavigation and many to- should be decorated with Navigation.
DefaultSort effects List queries and there default order.

#### Circular Reference

When scaffolded, models will contain reference to each other if they are related. So you need to remove it from one of them. If needed, you can create a duplicated class of
a table for a workaround. 

### Repo

```
public class ExampleRepository : EFCoreRepository<Example, ExampleRepository, T> 
{
    // override any route here
}
```

### Controllers

```
public class ExampleController : JalapenopAPIController<Example, ExampleRepository, T> 
{
    // override any route here
}
```

### Startup

Add `app.AddScoped<ExampleRepository>` to ur startup ConfigureServices function.


### Jon Skeet

Most of this is either inspired or copy pasted from answers by Jon Skeet the csharp saviour.

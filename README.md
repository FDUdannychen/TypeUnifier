# Type Unifier

A library that dispatches member access of a type A to its mapped types when there are no common interfaces or inheritances between them.

# Concepts
##### 1. Abstraction
An abstraction is an interface or an enum that implementations are unified to. Usually you write code on abstractions, the calls are forwarded to its mapped types (implementations).
```csharp
//in assembly "DemoAbstraction"
public enum UserType
{
    Guest = 0,
    User = 1,
    Admin = 2
}
public interface IUser
{
    [Constructor] IUser New(string name, UserType type);
    UserType Type { get; }
    string Name { get; }
    void ChangeType(UserType newType);
}
```
##### 2. Implementation
An implementation is a class or an enum that mapped to an abstraction. Method calls on abstractions are forwarded to implementations.
```csharp
//in assembly "DemoImplementation1"
public enum UserType
{
    Guest = 0,
    User = 1
}
public class User
{
    public User(string name, UserType type)
    {
        this.Name = name;
        this.Type = type;
    }
    
    public UserType Type { get; }
    public string Name { get; }
    //note that ChangeType is not implemented
}
```
##### 3. Node
A node is a collection of implementations, where the mappings between abstractions and implementations are defined. You can implement `INode` or inherit from `Node`.
```csharp
public class Node1 : Node
{
    public Node1() : base("node1")
    {
        this.AddMapping<DemoAbstraction.UserType, DemoImplementation1.UserType>();
        this.AddMapping<DemoAbstraction.IUser, DemoImplementation1.User>();
        //or you can use Scan method to add mappings in batch
    }
}
```
#### 4. Dispatcher
A dispatcher is built from a collection of nodes, using which you can create instances and invoke methods of abstractions.
```csharp
var dispatcher = new Dispatcher(new Node1(), new Node2());
var user = dispatcher.For<IUser>("node1").New("test user", UserType.User);
var userName = user.Name;           //OK
user.ChangeType(UserType.Guest);    //throws NodeNotImplementedException
```
# Features
#### 1. Constructors
In abstractions, methods with `ConstructorAttribute` are mapped to constructors. Usually constructors are named `New`, but other names are also fine. It's a good practice to use the same literal for constructors because you will have friendly overload prompts from your IDE. A constructor's return type should be the same as it's declearing type(`IUser` in the below example).
```csharp
public interface IUser
{
    [Constructor] IUser New(string name);
    [Constructor] IUser SomethingElse(string name, int age);   //works but not recommended
}
```
### 2. Static Members
In abstrctions, members with `StaticAttribute` are mapped to corresponding static members. You don't need to create an instance when invokeing static members just like C# itself. Note that `StaticAttribute` can be applied to methods only.
```csharp
public interface IUserService
{
    [Static] IUser CreateUser(string name);
    int MaxUserCount { [Static]get; }
    [method: Static] event EventHandler OnUserCreated;
}
public class UserService
{
    public static User CreateUser(string name)
    {
        var user = new User(name);
        OnUserCreated(this, EventArgs.Empty);
        return user;
    }
    public static int MaxUserCount { get { return 100; } }
    public static event EventHandler OnUserCreated;
}
var userService = _dispatcher.For<IUserService>("node");
var maxUserCount = userService.MaxUserCount;
userService.OnUserCreated += delegate { Trace.WriteLine("User Created"); };
```
### 3. Instance Members
The instance members are similar to static members, except that there is no `StaticAttribute` and you need to create an instance before accessing them.
```csharp
//var userName = _dispatcher.For<IUser>("node").Name;       //throws NotConstructedException
var user = _dispatcher.For<IUser>("node").New("test user");
var userName = user.Name;                                   //OK
user.New("another");                                        //throws MultipleConstructionException
```
### 4. Member Alias
For both static and instance members, you can use `AliasAttribute` in case the corresponding names are different in implementations.
```csharp
//abstraction
public interface IUser
{
    [Alias("ChangeType", "ChangeTypeUnsafe")] void ChangeType(UserType newType);
    void ChangeTypeSafe(UserType newType);
}
//implementation1
public class User
{
    public void ChangeType(UserType newType) {  }
}
//implementation2
public class User
{
    public void ChangeTypeUnsafe(UserType newType) { }
    public void ChangeTypeSafe(UserType newType) { }
}
```
> Currently when using `AliasAttribute` on properties/events, you should add the corresponding prefix because the compiler will generate `get_XXX` `set_XXX` methods for properties and `add_XXX` `remove_XXX` for events. For example, if the property `Name` has its alias `FullName`, the correct abstraction should be
`string Name { [Alias("get_Name", "get_FullName")] get; }`
### 5. Exceptions
Exceptions can also be unified and handled. Your abstraction interface of the exception should inherit from `IException`, and catched by `Exception<T> where T : IException`.
```csharp
//abstraction
public interface IUserService
{
    [Static] IUser CreateUser(string name);
}
public interface IUserAlreadyExistsException : IException
{
    string Name { get; }
}
```
```csharp
//implementation1
public class UserAlreadyExistsException : Exception
{
    public string Name { get; }
    public UserAlreadyExistsException(string name) 
        : base($"User {name} already exists")
    { 
        this.Name = name;
    }
}
public class UserService
{
    public static User CreateUser(string name)
    {
        var user = GetUser(name);
        if (user != null) throw new UserAlreadyExistsException(name);
        //...
    }
}
```
```csharp
//implementation2
public class UserAlreadyExistsException : Exception
{
    public string Name { get; }
    public UserAlreadyExistsException(string name, string createdBy) 
        : base($"User {name} already exists, created by {createdBy}")
    { 
        this.Name = name;
    }
}
public class UserService
{
    public static User CreateUser(string name)
    {
        var user = GetUser(name);
        if (user != null) throw new UserAlreadyExistsException(name, user.CreatedBy);
        //...
    }
}
```
```csharp
//usage
try
{
    _dispatcher.For<IUserService>(nodeId).CreateUser("test");
}
catch (Exception<IUserAlreadyExistsException> e)
{
    logger.Error(e.Message);        //the properties of Exception are mapped automatically
    IUserAlreadyExistsException ex = e.Abstraction;
    var existingName = ex.Name;
    //...
}
```
### 6. Type Mapping
You can add mappings between abstractions and implementations using `Node.AddMapping`, or use `Node.Scan` to add mappings in batch. `Node.AddMapping` explicitly has a higher priority than `Node.Scan`. For one abstraction in one node, only one implementation is allowed. When you add mapping between `TAbs` and `TImpl`, some related mappings are added automatically including `T[]` `IEnumerable<T>` `ICollection<T>` `IList<T>` and `T?` for enums, as well as their possible combinations like `IEnumerable<TAbs[]>` to `IEnumerable<TImpl[]>`. When using `Node.Scan`, you can use a custom `IScanConvention` (or inherit from `DefaultScanConvention`) to filter mappings you don't want.

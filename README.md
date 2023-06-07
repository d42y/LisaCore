## LisaCore

### 1 Code Processor

LisaCore Code Processor .NET library provides a powerful and flexible way to execute C# code at runtime. With the ability to add references, import namespaces, and execute code with optional callbacks, this library is a valuable tool for developers looking to add dynamic code execution capabilities to their applications.

### 2 Requirements

.NET Core 3.1 or later

### 3 Getting Started

#### 3.1 Install the library

To use the CodeProcessor library, first, add a reference to the library in your project. Install package from NuGet.  [https://www.nuget.org/packages/LisaCore](https://www.nuget.org/packages/LisaCore)

#### 3.2 Initialize CodeProcessor

Create an instance of the CodeProcessor class:

```csharp
using LisaCore.Interpreter;
var codeProcessor = new CodeProcessor();

```

#### 3.3 Add references and namespaces

Add the necessary assembly references and namespaces for your code. By default, some common namespaces are already imported. Default assemblies:

```css
LisaCore Code Processor loads all relevant assemblies from your project and
assemblies for default namespaces below.

```

Default namespaces:

```python
System
System.IO
System.Linq
System.Text
System.Collections.Generic
Microsoft.EntityFrameworkCore

```

Loading your custom namespaces:

```less
codeProcessor.AddReference(Assembly.Load("MyAssembly"));
codeProcessor.AddNamespace("MyNamespace");

```

#### 3.4 Execute code

Execute C# code snippets by calling the ExecuteAsync method:

```csharp
string code = "return 1 = 2;";
var (result, error) = await codeProcessor.ExcecuteAsync(code);
if(error != null)
{
    Console.WriteLine($"Error: {error.Message}");
}
else 
{
    Console.WriteLine($"Result: {result}");
}

```

### 4 Advanced Usage

#### 4.1 Callbacks

You can also provide a callback function to return a value during code processing or get the result of the executed code:

```csharp
async Task MyCallBack(object? result) 
{
    Console.WriteLine($"Result from callback: {result}");
}
string code = "int a = 1; Callback(a); return a + 3;";
await codeProcessor.ExecuteAsync(code, MyCallBack);

```

Output:

```sql
Result from callback: 1
Result from callback: 4

```

#### 4.2 Code validation

To check whether a code snippet is valid without executing it, call the IsCodeValid method:

```csharp
string code = "return 5 - 3;";
bool isValid = codeProcessor.IsCodeValid(code, out var diagnostics);
if (isValid)
{
    Console.Writeline("Code is valid.");
}
else 
{
    Console.WriteLine("Code is not valid. Errors:");
    foreach (var diagnostic in diagnostics)
    {
	    Console.WriteLine(diagnostic.ToString());
    }
}

```

#### 4.3 Custom Class

As noted in section 3.3, LisaCore Code Processor loaded all relevant assemblies form your project. However, you need to reference namespaces you want to use in our script. There are two methods to load assembly.

1.  Use AddNamespace function. This method is persistance and allows you to reference MyApp.Models in all of your scripts.
    
    codeProcessor.AddNamespace("MyApp.Models");
    
2.  Include using statement in your script. The using namespace only apply to current code.
    
    string code =@" using MyApp.Models; var person = new Person { Name = ""John Doe"", Age = 30 }; return person.Name;";
    

Completed code:

```csharp
codeProcessor.AddNamespace("MyApp.Models");
string code = @"
var person = new Person { Name = ""John Doe"",  Age = 30 };
return person.Name;";
var (result, error) = await codeProcessor.ExecuteAsync(code);
if (error != null)
{
    Console.WriteLine($"Error: {error.Message}");
}
else 
{
    Console.WriteLine($"Result: {result}");
}

```

#### 4.4 Custom Function

##### Your Custome Function

```cpp
// CustomFunctions.cs
namespace MyCustomFunctions
{
     public static class MathFunctions
     {
	     public static int Square(int number)
	     {
		     return number * number;
	     }
     }
}

```

##### Add a reference to the assembly containing the custom function and the namespace in the CodeProcessor:

```csharp
//Add reference to the assembly containing the custom functions
codeProcessor.AddReference(typeof(MathFunctions).Assembly);
//Add the namespace containing the custom functions
codeProcessor.AddNamespace("MyCustomFunctions");

```

##### Your code can reference and use the custom function:

```csharp
string code = "return MathFunctions.Square(5);";
var (result, error) = await codeProcessor.ExecuteAsync(code);
if (error != null)
{
    Console.WriteLine($"Error: {error.Message}");
}
else 
{
    Console.WriteLine($"Result: {result}");
}

```

#### 4.5 Using DbContext for database operations

```csharp
//your EF code
var dbContext = new MyDbContext();
string code = @"
var customers = DbContext.Set<Customer>().ToList();
return customers.Count;";
var (result, error) = await codeProcessor.ExecuteAsync(code, null, dbContext);
if (error != null)
{
    Console.WriteLine($"Error: {error.Message}");
}
else 
{
    Console.WriteLine($"Result: {result}");
}
```
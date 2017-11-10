# [CheddarGetter](https://cheddargetter.com) .Net Core API Services Wrapper

Uses .Net Core Dependancy Injection for ChaddarGetter API (extended from John Siladie's [nofxsnap/CheddarGetter repo](https://github.com/nofxsnap/CheddarGetter)).  API wrapper has implemented all of CheddarGetter's APIs as of 10/21/2017.

**1) Install required NuGet packages**

To use .NET Core dependency injection with options, these two packages are needed:

```Microsoft.Extensions.DependencyInjection``` - this is the package for the core dependency injection features, such as the ```ServiceCollection``` class.

```Microsoft.Extensions.Options``` – this package contains the ```IOptions``` interface, the ```OptionsManager``` for instantiating the options, as well as the extension methods ```AddOptions``` and ```Configure```.


**2) Install CheddarGetter.CollabCoders NuGet Package**

https://www.nuget.org/packages/CheddarGetter.CollabCoders


**3) Add the CheddarGetter namespace of your Startup.cs** 

```csharp
using CheddarGetter;
```


**4) Register the CheddarGetter Service with dependancy injection in the ConfigureServices portion of your Startup.cs**: 

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddCheddarGetterService(options =>
    {
	options.productCode = "YourCheddarGetterProductCode";
	options.username = "YourCheddarGetterUserName";
	options.password = "YourCheddarGetterPassword";
    });
}
```


**5) Add the CheddarGetter Service to you constructor on your Controller or Services**

```csharp
using CheddarGetter;

public class YourController
{
    //Constructor
    private readonly ICheddarGetterService _cheddarGetterService;
    
    public YourController(ICheddarGetterService cheddarGetterService)
    {
    	_cheddarGetterService = cheddarGetterService;
    }
}
```


**6) Add the CheddarGetter Service interface to you constructor on your Controller or Services**

```csharp
public class YourController
{
    //Constructor
    private readonly ICheddarGetterService _cheddarGetterService;

    public YourController(ICheddarGetterService cheddarGetterService)
    {
        _cheddarGetterService = cheddarGetterService;
    }

    //Sample Usage of CheddarGetterService
    public async Task<IActionResult> UpdateCustomer(SomeSampleUserModel user) 
    {
        var customer = new Customer
	{
	    Code = user.userId.ToString(),
	    FirstName = user.firstName,
	    LastName = user.lastName,
	    Email = user.email,
	    Company = user.Company,
	    RemoteAddress = Request.HttpContext.Connection.RemoteIpAddress,
	    AdditionalMetaData = "metaData[someCustomParam]=someCustomValue&metaData[someOtherParam]=SomeOtherValue"
        };
	await _cheddarGetterService.CreateCustomer(customer);
	return Json(customer);
    }
}
```
NOTE: CheddarGetter allows you to store custom meta data for each customer.  The ```Customer``` and ```CustomerPost``` models have an extra string parameter called ```AdditionalMetaData```, which you can use to pass an array of meta data parameters in a standard QueryString format (like in the example above).

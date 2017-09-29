# [CheddarGetter](https://cheddargetter.com) .Net Core API Services Wrapper

Uses .Net Core Dependancy Injection for ChaddarGetter API (extended from John Siladie's [nofxsnap/CheddarGetter repo](https://github.com/nofxsnap/CheddarGetter)).  API wrapper has implemented all of CheddarGetter's APIs as of 09/28/2017.


**1) Install CheddarGetter.CollabCoders NuGet Package**

https://www.nuget.org/packages/CheddarGetter.CollabCoders/1.0.0


**2) Add the CheddarGetter namespace of your Startup.cs**: 

```csharp
using CheddarGetter;
```


**3) Register the CheddarGetter Service with dependancy injection in the ConfigureServices portion of your Startup.cs**: 

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


**4) Add the CheddarGetter Service to you constructor on your Controller or Services**

```csharp
using CheddarGetter;

public class YourController
{
    private readonly CheddarGetterConfig _config;
    public YourController(IOptions<CheddarGetterConfig> options)
    {
        _config.productCode = options.Value.productCode;
        _config.username = options.Value.username;
        _config.password = options.Value.password;
    }
}
```


**5) Add the CheddarGetter Service to you constructor on your Controller or Services**

```csharp
public class YourController
{
    private readonly CheddarGetterConfig _config;

    public YourController(IOptions<CheddarGetterConfig> options)
    {
        _config.productCode = options.Value.productCode;
        _config.username = options.Value.username;
        _config.password = options.Value.password;
    }

    public async Task<IActionResult> UpdateCustomer(SomeSampleUserModel user) 
    {
        var customer = new Customer
	{
	    Code = user.userId.ToString(),
	    FirstName = user.firstName,
	    LastName = user.lastName,
	    Email = user.email,
	    Company = user.Company,
	    AdditionalMetaData = "metaData[ip]=" + Request.HttpContext.Connection.RemoteIpAddress + "&metaData[someOtherParam]=SomeOtherValue"
        };
	await _cheddar.CreateCustomer(customer);
	return Json(customer);
    }
}
```
NOTE: CheddarGetter allows you to store custom meta data for each customer.  The ```Customer``` and ```CustomerPost``` models have an extra string parameter called ```AdditionalMetaData```, which you can use to pass an array of meta data parameters in a standard QueryString format (like in the example above).

# Introduction

Write something fancy here

# Configuration
## Configure DI
In startup.cs:
````
public void ConfigureServices(IServiceCollection services)
{
    // Add this at the end of the method
    services.AddMtgDatabase();
}
````
## Configure Services
In startup.cs:
````
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    // Initialize mtg app service
    var baseFolder = new DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MtgDatabase"));
    var service = app.ApplicationServices.GetService<IMtgDatabaseService>();
    service.Configure(baseFolder, new ScryfallConfiguration());
}
````
 Feel free to set different cache values in ````ScryfallConfiguration````
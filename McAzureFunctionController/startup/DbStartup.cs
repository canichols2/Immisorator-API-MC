//using CosmosDataStore;
//using McAzureFunctionController.startup;
//using Microsoft.Azure.Functions.Extensions.DependencyInjection;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using System;

//[assembly: FunctionsStartup(typeof(DbStartup))]

//namespace McAzureFunctionController.startup;

//internal class DbStartup : FunctionsStartup
//{
//    private IConfigurationRoot Configuration;

//    public override void Configure(IFunctionsHostBuilder builder)
//    {
//        var configSectionCosmos = Configuration.GetSection("cosmos");

//        builder.Services.AddDbContext<AzureContext>(
//            options => options.UseCosmos(configSectionCosmos.Value, "ServerStore"));

//    }
//    public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
//    {
//        base.ConfigureAppConfiguration(builder);
//        Configuration = builder.ConfigurationBuilder.Build();
//    }
//}

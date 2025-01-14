using AuthoringTool.API;
using AuthoringTool.API.Configuration;
using AuthoringTool.BusinessLogic;
using AuthoringTool.BusinessLogic.API;
using AuthoringTool.DataAccess.API;
using AuthoringTool.PresentationLogic.API;
using AuthoringTool.PresentationLogic.AuthoringToolWorkspace;
using AuthoringTool.PresentationLogic.LearningSpace;
using AuthoringTool.PresentationLogic.LearningWorld;
using ElectronNET.API;
using ElectronNET.API.Entities;

public class Startup
{
    public IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        //NLog for logging
        //TODO: find out why nlog won't log our runtime errors to console, so its disabled for now :/
        /*
        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.SetMinimumLevel(LogLevel.Trace);
            builder.AddNLog();
        });
        */

        //AuthoringTool
        services.AddSingleton<IAuthoringToolConfiguration, AuthoringToolConfiguration>();
        services.AddSingleton<IDataAccess, DataAccess>();
        services.AddSingleton<IBusinessLogic, BusinessLogic>();
        services.AddSingleton<IPresentationLogic, PresentationLogic>();
        services.AddSingleton<IAuthoringTool, AuthoringTool.API.AuthoringTool>();
        services.AddSingleton<ILearningWorldPresenter, LearningWorldPresenter>();
        services.AddSingleton<ILearningSpacePresenter, LearningSpacePresenter>();
        services.AddSingleton<IAuthoringToolWorkspaceViewModel, AuthoringToolWorkspaceViewModel>();
        services.AddSingleton<AuthoringToolWorkspacePresenter>();

        //Blazor and Electron
        services.AddRazorPages();
        services.AddElectron();
        services.AddServerSideBlazor();
        services.AddSingleton<MouseService>();
        services.AddSingleton<IMouseService>(provider => provider.GetRequiredService<MouseService>());
        if (HybridSupport.IsElectronActive)
        {
        }
        else
        {
        }
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapBlazorHub();
            endpoints.MapFallbackToPage("/_Host");
        });
        if (HybridSupport.IsElectronActive)
            Task.Run(async () =>
            {
                var options = new BrowserWindowOptions
                {
                    Fullscreenable = true,
                };
                return await Electron.WindowManager.CreateWindowAsync(options);
            });
        //exit app on all windows closed
        Electron.App.WindowAllClosed += () => Electron.App.Exit();
    }
}
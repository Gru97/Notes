# Changing Log Level At Runtime
Due to some restrictions we are facing in a project when it comes to using logging and monitoring tools, and other reasons beyond the scope of this document, we've decided to implement a machanism to change log level at runtime.
Here are the steps we took to do it:

We need to define categories for different processes of the application to temporarily change the log level of a specific category for debugging purpose . Changing global log level is also possible, but not our choice.
The first thing is to add our contextual data to all logs in a specific part of the program to distinguish different parts with a *category*. We are using **Serilog** library in **dotnet**, and it has a feature called **LogContext** . It's a way to enrich logs with custom data. [Here](https://github.com/serilog/serilog/wiki/Enrichment "Here") is the documents to setup and use LogContext.
There is a catch to using LogContext. It's thread local and since we are using Task.Run all over the project, we will lose the trace of our logs in a specific category if part of the code runs in a different thread. To fix this issue, we can use Microsoft logger **BeginScope** to add custom data to logs inside a specific scope. For example:

```csharp
using (logger.BeginScope(new Dictionary<string, object>{
    ["Category"] = "StockProcessingJob",
    ["CorrelationId"] = Guid.NewGuid()
}))
{
    logger.LogInformation("Processing credit card payment");
}
```
This way we can add custom data like **Category** to the scope we needed to change log level later.

We don't want the repetitive action of enriching logs with log scope to be all over the code, so we use Autofac interceptors to do it the AOP way using attributes. [Here](https://autofac.readthedocs.io/en/latest/advanced/interceptors.html "Here") is what you need to know about autofac interceptors.
We define an attribute like bellow:
```csharp
public class LogScopeAttribute : Attribute
{
    public LogScopeAttribute(Type type)
    {
        Type = type;
    }

    public Type Type { get; private set; }
}
```
And we write an interceptor:

```csharp
public class AsyncLogScopeInterceptor : IAsyncInterceptor
{
    private readonly IServiceProvider _serviceProvider; // To resolve ILogger dynamically

    public AsyncLogScopeInterceptor(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void InterceptSynchronous(IInvocation invocation)
    {
        var methodInfo = invocation.MethodInvocationTarget ?? invocation.Method;

        if (
            methodInfo.GetCustomAttributes(typeof(LogScopeAttribute), true).FirstOrDefault()
            is LogScopeAttribute attribute
        )
        {
            var loggerGenericType = typeof(ILogger<>).MakeGenericType(attribute.Type);

            var logger = (ILogger)_serviceProvider.GetRequiredService(loggerGenericType);

            using var scope = logger.BeginScope(
                new Dictionary<string, object>
                {
                    ["CorrelationId"] = Guid.NewGuid(),
                    ["Category"] = attribute.Type.Name,
                }
            );

            invocation.Proceed();
        }
        else
        {
            invocation.Proceed();
        }
    }

    public void InterceptAsynchronous(IInvocation invocation)
    {
        invocation.ReturnValue = InternalInterceptAsynchronous(invocation);
    }

    private async Task InternalInterceptAsynchronous(IInvocation invocation)
    {
        var methodInfo = invocation.MethodInvocationTarget ?? invocation.Method;

        if (
            methodInfo.GetCustomAttributes(typeof(LogScopeAttribute), true).FirstOrDefault()
            is LogScopeAttribute attribute
        )
        {
            var loggerGenericType = typeof(ILogger<>).MakeGenericType(attribute.Type);

            var logger = (ILogger)_serviceProvider.GetRequiredService(loggerGenericType);

            using var scope = logger.BeginScope(
                new Dictionary<string, object>
                {
                    ["CorrelationId"] = Guid.NewGuid(),
                    ["Category"] = attribute.Type.Name,
                }
            );

            invocation.Proceed();
            var task = (Task)invocation.ReturnValue;
            await task;
        }
        else
        {
            invocation.Proceed();
            var task = (Task)invocation.ReturnValue;
            await task;
        }
    }

    public void InterceptAsynchronous<TResult>(IInvocation invocation)
    {
        invocation.ReturnValue = InternalInterceptAsynchronous<TResult>(invocation);
    }

    private async Task<TResult> InternalInterceptAsynchronous<TResult>(IInvocation invocation)
    {
        var methodInfo = invocation.MethodInvocationTarget ?? invocation.Method;

        if (
            methodInfo.GetCustomAttributes(typeof(LogScopeAttribute), true).FirstOrDefault()
            is LogScopeAttribute attribute
        )
        {
            var loggerGenericType = typeof(ILogger<>).MakeGenericType(attribute.Type);

            var logger = (ILogger)_serviceProvider.GetRequiredService(loggerGenericType);

            using var scope = logger.BeginScope(
                new Dictionary<string, object>
                {
                    ["CorrelationId"] = Guid.NewGuid(),
                    ["Category"] = attribute.Type.Name,
                }
            );

            invocation.Proceed();
            var task = (Task<TResult>)invocation.ReturnValue;
            TResult result = await task;
            return result;
        }
        else
        {
            invocation.Proceed();
            var task = (Task<TResult>)invocation.ReturnValue;
            TResult result = await task;
            return result;
        }
    }
}

```
As you can see the interceptor implements the interface IAsyncInterceptor so that it can intercept methods that return void, Task or Task<TResult>.
Here is how we can register a class in autofac with this interceptor:
```csharp

   builder.RegisterType<AsyncLogInterceptorAdapter>();

   // Register all types that implement IJob (or your intermediary interface ILoggableJob)
   builder
       .RegisterAssemblyTypes(typeof(ReceiptAndPaymentLedgerManagerJob).Assembly) // or specify the assembly where your jobs are located
       .Where(t => typeof(IJob).IsAssignableFrom(t)) // Find all types that implement IJob
       .AsSelf()
       .SingleInstance()
       .EnableClassInterceptors()
       .InterceptedBy(typeof(AsyncLogInterceptorAdapter));
```

There's an important thing here. Autofac only accepts interceptors that implement *IInterceptor* and not *IAsyncInterceptor*. So  we use an adapter to fix this:

```csharp
public class AsyncLogInterceptorAdapter : IInterceptor
{
   
    //https://stackoverflow.com/questions/59875973/registering-using-castle-core-asyncinterceptor-interceptors
    private readonly IInterceptor inner;

    public AsyncLogInterceptorAdapter(IServiceProvider serviceProvider)
    {
        inner = new AsyncLogScopeInterceptor(serviceProvider).ToInterceptor();
    }

    public void Intercept(IInvocation invocation)
    {
        inner.Intercept(invocation);
    }
}

```

Here I wanted all my Quartz Jobs to be intercepted, hence the registration part.
Now we only need to add LogScope attribute to a method of a class. It takes the type of that class which will be used to resolve a logger for that type and call BeginScope on it.
Now that we have our contextual data added to logs, we need to setup Serilog to use it.
We want to change log level of a particular category to another level. So, for any category, we can configure a **sub logger**.
**Serilog subloggers** give us ways to do setup sinks and log level based on conditions. [Here](https://github.com/serilog/serilog/wiki/Configuration-Basics#sub-loggers "Here") is anything you need to know about sub loggers. This is how we can use it:

```csharp
 loggerConfiguration
        .WriteTo.Logger(lc =>
            lc.Filter.ByIncludingOnly(LogLevelManager.SourceContains("Mynamespace.Financial"))
                .MinimumLevel.ControlledBy(new LoggingLevelSwitch(LogEventLevel.Error))
                .WriteTo.Elasticsearch(elasticsearchSinkOptions)
                .WriteTo.Console()
        )
```
LogLevelManager is a class I created to handle log levels and filtering. We can use methods like ByIncludingOnly to tell Serilog when to use this sub logger's configuration. The method SourceContains is also a simple Func that filters logs based on their SourceContext property (or any property we need to do filtering by). 
There is another thing used here, **LogSwitch**. It's what we need when we want to change the log level dynamically at runtime.
There is another important point here. We must set the **global log level to the minimum level**, since sub loggers can not log any more verbose than the global log level.
Here is the complete LogLevelManager:
```csharp
public class LogLevelManager
{
    public static LoggingLevelSwitch GlobalLogLevel = new LoggingLevelSwitch(LogEventLevel.Verbose);
    private static Dictionary<string, LoggingLevelSwitch> _dynamicLogLevelCategories;

    static LogLevelManager()
    {
        InitializeDynamicLogLevelCategories();
    }

    private static void InitializeDynamicLogLevelCategories()
    {
        _dynamicLogLevelCategories = new Dictionary<string, LoggingLevelSwitch>();

        // Use reflection to find all classes with the LogScopeMarker attribute
        var typesWithLogScopeMarker = AppDomain
            .CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type =>
                type.GetMethods().Any(e => e.GetCustomAttributes(typeof(LogScopeAttribute)).Any())
            );

        // Add each class to the dictionary with the default LoggingLevelSwitch
        foreach (var type in typesWithLogScopeMarker)
        {
            _dynamicLogLevelCategories[type.Name] = new LoggingLevelSwitch(LogEventLevel.Error);
        }
    }

    public static LoggingLevelSwitch GetLogSwitchOfCategory(string key)
    {
        var logSwitch = _dynamicLogLevelCategories.TryGetValue(key, out var levelSwitch)
            ? levelSwitch
            : throw new Exception();
        return logSwitch;
    }

    public static void SetLogLevel(string category, LogEventLevel newLevel)
    {
        if (_dynamicLogLevelCategories.TryGetValue(category, out var levelSwitch))
        {
            levelSwitch.MinimumLevel = newLevel;
        }
    }

    public static Func<LogEvent, bool> IsInDynamicCategories()
    {
        return logEvent =>
        {
            if (logEvent.Properties.TryGetValue("Category", out var categoryValue))
            {
                return _dynamicLogLevelCategories
                    .Select(e => e.Key)
                    .Any(e => categoryValue.ToString().Trim('"').Contains(e));
            }
            return false;
        };
    }

    public static Func<LogEvent, bool> IsInCategory(string category)
    {
        return logEvent =>
        {
            if (logEvent.Properties.TryGetValue("Category", out var categoryValue))
            {
                return categoryValue.ToString().Trim('"').Contains(category);
            }
            return false;
        };
    }

    public static Func<LogEvent, bool> SourceContains(string substring)
    {
        return logEvent =>
        {
            if (logEvent.Properties.TryGetValue("SourceContext", out var sourceContext))
            {
                var containsSource = sourceContext.ToString().Contains(substring);
                var isDynamicCategory = LogLevelManager.IsInDynamicCategories().Invoke(logEvent);
                return !isDynamicCategory && containsSource;
            }

            return false;
        };
    }

    public static Dictionary<string, LoggingLevelSwitch> GetDynamicCategories() =>
        _dynamicLogLevelCategories;
}

```
And here is our log configuration:
```csharp
  public static LoggerConfiguration AddLoggingConfigurations(
      this LoggerConfiguration loggerConfiguration,
      IConfiguration configuration,
      string environment
  )
  {
      var elasticLogSettings = new ElasticLogSettings();
      configuration
          .GetRequiredSection(ElasticLogSettings.ConfigurationSectionName)
          .Bind(elasticLogSettings);

      var elasticsearchSinkOptions = new ElasticsearchSinkOptions(
          new Uri(elasticLogSettings.ServerUrl)
      )
      {
          AutoRegisterTemplate = false,
          IndexFormat = elasticLogSettings.IndexFormat,
          CustomFormatter = new EcsTextFormatter(),
          DetectElasticsearchVersion = true,
          RegisterTemplateFailure = RegisterTemplateRecovery.IndexAnyway,
          ModifyConnectionSettings = c =>
              c.GlobalHeaders(
                      new NameValueCollection()
                      {
                          { "Authorization", "ApiKey " + elasticLogSettings.ApiKey },
                      }
                  )
                  .ServerCertificateValidationCallback(
                      (o, certificate, arg3, arg4) =>
                      {
                          return true;
                      }
                  ),
      };
      loggerConfiguration
          .Enrich.FromLogContext()
          .Enrich.WithElasticApmCorrelationInfo()
          //.ReadFrom.Configuration(configuration)
          .WriteTo.Console()
          .MinimumLevel.ControlledBy(LogLevelManager.GlobalLogLevel)
      //.MinimumLevel.Override("Volo.Abp", LogEventLevel.Error)
      //.MinimumLevel.Override("OpenIddict", LogEventLevel.Error)
      //.MinimumLevel.Override("Microsoft", LogEventLevel.Error)
      //.MinimumLevel.Override("Elastic", LogEventLevel.Error)
      //.MinimumLevel.Override("System", LogEventLevel.Error)
      //.MinimumLevel.Override("Quartz", LogEventLevel.Error)
      ;
      foreach (var category in LogLevelManager.GetDynamicCategories())
      {
          loggerConfiguration.WriteTo.Logger(lc =>
              lc.Filter.ByIncludingOnly(LogLevelManager.IsInCategory(category.Key)) //e.g. ReceiptAndPaymentLedgerManagerJob
                  .WriteTo.Console()
                  .WriteTo.Elasticsearch(elasticsearchSinkOptions)
                  .MinimumLevel.ControlledBy(LogLevelManager.GetLogSwitchOfCategory(category.Key))
          );
      }

      loggerConfiguration
          .WriteTo.Logger(lc =>
              lc.Filter.ByIncludingOnly(LogLevelManager.SourceContains("Asa.Financial"))
                  .MinimumLevel.ControlledBy(new LoggingLevelSwitch(LogEventLevel.Error))
                  .WriteTo.Elasticsearch(elasticsearchSinkOptions)
                  .WriteTo.Console()
          )
          //// Sub-logger for Core
          .WriteTo.Logger(lc =>
              lc.Filter.ByIncludingOnly(LogLevelManager.SourceContains("Core"))
                  .MinimumLevel.ControlledBy(new LoggingLevelSwitch(LogEventLevel.Error))
                  .WriteTo.Elasticsearch(elasticsearchSinkOptions)
                  .WriteTo.Console()
          )
          //// Sub-logger for AccountingHub
          .WriteTo.Logger(lc =>
              lc.Filter.ByIncludingOnly(LogLevelManager.SourceContains("AccountingHub"))
                  .MinimumLevel.ControlledBy(new LoggingLevelSwitch(LogEventLevel.Error))
                  .WriteTo.Elasticsearch(elasticsearchSinkOptions)
                  .WriteTo.Console()
          );
      //.WriteTo.Async(x => x.Console())
      //.WriteTo.Elasticsearch(elasticsearchSinkOptions);
      return loggerConfiguration;
  }

```
As you see in the code, we commented out Overrides and global sink. If something like Microsoft logs has and override with a log level of Error, the sub loggers will bypass it. If you need this behavior you can uncomment it. But if you need to see *all* logs of a specific category when you change the log level to e.g. Info, you shouldn't use Overrides. Also the you do not need a global sink configuration since we wrote sub loggers in a way that any log will go through at least one of them.

We used reflection to get all classes that contain the attribute to create a category for them and store it on log manager.  We setup logger the first thing inside Main method so that we can have log at the very begining of runtime. `AppDomain
            .CurrentDomain.GetAssemblies()`  returns loaded assemblies. Since we load some of our modules at runtime, later, we had to configure Logger twice. Serilog has a clean way to change log configuration later. [Here](https://nblumhardt.com/2020/10/bootstrap-logger/ "Here") you can read about it.

At the startup:
```csharp
Log.Logger = new LoggerConfiguration()
            .AddLoggingConfigurationsBootstrapper(configurationRoot, environment)
            .CreateBootstrapLogger();
```

When we need a HostBuilderContext (and in our case, when modules are all loaded):
```csharp
  builder
      .Host.AddAppSettingsSecretsJson()
      .UseAutofac()
      .UseSerilog(
          (context, configuration) =>
          {
              configuration.AddLoggingConfigurations(context.Configuration, environment);
          }
      )
      .UseMetrics();
```

# Packages to keep back until further testing is done

| Package | Version | Reason to keep back |
|---------|---------|---------------------|
| CacheManager.Core | = 1.2.0 | Has some breaking changes, too risky to update for now. |
| CacheManager.SystemRuntimeCaching | = 1.2.0 | Has some breaking changes, too risky to update for now. |
| CronExpressionDescriptor | = 2.16.0 | Hoping to remove it completely, leave it for now. |
| CsvHelper | = 2.16.3 | Newer versions have breaking changes that need to be addressed. |
| dotless | 1.5.2 | This is deprecated anyway, so maybe we should just leave it. |
| EntityFramework | = 6.4.4 | Should go through some more testing before upgrading. |
| Fluid.Core | = 2.25.0 | Requires very careful testing when upgrading. |
| Ical.Net | = 4.2.0 | Version 5 has some minor breaking changes that we need to address internally. |
| jQuery | = 3.4.1 | Needs testing and verification that nothing will break. |
| MassTransit.Azure.ServiceBus.Core | = 8.2.5 | Newer versions indirectly depend on Microsoft.IdentityModel.Clients.ActiveDirectory > 3.0.0 which we can't do. |
| MassTransit.AzureServiceBus | = 5.5.6 | Newer versions indirectly depend on Microsoft.IdentityModel.Clients.ActiveDirectory > 3.0.0 which we can't do. This is actually legacy and only required because we are using old code in AzureServiceBus.cs, MassTransit.Azure.ServiceBus.Core is the one we should be using. |
| MassTransit.RabbitMQ | = 8.2.5 | Newer versions indirectly depend on Microsoft.IdentityModel.Clients.ActiveDirectory > 3.0.0 which we can't do. |
| Microsoft.Azure.SignalR.AspNet | = 1.0.14 | Latest version adds a dependency on a newer jQuery version than we have. |
| Microsoft.CodeDom.Providers.DotNetCompilerPlatform | = 3.6.0 | This is the Roslyn compiler for WebForms, needs extensive testing on upgrade. |
| Microsoft.SqlServer.Types | = 11.0.2 | I really just don't want to touch this unless we have to, even though it is probably safe. |
| System.Interactive.Async | = 3.2.0 | Don't think we need this, but don't want to remove until we know for sure. |

# Packages to keep back

| Package | Version | Reason to keep back |
|---------|---------|---------------------|
| CS-Script | < 4.0.0 | Version 4.0.0 has some pretty heavy breaking changes that require a .NET 5 SDK installed to work. |
| Microsoft.AspNet.Cors | = 5.2.9 | Microsoft.AspNet.WebApi.OData indirectly depends on this version and has no update. |
| Microsoft.AspNet.WebApi.Core | = 5.2.9 | Microsoft.AspNet.WebApi.OData indirectly depends on this version and has no update. |
| Microsoft.AspNet.WebApi.Cors | = 5.2.9 | Microsoft.AspNet.WebApi.OData indirectly depends on this version and has no update. |
| Microsoft.AspNet.WebApi.WebHost | = 5.2.9 | Microsoft.AspNet.WebApi.OData indirectly depends on this version and has no update. |
| Microsoft.IdentityModel.Clients.ActiveDirectory | < 3.0.0 | PowerBiUtilities.cs in Rock requires a version less than this to compile. |
| Microsoft.Net.Http.Headers | = 2.3.4 | Newer versions are not compatible with .NET Framework. |
| Microsoft.Owin | = 4.2.3 | Microsoft.AspNet.WebApi.OData depends on version less than 4.3.0 and has no update. |
| Microsoft.Owin.Cors | = 4.2.3 | Microsoft.AspNet.WebApi.OData depends on version less than 4.3.0 and has no update. |
| Microsoft.Owin.Host.SystemWeb | = 4.2.3 | Microsoft.AspNet.WebApi.OData depends on version less than 4.3.0 and has no update. |
| Microsoft.Owin.Security | = 4.2.3 | Microsoft.AspNet.WebApi.OData depends on version less than 4.3.0 and has no update. |
| OpenTelemetry.Exporter.OpenTelemetryProtocol | = 1.11.1 | gRPC not supported in later versions on .NET Framework. |
| RestSharp | = 105.2.3 | Breaking changes in update. |
| RestSharp.Newtonsoft.Json | = 1.0.0 | Some plugins depend on this DLL. |
| SixLabors.Fonts | < 2.0.0 | License changed. |
| SixLabors.Imagesharp | < 3.0.0 | License changed. |
| SixLabors.ImageSharp.Drawing | < 2.0.0 | License changed. |
| SmtpServer | < 11.0.0 | No longer supports .NET Framework. |
| WebGrease | = 1.5.2 | The newer version has a bug that throws an exception when minifying our JS files on production builds. |
| Z.EntityFramework.Plus.EF6 | = 5.1.31 | Newer versions require DbSet properties, see https://github.com/zzzprojects/EntityFramework-Plus/issues/836 |

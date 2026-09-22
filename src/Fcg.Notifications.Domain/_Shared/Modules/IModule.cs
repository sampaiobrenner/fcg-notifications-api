using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fcg.Notifications.Domain._Shared.Modules;

public interface IModule
{
    void ConfigureServices(IServiceCollection services, IConfiguration configuration);
}

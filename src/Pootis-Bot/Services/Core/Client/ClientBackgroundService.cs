using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Pootis_Bot.Services.Core.Client;

/// <summary>
///     Background service for running <see cref="ClientService"/>
/// </summary>
public sealed class ClientBackgroundService : BackgroundService
{
    private readonly ClientService clientService;
    
    public ClientBackgroundService(ClientService clientService)
    {
        this.clientService = clientService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await clientService.Start();
        
        await Task.Delay(-1, stoppingToken);

        await clientService.Stop();
    }
}
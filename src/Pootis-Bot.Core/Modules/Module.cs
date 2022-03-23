using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Pootis_Bot.Commands.Permissions;

#nullable enable
namespace Pootis_Bot.Modules;
#pragma warning disable 1998
/// <summary>
///     A module for Pootis-Bot. Can be used to add command and functions to the bot
/// </summary>
public abstract class Module
{
    private ModuleInfo cachedModuleInfo;

    /// <summary>
    ///     Gets info relating to the modules
    /// </summary>
    /// <returns></returns>
    protected abstract ModuleInfo GetModuleInfo();

    /// <summary>
    ///     Called on initialization
    /// </summary>
    protected virtual async Task Init()
    {
    }

    /// <summary>
    ///     Called after all modules are initialized.
    ///     <para>
    ///         Here is a good spot to check if other modules are loaded
    ///         with <see cref="ModuleManager.CheckIfModuleIsLoaded" />, in-case you want to soft-depend on another module.
    ///     </para>
    /// </summary>
    protected virtual async Task PostInit()
    {
    }

    /// <summary>
    ///     Called on shutdown
    /// </summary>
    protected virtual void Shutdown()
    {
    }
    
    /// <summary>
    ///     Return a non-null <see cref="IPermissionProvider" /> to add a permission provider to Pootis's command handler.
    /// </summary>
    /// <returns></returns>
    protected virtual IPermissionProvider? AddPermissionProvider()
    {
        return null;
    }

    /// <summary>
    ///     Called while Pootis is setting up the command handler. Allows you to add a service
    /// </summary>
    /// <param name="services"></param>
    protected virtual void AddToServices(IServiceCollection services)
    {
    }

    #region Discord Client Events

    /// <summary>
    ///     Called when the <see cref="DiscordSocketClient" /> connects
    /// </summary>
    /// <param name="client"></param>
    protected virtual async Task ClientConnected([DisallowNull] DiscordSocketClient client)
    {
    }

    /// <summary>
    ///     Called when the <see cref="DiscordSocketClient" /> is ready
    /// </summary>
    /// <param name="client"></param>
    /// <param name="firstReady">
    ///     Is this the first time that the bot's been ready?
    ///     <para>
    ///         The bot may disconnect and reconnect, invoking that the client is ready multiple times.
    ///     </para>
    /// </param>
    protected virtual async Task ClientReady([DisallowNull] DiscordSocketClient client, bool firstReady)
    {
    }

    /// <summary>
    ///     Called when the <see cref="DiscordSocketClient" /> gets a message that isn't installed command
    /// </summary>
    /// <param name="client"></param>
    /// <param name="message"></param>
    protected virtual async Task ClientMessage([DisallowNull] DiscordSocketClient client, SocketUserMessage message)
    {
    }

    #endregion

    #region Internal

    /// <summary>
    ///     Call this if you are accessing <see cref="ModuleInfo" /> from Pootis's core
    ///     <para>
    ///         It uses a cached version of <see cref="ModuleInfo" />, just in-case the module returns something different
    ///         each time.
    ///     </para>
    /// </summary>
    /// <returns></returns>
    internal ModuleInfo GetModuleInfoInternal()
    {
        if (cachedModuleInfo.ModuleName == null)
            cachedModuleInfo = GetModuleInfo();

        return cachedModuleInfo;
    }

    /// <summary>
    ///     Call this from Pootis's Core to call <see cref="Init" />
    /// </summary>
    internal void InitInternal()
    {
        Init().ConfigureAwait(false);
    }

    /// <summary>
    ///     Call this from Pootis's Core to call <see cref="AddPermissionProvider" />
    /// </summary>
    /// <returns></returns>
    internal IPermissionProvider? AddPermissionProviderInternal()
    {
        return AddPermissionProvider();
    }

    /// <summary>
    ///     Call this from Pootis's Core to call <see cref="PostInit" />
    /// </summary>
    internal void PostInitInternal()
    {
        PostInit().ConfigureAwait(false);
    }

    /// <summary>
    ///     Call this from Pootis's Core to call <see cref="Shutdown" />
    /// </summary>
    internal void ShutdownInternal()
    {
        Shutdown();
    }

    /// <summary>
    ///     Call this from Pootis's Core to call <see cref="ClientConnected" />
    /// </summary>
    internal void ClientConnectedInternal(DiscordSocketClient client)
    {
        ClientConnected(client).ConfigureAwait(false);
    }

    /// <summary>
    ///     Call this from Pootis's Core to call <see cref="ClientReady" />
    /// </summary>
    internal void ClientReadyInternal(DiscordSocketClient client, bool firstReady)
    {
        ClientReady(client, firstReady).ConfigureAwait(false);
    }

    /// <summary>
    ///     Call this from Pootis's Core to call <see cref="ClientMessage" />
    /// </summary>
    internal void ClientMessageInternal(DiscordSocketClient client, SocketUserMessage message)
    {
        ClientMessage(client, message).ConfigureAwait(false);
    }

    /// <summary>
    ///     Call this from Pootis's Core to call <see cref="AddToServices" />
    /// </summary>
    /// <param name="serviceCollection"></param>
    internal void AddToServicesInternal(IServiceCollection serviceCollection)
    {
        AddToServices(serviceCollection);
    }

    #endregion
}
#pragma warning restore 1998
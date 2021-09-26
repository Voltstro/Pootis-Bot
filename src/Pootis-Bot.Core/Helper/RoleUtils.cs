using System.Linq;
using Discord.WebSocket;

namespace Pootis_Bot.Helper
{
    /// <summary>
    ///     Utils for roles
    /// </summary>
    public static class RoleUtils
    {
        /// <summary>
        ///     Does the <see cref="SocketGuildUser"/> contain a role
        /// </summary>
        /// <param name="user"></param>
        /// <param name="roleId"></param>
        /// <returns></returns>
        public static bool HasRole(this SocketGuildUser user, ulong roleId) => user.Roles.Any(x => x.Id == roleId);
    }
}
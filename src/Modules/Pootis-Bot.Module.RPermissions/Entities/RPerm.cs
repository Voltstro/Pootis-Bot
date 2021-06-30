using System.Collections.Generic;

namespace Pootis_Bot.Module.RPermissions.Entities
{
    internal class RPerm
    {
        public string Command { get; set; }

        public List<RPermArgument> Arguments { get; set; } = new List<RPermArgument>();

        public List<ulong> Roles { get; set; } = new List<ulong>();

        public void AddRole(ulong role)
        {
            Roles.Add(role);
        }

        public bool DoesRoleExist(ulong role)
        {
            return Roles.Contains(role);
        }
    }
}
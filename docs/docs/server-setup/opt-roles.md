# Opt Roles

Opt roles a roles that users can opt into (as the name suggests). This is designed so if you you have a notification role for pings and you want user to chooses wether or not to get pings.

You can also optionally set them up so a user requires a separate role to get the opt role. You can use this with [point roles](../point-roles/).

## Adding an opt role

To add an opt role, execute the following command:
```bash
setup add optrole [optRoleBaseName] [RoleToAssignName] [?RequiredRoleName]`
```

So for example, if you want an announcements role you would do:
```bash
setup add optrole Announcements AnnouncementsPing
```

And then your users can do the following command:
```bash
role Announcements
```

## Removing an opt role

To remove an opt role, execute the following command:
```bash
setup remove optrole [OptRoleName]
```

So for the example in adding an opt role, you would do:
```bash
setup remove optrole Announcements
```
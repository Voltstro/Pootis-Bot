# Permission System

Pootis Bot includes a permission system that allows you to have a specific role(s) allowed to use a specific commands, but anyone who doesn't have the role(s) allowed to use the command will not be allowed to execute the command.

By default, if a command doesn't have any roles assigned to it, **ANYONE** can use the command! Some commands such as kick or ban also require the user to have guild permissions. So the kick command will require the user to have the kick guild permission.

## Adding a permission

You can set a permission for a command using the command:

```
perm [Command] add [Role1], [Role2], ect...
```

The `[Command]` part can be any command that is not on [this list](#commands-you-cant-add).

Example
```
perm youtube add Admin, Staff
```

This would make the command [youtube](../../commands/discord-commands/#fun-commands) only accessible to users with the role Admin or Staff.

You can add any amount of roles as you want, but they have to be separated by ','.

## Removing a permission

Removing a permission is simple, execute this command to remove roles from a command.
```
perm [Command] remove [Role1], [Role2], ect... 
```

Once again, same deal with the roles in a list.

## Commands you can't add

You cannot add a permission to these commands for reasons:

- `profile`
- `profilemsg`
- `hello`
- `ping`
- `perm`
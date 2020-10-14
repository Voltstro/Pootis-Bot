# Point Roles

Lets say if a user hits a certain amount of points (not XP) on your server, then you might want to reward them with a special role.

So for example, if John Smith hits 500 points, you can set it up so he is given the 500+ points role.

## Setup

### Adding a point role

To add a point role, execute the command:
```
setup add pointrole [PointsAmount] [Role]
```

So for the John Smith example you would do:
```
setup add pointrole 500 "500+ Points"
```

### Removing a point role

To remove a point role, execute the command:
```
setup remove pointrole [PointsAmount]
```

So for example:
```
setup remove pointrole 500
```

## Get a list of all point roles

To get a list of all the point roles that have been created execute the command:
```
setup pointroles
```
# Points

Points are similar to XP, however they are completely separate, and are per server. You can control how many points are given out, as well as the cooldown between when points are given out.

You can reward you users for getting points with [point roles](../point-roles/).

## Changing how many points are given out

To set how many points are given out each time, execute the following command:
```bash
setup set points [amount]
```

So for example, to give 25 points each time you would do:
```
setup set points 25
```

## Changing the cooldown

To change the cooldown, do the following command:
```bash
setup set pointscooldown [time]
```

The time is in seconds.

So if you wanted a 10 seconds cooldown, you would do:
```bash
setup set pointscooldown 10
```
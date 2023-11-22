# Warnings

To inform the users of the AvaSci app about potential tracking issues, the AvaSci app provides several warning messages.

- **Battery saver** - the AvaSci app is running in battery saver mode. This means that the AvaSci app is not using the full power of the device to track the user. This may result in slower performance.
- **Distance** - the AvaSci app informs the user they are too close or too far away from the device.
- **Rotation** - the torso of the tracked user is rotated.
- **Joint visibility** - key joints of the tracked user are not visible.
- **Joint tracking confidence** - key joints of the tracked user have low tracking confidence.

## How to create new warnings

To create a new warning, follow these steps:

1. Create a new class that inherits from `Warning`.
1. Implement the new warning's `Check()` method.
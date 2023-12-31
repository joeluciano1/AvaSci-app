# AvaSci

AvaSci motion analysis app for iOS.

## System requirements

- Unity 2022.3.10f1 with iOS Build Support
- Xcode 15.0
- macOS Sonoma 14.0

## Project setup

1. Create a new Unity project.
1. Import the dependency packages in the following order:
    1. **LightBuzz Body Tracking**
    1. **Auto Canvas Scaler**
    1. **Safe Area Utility**
    1. **Native Share**
    1. **AvaSci**
1. Navigate to `AvaSci` &rarr; `Runtime` &rarr; `Samples` and open the `Main` scene.
1. Select `LightBuzz` &rarr; `Apply Build Settings` &rarr; `iOS`.
1. Add the `Main` scene to the build settings.
1. Select `Build` to create the XCode project.
1. Open the XCode project, sign it, and deploy it to your device.

## Dependencies

The AvaSci iOS app has the following package dependencies:

| Author | Package | Location |
| ------ | ------- | -------- |
| _LightBuzz_ | Body Tracking SDK | [com.lightbuzz.bodytracking-6.0.0.tgz](https://drive.google.com/file/d/1jDm01up1gQdnDfYnMMNEwJqN1cjwmyVK/view?usp=drive_link) |
| _LightBuzz_ | Auto Canvas Scaler | https://github.com/lightbuzz/unity-canvas-scaler.git |
| _Epic Brain Games_ | Safe Area Utility | [com.ebg.safeareautility.unitypackage](https://drive.google.com/file/d/1Y4dn5ZyQsUK8stOYNTiR1o6J_tS2xKfu/view?usp=drive_link) |
| _Yasirkula_ | Native Share | https://github.com/yasirkula/unitynativeshare.git |
| _LightBuzz_ | AvaSci | [com.lightbuzz.avasci.unitypackage](https://drive.google.com/file/d/1ixPOPRaVNu8LovwZNXLlHlmYe38uSOFV/view?usp=drive_link) |

#### Body Tracking SDK

Provides the body tracking functionality for the AvaSci app. must be installed manually.

- [Download the LightBuzz SDK package](https://drive.google.com/file/d/1jDm01up1gQdnDfYnMMNEwJqN1cjwmyVK/view?usp=drive_link).
- Move the package under `<PROJECT_ROOT>/Packages`.
- Launch Unity Package Manager.
- Click the `+` button and select `Add package from tarball...`.
- Select `com.lightbuzz.bodytracking-6.0.0.tgz`.
- Click `Open` to install.

#### Auto Canvas Scaler

Provides automatic scaling of the UI canvas to allow for proper UI element sizing on mobile devices. Install this package from its GitHub repository.

- Select `Window` &rarr; `Package Manager`.
- Click the `+` button and select `Add package from git URL...`.
- Enter `https://github.com/lightbuzz/unity-canvas-scaler.git`.
- Click `Add` to install.

#### Safe Area Utility

Provides safe area adjustment tools for Unity. Even though the package is available on the Unity Asset Store, we have made several changes and bug fixes to it, so you need to install it manually.

- [Download the Safe Area Utility package](https://drive.google.com/file/d/1Y4dn5ZyQsUK8stOYNTiR1o6J_tS2xKfu/view?usp=drive_link).
- Select `Assets` &rarr; `Import Package` &rarr; `Custom Package...`.
- Select `com.ebg.safeareautility.unitypackage`.
- Click `Open` to install.

_LightBuzz has purchased a separate license to transfer to the Safe Area Utility package to you._

#### Native Share

Provides the ability to share content from the app to other apps on the device. It's used to save and share the CSV file. Install this package from its GitHub repository.

- Select `Window` &rarr; `Package Manager`.
- Click the `+` button and select `Add package from git URL...`.
- Enter `https://github.com/yasirkula/unitynativeshare.git`.
- Click `Add` to install.

#### AvaSci

Provides the core functionality and source code of the AvaSci app.

- [Download the AvaSci package](https://drive.google.com/file/d/1ixPOPRaVNu8LovwZNXLlHlmYe38uSOFV/view?usp=drive_link).
- Select `Assets` &rarr; `Import Package` &rarr; `Custom Package...`.
- Select `com.lightbuzz.avasci.unitypackage`.
- Click `Open` to install.
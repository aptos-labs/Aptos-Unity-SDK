# Aptos Unity SDK
The Aptos Unity SDK is a .NET implementation of the [Aptos SDK](https://aptos.dev/sdks/index/), compatible with .NET Standard 2.0 and .NET 4.x for Unity. 
The goal of this SDK is to provide a set of tools for developers to build multi-platform applications (mobile, desktop, web, VR) using the Unity game engine and the Aptos blockchain infrastructure.

## Getting Started
To get started, you may check our [Quick Start Guide](#). A set of examples and templates is also provide for easy start.
An accompanying README file can be found in the open-source repository which provides further details on the integration.

### Installation
Two installation methods are provided: (1) installation through our `unitypackage`, and (2) installation through the Unity Package Manager

1. Install by `unitypackage`
    1. Download the latest `Aptos.Unity.unitypackge` file from [Release](https://www.google.com/) 
    2. Inside Unity, Click on `Assets` → `Import Packages` → `Custom Package.` and select the downloaded file.
2. Install by Unity Package Manager
    1. Open [Unity Package Manager](https://docs.unity3d.com/Manual/upm-ui.html) window.
    2. Click the add **+** button in the top status bar.
    3. Select `Add package from git URL` from the dropdown menu.
    4. Enter the `https://github.com/xxxxxxxxxx.git` and click Add

**NOTE:**  As of Unity 2021.x.x, Newtonsoft Json is common dependency. Prior versions of Unity require intalling Newtonsoft.



https://user-images.githubusercontent.com/25370590/216011102-e9c1fb39-fe6f-4f06-a603-ecf26a4e6986.mp4



## Technical Details

### Core Features
- HD Wallet Creation & Recovery
- Account Management
    - Account Recovery
    - Message Signing
    - Message Verification
    - Transaction Management
    - Single / Multi-signer Authentication
    - Authentication Key Rotation
- Native BCS Support
- Faucet Client for Devnet

### Unity Support
| Supported Version: | Tested |
| -- | -- |
| 2021.3.x | ✅ |
| 2022.2.x | ✅ |

| Windows | Mac  | iOS | Android | WebGL |
| -- | -- | -- | -- | -- |
| ✅ | ✅ | ✅ | ✅ | ✅ |

### Dependencies
- [Chaos.NaCl.Standard](https://www.nuget.org/packages/Chaos.NaCl.Standard/)
- Microsoft.Extensions.Logging.Abstractions.1.0.0 — required by NBitcoin.7.0.22
- Newtonsoft.Json
- NBitcoin.7.0.22
- [Portable.BouncyCastle](https://www.nuget.org/packages/Portable.BouncyCastle)
- Zxing

## Support

### Examples

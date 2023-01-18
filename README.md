# Aptos Unity SDK
The Aptos Unity SDK is a .NET implementation of the [Aptos SDK](https://aptos.dev/sdks/index/), compatible with .NET Standard 2.0 and .NET 4.x for Unity. 
The goal of this SDK is to provide a set of tools for developers to build multi-platform applications (mobile, desktop, web, VR) using the Unity game engine and the Aptos blockchain infrastructure.

## Getting Started
To get started, you may check our [Quick Start Guide](#). A set of examples and templates is also provide for easy start.
An accompanying README file can be found in the open-source repository which provides further details on the integration.

### Installation

**NOTE:**  As of Unity 2020.x.x, Newtonsoft Json is common dependency. Prior versions of Unity require intalling Newtonsoft.

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

### Dependencies
- [Chaos.NaCl.Standard](https://www.nuget.org/packages/Chaos.NaCl.Standard/)
- Microsoft.Extensions.Logging.Abstractions.1.0.0 — required by NBitcoin.7.0.22
- Newtonsoft.Json
- NBitcoin.7.0.22
- [Portable.BouncyCastle](https://www.nuget.org/packages/Portable.BouncyCastle)
- Zxing

## Support

### Examples

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

## Examples


https://user-images.githubusercontent.com/25370590/216013906-ee46a940-ee91-4b7f-8e84-febfa315480e.mp4


You can found Examples under `SDK-Examples/SDK Demo` and `SDK-Examples/UI Demo`. We will use the scene under `UI Demo` for the tutorial

### Wallet
![wallet_3](https://user-images.githubusercontent.com/25370590/216012383-c7c959f0-1fba-4b29-9452-dbfd9308cc61.jpg)
![wallet_1](https://user-images.githubusercontent.com/25370590/216012379-ff59c611-8843-41ca-9853-6ab739361a2c.jpg)
![wallet_2](https://user-images.githubusercontent.com/25370590/216012381-eaf8d10a-1063-421e-8748-c336ac212d65.jpg)
Once you open up the demo scene, you will see all tab are locked except `Add Account` , you would have the choice to create and import wallet. 

We currently store the mnemonics words in `PlayerPrefs`

```csharp
// Create Wallet

Mnemonic mnemo = new Mnemonic(Wordlist.English, WordCount.Twelve);
wallet = new Wallet(mnemo);

PlayerPrefs.SetString(mnemonicsKey, mnemo.ToString());
```

### Account
![Account_2](https://user-images.githubusercontent.com/25370590/216012625-3638c113-9b69-4fc0-950a-13daaaf4d27e.jpg)
![Account_1](https://user-images.githubusercontent.com/25370590/216012623-17116053-fb58-4de5-84ee-f57d7a282a00.jpg)

Once you create the wallet, you would be able to unlock rest of the panel, on `Account` Panel, you able to airdrop 1 APT to your account

```csharp
// Airdrop

Coroutine cor = StartCoroutine(FaucetClient.Instance.FundAccount((returnResult) =>
{
    Debug.Log("FAUCET RESPONSE: " + returnResult);
}, wallet.GetAccount(PlayerPrefs.GetInt(currentAddressIndexKey)).AccountAddress.ToString()
    , _amount
    , faucetEndpoint));

yield return cor;
```

And you able to create sub-wallet base from the same mnemonics words

```csharp
// Create sub-wallets

for (int i = 0; i < accountNumLimit; i++)
{
    var account = wallet.GetAccount(i);
    var addr = account.AccountAddress.ToString();

    addressList.Add(addr);
} 
```

### NFT Minter
![NFTMinter_1](https://user-images.githubusercontent.com/25370590/216013385-f896bccf-4bb4-4c13-9b8d-18cc1aac9ee9.jpg)
![NFTMinter_2](https://user-images.githubusercontent.com/25370590/216013383-134d56d8-101b-4fbe-8410-542e154405fc.jpg)

On the `Mint NFT` tab, You can create NFT on your own. In order to do that, you would need to `Creat Collection` First, Then `Create NFT`

```csharp
// Create Collection

string createCollectionResult = "";
Coroutine createCollectionCor = StartCoroutine(RestClient.Instance.CreateCollection((returnResult) =>
{
    createCollectionResult = returnResult;
}, wallet.GetAccount(PlayerPrefs.GetInt(currentAddressIndexKey)),
_collectionName, _collectionDescription, _collectionUri));
yield return createCollectionCor;

Debug.Log("Create Collection Response: " + createCollectionResult);
Aptos.Unity.Rest.Model.Transaction createCollectionTxn = JsonConvert.DeserializeObject<Aptos.Unity.Rest.Model.Transaction>(createCollectionResult, new TransactionConverter());
string transactionHash = createCollectionTxn.Hash;
Debug.Log("Create Collection Hash: " + createCollectionTxn.Hash);
```

```csharp
// Create NFT

string createTokenResult = "";
Coroutine createTokenCor = StartCoroutine(
    RestClient.Instance.CreateToken((returnResult) =>
    {
        createTokenResult = returnResult;
    }, wallet.GetAccount(PlayerPrefs.GetInt(currentAddressIndexKey)),
    _collectionName,
    _tokenName,
    _tokenDescription,
    _supply,
    _max,
    _uri,
    _royaltyPointsPerMillion)
);
yield return createTokenCor;

Debug.Log("Create Token Response: " + createTokenResult);
Aptos.Unity.Rest.Model.Transaction createTokenTxn = JsonConvert.DeserializeObject<Aptos.Unity.Rest.Model.Transaction>(createTokenResult, new TransactionConverter());
string createTokenTxnHash = createTokenTxn.Hash;
Debug.Log("Create Token Hash: " + createTokenTxn.Hash);
```

### Transaction Executer
![Transaction Executer_1](https://user-images.githubusercontent.com/25370590/216013743-48dc3c1c-c180-4775-bf2a-51a6b9de1952.jpg)
On `Send Transaction` panel, You can send token by paste the target address and token amount

```csharp
string transferResult = "";
Coroutine cor = StartCoroutine(RestClient.Instance.Transfer((_transferResult) =>
{
    transferResult = _transferResult;
}, wallet.GetAccount(PlayerPrefs.GetInt(currentAddressIndexKey)), targetAddress, amount));

yield return cor;
```

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

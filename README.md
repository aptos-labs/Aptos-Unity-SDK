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

## How To


https://user-images.githubusercontent.com/25370590/216013906-ee46a940-ee91-4b7f-8e84-febfa315480e.mp4


## Wallet Example Walthrough

You can find a set of examples under `SDK-Examples/SDK Demo` and `SDK-Examples/UI Demo` directory. We will use the scene under `UI Demo` for this walkthrough.

<img src="https://user-images.githubusercontent.com/25370590/216012383-c7c959f0-1fba-4b29-9452-dbfd9308cc61.jpg" alt="wallet_seedphrase1" width="400"/>

<img src="https://user-images.githubusercontent.com/25370590/216012379-ff59c611-8843-41ca-9853-6ab739361a2c.jpg" alt="wallet_seedphrase2" width="400"/>

<img src="https://user-images.githubusercontent.com/25370590/216012381-eaf8d10a-1063-421e-8748-c336ac212d65.jpg" alt="wallet_seedphrase3" width="400"/>

Once you open  the demo scene, you will see all tab are locked except `Add Account` , you have the choice to create or import a wallet. 

We currently store the mnemonics words in `PlayerPrefs`

```csharp
// Create Wallet

Mnemonic mnemo = new Mnemonic(Wordlist.English, WordCount.Twelve);
wallet = new Wallet(mnemo);

PlayerPrefs.SetString(mnemonicsKey, mnemo.ToString());
```

### Account

<img src="https://user-images.githubusercontent.com/25370590/216012625-3638c113-9b69-4fc0-950a-13daaaf4d27e.jpg" alt="wallet_account0" width="400"/>

<img src="https://user-images.githubusercontent.com/25370590/216012623-17116053-fb58-4de5-84ee-f57d7a282a00.jpg" alt="wallet_account1" width="400"/>

Once you create the wallet, you will be able to unlock rest of the panel, on `Account` Panel. 

#### Airdrop
You will also be able to airdrop 1 APT to your account

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

#### Deriviving Accounts from HD Wallet
You can derive accounts from the HD Wallet by selecting an account index as follows:

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
<img src="https://user-images.githubusercontent.com/25370590/216013385-f896bccf-4bb4-4c13-9b8d-18cc1aac9ee9.jpg" alt="wallet_nft_minter0" width="400"/>

<img src="https://user-images.githubusercontent.com/25370590/216013383-134d56d8-101b-4fbe-8410-542e154405fc.jpg" alt="wallet_nft_minter1" width="400"/>

On the `Mint NFT` tab, You can mint a NFT of your own. In order to do that, you need to `Creat Collection` first, then `Create NFT`.

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
<img src="https://user-images.githubusercontent.com/25370590/216013743-48dc3c1c-c180-4775-bf2a-51a6b9de1952.jpg" alt="wallet_transaction_execution" width="400"/>

On the `Send Transaction` panel, you can send tokens by pasting the recipient address and token amount.

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

For additional support, please join our community [Discord Server](https://discord.gg/aptoslabs), and ask questions in the `#dev-discussion` channel.
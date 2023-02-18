# Aptos Unity SDK
The Aptos Unity SDK is a .NET implementation of the [Aptos SDK](https://aptos.dev/sdks/index/), compatible with .NET Standard 2.0 and .NET 4.x for Unity. 
The goal of this SDK is to provide a set of tools for developers to build multi-platform applications (mobile, desktop, web, VR) using the Unity game engine and the Aptos blockchain infrastructure.

## Getting Started
To get started, you may check our [Quick Start Guide](#quick-start-video). A set of examples is also provided in the `Assets/Aptos-Unity-SDK/SDK-Examples` directory. A local version of Doxygen-generated documentation for the classes can be found in `Assets/Aptos-Unity-SDK/Documentation/html/index.html`. A hosted version of the latter documentation can found in [here](https://aptos-unity-sdk-docs.netlify.app/).    

This accompanying README file provides further details on the SDK and integration. 

### Installation

1. Download the latest `aptos-unity-sdk-xx.unitypackge` file from [Release](https://github.com/aptos-labs/Aptos-Unity-SDK/releases) 
2. Inside Unity, Click on `Assets` → `Import Packages` → `Custom Package.` and select the downloaded file.

**NOTE:**  As of Unity 2021.x.x, Newtonsoft Json is a common dependency. Prior versions of Unity require installing Newtonsoft.

## Quick Start Video


https://user-images.githubusercontent.com/25370590/216013906-ee46a940-ee91-4b7f-8e84-febfa315480e.mp4


## Wallet Example Walthrough

You can find a set of examples under `SDK-Examples/SDK Demo` and `SDK-Examples/UI Demo` directory. We will use the scene under `UI Demo` for this walkthrough.

<img src="https://user-images.githubusercontent.com/25370590/216012383-c7c959f0-1fba-4b29-9452-dbfd9308cc61.jpg" alt="wallet_seedphrase1" width="400"/>   
<br />
<img src="https://user-images.githubusercontent.com/25370590/216012379-ff59c611-8843-41ca-9853-6ab739361a2c.jpg" alt="wallet_seedphrase2" width="400"/>   
<br />
<img src="https://user-images.githubusercontent.com/25370590/216012381-eaf8d10a-1063-421e-8748-c336ac212d65.jpg" alt="wallet_seedphrase3" width="400"/>   
<br />

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
<br />
<img src="https://user-images.githubusercontent.com/25370590/216012623-17116053-fb58-4de5-84ee-f57d7a282a00.jpg" alt="wallet_account1" width="400"/>
<br />

Once you create the wallet, you will be able to unlock rest of the panel, on `Account` Panel.    

In code you can derive an account from the wallet object or generate a new one as follows:

```csharp
// Create new account

Account alice = new Account();
AccountAddress aliceAddress = alice.AccountAddress;
```

#### Airdrop
You will also be able to airdrop 1 APT to your account

```csharp
// Airdrop

bool success = false;
ResponseInfo responseInfo = new ResponseInfo();

Coroutine fundAliceAccountCor = StartCoroutine(
    FaucetClient.Instance.FundAccount((_success, _responseInfo) =>
    {
        success = _success;
        responseInfo = _responseInfo;
    }, aliceAddress.ToString(), 100000000, faucetEndpoint));

yield return fundAliceAccountCor;

// Check if funding the account was succesful
if(responseInfo.status != ResponseInfo.Status.Success)
{
    Debug.LogError("Faucet funding for Alice failed: " + responseInfo.message);
    yield break;
}
```

#### Deriviving Accounts from HD Wallet
You can derive accounts from the HD Wallet by selecting an account index as follows:

```csharp
// Create sub-wallets

Mnemonic mnemo = new Mnemonic(Wordlist.English, WordCount.Twelve);
wallet = new Wallet(mnemo);

for (int i = 0; i < accountNumLimit; i++)
{
    var account = wallet.GetAccount(i);
    var addr = account.AccountAddress.ToString();

    addressList.Add(addr);
} 
```

### NFT Minter
<img src="https://user-images.githubusercontent.com/25370590/216013385-f896bccf-4bb4-4c13-9b8d-18cc1aac9ee9.jpg" alt="wallet_nft_minter0" width="400"/>
<br />
<img src="https://user-images.githubusercontent.com/25370590/216013383-134d56d8-101b-4fbe-8410-542e154405fc.jpg" alt="wallet_nft_minter1" width="400"/>
<br />

On the `Mint NFT` tab, You can mint a NFT of your own. In order to do that, you need to `Creat Collection` first, then `Create NFT`. Note that you must confirm that the creation of the collection was sucessful before creating the token, you can use the `WaitForTransaction` corouting for this.

```csharp
// Create Collection

string collectionName = "Alice's";
string collectionDescription = "Alice's simple collection";
string collectionUri = "https://aptos.dev";

Transaction createCollectionTxn = new Transaction();

Coroutine createCollectionCor = StartCoroutine(
    RestClient.Instance.CreateCollection((_createCollectionTxn, _responseInfo) =>
    {
        createCollectionTxn = _createCollectionTxn;
        responseInfo = _responseInfo;
    }, alice, collectionName, collectionDescription, collectionUri));
yield return createCollectionCor;

// Check if collection creation was successful 
if(responseInfo.status != ResponseInfo.Status.Success)
{
    Debug.LogError("Cannot create collection. " + responseInfo.message);
}

// Check response and transaction hash
Debug.Log("Create Collection Response: " + responseInfo.message);
string transactionHash = createCollectionTxn.Hash;
Debug.Log("Create Collection Hash: " + createCollectionTxn.Hash);
```

```csharp 
// Wait for Transaction
bool waitForTxnSuccess = false;
Coroutine waitForTransactionCor = StartCoroutine(
    RestClient.Instance.WaitForTransaction((_pending, _responseInfo) =>
    {
        waitForTxnSuccess = _pending;
        responseInfo = _responseInfo;
    }, transactionHash)
);
yield return waitForTransactionCor;

if(!waitForTxnSuccess)
{
    Debug.LogWarning("Transaction was not found.");
}

```


```csharp
// Create NFT

string tokenName = "Alice's first token";
string tokenDescription = "Alice's simple token";
string tokenUri = "https://aptos.dev/img/nyan.jpeg";

Transaction createTokenTxn = new Transaction();
Coroutine createTokenCor = StartCoroutine(
    RestClient.Instance.CreateToken((_createTokenTxn, _responseInfo) =>
    {
        createTokenTxn = _createTokenTxn;
        responseInfo = _responseInfo;
    }, alice, collectionName, tokenName, tokenDescription, 1, 1, tokenUri, 0)
);
yield return createTokenCor;

if(responseInfo.status != ResponseInfo.Status.Success)
{
    Debug.LogError("Error creating token. " + responseInfo.message);
}

Debug.Log("Create Token Response: " + responseInfo.message);
string createTokenTxnHash = createTokenTxn.Hash;
Debug.Log("Create Token Hash: " + createTokenTxn.Hash);
```

### Transaction Executer
<img src="https://user-images.githubusercontent.com/25370590/216013743-48dc3c1c-c180-4775-bf2a-51a6b9de1952.jpg" alt="wallet_transaction_execution" width="400"/>
<br />

On the `Send Transaction` panel, you can send tokens by pasting the recipient address and token amount.

```csharp

Transaction transferTxn = new Transaction();
Coroutine transferCor = StartCoroutine(
    RestClient.Instance.Transfer((_transaction, _responseInfo) => {
        transferTxn = _transaction;
        responseInfo = _responseInfo;
    }, alice, bob.AccountAddress.ToString(), 1000));

yield return transferCor;

if(responseInfo.status != ResponseInfo.Status.Success)
{
    Debug.LogWarning("Transfer failed: " + responseInfo.message);
    yield break;
}

Debug.Log("Transfer Response: " + responseInfo.message);
string transactionHash = transferTxn.Hash;
Debug.Log("Transfer Response Hash: " + transferTxn.Hash);

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
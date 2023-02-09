using Aptos.Accounts;
using Aptos.Rest;
using Aptos.Unity.Rest;
using Aptos.Unity.Rest.Model;
using Newtonsoft.Json;
using System.Collections;
using UnityEngine;

namespace Aptos.Unity.Sample
{
    public class SimpleNftExample : MonoBehaviour
    {
        void Start()
        {
            StartCoroutine(RunSimpleNftExample());
        }

        IEnumerator RunSimpleNftExample()
        {
            #region Generate Accounts
            Account alice = new Account();
            Account bob = new Account();

            Debug.Log("<color=cyan>=== Addresses ===</color>");
            Debug.Log("Alice: " + alice.AccountAddress.ToString());
            Debug.Log("Bob: " + bob.AccountAddress.ToString());
            #endregion

            #region REST & Faucet Client Setup
            RestClient.Instance.SetEndPoint(Constants.DEVNET_BASE_URL);
            string faucetEndpoint = "https://faucet.devnet.aptoslabs.com";
            #endregion

            #region Fund Alice Account Through Devnet Faucet
            AccountAddress aliceAddress = alice.AccountAddress;
            Coroutine fundAliceAccountCor = StartCoroutine(FaucetClient.Instance.FundAccount((success, returnResult) =>
            {
                Debug.Log("Faucet Response: " + returnResult);
            }, aliceAddress.ToString(), 100000000, faucetEndpoint));
            yield return fundAliceAccountCor;
            #endregion

            #region Fund Bob Account Through Devnet Faucet
            AccountAddress bobAddress = bob.AccountAddress;
            Coroutine fundBobAccountCor = StartCoroutine(FaucetClient.Instance.FundAccount((success, returnResult) =>
            {
                Debug.Log("Faucet Response: " + returnResult);
            }, bobAddress.ToString(), 100000000, faucetEndpoint));
            yield return fundBobAccountCor;
            #endregion

            #region Initial Coin Balances
            Debug.Log("<color=cyan>=== Initial Coin Balances ===</color>");
            Coroutine getAliceBalanceCor1 = StartCoroutine(RestClient.Instance.GetAccountBalance((success, returnResult) =>
            {
                if (returnResult == null)
                {
                    Debug.LogWarning("Account address not found, balance is 0");
                    Debug.Log("Alice Balance: " + 0);
                }
                else
                {
                    AccountResourceCoin acctResourceCoin = JsonConvert.DeserializeObject<AccountResourceCoin>(returnResult);
                    Debug.Log("Alice Balance: " + acctResourceCoin.DataProp.Coin.Value);
                }

            }, aliceAddress));
            yield return getAliceBalanceCor1;

            Coroutine getBobAccountBalance = StartCoroutine(RestClient.Instance.GetAccountBalance((succes, returnResult) =>
            {
                if (returnResult == null)
                {
                    Debug.LogWarning("Account address not found, balance is 0");
                    Debug.Log("Bob Balance: " + 0);
                }
                else
                {
                    AccountResourceCoin acctResourceCoin = JsonConvert.DeserializeObject<AccountResourceCoin>(returnResult);
                    Debug.Log("Bob Balance: " + acctResourceCoin.DataProp.Coin.Value);
                }

            }, bobAddress));
            yield return getBobAccountBalance;
            #endregion

            #region Collection & Token Naming Details
            string collectionName = "Alice's";
            string collectionDescription = "Alice's simple collection";
            string collectionUri = "https://aptos.dev";

            string tokenName = "Alice's first token";
            string tokenDescription = "Alice's simple token";
            string tokenUri = "https://aptos.dev/img/nyan.jpeg";
            int propertyVersion = 0;
            #endregion

            #region Create Collection
            Debug.Log("<color=cyan>=== Creating Collection and Token ===</color>");
            Transaction createCollectionTxn = new Transaction();
            ResponseInfo responseInfo = new ResponseInfo();
            Coroutine createCollectionCor = StartCoroutine(RestClient.Instance.CreateCollection((_createCollectionTxn, _responseInfo) =>
            {
                createCollectionTxn = _createCollectionTxn;
                responseInfo = _responseInfo;
            }, alice, collectionName, collectionDescription, collectionUri));
            yield return createCollectionCor;

            if(responseInfo.status != ResponseInfo.Status.Success)
            {
                Debug.LogError("Cannot create collection. " + responseInfo.message);
            }

            Debug.Log("Create Collection Response: " + responseInfo.message);
            string transactionHash = createCollectionTxn.Hash;
            Debug.Log("Create Collection Hash: " + createCollectionTxn.Hash);
            #endregion

            #region Wait For Transaction
            Coroutine waitForTransactionCor = StartCoroutine(
                RestClient.Instance.WaitForTransaction((pending, transactionWaitResult) =>
                {
                    Debug.Log(transactionWaitResult);
                }, transactionHash)
            );
            yield return waitForTransactionCor;

            #endregion

            #region Create Non-Fungible Token
            string createTokenResult = "";
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

            Debug.Log("Create Token Response: " + createTokenResult);
            string createTokenTxnHash = createTokenTxn.Hash;
            Debug.Log("Create Token Hash: " + createTokenTxn.Hash);
            #endregion

            #region Wait For Transaction
            waitForTransactionCor = StartCoroutine(
                RestClient.Instance.WaitForTransaction((pending, transactionWaitResult) =>
                {
                    Debug.Log(transactionWaitResult);
                }, createTokenTxnHash)
            );
            yield return waitForTransactionCor;
            #endregion

            #region Get Collection
            string getCollectionResult = "";
            Coroutine getCollectionCor = StartCoroutine(
                RestClient.Instance.GetCollection((returnResult) =>
                {
                    getCollectionResult = returnResult;
                }, aliceAddress, collectionName)
            );
            yield return getCollectionCor;
            Debug.Log("Alice's Collection: " + getCollectionResult);
            #endregion

            #region Get Token Balance 
            Debug.Log("<color=cyan>=== Get Token Balance for Alice NFT ===</color>");
            string getTokenBalanceResultAlice = "";
            Coroutine getTokenBalanceCor = StartCoroutine(
                RestClient.Instance.GetTokenBalance((returnResult) =>
                {
                    getTokenBalanceResultAlice = returnResult;
                }, aliceAddress, aliceAddress, collectionName, tokenName, propertyVersion)
            );
            yield return getTokenBalanceCor;
            Debug.Log("Alice's NFT Token Balance: " + getTokenBalanceResultAlice);

            string getTokenDataResultAlice = "";
            Coroutine getTokenDataCor = StartCoroutine(
                RestClient.Instance.GetTokenData((returnResult) =>
                {
                    getTokenDataResultAlice = returnResult;
                }, aliceAddress, collectionName, tokenName, propertyVersion)
            );
            yield return getTokenDataCor;
            Debug.Log("Alice's Token Data: " + getTokenDataResultAlice);
            #endregion

            #region Transferring the Token to Bob
            Debug.Log("<color=cyan>=== Get Token Balance for Alice NFT ===</color>");
            string offerTokenResult = "";
            Transaction offerTokenTxn = new Transaction();
            Coroutine offerTokenCor = StartCoroutine(RestClient.Instance.OfferToken((_offerTokenTxn, _responseInfo) =>
            {
                offerTokenTxn = _offerTokenTxn;
                responseInfo = _responseInfo;
            }, alice, bob.AccountAddress, alice.AccountAddress, collectionName, tokenName, "1"));

            yield return offerTokenCor;

            if(responseInfo.status != ResponseInfo.Status.Success)
            {
                Debug.LogError("Error offering token. " + responseInfo.message);
                yield break;
            }

            Debug.Log("Offer Token Response: " + offerTokenResult);
            string offerTokenTxnHash = offerTokenTxn.Hash;
            Debug.Log("Offer Token Hash: " + offerTokenTxnHash);

            Coroutine waitForTransaction = StartCoroutine(WaitForTransaction(offerTokenTxnHash));
            yield return waitForTransaction;
            #endregion

            #region Bob Claims Token
            Debug.Log("<color=cyan>=== Bob Claims Token ===</color>");
            Transaction claimTokenTxn = new Transaction();
            Coroutine claimTokenCor = StartCoroutine(RestClient.Instance.ClaimToken((_claimTokenTxn, _responseInfo) =>
            {
                claimTokenTxn = _claimTokenTxn;
                responseInfo = _responseInfo;
            }, bob, alice.AccountAddress, alice.AccountAddress, collectionName, tokenName, propertyVersion));

            yield return claimTokenCor;

            Debug.Log("Claim Token Response: " + responseInfo.message);
            string claimTokenTxnHash = claimTokenTxn.Hash;
            Debug.Log("Claim Token Hash: " + claimTokenTxnHash);

            waitForTransaction = StartCoroutine(WaitForTransaction(claimTokenTxnHash));
            yield return waitForTransaction;
            #endregion

            #region Get Token Balance for NFT Alice
            Debug.Log("<color=cyan>=== Get Token Balance for Alice NFT ===</color>");
            getTokenBalanceResultAlice = "";
            getTokenBalanceCor = StartCoroutine(
                RestClient.Instance.GetTokenBalance((returnResult) =>
                {
                    getTokenBalanceResultAlice = returnResult;
                }, aliceAddress, aliceAddress, collectionName, tokenName, propertyVersion)
            );
            yield return getTokenBalanceCor;
            Debug.Log("Alice's NFT Token Balance: " + getTokenBalanceResultAlice);
            #endregion

            #region Get Token Balance for NFT Bob
            Debug.Log("<color=cyan>=== Get Token Balance for Bob NFT ===</color>");
            string getTokenBalanceResultBob = "";
            getTokenBalanceCor = StartCoroutine(
                RestClient.Instance.GetTokenBalance((returnResult) =>
                {
                    getTokenBalanceResultBob = returnResult;
                }, bobAddress, aliceAddress, collectionName, tokenName, propertyVersion)
            );
            yield return getTokenBalanceCor;
            Debug.Log("Bob's NFT Token Balance: " + getTokenBalanceResultBob);
            #endregion

            yield return null;
        }

        IEnumerator WaitForTransaction(string txnHash)
        {
            Coroutine waitForTransactionCor = StartCoroutine(
                RestClient.Instance.WaitForTransaction((pending, transactionWaitResult) =>
                {
                    Debug.Log(transactionWaitResult);
                }, txnHash)
            );
            yield return waitForTransactionCor;
        }

    }
}
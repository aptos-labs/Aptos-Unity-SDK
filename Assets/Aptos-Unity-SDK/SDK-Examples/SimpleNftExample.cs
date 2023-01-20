using Aptos.Accounts;
using Aptos.Rest;
using Aptos.Unity.Rest;
using Aptos.Unity.Rest.Model;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        Debug.Log("=== Addresses ===");
        Debug.Log("Alice: " + alice.AccountAddress.ToString());
        Debug.Log("Bob: " + bob.AccountAddress.ToString());
        #endregion

        #region REST & Faucet Client Setup
        RestClient.Instance.SetEndPoint(Constants.DEVNET_BASE_URL);
        string faucetEndpoint = "https://faucet.devnet.aptoslabs.com";
        #endregion

        #region Fund Alice Account Through Devnet Faucet
        AccountAddress aliceAddress = alice.AccountAddress;
        Coroutine fundAliceAccountCor = StartCoroutine(FaucetClient.Instance.FundAccount((returnResult) =>
        {
            Debug.Log("Faucet Response: " + returnResult);
        }, aliceAddress.ToString(), 100000000, faucetEndpoint));
        yield return fundAliceAccountCor;
        #endregion

        #region Fund Bob Account Through Devnet Faucet
        AccountAddress bobAddress = bob.AccountAddress;
        Coroutine fundBobAccountCor = StartCoroutine(FaucetClient.Instance.FundAccount((returnResult) =>
        {
            Debug.Log("Faucet Response: " + returnResult);
        }, bobAddress.ToString(), 100000000, faucetEndpoint));
        yield return fundBobAccountCor;
        #endregion

        #region Initial Coin Balances
        Debug.Log("=== Initial Coin Balances ===");
        Coroutine getAliceBalanceCor1 = StartCoroutine(RestClient.Instance.GetAccountBalance((returnResult) =>
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

        Coroutine getBobAccountBalance = StartCoroutine(RestClient.Instance.GetAccountBalance((returnResult) =>
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
        string propertyVersion = "0";
        #endregion

        #region Create Collection
        Debug.Log("=== Creating Collection and Token ===");
        string createCollectionResult = "";
        Coroutine createCollectionCor = StartCoroutine(RestClient.Instance.CreateCollection((returnResult) =>
        {
            createCollectionResult = returnResult;
        }, alice, collectionName, collectionDescription, collectionUri));
        yield return createCollectionCor;

        Debug.Log("Create Collection Response: " + createCollectionResult);
        Transaction createCollectionTxn = JsonConvert.DeserializeObject<Transaction>(createCollectionResult, new TransactionConverter());
        string transactionHash = createCollectionTxn.Hash;
        Debug.Log("Create Collection Hash: " + createCollectionTxn.Hash);

        #endregion

        #region Wait For Transaction
        Coroutine waitForTransactionCor = StartCoroutine(
            RestClient.Instance.WaitForTransaction((pending, transactionWaitResult) => {
                Debug.Log(transactionWaitResult);
            }, transactionHash)
        );
        yield return waitForTransactionCor;

        #endregion

        #region Create Non-Fungible Token
        string createTokenResult = "";
        Coroutine  createTokenCor = StartCoroutine(
            RestClient.Instance.CreateToken((returnResult) => {
                createTokenResult = returnResult;
            }, alice, collectionName, tokenName, tokenDescription, 1, 1, tokenUri, 0)
        );
        yield return createTokenCor;

        Debug.Log("Create Token Response: " + createTokenResult);
        Transaction createTokenTxn = JsonConvert.DeserializeObject<Transaction>(createTokenResult, new TransactionConverter());
        string createTokenTxnHash = createTokenTxn.Hash;
        Debug.Log("Create Token Hash: " + createTokenTxn.Hash);
        #endregion

        #region Wait For Transaction
        waitForTransactionCor = StartCoroutine(
            RestClient.Instance.WaitForTransaction((pending, transactionWaitResult) => {
                Debug.Log(transactionWaitResult);
            }, createTokenTxnHash)
        );
        yield return waitForTransactionCor;

        #endregion

        #region Get Collection
        string getCollectionResult = "";
        Coroutine getCollectionCor = StartCoroutine(
            RestClient.Instance.GetCollection((returnResult) => {
                getCollectionResult = returnResult;
            }, aliceAddress, collectionName)
        );
        yield return getCollectionCor;
        Debug.Log("Alice's Collection: " + getCollectionResult);
        #endregion

        #region Get Token Balance
        string getTokenBalanceResult = "";
        Coroutine getTokenBalanceCor = StartCoroutine(
            RestClient.Instance.GetTokenBalance((returnResult) => {
                getTokenBalanceResult = returnResult;
            }, aliceAddress, aliceAddress, collectionName, tokenName, propertyVersion)
        );
        yield return getTokenBalanceCor;
        Debug.Log("Alice's Token Balance: " + getTokenBalanceResult);
        #endregion

        yield return null;
    }
}

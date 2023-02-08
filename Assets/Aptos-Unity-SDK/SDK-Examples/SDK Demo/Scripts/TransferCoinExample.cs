using Aptos.Accounts;
using Aptos.HdWallet;
using Aptos.Rest;
using Aptos.Unity.Rest;
using Aptos.Unity.Rest.Model;
using NBitcoin;
using Newtonsoft.Json;
using System.Collections;
using UnityEngine;
using Transaction = Aptos.Unity.Rest.Model.Transaction;

namespace Aptos.Unity.Sample
{
    public class TransferCoinExample : MonoBehaviour
    {
        string mnemo = "stadium valid laundry unknown tuition train december camera fiber vault sniff ripple";
        Mnemonic mnemo1 = new Mnemonic(Wordlist.English, WordCount.Twelve);

        void Start()
        {
            StartCoroutine(RunTransferCoinExample());
        }

        IEnumerator RunTransferCoinExample()
        {
            Wallet wallet = new Wallet(mnemo);

            #region Alice Account
            Account alice = wallet.GetAccount(0);
            string authKey = alice.AuthKey();
            Debug.Log("Alice Auth Key: " + authKey);

            AccountAddress aliceAddress = alice.AccountAddress;

            PrivateKey privateKey = alice.PrivateKey;
            Debug.Log("Aice Private Key: " + privateKey);
            #endregion

            #region Bob Account
            Account bob = wallet.GetAccount(1);
            AccountAddress bobAddress = bob.AccountAddress;

            Debug.Log("Wallet: Account 0: Alice: " + aliceAddress.ToString());
            Debug.Log("Wallet: Account 1: Bob: " + bobAddress.ToString());
            #endregion

            #region REST Client Setup
            RestClient.Instance.SetEndPoint(Constants.DEVNET_BASE_URL);

            bool successLedgerInfo;
            string result = "";
            Coroutine ledgerInfoCor = StartCoroutine(RestClient.Instance.GetInfo((success, returnResult) =>
            {
                successLedgerInfo = success;
                result = returnResult;
            }));
            yield return ledgerInfoCor;
            LedgerInfo ledgerInfo = JsonConvert.DeserializeObject<LedgerInfo>(result);
            Debug.Log("CHAIN ID: " + ledgerInfo.ChainId);
            #endregion

            #region Get Alice Account Balance
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
            #endregion

            string faucetEndpoint = "https://faucet.devnet.aptoslabs.com";

            #region Fund Alice Account Through Devnet Faucet
            Coroutine fundAliceAccountCor = StartCoroutine(FaucetClient.Instance.FundAccount((success, returnResult) =>
            {
                Debug.Log("Faucet Response: " + returnResult);
            }, aliceAddress.ToString(), 100000000, faucetEndpoint));
            yield return fundAliceAccountCor;
            #endregion

            #region Get Alice Account Balance After Funding
            Coroutine getAliceAccountBalance2 = StartCoroutine(RestClient.Instance.GetAccountBalance((success,returnResult) =>
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
            yield return getAliceAccountBalance2;
            #endregion

            #region Fund Bob Account Through Devnet Faucet
            Coroutine fundBobAccountCor = StartCoroutine(FaucetClient.Instance.FundAccount((success, returnResult) =>
            {
                Debug.Log("Faucet Response: " + returnResult);
            }, bobAddress.ToString(), 100000000, faucetEndpoint));
            yield return fundBobAccountCor;
            #endregion

            #region Get Bob Account Balance After Funding
            Coroutine getBobAccountBalance = StartCoroutine(RestClient.Instance.GetAccountBalance((success, returnResult) =>
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

            #region Have Alice give Bob 1_000 coins - Submit Transfer Transaction
            string transferResult = "";
            Coroutine transferCor = StartCoroutine(RestClient.Instance.Transfer((_transferResult) =>
            {
                transferResult = _transferResult;
            }, alice, bob.AccountAddress.ToHexString(), 1000));

            yield return transferCor;

            Debug.Log("Transfer Response: " + transferResult);
            Transaction transaction = JsonConvert.DeserializeObject<Transaction>(transferResult, new TransactionConverter());
            string transactionHash = transaction.Hash;
            Debug.Log("Transfer Response Hash: " + transaction.Hash);
            #endregion

            #region Wait For Transaction
            bool waitForTxnSuccess = false;
            string txnResult = "";
            Coroutine waitForTransactionCor = StartCoroutine(
                RestClient.Instance.WaitForTransaction((pending, transactionWaitResult) =>
                {
                    waitForTxnSuccess = pending;
                    txnResult = transactionWaitResult;
                    Debug.Log(transactionWaitResult);
                }, transactionHash)
            );

            yield return waitForTransactionCor;

            if(!waitForTxnSuccess)
            {
                Debug.LogWarning("Transaction was not found. Breaking out of example", gameObject);
                yield break;
            }

            #endregion

            #region Get Alice Account Balance After Transfer
            Coroutine getAliceAccountBalance3 = StartCoroutine(RestClient.Instance.GetAccountBalance((success, returnResult) =>
            {
                if (returnResult == null)
                {
                    Debug.LogWarning("Account address not found, balance is 0");
                    Debug.Log("Alice Balance: " + 0);
                }
                else
                {
                    AccountResourceCoin acctResourceCoin = JsonConvert.DeserializeObject<AccountResourceCoin>(returnResult);
                    Debug.Log("Alice Balance After Funding: " + acctResourceCoin.DataProp.Coin.Value);
                }

            }, aliceAddress));
            yield return getAliceAccountBalance3;
            #endregion

            #region Get Bob Account Balance After Transfer
            Coroutine getBobAccountBalance2 = StartCoroutine(RestClient.Instance.GetAccountBalance((success, returnResult) =>
            {
                if (returnResult == null)
                {
                    Debug.LogWarning("Account address not found, balance is 0");
                    Debug.Log("Bob Balance: " + 0);
                }
                else
                {
                    AccountResourceCoin acctResourceCoin = JsonConvert.DeserializeObject<AccountResourceCoin>(returnResult);
                    Debug.Log("Bob Balance After Funding: " + acctResourceCoin.DataProp.Coin.Value);
                }

            }, bobAddress));
            yield return getBobAccountBalance2;
            #endregion
        }
    }
}
using Aptos.Accounts;
using Aptos.HdWallet;
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

            LedgerInfo ledgerInfo = new LedgerInfo();
            ResponseInfo responseInfo = new ResponseInfo();
            Coroutine ledgerInfoCor = StartCoroutine(RestClient.Instance.GetInfo((_ledgerInfo, _responseInfo) =>
            {
                ledgerInfo = _ledgerInfo;
                responseInfo = _responseInfo;
            }));
            yield return ledgerInfoCor;

            if(responseInfo.status != ResponseInfo.Status.Success)
            {
                Debug.LogError(responseInfo.message);
                yield break;
            }

            Debug.Log("Chain ID: " + ledgerInfo.ChainId);
            #endregion

            #region Get Alice Account Balance
            AccountResourceCoin.Coin coin = new AccountResourceCoin.Coin();
            responseInfo = new ResponseInfo();
            Coroutine getAliceBalanceCor1 = StartCoroutine(RestClient.Instance.GetAccountBalance((_coin, _responseInfo) =>
            {
                coin = _coin;
                responseInfo = _responseInfo;

            }, aliceAddress));
            yield return getAliceBalanceCor1;

            if (responseInfo.status == ResponseInfo.Status.Failed)
            {
                Debug.LogError(responseInfo.message);
                yield break;
            }

            Debug.Log("Alice's Balance Before Funding: " + coin.Value);


            #endregion

            string faucetEndpoint = "https://faucet.devnet.aptoslabs.com";

            #region Fund Alice Account Through Devnet Faucet
            bool success = false;
            responseInfo = new ResponseInfo();
            Coroutine fundAliceAccountCor = StartCoroutine(FaucetClient.Instance.FundAccount((_success, _responseInfo) =>
            {
                success = _success;
                responseInfo = _responseInfo;
            }, aliceAddress.ToString(), 100000000, faucetEndpoint));
            yield return fundAliceAccountCor;

            if (responseInfo.status != ResponseInfo.Status.Success)
            {
                Debug.LogError("Faucet funding for Alice failed: " + responseInfo.message);
                yield break;
            }
            #endregion

            #region Get Alice Account Balance After Funding
            Coroutine getAliceAccountBalance2 = StartCoroutine(RestClient.Instance.GetAccountBalance((_coin, _responseInfo) =>
            {
                coin = _coin;
                responseInfo = _responseInfo;

            }, aliceAddress));
            yield return getAliceAccountBalance2;

            if (responseInfo.status == ResponseInfo.Status.Failed)
            {
                Debug.LogError(responseInfo.message);
                yield break;
            }

            Debug.Log("Alice's Balance After Funding: " + coin.Value);

            #endregion

            #region Fund Bob Account Through Devnet Faucet
            success = false;
            responseInfo = new ResponseInfo();
            Coroutine fundBobAccountCor = StartCoroutine(FaucetClient.Instance.FundAccount((_success, _responseInfo) =>
            {
                success = _success;
                responseInfo = _responseInfo;
            }, bobAddress.ToString(), 100000000, faucetEndpoint));
            yield return fundBobAccountCor;

            if (responseInfo.status != ResponseInfo.Status.Success)
            {
                Debug.LogError("Faucet funding for Bob failed: " + responseInfo.message);
                yield break;
            }
            #endregion

            #region Get Bob Account Balance After Funding
            Coroutine getBobAccountBalance = StartCoroutine(RestClient.Instance.GetAccountBalance((_coin, _responseInfo) =>
            {
                coin = _coin;
                responseInfo = _responseInfo;
            }, bobAddress));
            yield return getBobAccountBalance;

            if (responseInfo.status == ResponseInfo.Status.Failed)
            {
                Debug.LogError(responseInfo.message);
                yield break;
            }

            Debug.Log("Bob's Balance After Funding: " + coin.Value);

            #endregion

            #region Have Alice give Bob 1_000 coins - Submit Transfer Transaction
            Transaction transferTxn = new Transaction();
            Coroutine transferCor = StartCoroutine(RestClient.Instance.Transfer((_transaction, _responseInfo) =>
            {
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
            #endregion

            #region Wait For Transaction
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
                Debug.LogWarning("Transaction was not found. Breaking out of example", gameObject);
                yield break;
            }

            #endregion

            #region Get Alice Account Balance After Transfer
            Coroutine getAliceAccountBalance3 = StartCoroutine(RestClient.Instance.GetAccountBalance((_coin, _responseInfo) =>
            {
                coin = _coin;
                responseInfo = _responseInfo;
            }, aliceAddress));
            yield return getAliceAccountBalance3;

            if(responseInfo.status == ResponseInfo.Status.Failed)
            {
                Debug.LogError(responseInfo.message);
                yield break;
            }

            Debug.Log("Alice Balance After Transfer: " + coin.Value);
            #endregion

            #region Get Bob Account Balance After Transfer
            Coroutine getBobAccountBalance2 = StartCoroutine(RestClient.Instance.GetAccountBalance((_coin, _responseInfo) =>
            {
                coin = _coin;
                responseInfo = _responseInfo;
            }, bobAddress));
            yield return getBobAccountBalance2;

            if (responseInfo.status == ResponseInfo.Status.Failed)
            {
                Debug.LogError(responseInfo.message);
                yield break;
            }

            Debug.Log("Bob Balance After Transfer: " + coin.Value);

            #endregion
        }
    }
}
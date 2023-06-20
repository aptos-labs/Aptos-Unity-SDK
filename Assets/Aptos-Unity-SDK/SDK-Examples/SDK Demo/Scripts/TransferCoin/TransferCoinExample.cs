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
            #region REST & Faucet Client Setup
            Debug.Log("<color=cyan>=== =========================== ===</color>");
            Debug.Log("<color=cyan>=== Set Up Faucet & REST Client ===</color>");
            Debug.Log("<color=cyan>=== =========================== ===</color>");
            string faucetEndpoint = "https://faucet.devnet.aptoslabs.com";

            FaucetClient faucetClient = FaucetClient.Instance;

            RestClient restClient = RestClient.Instance;
            Coroutine restClientSetupCor = StartCoroutine(RestClient.Instance.SetUp((_restClient) => {
                restClient = _restClient;
            }, Constants.DEVNET_BASE_URL));
            yield return restClientSetupCor;
            #endregion

            Wallet wallet = new Wallet(mnemo);

            #region Alice Account
            Debug.Log("<color=cyan>=== ========= ===</color>");
            Debug.Log("<color=cyan>=== Addresses ===</color>");
            Debug.Log("<color=cyan>=== ========= ===</color>");
            Account alice = wallet.GetAccount(0);
            string authKey = alice.AuthKey();
            Debug.Log("Alice Auth Key: " + authKey);

            AccountAddress aliceAddress = alice.AccountAddress;
            Debug.Log("Alice's Account Address: " + aliceAddress.ToString());

            PrivateKey privateKey = alice.PrivateKey;
            Debug.Log("Aice Private Key: " + privateKey);
            #endregion

            #region Bob Account
            Account bob = wallet.GetAccount(1);
            AccountAddress bobAddress = bob.AccountAddress;
            Debug.Log("Bob's Account Address: " + bobAddress.ToString());

            Debug.Log("Wallet: Account 0: Alice: " + aliceAddress.ToString());
            Debug.Log("Wallet: Account 1: Bob: " + bobAddress.ToString());
            #endregion

            Debug.Log("<color=cyan>=== ================ ===</color>");
            Debug.Log("<color=cyan>=== Initial Balances ===</color>");
            Debug.Log("<color=cyan>=== ================ ===</color>");

            #region Get Alice Account Balance
            ResponseInfo responseInfo = new ResponseInfo();
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

            Debug.Log("<color=cyan>=== ===================== ===</color>");
            Debug.Log("<color=cyan>=== Intermediate Balances ===</color>");
            Debug.Log("<color=cyan>=== ===================== ===</color>");

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

            #region Have Alice give Bob another 1_000 coins using BCS
            string responseStr = "";
            Coroutine bcsTransferCor = StartCoroutine(RestClient.Instance.BCSTransfer((_responseStr, _responseInfo) =>
            {
                responseStr = _responseStr;
                responseInfo = _responseInfo;

            }, alice, bobAddress, 1000));
            yield return bcsTransferCor;

            Debug.Log("BCS TRANSFER: " + responseStr);
            TransferCoinBCSResponse response = JsonConvert.DeserializeObject<TransferCoinBCSResponse>(responseStr);

            // Wait for transaction
            waitForTxnSuccess = false;
            waitForTransactionCor = StartCoroutine(
                RestClient.Instance.WaitForTransaction((_pending, _responseInfo) =>
                {
                    waitForTxnSuccess = _pending;
                    responseInfo = _responseInfo;
                }, transactionHash)
            );
            yield return waitForTransactionCor;

            if (!waitForTxnSuccess)
            {
                Debug.LogWarning("Transaction was not found. Breaking out of example", gameObject);
                yield break;
            }
            #endregion

            Debug.Log("<color=cyan>=== ============== ===</color>");
            Debug.Log("<color=cyan>=== Final Balances ===</color>");
            Debug.Log("<color=cyan>=== ============== ===</color>");
            #region Final Balances
            Coroutine getAliceAccountBalance4 = StartCoroutine(RestClient.Instance.GetAccountBalance((_coin, _responseInfo) =>
            {
                coin = _coin;
                responseInfo = _responseInfo;
            }, aliceAddress));
            yield return getAliceAccountBalance4;

            if (responseInfo.status == ResponseInfo.Status.Failed)
            {
                Debug.LogError(responseInfo.message);
                yield break;
            }

            Debug.Log("Alice Balance After Transfer: " + coin.Value);

            Coroutine getBobAccountBalance3 = StartCoroutine(RestClient.Instance.GetAccountBalance((_coin, _responseInfo) =>
            {
                coin = _coin;
                responseInfo = _responseInfo;
            }, bobAddress));
            yield return getBobAccountBalance3;

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
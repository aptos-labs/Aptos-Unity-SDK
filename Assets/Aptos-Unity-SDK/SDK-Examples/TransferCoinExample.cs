using Aptos.Accounts;
using Aptos.HdWallet;
using Aptos.Rest;
using Aptos.Unity.Rest;
using Aptos.Unity.Rest.Model;
using NBitcoin;
using Newtonsoft.Json;
using System.Collections;
using UnityEngine;

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
        AccountAddress aliceAddress = alice.AccountAddress;
        #endregion

        #region Bob Account
        Account bob = wallet.GetAccount(1);
        AccountAddress bobAddress = bob.AccountAddress;

        Debug.Log("Wallet: Account 0: Alice: " + aliceAddress.ToString());
        Debug.Log("Wallet: Account 1: Bob: " + bobAddress.ToString());
        #endregion

        #region REST Client Setup
        RestClient.Instance.SetEndPoint(Constants.DEVNET_BASE_URL);
        #endregion

        #region Get Alice Account Balance
        Coroutine getAliceBalanceCor1 = StartCoroutine(RestClient.Instance.GetAccountBalance((returnResult) =>
        {
            if (returnResult == null)
            {
                Debug.LogWarning("Account address not found, balance is 0");
                Debug.Log("Alice: ::: " + 0);
            }
            else
            {
                AccountResourceCoin acctResourceCoin = JsonConvert.DeserializeObject<AccountResourceCoin>(returnResult);
                Debug.Log("Alice: ::: " + acctResourceCoin.DataProp.Coin.Value);
            }

        }, aliceAddress));
        yield return getAliceBalanceCor1;
        #endregion

        string faucetEndpoint = "https://faucet.devnet.aptoslabs.com";

        #region Fund Alice Account Through Devnet Faucet
        Coroutine fundAliceAccountCor = StartCoroutine(FaucetClient.Instance.FundAccount((returnResult) =>
        {
            Debug.Log("Faucet Response: " + returnResult);
        }, aliceAddress.ToString(), 100000000, faucetEndpoint));
        yield return fundAliceAccountCor;
        #endregion

        #region Get Alice Account Balance After Funding
        Coroutine getAliceAccountBalance2 = StartCoroutine(RestClient.Instance.GetAccountBalance((returnResult) =>
        {
            if (returnResult == null)
            {
                Debug.LogWarning("Account address not found, balance is 0");
                Debug.Log("Alice: ::: " + 0);
            }
            else
            {
                AccountResourceCoin acctResourceCoin = JsonConvert.DeserializeObject<AccountResourceCoin>(returnResult);
                Debug.Log("Alice: ::: " + acctResourceCoin.DataProp.Coin.Value);
            }

        }, aliceAddress));
        yield return getAliceAccountBalance2;
        #endregion

        #region Fund Bob Account Through Devnet Faucet
        Coroutine fundBobAccountCor = StartCoroutine(FaucetClient.Instance.FundAccount((returnResult) =>
        {
            Debug.Log("Faucet Response: " + returnResult);
        }, bobAddress.ToString(), 100000000, faucetEndpoint));
        yield return fundBobAccountCor;
        #endregion

        #region Get Bob Account Balance After Funding
        Coroutine getBobAccountBalance = StartCoroutine(RestClient.Instance.GetAccountBalance((returnResult) =>
        {
            if (returnResult == null)
            {
                Debug.LogWarning("Account address not found, balance is 0");
                Debug.Log("Bob: ::: " + 0);
            }
            else
            {
                AccountResourceCoin acctResourceCoin = JsonConvert.DeserializeObject<AccountResourceCoin>(returnResult);
                Debug.Log("Bob: ::: " + acctResourceCoin.DataProp.Coin.Value);
            }

        }, bobAddress));
        yield return getBobAccountBalance;
        #endregion

        #region Have Alice give Bob 1_000 coins
        string transferResult = "";
        Coroutine transferCor = StartCoroutine(RestClient.Instance.Transfer((_transferResult) =>
        {
            transferResult = _transferResult;
        }, alice, bob.AccountAddress.ToHexString(), 1000));

        yield return transferCor;

        Debug.Log("TRANSFER RESPONSE: " + transferResult);
        #endregion
    }
}

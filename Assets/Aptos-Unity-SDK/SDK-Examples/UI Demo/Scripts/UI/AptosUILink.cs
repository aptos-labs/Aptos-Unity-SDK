using System;
using System.Collections;
using System.Collections.Generic;
using Aptos.HdWallet;
using NBitcoin;
using UnityEngine;
using Aptos.Unity.Rest;
using Aptos.Unity.Rest.Model;

namespace Aptos.Unity.Sample.UI
{
    public class AptosUILink : MonoBehaviour
    {
        static public AptosUILink Instance { get; set; }

        [HideInInspector]
        public string mnemonicsKey = "MnemonicsKey";
        [HideInInspector]
        public string privateKey = "PrivateKey";
        [HideInInspector]
        public string currentAddressIndexKey = "CurrentAddressIndexKey";

        [SerializeField] private int accountNumLimit = 10;
        public List<string> addressList;

        public event Action<float> onGetBalance;

        private Wallet wallet;
        private string faucetEndpoint = "https://faucet.devnet.aptoslabs.com";

        private void Awake()
        {
            Instance = this;
        }

        void Start()
        {

        }

        void Update()
        {

        }

        public void InitWalletFromCache()
        {
            wallet = new Wallet(PlayerPrefs.GetString(mnemonicsKey));
            GetWalletAddress();
            LoadCurrentWalletBalance();
        }

        public bool CreateNewWallet()
        {
            Mnemonic mnemo = new Mnemonic(Wordlist.English, WordCount.Twelve);
            wallet = new Wallet(mnemo);

            PlayerPrefs.SetString(mnemonicsKey, mnemo.ToString());
            PlayerPrefs.SetInt(currentAddressIndexKey, 0);

            GetWalletAddress();
            LoadCurrentWalletBalance();

            if (mnemo.ToString() != string.Empty)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool RestoreWallet(string _mnemo)
        {
            try
            {
                wallet = new Wallet(_mnemo);
                PlayerPrefs.SetString(mnemonicsKey, _mnemo);
                PlayerPrefs.SetInt(currentAddressIndexKey, 0);

                GetWalletAddress();
                LoadCurrentWalletBalance();

                return true;
            }
            catch
            {

            }

            return false;
        }

        public List<string> GetWalletAddress()
        {
            addressList = new List<string>();

            for (int i = 0; i < accountNumLimit; i++)
            {
                var account = wallet.GetAccount(i);
                var addr = account.AccountAddress.ToString();

                addressList.Add(addr);
            }

            return addressList;
        }

        public string GetCurrentWalletAddress()
        {
            return addressList[PlayerPrefs.GetInt(currentAddressIndexKey)];
        }

        public string GetPrivateKey()
        {
            return wallet.Account.PrivateKey;
        }

        public void LoadCurrentWalletBalance()
        {
            AccountResourceCoin.Coin coin = new AccountResourceCoin.Coin();
            ResponseInfo responseInfo = new ResponseInfo();

            StartCoroutine(RestClient.Instance.GetAccountBalance((_coin, _responseInfo) =>
            {
                coin = _coin;
                responseInfo = _responseInfo;

                if(responseInfo.status != ResponseInfo.Status.Success)
                {
                    onGetBalance?.Invoke(0.0f);
                }
                else
                {
                    onGetBalance?.Invoke(float.Parse(coin.Value));
                }

            }, wallet.GetAccount(PlayerPrefs.GetInt(currentAddressIndexKey)).AccountAddress));
        }

        public IEnumerator AirDrop(int _amount)
        {
            Coroutine cor = StartCoroutine(FaucetClient.Instance.FundAccount((success, returnResult) =>
            {
            }, wallet.GetAccount(PlayerPrefs.GetInt(currentAddressIndexKey)).AccountAddress.ToString()
                , _amount
                , faucetEndpoint));

            yield return cor;

            yield return new WaitForSeconds(1f);
            LoadCurrentWalletBalance();
            UIController.Instance.ToggleNotification(ResponseInfo.Status.Success, "Successfully Get Airdrop of " + AptosTokenToFloat((float)_amount) + " APT");
        }

        public IEnumerator SendToken(string _targetAddress, long _amount)
        {
            Rest.Model.Transaction transferTxn = new Rest.Model.Transaction();
            ResponseInfo responseInfo = new ResponseInfo();
            Coroutine transferCor = StartCoroutine(RestClient.Instance.Transfer((_transferTxn, _responseInfo) =>
            {
                transferTxn = _transferTxn;
                responseInfo = _responseInfo;
            }, wallet.GetAccount(PlayerPrefs.GetInt(currentAddressIndexKey)), _targetAddress, _amount));

            yield return transferCor;

            if (responseInfo.status == ResponseInfo.Status.Success)
            {
                string transactionHash = transferTxn.Hash;
                bool waitForTxnSuccess = false;
                Coroutine waitForTransactionCor = StartCoroutine(
                    RestClient.Instance.WaitForTransaction((_pending, _responseInfo) =>
                    {
                        waitForTxnSuccess = _pending;
                        responseInfo = _responseInfo;
                    }, transactionHash)
                );
                yield return waitForTransactionCor;

                if (waitForTxnSuccess)
                {
                    UIController.Instance.ToggleNotification(ResponseInfo.Status.Success, "Successfully send " + AptosTokenToFloat((float)_amount) + " APT to " + UIController.Instance.ShortenString(_targetAddress, 4));
                }
                else
                {
                    UIController.Instance.ToggleNotification(ResponseInfo.Status.Failed, "Send Token Transaction Failed");
                }
                
            }
            else
            {
                UIController.Instance.ToggleNotification(ResponseInfo.Status.Failed, responseInfo.message);
            }

            yield return new WaitForSeconds(1f);
            LoadCurrentWalletBalance();
        }

        public IEnumerator CreateCollection(string _collectionName, string _collectionDescription, string _collectionUri)
        {
            Rest.Model.Transaction createCollectionTxn = new Rest.Model.Transaction();
            ResponseInfo responseInfo = new ResponseInfo();
            Coroutine createCollectionCor = StartCoroutine(RestClient.Instance.CreateCollection((_createCollectionTxn, _responseInfo) =>
            {
                createCollectionTxn = _createCollectionTxn;
                responseInfo = _responseInfo;
            }, wallet.GetAccount(PlayerPrefs.GetInt(currentAddressIndexKey)),
            _collectionName, _collectionDescription, _collectionUri));
            yield return createCollectionCor;

            if (responseInfo.status == ResponseInfo.Status.Success)
            {
                UIController.Instance.ToggleNotification(ResponseInfo.Status.Success, "Successfully Create Collection: " + _collectionName);
            }
            else
            {
                UIController.Instance.ToggleNotification(ResponseInfo.Status.Failed, responseInfo.message);
            }

            yield return new WaitForSeconds(1f);
            LoadCurrentWalletBalance();

            string transactionHash = createCollectionTxn.Hash;
        }

        public IEnumerator CreateNFT(string _collectionName, string _tokenName, string _tokenDescription, int _supply, int _max, string _uri, int _royaltyPointsPerMillion)
        {
            Rest.Model.Transaction createTokenTxn = new Rest.Model.Transaction();
            ResponseInfo responseInfo = new ResponseInfo();
            Coroutine createTokenCor = StartCoroutine(
                RestClient.Instance.CreateToken((_createTokenTxn, _responseInfo) =>
                {
                    createTokenTxn = _createTokenTxn;
                    responseInfo = _responseInfo;
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

            if (responseInfo.status == ResponseInfo.Status.Success)
            {
                UIController.Instance.ToggleNotification(ResponseInfo.Status.Success, "Successfully Create NFT: " + _tokenName);
            }
            else
            {
                UIController.Instance.ToggleNotification(ResponseInfo.Status.Failed, responseInfo.message);
            }

            yield return new WaitForSeconds(1f);
            LoadCurrentWalletBalance();

            string createTokenTxnHash = createTokenTxn.Hash;
        }

        public float AptosTokenToFloat(float _token)
        {
            return _token / 100000000f;
        }

        public long AptosFloatToToken(float _amount)
        {
            return Convert.ToInt64(_amount * 100000000);
        }
    }
}
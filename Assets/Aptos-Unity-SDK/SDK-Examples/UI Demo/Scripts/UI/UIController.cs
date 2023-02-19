using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using Aptos.Unity.Rest;
using Aptos.Unity.Rest.Model;

namespace Aptos.Unity.Sample.UI
{
    public class UIController : MonoBehaviour
    {
        static public UIController Instance { get; set; }

        [Header("General")]
        public List<PanelTab> panelTabs;
        [Space]
        [SerializeField] private TMP_Text mainPanelTitle;
        [SerializeField] private Canvas mainCanvas;
        [SerializeField] private GameObject notificationPrefab;
        [Space]
        [SerializeField] private PanelTab accountTab;
        [SerializeField] private PanelTab sendTransactionTab;
        [SerializeField] private PanelTab mintNFTTab;
        [SerializeField] private PanelTab nftLoaderTab;
        [SerializeField] private PanelTab addAccountTab;

        [Header("Infos")]
        [SerializeField] private TMP_Dropdown walletListDropDown;
        [SerializeField] private TMP_Dropdown networkDropDown;
        [SerializeField] private TMP_Text balanceText;

        [Header("Add Account")]
        [SerializeField] private TMP_InputField createdMnemonicInputField;
        [SerializeField] private TMP_InputField importMnemonicInputField;

        [Header("Send Transaction")]
        [SerializeField] private TMP_Text senderAddress;
        [SerializeField] private TMP_InputField receiverAddressInput;
        [SerializeField] private TMP_InputField sendAmountInput;

        [Header("Mint NFT")]
        [SerializeField] private TMP_InputField c_collectionNameInputField;
        [SerializeField] private TMP_InputField collectionDescriptionInputField;
        [SerializeField] private TMP_InputField collectionUriInputField;
        [Space]
        [SerializeField] private TMP_InputField n_collectionNameInputField;
        [SerializeField] private TMP_InputField tokenNameInputField;
        [SerializeField] private TMP_InputField tokenDescriptionInputField;
        [SerializeField] private TMP_InputField supplyInputField;
        [SerializeField] private TMP_InputField maxInputField;
        [SerializeField] private TMP_InputField tokenURIInputField;
        [SerializeField] private TMP_InputField royaltyPointsInputField;

        [Header("Notification")]
        [SerializeField] private Transform notificationPanel;

        private void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            InitStatusCheck();

            AptosUILink.Instance.onGetBalance += UpdateBalance;
        }

        void Update()
        {

        }

        #region General

        void InitStatusCheck()
        {
            networkDropDown.onValueChanged.AddListener(delegate
            {
                SetNetwork(networkDropDown);
            });

            SetNetwork(networkDropDown);

            if (PlayerPrefs.GetString(AptosUILink.Instance.mnemonicsKey) != string.Empty)
            {
                AptosUILink.Instance.InitWalletFromCache();
                AddWalletAddressListUI(AptosUILink.Instance.addressList);
                ToggleEmptyState(false);
                ToggleNotification(ResponseInfo.Status.Success, "Successfully Import the Wallet");
            }
            else
            {
                ToggleEmptyState(true);
            }

            walletListDropDown.onValueChanged.AddListener(delegate
            {
                OnWalletListDropdownValueChanged(walletListDropDown);
            });
        }

        public void ToggleEmptyState(bool _empty)
        {
            accountTab.DeActive(_empty);
            sendTransactionTab.DeActive(_empty);
            mintNFTTab.DeActive(_empty);
            nftLoaderTab.DeActive(_empty);

            if (_empty)
            {
                walletListDropDown.ClearOptions();
                List<string> options = new List<string>();
                options.Add("Please Create Wallet First");
                walletListDropDown.AddOptions(options);
                balanceText.text = "n/a APT";
                createdMnemonicInputField.text = String.Empty;
                importMnemonicInputField.text = String.Empty;

                OpenTabPanel(addAccountTab);
            }
        }

        public void OpenTabPanel(PanelTab _panelTab)
        {
            foreach (PanelTab _childPanelTab in panelTabs)
            {
                if (_childPanelTab.panelGroup == _panelTab.panelGroup)
                {
                    _childPanelTab.UnSelected();
                }
            }

            _panelTab.Selected();

            if (_panelTab.panelGroup == PanelGroup.mainPanel)
            {
                mainPanelTitle.text = _panelTab.tabName;
            }
        }

        public void ToggleNotification(ResponseInfo.Status status, string _message)
        {
            NotificationPanel np = Instantiate(notificationPrefab, notificationPanel).GetComponent<NotificationPanel>();
            np.Toggle(status, _message);
        }

        #endregion

        #region Account

        public void OnCreateWalletClicked()
        {
            if (AptosUILink.Instance.CreateNewWallet())
            {
                createdMnemonicInputField.text = PlayerPrefs.GetString(AptosUILink.Instance.mnemonicsKey);
                ToggleEmptyState(false);
                ToggleNotification(ResponseInfo.Status.Success, "Successfully Create the Wallet");
            }
            else
            {
                ToggleEmptyState(true);
                ToggleNotification(ResponseInfo.Status.Failed, "Fail to Create the Wallet");
            }

            AddWalletAddressListUI(AptosUILink.Instance.addressList);
        }

        public void OnImportWalletClicked(TMP_InputField _input)
        {
            if (AptosUILink.Instance.RestoreWallet(_input.text))
            {
                AddWalletAddressListUI(AptosUILink.Instance.addressList);
                ToggleEmptyState(false);
                ToggleNotification(ResponseInfo.Status.Success, "Successfully Import the Wallet");
            }
            else
            {
                ToggleEmptyState(true);
                ToggleNotification(ResponseInfo.Status.Failed, "Fail to Import the Wallet");
            }
        }

        public void AddWalletAddressListUI(List<string> _addressList)
        {
            walletListDropDown.ClearOptions();
            walletListDropDown.value = 0;

            List<string> addressList = new List<string>();
            foreach (string _s in _addressList)
            {
                //addressList.Add(ShortenString(_s, 4));
                addressList.Add(_s);
            }

            walletListDropDown.AddOptions(addressList);

            senderAddress.text = AptosUILink.Instance.GetCurrentWalletAddress();
        }

        void OnWalletListDropdownValueChanged(TMP_Dropdown _target)
        {
            PlayerPrefs.SetInt(AptosUILink.Instance.currentAddressIndexKey, _target.value);
            AptosUILink.Instance.LoadCurrentWalletBalance();
            senderAddress.text = AptosUILink.Instance.addressList[_target.value];
        }

        void UpdateBalance(float _amount)
        {
            balanceText.text = AptosUILink.Instance.AptosTokenToFloat(_amount).ToString("0.0000") + " APT";
        }

        public void Logout()
        {
            PlayerPrefs.DeleteKey(AptosUILink.Instance.mnemonicsKey);

            ToggleEmptyState(true);
        }

        public void SetNetwork(TMP_Dropdown _target)
        {
            switch (_target.options[_target.value].text)
            {
                case "Mainnet":
                    RestClient.Instance.SetEndPoint(Constants.MAINNET_BASE_URL);
                    break;
                case "Devnet":
                    RestClient.Instance.SetEndPoint(Constants.DEVNET_BASE_URL);
                    break;
                case "Testnet":
                    RestClient.Instance.SetEndPoint(Constants.TESTNET_BASE_URL);
                    break;
                default:
                    RestClient.Instance.SetEndPoint(Constants.DEVNET_BASE_URL);
                    break;
            }

            ToggleNotification(ResponseInfo.Status.Success, "Set Network to " + _target.options[_target.value].text);
        }

        public void CopyMnemonicWords()
        {
            CopyToClipboard(PlayerPrefs.GetString(AptosUILink.Instance.mnemonicsKey));
        }

        public void CopyPrivateKey()
        {
            CopyToClipboard(AptosUILink.Instance.GetPrivateKey());
        }

        #endregion

        #region Send Transaction

        public void SendToken()
        {
            if (receiverAddressInput.text == string.Empty || sendAmountInput.text == String.Empty)
            {
                ToggleNotification(ResponseInfo.Status.Failed, "Please Fill Out All Required Fields");
            }
            else
            {
                StartCoroutine(AptosUILink.Instance.SendToken(receiverAddressInput.text, AptosUILink.Instance.AptosFloatToToken(float.Parse(sendAmountInput.text))));
            }
        }

        public void Airdrop(int _amount)
        {
            StartCoroutine(AptosUILink.Instance.AirDrop(_amount));
        }

        #endregion

        #region NFT

        public void CreateCollection()
        {
            if (c_collectionNameInputField.text == String.Empty || collectionDescriptionInputField.text == String.Empty || collectionUriInputField.text == String.Empty)
            {
                ToggleNotification(ResponseInfo.Status.Failed, "Please Fill Out All Required Fields");
            }
            else
            {
                StartCoroutine(AptosUILink.Instance.CreateCollection(
                    c_collectionNameInputField.text,
                    collectionDescriptionInputField.text,
                    collectionUriInputField.text
                    ));
            }
        }

        public void CreateNFT()
        {
            if (n_collectionNameInputField.text == String.Empty ||
                tokenNameInputField.text == String.Empty ||
                tokenDescriptionInputField.text == String.Empty ||
                supplyInputField.text == String.Empty ||
                maxInputField.text == String.Empty ||
                tokenURIInputField.text == String.Empty ||
                royaltyPointsInputField.text == String.Empty)
            {
                ToggleNotification(ResponseInfo.Status.Failed, "Please Fill Out All Required Fields");
            }
            else
            {
                StartCoroutine(AptosUILink.Instance.CreateNFT(
                n_collectionNameInputField.text,
                tokenNameInputField.text,
                tokenDescriptionInputField.text,
                Int32.Parse(supplyInputField.text),
                Int32.Parse(maxInputField.text),
                tokenURIInputField.text,
                Int32.Parse(royaltyPointsInputField.text)
                ));
            }
        }

        #endregion

        #region Utilities

        public void TapToCopy(TMP_InputField _target)
        {
            CopyToClipboard(_target.text);
        }

        public void TapToCopy(TMP_Text _target)
        {
            CopyToClipboard(_target.text);
        }

        public void TapToCopy(TMP_Dropdown _target)
        {
            CopyToClipboard(_target.options[_target.value].text);
        }

        public string ShortenString(string _input, int _length)
        {
            return _input.Substring(0, _length) + "..." + _input.Substring(_input.Length - _length, _length);
        }

        void CopyToClipboard(string _input)
        {
            TextEditor te = new TextEditor();
            te.text = _input;
            te.SelectAll();
            te.Copy();
        }

        #endregion
    }
}
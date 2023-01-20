using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;

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
    [SerializeField] private PanelTab addAccountTab;

    [Header("Infos")]
    [SerializeField] private TMP_Dropdown walletListDropDown;
    [SerializeField] private TMP_Text balanceText;

    [Header("Add Account")]
    [SerializeField] private TMP_InputField createdMnemonicInputField;
    [SerializeField] private TMP_InputField importMnemonicInputField;

    [Header("Send Transaction")]
    [SerializeField] private TMP_Text senderAddress;

    [Header("Notification")]
    [SerializeField] private Transform notificationPanel;

    public event Action<float> onGetBalance;

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
        if (PlayerPrefs.GetString(AptosUILink.Instance.MnemonicsKey) != string.Empty)
        {
            AptosUILink.Instance.InitWalletFromCache();
            AddWalletAddressListUI(AptosUILink.Instance.addressList);
            ToggleEmptyState(false);
        }
        else
        {
            ToggleEmptyState(true);
        }

        walletListDropDown.onValueChanged.AddListener(delegate {
            OnWalletListDropdownValueChanged(walletListDropDown);
        });
    }

    public void ToggleEmptyState(bool _empty)
    {
        accountTab.DeActive(_empty);
        sendTransactionTab.DeActive(_empty);
        mintNFTTab.DeActive(_empty);

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

    public void ToggleNotification(bool _success, string _message)
    {
        NotificationPanel np = Instantiate(notificationPrefab, notificationPanel).GetComponent<NotificationPanel>();
        np.Toggle(_success, _message);
        Debug.Log("Operation: " + _success + " || Got Message: " + _message);
    }

    #endregion

    #region Account

    public void OnCreateWalletClicked()
    {
        if (AptosUILink.Instance.CreateNewWallet())
        {
            createdMnemonicInputField.text = PlayerPrefs.GetString(AptosUILink.Instance.MnemonicsKey);
            ToggleEmptyState(false);
            ToggleNotification(true, "Successfully Create the Wallet");
        }
        else
        {
            ToggleEmptyState(true);
            ToggleNotification(false, "Fail to Create the Wallet");
        }

        AddWalletAddressListUI(AptosUILink.Instance.addressList);
    }

    public void OnImportWalletClicked(TMP_InputField _input)
    {
        if (AptosUILink.Instance.RestoreWallet(_input.text))
        {
            AddWalletAddressListUI(AptosUILink.Instance.addressList);
            ToggleEmptyState(false);
            ToggleNotification(true, "Successfully Import the Wallet");
        }
        else
        {
            ToggleEmptyState(true);
            ToggleNotification(false, "Fail to Import the Wallet");
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
        PlayerPrefs.SetInt(AptosUILink.Instance.CurrentAddressIndexKey, _target.value);
        AptosUILink.Instance.LoadCurrentWalletBalance();
        senderAddress.text = AptosUILink.Instance.addressList[_target.value];
        Debug.Log(AptosUILink.Instance.addressList[_target.value]);
    }

    void UpdateBalance(float _amount)
    {
        balanceText.text = AptosUILink.Instance.AptoTokenToFloat(_amount).ToString("0.0000") + " APT";
    }

    public void Airdrop(int _amount)
    {
        StartCoroutine(AptosUILink.Instance.AirDrop(_amount));
    }

    public void Logout()
    {
        PlayerPrefs.DeleteKey(AptosUILink.Instance.MnemonicsKey);

        ToggleEmptyState(true);
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

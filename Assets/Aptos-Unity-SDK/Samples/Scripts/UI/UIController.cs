using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIController : MonoBehaviour
{
    static public UIController instance { get; set; }

    public List<PanelTab> panelTabs;
    [Space]
    [SerializeField] private TMP_Text mainPanelTitle;
    [SerializeField] private Canvas mainCanvas;
    [SerializeField] private GameObject notificationPrefab;

    [Header("Wallet")]
    [SerializeField] private TMP_InputField createdMnemonicInputField;



    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
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

        if(_panelTab.panelGroup == PanelGroup.mainPanel)
        {
            mainPanelTitle.text = _panelTab.tabName;
        }
    }

    public void ToggleNotification(bool _success, string _message)
    {
        NotificationPanel np = Instantiate(notificationPrefab, mainCanvas.transform).GetComponent<NotificationPanel>();
        np.Toggle(_success, _message);
    }

    public void OnCreateWalletClicked()
    {

        createdMnemonicInputField.text = "";
        ToggleNotification(true, "Create Wallet Success");
    }

    public void OnImportWalletClicked(TMP_InputField _input)
    {
        AptosUILink.instance.RestoreWallet(_input.text);
        ToggleNotification(false, "Import Wallet Fail");
    }

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

    void CopyToClipboard(string _input)
    {
        TextEditor te = new TextEditor();
        te.text = _input;
        te.SelectAll();
        te.Copy();
    }

    #endregion
}

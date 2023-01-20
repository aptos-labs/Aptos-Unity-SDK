using System;
using System.Collections;
using System.Collections.Generic;
using Aptos.Rest;
using NBitcoin;
using Unity.VisualScripting;
using UnityEngine;

public class AptosUILink : MonoBehaviour
{
    static public AptosUILink instance { get; set; }

    [SerializeField] private int accountNumLimit = 10;

    [HideInInspector]
    public string MnemonicsKey = "MnemonicsKey";
    [HideInInspector]
    public string CurrentAddressIndexKey = "CurrentAddressIndexKey";

    public List<string> addressList;
    //private WalletTwo wallet;

    public event Action<float> onGetBalance;

    private void Awake()
    {
        instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void InitWalletFromCache()
    {
    }

    public bool CreateNewWallet()
    {
        return false;
    }

    public bool RestoreWallet(string _mnemo)
    {
        return false;
    }

    public List<string> GetWalletAddress()
    {
        addressList = new List<string>();

        return addressList;
    }

    public string GetCurrentWalletAddress()
    {
        return addressList[PlayerPrefs.GetInt(CurrentAddressIndexKey)];
    }

    public void LoadCurrentWalletBalance()
    {
    }
}

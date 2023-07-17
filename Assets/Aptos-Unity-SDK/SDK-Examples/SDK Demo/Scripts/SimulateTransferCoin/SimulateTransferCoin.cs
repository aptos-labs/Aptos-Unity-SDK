using System.Collections;
using System.Collections.Generic;
using Aptos.Accounts;
using Aptos.BCS;
using Aptos.Unity.Rest;
using Aptos.Unity.Rest.Model;
using UnityEngine;
using TransactionPayload = Aptos.BCS.TransactionPayload;

namespace Aptos.Unity.Sample
{
    public class SimulateTransferCoin : MonoBehaviour
    {
        void Start()
        {
            StartCoroutine(RunSimulateTransferExample());
        }

        IEnumerator RunSimulateTransferExample()
        {
            #region REST & Faucet Client Setup
            Debug.Log("<color=cyan>=== =========================== ===</color>");
            Debug.Log("<color=cyan>=== Set Up Faucet & REST Client ===</color>");
            Debug.Log("<color=cyan>=== =========================== ===</color>");
            string faucetEndpoint = "https://faucet.devnet.aptoslabs.com";

            FaucetClient faucetClient = FaucetClient.Instance;

            RestClient restClient = new RestClient();
            Coroutine restClientSetupCor = StartCoroutine(RestClient.Instance.SetUp((_restClient) => {
                restClient = _restClient;
            }, Constants.DEVNET_BASE_URL));
            yield return restClientSetupCor;

            AptosTokenClient tokenClient = AptosTokenClient.Instance.SetUp(restClient);
            #endregion

            #region Generate Alice and Bob's Accounts and Only Fund Alice's Account
            Debug.Log("<color=cyan>=== ========= ===</color>");
            Debug.Log("<color=cyan>=== Addresses ===</color>");
            Debug.Log("<color=cyan>=== ========= ===</color>");
            Account alice = Account.Generate();
            Account bob = Account.Generate();

            Debug.Log(string.Format("Alice: {0}", alice.AccountAddress));
            Debug.Log(string.Format("Bob: {0}", bob.AccountAddress));

            // Fund Alice account
            bool success = false;
            ResponseInfo responseInfo = new ResponseInfo();
            Coroutine fundAliceAccountCor = StartCoroutine(faucetClient.FundAccount((_success, _responseInfo) =>
            {
                success = _success;
                responseInfo = _responseInfo;
            }, alice.AccountAddress.ToString(), 100000000, faucetEndpoint));
            yield return fundAliceAccountCor;

            if (responseInfo.status != ResponseInfo.Status.Success)
            {
                Debug.LogError("Faucet funding for Alice failed: " + responseInfo.message);
                yield break;
            }
            #endregion

            #region Create BCS Transaction
            // Transaction Arguments
            AccountAddress aliceAccountAddress = alice.AccountAddress;
            List<ISerializable> txnArgumentsList = new List<ISerializable>() {
                aliceAccountAddress,
                new U64(100)
            };
            ISerializable[] transactionArguments = txnArgumentsList.ToArray();

            // Entry Function
            EntryFunction entryFunction = EntryFunction.Natural(
                new ModuleId(AccountAddress.FromHex("0x1"), "coin"),
                "transfer",
                new TagSequence(new ISerializableTag[] { StructTag.FromStr("0x1::aptos_coin::AptosCoin") }),
                new Sequence(new ISerializable[] { bob.AccountAddress, new U64(100000) })
            );

            // Create BCS Transaction
            RawTransaction rawTxn = null;
            Coroutine createBCSTransactionCor = StartCoroutine(
                restClient.CreateBCSTransaction(
                    (_rawTxn) => {
                        rawTxn = _rawTxn;
                    },
                    alice,
                    new TransactionPayload(entryFunction)
                )
            );
            yield return createBCSTransactionCor;
            #endregion

            #region Simulate Before Creating Bob's Account
            Debug.Log("<color=cyan>=== ===================================== ===</color>");
            Debug.Log("<color=cyan>=== Simulate before creatng Bob's Account ===</color>");
            Debug.Log("<color=cyan>=== ===================================== ===</color>");

            string simulateTxnResponse = "";
            Coroutine simulateTxCor = StartCoroutine(
                restClient.SimulateTransaction(
                    (_respJson, _responseInfo) =>
                    {
                        simulateTxnResponse = _respJson;
                        responseInfo = _responseInfo;
                    },
                    rawTxn, alice
                )
            );
            yield return null;

            // TODO: Check output
            // assert output[0]["vm_status"] != "Executed successfully", "This shouldn't succeed"
            // print(json.dumps(output, indent = 4, sort_keys = True))

            #endregion

            #region Simulate After Creating Bob's Account
            Debug.Log("<color=cyan>=== ==================================== ===</color>");
            Debug.Log("<color=cyan>=== Simulate after creatng Bob's Account ===</color>");
            Debug.Log("<color=cyan>=== ==================================== ===</color>");

            // Fund Alice account
            success = false;
            responseInfo = new ResponseInfo();
            Coroutine fundBobAccountCor = StartCoroutine(faucetClient.FundAccount((_success, _responseInfo) =>
            {
                success = _success;
                responseInfo = _responseInfo;
            }, bob.AccountAddress.ToString(), 0, faucetEndpoint));
            yield return fundBobAccountCor;

            if (responseInfo.status != ResponseInfo.Status.Success)
            {
                Debug.LogError("Faucet funding for Alice failed: " + responseInfo.message);
                yield break;
            }

            // Simulate transaction again
            simulateTxnResponse = "";
            simulateTxCor = StartCoroutine(
                restClient.SimulateTransaction(
                    (_respJson, _responseInfo) =>
                    {
                        simulateTxnResponse = _respJson;
                        responseInfo = _responseInfo;
                    },
                    rawTxn, alice
                )
            );
            yield return null;

            // TODO: Check output
            // assert output[0]["vm_status"] == "Executed successfully", "This should succeed"
            // print(json.dumps(output, indent = 4, sort_keys = True))

            #endregion

            yield return null;
        }
    }

}

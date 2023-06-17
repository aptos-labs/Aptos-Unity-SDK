using Aptos.Accounts;
using Aptos.BCS;
using Aptos.HdWallet;
using Aptos.Unity.Rest;
using Aptos.Unity.Rest.Model;
using NBitcoin;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Aptos.Unity.Sample
{
    public class Multisig : MonoBehaviour
    {
        void Start()
        {
            StartCoroutine(RunAptosMultisigExample());
        }

        IEnumerator RunAptosMultisigExample()
        {
            #region REST & Faucet Client Setup
            Debug.Log("<color=cyan>=== =========================== ===</color>");
            Debug.Log("<color=cyan>=== Set Up Faucet & REST Client ===</color>");
            Debug.Log("<color=cyan>=== ========= ===</color>");
            string faucetEndpoint = "https://faucet.devnet.aptoslabs.com";

            FaucetClient faucetClient = FaucetClient.Instance;

            RestClient restClient = new RestClient();
            Coroutine restClientSetupCor = StartCoroutine(RestClient.Instance.SetUp((_restClient) => {
                restClient = _restClient;
            }, Constants.DEVNET_BASE_URL));
            yield return restClientSetupCor;

            AptosTokenClient tokenClient = AptosTokenClient.Instance.SetUp(restClient);
            #endregion

            #region Section 1. Generate accounts
            Debug.Log("<color=cyan>=== ================= ===</color>");
            Debug.Log("<color=cyan>=== Generate Accounts ===</color>");
            Debug.Log("<color=cyan>=== ================= ===</color>");
            Account alice = Account.Generate();
            Account bob = Account.Generate();
            Account chad = Account.Generate();

            Debug.Log("<color=cyan>=== Account addresses ===</color>");
            Debug.Log(string.Format("Alice: {0}", alice.AccountAddress));
            Debug.Log(string.Format("Bob: {0}", bob.AccountAddress));
            Debug.Log(string.Format("Chad: {0}", chad.AccountAddress));

            Debug.Log("<color=cyan>=== Authentication keys ===</color>");
            Debug.Log(string.Format("Alice: {0}", alice.AuthKey()));
            Debug.Log(string.Format("Bob: {0}", bob.AuthKey()));
            Debug.Log(string.Format("Chad: {0}", chad.AuthKey()));

            Debug.Log("<color=cyan>=== Public keys ===</color>");
            Debug.Log(string.Format("Alice: {0}", alice.PublicKey));
            Debug.Log(string.Format("Bob: {0}", bob.PublicKey));
            Debug.Log(string.Format("Chad: {0}", chad.PublicKey));
            #endregion

            #region Section 2. Create MultiPublicKey and MultiSig
            Debug.Log("<color=cyan>=== ================= ===</color>");
            Debug.Log("<color=cyan>=== Create MultiSig ===</color>");
            Debug.Log("<color=cyan>=== ================= ===</color>");

            byte threshold = 2;

            MultiPublicKey multisigPublicKey = new MultiPublicKey(
                new List<PublicKey>() { alice.PublicKey, bob.PublicKey, chad.PublicKey },
                threshold
            );

            AccountAddress multisigAddress = AccountAddress.FromMultiEd25519(multisigPublicKey);

            Debug.Log("<color=cyan>=== ======================= ===</color>");
            Debug.Log("<color=cyan>=== 2-of-3 Multisig account ===</color>");
            Debug.Log("<color=cyan>=== ======================= ===</color>");
            Debug.Log(string.Format("Account public key: {0}", multisigPublicKey));
            Debug.Log(string.Format("Account address: {0}", multisigAddress));
            #endregion

            #region Funding Accounts
            Debug.Log("<color=cyan>=== ================ ===</color>");
            Debug.Log("<color=cyan>=== Funding accounts ===</color>");
            Debug.Log("<color=cyan>=== ================ ===</color>");
            int aliceStart = 10000000;
            int bobStart = 20000000;
            int chadStart = 30000000;
            int multisigStart = 40000000;

            // Fund Alice account
            bool success = false;
            ResponseInfo responseInfo = new ResponseInfo();
            Coroutine fundAliceAccountCor = StartCoroutine(faucetClient.FundAccount((_success, _responseInfo) =>
            {
                success = _success;
                responseInfo = _responseInfo;
            }, alice.AccountAddress.ToString(), aliceStart, faucetEndpoint));
            yield return fundAliceAccountCor;

            if (responseInfo.status != ResponseInfo.Status.Success)
            {
                Debug.LogError("Faucet funding for Alice failed: " + responseInfo.message);
                yield break;
            }

            AccountResourceCoin.Coin coin = new AccountResourceCoin.Coin();
            Coroutine getAliceBalanceCor1 = StartCoroutine(RestClient.Instance.GetAccountBalance((_coin, _responseInfo) =>
            {
                coin = _coin;
                responseInfo = _responseInfo;
            }, alice.AccountAddress));
            yield return getAliceBalanceCor1;

            if (responseInfo.status == ResponseInfo.Status.Failed)
            {
                Debug.LogError(responseInfo.message);
                yield break;
            }

            Debug.Log("ALICE BALANCE: " + responseInfo.message);
            Debug.Log(string.Format("Alice's balance: ", coin.Value));

            // Fund Bob account
            success = false;
            responseInfo = new ResponseInfo();
            Coroutine fundBobAccountCor = StartCoroutine(faucetClient.FundAccount((_success, _responseInfo) =>
            {
                success = _success;
                responseInfo = _responseInfo;
            }, bob.AccountAddress.ToString(), bobStart, faucetEndpoint));
            yield return fundBobAccountCor;

            if (responseInfo.status != ResponseInfo.Status.Success)
            {
                Debug.LogError("Faucet funding for Bob failed: " + responseInfo.message);
                yield break;
            }

            coin = new AccountResourceCoin.Coin();
            Coroutine getBobBalanceCor1 = StartCoroutine(RestClient.Instance.GetAccountBalance((_coin, _responseInfo) =>
            {
                coin = _coin;
                responseInfo = _responseInfo;
            }, bob.AccountAddress));
            yield return getBobBalanceCor1;

            if (responseInfo.status == ResponseInfo.Status.Failed)
            {
                Debug.LogError(responseInfo.message);
                yield break;
            }

            Debug.Log("BOB BALANCE: " + responseInfo.message);
            Debug.Log(string.Format("Bob's balance: ", coin.Value));

            // Fund Chad account
            success = false;
            responseInfo = new ResponseInfo();
            Coroutine fundChadAccountCor = StartCoroutine(faucetClient.FundAccount((_success, _responseInfo) =>
            {
                success = _success;
                responseInfo = _responseInfo;
            }, chad.AccountAddress.ToString(), chadStart, faucetEndpoint));
            yield return fundChadAccountCor;

            if (responseInfo.status != ResponseInfo.Status.Success)
            {
                Debug.LogError("Faucet funding for Chad failed: " + responseInfo.message);
                yield break;
            }

            coin = new AccountResourceCoin.Coin();
            Coroutine getChadBalanceCor1 = StartCoroutine(RestClient.Instance.GetAccountBalance((_coin, _responseInfo) =>
            {
                coin = _coin;
                responseInfo = _responseInfo;
            }, chad.AccountAddress));
            yield return getChadBalanceCor1;

            if (responseInfo.status == ResponseInfo.Status.Failed)
            {
                Debug.LogError(responseInfo.message);
                yield break;
            }

            Debug.Log("CHAD BALANCE: " + responseInfo.message);
            Debug.Log(string.Format("Chad's balance: ", coin.Value));

            // Fund Multisig account
            success = false;
            responseInfo = new ResponseInfo();
            Coroutine fundMultisigAccountCor = StartCoroutine(faucetClient.FundAccount((_success, _responseInfo) =>
            {
                success = _success;
                responseInfo = _responseInfo;
            }, multisigAddress.ToString(), multisigStart, faucetEndpoint));
            yield return fundMultisigAccountCor;

            if (responseInfo.status != ResponseInfo.Status.Success)
            {
                Debug.LogError("Faucet funding for Multisig failed: " + responseInfo.message);
                yield break;
            }

            coin = new AccountResourceCoin.Coin();
            Coroutine getMultisigBalanceCor1 = StartCoroutine(RestClient.Instance.GetAccountBalance((_coin, _responseInfo) =>
            {
                coin = _coin;
                responseInfo = _responseInfo;
            }, multisigAddress));
            yield return getMultisigBalanceCor1;

            if (responseInfo.status == ResponseInfo.Status.Failed)
            {
                Debug.LogError(responseInfo.message);
                yield break;
            }

            Debug.Log("MULTISIG BALANCE: " + responseInfo.message);
            Debug.Log(string.Format("Multisig balance: ", coin.Value));
            #endregion

            #region Section 4. Initiate Transfer
            Debug.Log("<color=cyan>=== ================= ===</color>");
            Debug.Log("<color=cyan>=== Initiate Transfer ===</color>");
            Debug.Log("<color=cyan>=== ================= ===</color>");

            // Transaction Arguments
            List<ISerializable> txnArgumentsList = new List<ISerializable>() {
                chad.AccountAddress,
                new U64(100)
            };
            ISerializable[] transactionArguments = txnArgumentsList.ToArray();

            // Entry Function
            EntryFunction entryFunction = EntryFunction.Natural(
                new ModuleId(AccountAddress.FromHex("0x1"), "coin"),
                "transfer",
                new TagSequence(new ISerializableTag[] { new StructTag(AccountAddress.FromHex("0x1"), "aptos_coin", "AptosCoin", new ISerializableTag[0]) }),
                new BCS.Sequence(transactionArguments)
            );

            ulong expirationTimestamp = ((ulong)(DateTime.Now.ToUnixTimestamp() + Constants.EXPIRATION_TTL));

            RawTransaction rawTransaction = new RawTransaction(
                multisigAddress,
                0,
                new BCS.TransactionPayload(entryFunction),
                Constants.MAX_GAS_AMOUNT,
                Constants.GAS_UNIT_PRICE,
                expirationTimestamp,
                restClient.ChainId
            );

            Debug.Log("RAW TXN: " + rawTransaction.ToString());

            Signature aliceSignature = alice.Sign(rawTransaction.Keyed());
            Signature bobSignature = bob.Sign(rawTransaction.Keyed());

            Assert.IsTrue(rawTransaction.Verify(alice.PublicKey, aliceSignature));
            Assert.IsTrue(rawTransaction.Verify(bob.PublicKey, bobSignature));

            Debug.Log("<color=cyan>=== ================= ===</color>");
            Debug.Log("<color=cyan>=== Individual signatures ===</color>");
            Debug.Log("<color=cyan>=== ================= ===</color>");

            Debug.Log(string.Format("Alice: {0}", aliceSignature));
            Debug.Log(string.Format("Bob: {0}", bobSignature));
            #endregion

            #region Section 5: Submit transfer transaction
            List<Tuple<PublicKey, Signature>> SignatureMap = new List<Tuple<PublicKey, Signature>>()
            {
                Tuple.Create(alice.PublicKey, aliceSignature),
                Tuple.Create(bob.PublicKey, bobSignature)
            };
            MultiSignature multisigSignature = new MultiSignature(multisigPublicKey, SignatureMap);

            Authenticator authenticator = new Authenticator(
                new MultiEd25519Authenticator(multisigPublicKey, multisigSignature)
            );

            SignedTransaction signedTransaction = new SignedTransaction(
                rawTransaction, authenticator
            );
            Debug.Log("SIGNED TXN: " + signedTransaction.ToString());

            Debug.Log("<color=cyan>=== =============================== ===</color>");
            Debug.Log("<color=cyan>=== Submitting transfer transaction ===</color>");
            Debug.Log("<color=cyan>=== =============================== ===</color>");
            string submitBcsTxnJsonResponse = "";
            responseInfo = new ResponseInfo();

            Coroutine submitBcsTransactionCor = StartCoroutine(
                restClient.SubmitBCSTransaction(
                    (_responseJson, _responseInfo) => {
                        submitBcsTxnJsonResponse = _responseJson;
                        responseInfo = _responseInfo;
                    },
                    signedTransaction
                )
            );
            yield return submitBcsTransactionCor;

            Debug.Log("SUBMIT BCS TXN: " + submitBcsTxnJsonResponse);

            #endregion

            #region Section 6: New Account Balances
            Debug.Log("<color=cyan>=== ==================== ===</color>");
            Debug.Log("<color=cyan>=== New Account Balances ===</color>");
            Debug.Log("<color=cyan>=== ==================== ===</color>");

            // Get account balance for Alice`
            coin = new AccountResourceCoin.Coin();
            getAliceBalanceCor1 = StartCoroutine(RestClient.Instance.GetAccountBalance((_coin, _responseInfo) =>
            {
                coin = _coin;
                responseInfo = _responseInfo;
            }, alice.AccountAddress));
            yield return getAliceBalanceCor1;

            if (responseInfo.status == ResponseInfo.Status.Failed)
            {
                Debug.LogError(responseInfo.message);
                yield break;
            }

            Debug.Log("ALICE BALANCE: " + responseInfo.message);
            Debug.Log(string.Format("Alice's balance: ", coin.Value));

            // Get account balance for Bob
            coin = new AccountResourceCoin.Coin();
            getBobBalanceCor1 = StartCoroutine(RestClient.Instance.GetAccountBalance((_coin, _responseInfo) =>
            {
                coin = _coin;
                responseInfo = _responseInfo;
            }, bob.AccountAddress));
            yield return getBobBalanceCor1;

            if (responseInfo.status == ResponseInfo.Status.Failed)
            {
                Debug.LogError(responseInfo.message);
                yield break;
            }

            Debug.Log("BOB BALANCE: " + responseInfo.message);
            Debug.Log(string.Format("Bob's balance: ", coin.Value));

            // Get account balance for Chad
            coin = new AccountResourceCoin.Coin();
            getChadBalanceCor1 = StartCoroutine(RestClient.Instance.GetAccountBalance((_coin, _responseInfo) =>
            {
                coin = _coin;
                responseInfo = _responseInfo;
            }, chad.AccountAddress));
            yield return getChadBalanceCor1;

            if (responseInfo.status == ResponseInfo.Status.Failed)
            {
                Debug.LogError(responseInfo.message);
                yield break;
            }

            Debug.Log("CHAD BALANCE: " + responseInfo.message);
            Debug.Log(string.Format("Chad's balance: ", coin.Value));

            // Get account balance for Multisig
            coin = new AccountResourceCoin.Coin();
            getMultisigBalanceCor1 = StartCoroutine(RestClient.Instance.GetAccountBalance((_coin, _responseInfo) =>
            {
                coin = _coin;
                responseInfo = _responseInfo;
            }, multisigAddress));
            yield return getMultisigBalanceCor1;

            if (responseInfo.status == ResponseInfo.Status.Failed)
            {
                Debug.LogError(responseInfo.message);
                yield break;
            }

            Debug.Log("MULTISIG BALANCE: " + responseInfo.message);
            Debug.Log(string.Format("Multisig balance: ", coin.Value));
            #endregion

            #region Section 7: Funding Vanity Address
            Debug.Log("<color=cyan>=== ====================== ===</color>");
            Debug.Log("<color=cyan>=== Funding vanity address ===</color>");
            Debug.Log("<color=cyan>=== ====================== ===</color>");

            Account deedee = Account.Generate();

            while (deedee.AccountAddress.ToString()[2..4].Equals("dd"))
                deedee = Account.Generate();

            Debug.Log(string.Format("Deedee's address: {0}", deedee.AccountAddress));
            Debug.Log(string.Format("Deedee's public key: {0}", deedee.PublicKey));

            int deedeeStart = 50000000;

            // Fund Deedee account
            success = false;
            responseInfo = new ResponseInfo();
            Coroutine fundDeedeeAccountCor = StartCoroutine(faucetClient.FundAccount((_success, _responseInfo) =>
            {
                success = _success;
                responseInfo = _responseInfo;
            }, alice.AccountAddress.ToString(), deedeeStart, faucetEndpoint));
            yield return fundAliceAccountCor;

            if (responseInfo.status != ResponseInfo.Status.Success)
            {
                Debug.LogError("Faucet funding for Deedee failed: " + responseInfo.message);
                yield break;
            }

            coin = new AccountResourceCoin.Coin();
            Coroutine getDeedeeBalanceCor1 = StartCoroutine(RestClient.Instance.GetAccountBalance((_coin, _responseInfo) =>
            {
                coin = _coin;
                responseInfo = _responseInfo;
            }, alice.AccountAddress));
            yield return getAliceBalanceCor1;

            if (responseInfo.status == ResponseInfo.Status.Failed)
            {
                Debug.LogError(responseInfo.message);
                yield break;
            }

            Debug.Log("DEEDEE BALANCE: " + responseInfo.message);
            Debug.Log(string.Format("Deedee's balance: ", coin.Value));
            #endregion
            yield return null;
        }
    }
}

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Aptos.Unity.Rest
{
    /// <summary>
    /// Faucet Client for claiming APT from Devnet
    /// NOTE: Does not work on Testnet. Testnet only supports airdrops through an authenticated URL
    /// </summary>
    public class FaucetClient: MonoBehaviour
    {
        public static FaucetClient Instance { get; set; }

        private void Awake()
        {
            Instance = this;
        }

        /// <summary>
        /// Funds a Testnet Account
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="address"></param>
        /// <param name="amount"></param>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        public IEnumerator FundAccount(
            Action<string> callback, string address, int amount, string endpoint)
        {
            // TODO: Create Constant Variables in a separate file instead of hardcoding
            string faucetURL = endpoint + "/mint?amount=" + amount + "&Address=" + address;
            Debug.Log("FAUCET: " + faucetURL);
            Uri transactionsURI = new Uri(faucetURL);
            var request = new UnityWebRequest(transactionsURI, "POST");
            request.SetRequestHeader("Content-Type", "application/json");

            request.SendWebRequest();
            while (!request.isDone)
            {
                yield return null;
            }

            // TODO: Add proper callback return object that holds error code
            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log("Error While Submitting Transaction: " + request.error);
                callback(request.error);
            }
            else if (request.responseCode == 404)
            {
                // TODO: Implememt Error Code deserializer for Faucet Client
                callback("??????????????");
            }
            else if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(request.error);
            }
            else
            {
                Debug.Log("Account Funded!!");
            }

            request.Dispose();

            yield return null;
        }
    }
}
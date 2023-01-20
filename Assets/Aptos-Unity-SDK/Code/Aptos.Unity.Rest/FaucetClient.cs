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
        public IEnumerator FundAccount(Action<string> callback, string address, int amount, string endpoint)
        {
            string faucetURL = endpoint + "/mint?amount=" + amount + "&address=" + address;
            Uri transactionsURI = new Uri(faucetURL);
            var request = new UnityWebRequest(transactionsURI, "POST");
            request.SetRequestHeader("Content-Type", "application/json");

            request.SendWebRequest();
            while (!request.isDone)
            {
                yield return null;
            }

            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log("Error While Submitting Transaction: " + request.error);
                callback(request.error);
            }
            else if (request.responseCode == 404)
            {
                // TODO: Implememt Error Code deserializer for Faucet Client
                callback(request.error);
            }
            else if (request.responseCode == 400)
            {
                Debug.Log(request.error);
            }
            else
            {
                Debug.Log("Account Funded: " + address);
            }

            request.Dispose();

            // TODO: Inspect wait time until account balance is updated
            yield return new WaitForSeconds(1f);
        }
    }
}
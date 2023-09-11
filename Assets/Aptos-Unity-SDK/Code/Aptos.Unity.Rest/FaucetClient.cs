using Aptos.Unity.Rest.Model;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Aptos.Unity.Rest
{
    /// <summary>
    /// Faucet Client for claiming APT from Devnet.
    /// To claim APT from Testnet you can visit Aptos Testnet Airdrop site.
    /// </summary>
    public class FaucetClient : MonoBehaviour
    {
        public static FaucetClient Instance { get; set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
                Destroy(this);
            else
                Instance = this;
        }

        /// <summary>
        /// Funds a Testnet Account
        /// </summary>
        /// <param name="callback">Callback function used when response is received.</param>
        /// <param name="address">Address that will get funded.</param>
        /// <param name="amount">Amount of APT requested.</param>
        /// <param name="endpoint">Base URL for faucet.</param>
        /// <returns>Calls <c>callback</c> function with <c>(bool, ResponsiveInfo)</c>: \n
        /// A boolean stating that the request for funding was successful, and an object containg the response details</returns>
        public IEnumerator FundAccount(Action<bool, ResponseInfo> callback, string address, int amount, string endpoint)
        {
            string faucetURL = endpoint + "/mint?amount=" + amount + "&address=" + address;
            Uri transactionsURI = new(faucetURL);

            UnityWebRequest request = RequestClient.SubmitRequest(transactionsURI, UnityWebRequest.kHttpVerbPOST);
            request.SetRequestHeader("Content-Type", "application/json");

            ResponseInfo responseInfo = new();

            request.SendWebRequest();
            while (!request.isDone) yield return null;

            if (request.result != UnityWebRequest.Result.ConnectionError)
            {
                if (request.responseCode == 404)
                {
                    responseInfo.status = ResponseInfo.Status.NotFound;
                    responseInfo.message = request.error;
                    callback(false, responseInfo);
                }
                else if (request.responseCode >= 400)
                {
                    responseInfo.status = ResponseInfo.Status.Failed;
                    responseInfo.message = request.error;
                    callback(false, responseInfo);
                }
                else
                {
                    responseInfo.status = ResponseInfo.Status.Success;
                    responseInfo.message = "Funding succeeded!";
                    yield return new WaitForSeconds(2f);
                    callback(true, responseInfo);
                }
            }
            else
            {
                responseInfo.status = ResponseInfo.Status.Failed;
                responseInfo.message = request.error;
                callback(false, responseInfo);
            }

            request.Dispose();
            yield return new WaitForSeconds(1f);
        }
    }
}
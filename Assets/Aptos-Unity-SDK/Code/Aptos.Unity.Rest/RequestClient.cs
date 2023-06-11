using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Aptos.Unity.Rest {

    /// <summary>
    /// UnityWebRequest wrapper client
    /// </summary>
    public class RequestClient : MonoBehaviour {

        private Uri baseUri;

        public static string X_APTOS_HEADER = "x-aptos-client";

        /// <summary>
        /// Request Client for sending requests to Aptos mainnet, testnet, devnet or local/custom
        /// </summary>
        /// <param name="baseUri">Base URL for the network you want to connect to, e.g: Constants.MAINNET_BASE_URL </param>
        public RequestClient(string baseUri) {
            this.baseUri = new Uri(baseUri);
        }

        /// <summary>
        /// Get the default Aptos header value
        /// </summary>
        /// <returns>String with the default Aptos header value</returns>
        public static string Get_APTOS_HEADER_VALUE() {
            return "aptos-unity-sdk/" + Application.version;
        }

        /// <summary>
        /// Get the UnityWebRequest object for the given path, default to GET method
        /// </summary>
        /// <param name="path">Path to the endpoint</param>
        /// <param name="method">HTTP method</param>
        /// <returns>UnityWebRequest object</returns>
        public UnityWebRequest GetRequest(string path, string method = UnityWebRequest.kHttpVerbGET) {
            Uri uri = new Uri(baseUri, path);
            var request = new UnityWebRequest(uri, method);

            // Set the default Aptos header
            request.SetRequestHeader(X_APTOS_HEADER, Get_APTOS_HEADER_VALUE());
            return request;
        }
    }
}
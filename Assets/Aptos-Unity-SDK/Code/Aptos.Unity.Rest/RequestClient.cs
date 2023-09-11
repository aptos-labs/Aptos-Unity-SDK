using System;
using UnityEngine;
using UnityEngine.Networking;

namespace Aptos.Unity.Rest
{

    /// <summary>
    /// UnityWebRequest wrapper client
    /// </summary>
    public class RequestClient
    {

        public static string X_APTOS_HEADER = "x-aptos-client";

        /// <summary>
        /// Get the default Aptos header value
        /// </summary>
        /// <returns>String with the default Aptos header value</returns>
        public static string GetAptosHeaderValue()
        {
            return "aptos-unity-sdk/" + Application.version;
        }

        /// <summary>
        /// Get the UnityWebRequest object for the given path, default to GET method
        /// </summary>
        /// <param name="uri">endpoint uri</param>
        /// <param name="method">HTTP method</param>
        /// <returns>UnityWebRequest object</returns>
        public static UnityWebRequest SubmitRequest(Uri uri, string method = UnityWebRequest.kHttpVerbGET)
        {
            var request = new UnityWebRequest();

            if (method == UnityWebRequest.kHttpVerbGET)
            {
                // We are using UnityWebRequest.Get for GET requests because we need the default downloadhandler
                // Using 'new UnityWebRequest' for Get does not set it
                request = UnityWebRequest.Get(uri);
            }
            else
            {
                request = new UnityWebRequest(uri, method);
            }

            // Set the default Aptos header
            request.SetRequestHeader(X_APTOS_HEADER, GetAptosHeaderValue());
            return request;
        }
    }
}
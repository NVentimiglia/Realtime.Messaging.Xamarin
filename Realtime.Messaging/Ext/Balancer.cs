using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

using System.Net.Http;
using System.Threading.Tasks;
using ModernHttpClient;

namespace RealtimeFramework.Messaging.Ext
{
    /// <summary>
    /// Callback delegate type raised after resolving a cluster server from balancer
    /// </summary>
    /// <param name="server">The server.</param>
    /// <param name="ex">The exception</param>
    /// <remarks></remarks>
    public delegate void OnBalancerUrlResolvedDelegate(string server, Exception ex);

    /// <summary>
    /// Callback delegate type raised after resolving a cluster server from balancer
    /// </summary>
    /// <param name="ex">The exception</param>
    /// <param name="server">The server</param>
    public delegate void OnGetServerUrlDelegate(Exception ex, String server);

    /// <summary>
    /// A static class containing all the methods to communicate with the Ortc Balancer 
    /// </summary>
    /// <example>
    /// <code>
    /// String url = "http://ortc-developers.realtime.co/server/2.1/"";
    /// Balancer.GetServerFromBalancer(url, applicationKey, (server) =>
    ///		{
    ///			//Do something with the returned server      
    ///		});
    /// </code>
    /// </example>
    /// <remarks></remarks>
    public static class Balancer
    {
        #region Fields (1)

        private const String BALANCER_SERVER_PATTERN = "^var SOCKET_SERVER = \"(?<server>http.*)\";$";

        #endregion Fields

        #region Methods (2)
        // Public Methods (1) 

        /// <summary>
        /// Retrieves an Ortc Server url from the Ortc Balancer
        /// </summary>
        /// <param name="balancerUrl">The Ortc Balancer url.</param>
        /// <param name="applicationKey">The application key.</param>
        /// <param name="onClusterUrlResolved">Callback that is raised after an Ortc server url have been retrieved from the Ortc balancer.</param>
        /// <example>
        /// <code>
        /// String url = "http://ortc-developers.realtime.co/server/2.1/";
        /// Balancer.GetServerFromBalancer(url, applicationKey, (server) =>
        ///		{
        ///			//Do something with the returned server      
        ///		});
        /// </code>
        /// </example>
        /// <remarks></remarks>
        public async static void GetServerFromBalancerAsync(String balancerUrl, String applicationKey, OnBalancerUrlResolvedDelegate onClusterUrlResolved)
        {
            var parsedUrl = String.IsNullOrEmpty(applicationKey) ? balancerUrl : balancerUrl + "?appkey=" + applicationKey;
            var server = String.Empty;

            try
            {
                using (var client = new HttpClient(new NativeMessageHandler()))
                {
                    var response = await client.GetStreamAsync(new Uri(parsedUrl));
                    StreamReader streamReader = new StreamReader(response);

                    server = ParseBalancerResponse(streamReader);

                    if (onClusterUrlResolved != null)
                    {
                        onClusterUrlResolved(server, null);
                    }

                }
            }
            catch (Exception ex)
            {
                onClusterUrlResolved(server, ex);
            }
        }
        // Private Methods (1) 

        private static String ParseBalancerResponse(StreamReader response)
        {
            var responseBody = response.ReadToEnd();

            String server = "";

            var match = Regex.Match(responseBody, BALANCER_SERVER_PATTERN);

            if (match.Success)
            {
                server = match.Groups["server"].Value;
            }

            return server;
        }


        internal static void GetServerUrl(String url, bool isCluster, String applicationKey, OnGetServerUrlDelegate callback)
        {
            if (!String.IsNullOrEmpty(url) && isCluster)
            {
                GetServerFromBalancerAsync(url, applicationKey, (server, error) =>
                {
                    callback(error, server);
                });
            }
            else
            {
                callback(null, url);
            }
        }

        internal static String ResolveClusterUrl(String clusterUrl) {
            String server = "";
            try { 
                using (var client = new HttpClient(new NativeMessageHandler())) {
                    var response = client.GetAsync(clusterUrl).Result;

                    if (response.IsSuccessStatusCode) {
                        var responseContent = response.Content;
                        string responseString = responseContent.ReadAsStringAsync().Result;

                    

                        var match = Regex.Match(responseString, BALANCER_SERVER_PATTERN);

                        if (match.Success) {
                            server = match.Groups["server"].Value;
                            return server;
                        }
                    }
                }
            } catch {
                server = "";
            }
            return server;
        }        

        internal async static Task<String> ResolveClusterUrlAsync(String clusterUrl) {
            String server = "";
            try {
                HttpClient aClient = new HttpClient(new NativeMessageHandler());
				aClient.Timeout = new TimeSpan(0,0,Constants.HTTPCLIENT_TIMEOUT);
                Uri requestUri = new Uri(clusterUrl);

                var result = await aClient.GetAsync(requestUri, HttpCompletionOption.ResponseContentRead);
                var responseBody = await result.Content.ReadAsStringAsync();

                if (result.IsSuccessStatusCode) {
                    var match = Regex.Match(responseBody, BALANCER_SERVER_PATTERN);

                    if (match.Success) {
                        server = match.Groups["server"].Value;
                        return server;
                    }
                } 
            } catch(Exception ex) {
                Debug.WriteLine(ex.Message);
                server = "";
            }
            return server;
        }

        #endregion Methods
    }

}

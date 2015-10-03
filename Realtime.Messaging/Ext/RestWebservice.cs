using System;
using System.Net.Http;
using ModernHttpClient;
using RealtimeFramework.Messaging.Exceptions;

namespace RealtimeFramework.Messaging.Ext
{
    internal delegate void OnResponseDelegate(OrtcPresenceException ex, String result);

    internal static class RestWebservice
    {

        internal async static void DoRest(String url, String body, OnResponseDelegate callback) {
            try {
                HttpContent content = new StringContent(body);
                HttpClientHandler aHandler = new NativeMessageHandler();
                aHandler.ClientCertificateOptions = ClientCertificateOption.Automatic;
                HttpClient aClient = new HttpClient(aHandler);
                Uri requestUri = new Uri(url);
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUri);
                
                var result = await aClient.PostAsync(requestUri, content);
                var responseHeader = result.Headers;
                var responseBody = await result.Content.ReadAsStringAsync();
                if (result.IsSuccessStatusCode) {
                    callback(null, responseBody);
                } else {
                    callback(new OrtcPresenceException(responseBody), null);
                }
                
            } catch (Exception e) {
                callback(new OrtcPresenceException(e.Message), null);
            }
        }

        internal async static void DoRestGet(String url, OnResponseDelegate callback) {
            OrtcPresenceException cbError = null;
            String cbSuccess = null;
            try {
                HttpClientHandler aHandler = new NativeMessageHandler();
                aHandler.ClientCertificateOptions = ClientCertificateOption.Automatic;
                HttpClient aClient = new HttpClient(aHandler);
                Uri requestUri = new Uri(url);
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);

                var result = await aClient.GetAsync(requestUri, HttpCompletionOption.ResponseContentRead);
                var responseHeader = result.Headers;
                var responseBody = await result.Content.ReadAsStringAsync();
                
                if (result.IsSuccessStatusCode) {
                    cbSuccess = responseBody;
                } else {
                    cbError = new OrtcPresenceException(responseBody);
                }
            } catch (Exception e) {
                cbError = new OrtcPresenceException(e.Message);
            }
            callback(cbError, cbSuccess);
        }

        /*
        internal static void GetAsync(String url, OnResponseDelegate callback)
        {
            RestWebservice.RequestAsync(url, "GET",null, callback);
        }*/
        /*
        internal static void PostAsync(String url,String content, OnResponseDelegate callback)
        {
            RestWebservice.RequestAsync(url, "POST",content, callback);
        }
        */
        private static void RequestAsync(String url, String method,String content, OnResponseDelegate callback)
        {


            /*
            var request = (HttpWebRequest)WebRequest.Create(new Uri(url));

            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;

            request.Proxy = null;
            //request.Timeout = 10000;
            //request.ProtocolVersion = HttpVersion.Version11;
            request.Method = method;

            if (String.Compare(method,"POST") == 0 && !String.IsNullOrEmpty(content)) 
            {
                byte[] postBytes = Encoding.UTF8.GetBytes(content);

                request.ContentType = "application/x-www-form-urlencoded";
                //request.ContentLength = postBytes.Length;
               
                using (Stream requestStream = request.GetRequestStream())
                {
                    requestStream.Write(postBytes, 0, postBytes.Length);
                    requestStream.Close();
                }
            }

            request.BeginGetResponse(new AsyncCallback((asynchronousResult) =>
            {
                var server = String.Empty;

                var synchContext = System.Threading.SynchronizationContext.Current;

                try
                {
                    HttpWebRequest asyncRequest = (HttpWebRequest)asynchronousResult.AsyncState;

                    HttpWebResponse response = (HttpWebResponse)asyncRequest.EndGetResponse(asynchronousResult);
                    Stream streamResponse = response.GetResponseStream();
                    StreamReader streamReader = new StreamReader(streamResponse);

                    var responseBody = streamReader.ReadToEnd();

                    if (callback != null)
                    {
                        if (!String.IsNullOrEmpty(HttpRuntime.AppDomainAppVirtualPath))
                        {
                            callback(null, responseBody);
                        }
                        else
                        {
                            if (synchContext != null)
                            {
                                synchContext.Post(obj => callback(null, responseBody), null);
                            }
                            else
                            {
                                Task.Factory.StartNew(() => callback(null, responseBody));
                            }
                        }                        
                    }
                }
                catch (WebException wex)
                {
                    String errorMessage = String.Empty;
                    if (wex.Response == null)
                    {
                        errorMessage = "Uknown request error";
                        if (!String.IsNullOrEmpty(HttpRuntime.AppDomainAppVirtualPath))
                        {
                            callback(new OrtcPresenceException(errorMessage), null);
                        }
                        else
                        {
                            if (synchContext != null)
                            {
                                synchContext.Post(obj => callback(new OrtcPresenceException(errorMessage), null), null);
                            }
                            else
                            {
                                Task.Factory.StartNew(() => callback(new OrtcPresenceException(errorMessage), null));
                            }
                        }
                    }
                    else
                    {
                        using (var stream = wex.Response.GetResponseStream())
                        {
                            using (var reader = new StreamReader(stream))
                            {
                                errorMessage = reader.ReadToEnd();
                            }

                            if (!String.IsNullOrEmpty(HttpRuntime.AppDomainAppVirtualPath))
                            {
                                callback(new OrtcPresenceException(errorMessage), null);
                            }
                            else
                            {
                                if (synchContext != null)
                                {
                                    synchContext.Post(obj => callback(new OrtcPresenceException(errorMessage), null), null);
                                }
                                else
                                {
                                    Task.Factory.StartNew(() => callback(new OrtcPresenceException(errorMessage), null));
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (!String.IsNullOrEmpty(HttpRuntime.AppDomainAppVirtualPath))
                    {
                        callback(new OrtcPresenceException(ex.Message), null);
                    }
                    else
                    {

                        if (synchContext != null)
                        {
                            synchContext.Post(obj => callback(new OrtcPresenceException(ex.Message), null), null);
                        }
                        else
                        {
                            Task.Factory.StartNew(() => callback(new OrtcPresenceException(ex.Message), null));
                        }
                    }
                }
            }), request);
             */
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using RestServer.Helper;
using RestServer.NetCoreServer;
using UnityEngine;

namespace RestServer {
    /// <summary>
    /// Describes the current request.
    /// </summary>
    public struct RestRequest {
        #region Private Variables

        private readonly Logger _logger;

        #endregion

        #region Public Variables

        /// <summary>
        /// Reference to the endpoint definition that describes this request, can be null if no endpoint definition is found.
        /// </summary>
        public Endpoint? Endpoint;

        /// <summary>
        /// Internal reference to the low level http session
        /// </summary>
        public readonly LowLevelSession Session;

        /// <summary>
        /// Reference to the underlying http request object,
        /// </summary>
        public readonly HttpRequest HttpRequest;

        /// <summary>
        /// Reference to the underlying http response object to craft responses yourself. For maximum compatibility, try to use the send methods in the RestRequest struct.
        /// </summary>
        public readonly HttpResponse HttpResponse;

        /// <summary>
        /// Calling Query Parameters
        /// </summary>
        public readonly NameValueCollection QueryParameters;

        /// <summary>
        /// Calling Query Parameters as easy to handle, read-only dictionary
        /// </summary>
        public readonly IDictionary<string, IList<string>> QueryParametersDict;

        /// <summary>
        /// Dictionary of parsed path parameters. Only available if path parameters have been used in the endpoint.
        /// For example "/path/{param1}/path2/{param2}" will result in a dictionary with two entries.
        /// </summary>
        public readonly IDictionary<string, PathParamValue> PathParams;

        /// <summary>
        /// Request Url(i) that has been called. This is allows for easier url parsing.
        /// </summary>
        /// <remarks>Note that the host part is always 'localhost' regardless which endpoint the caller has used.</remarks>
        public readonly Uri RequestUri;

        /// <summary>
        /// Holds information about what response has been sent. Used for debug logging.
        /// </summary>
        internal RestRequestResponseLog ResponseLog { get; }

        /// <summary>String contents of the request body.</summary>
        public string Body => HttpRequest.Body;

        /// <summary>Byte content of the request body.</summary>
        public byte[] BodyBytes => HttpRequest.BodyBytes;

        /// <summary>
        /// Read only dictionary of the http request's headers. Copy to non-read-only dictionary with <see cref="HeaderBuilder.DeepClone"/> in class <see cref="HeaderBuilder"/>.
        /// </summary>
        public IDictionary<string, IList<string>> Headers => RequestHeaderHelper.ToReadOnlyHeaderDict(HttpRequest);

        /// <summary>
        /// Helper used to handle internal redirects.
        /// </summary>
        public readonly HttpRequestRedirectHelper RedirectHelper;

        #endregion

        public RestRequest(LowLevelSession session,
            HttpRequest httpRequest,
            HttpResponse httpResponse,
            Uri requestUri,
            bool debugLog,
            RestRequestResponseLog responseLog,
            HttpRequestRedirectHelper redirectHelper,
            Dictionary<string, PathParamValue> pathParams) {
            HttpRequest = httpRequest;
            Session = session;
            HttpResponse = httpResponse;
            RequestUri = requestUri;
            Endpoint = null;

            var queryParameters = HttpUtility.ParseAndFilter(requestUri.Query);
            QueryParameters = queryParameters;

            _logger = new Logger(Debug.unityLogger.logHandler);
            _logger.logEnabled = debugLog;

            ResponseLog = responseLog;

            var tempQP = new Dictionary<string, IList<string>>();
            foreach (var key in queryParameters.AllKeys) {
                var values = queryParameters.GetValues(key);
                if (values != null)
                    tempQP.Add(key, new ReadOnlyCollection<string>(values));
            }

            QueryParametersDict = new ReadOnlyDictionary<string, IList<string>>(tempQP);
            RedirectHelper = redirectHelper;
            if (pathParams == null) {
                PathParams = new ReadOnlyDictionary<string, PathParamValue>(new Dictionary<string, PathParamValue>());
            } else {
                PathParams = new ReadOnlyDictionary<string, PathParamValue>(pathParams);                
            }
        }

        #region Response Method

        /// <summary>
        /// Response builder (new). Use this method to create a response to the call.
        /// </summary>
        public ResponseBuilder CreateResponse() {
            return new ResponseBuilder(this, _logger);
        }

        #endregion

        #region Profiling

        internal bool SendResponseAsync(HttpResponse response) {
#if ENABLE_PROFILER && RESTSERVER_PROFILING_CORE
            RestServerProfilerCounters.OutgoingBytesCount.Value += response.BodyLength;
#endif

            var ret = Session.SendResponseAsync(response);
            ResponseLog.MarkSent(response.Status);
            return ret;
        }

        #endregion

        #region Internal Redirection

        /// <summary>
        /// After the this endpoint method has finished, redirect the endpoint to another endpoint. This is useful for 404 redirects, for example.
        /// The redirected endpoint will receive all information from the original request.
        /// </summary>
        /// <remarks>
        /// Make sure that there is only one response sent back to the caller.
        /// </remarks>
        /// <param name="redirectPath">Absolute path to redirect to</param>
        /// <param name="ignoreTag">Endpoint.Tag to ignore when trying to find the next endpoint to redirect to</param>
        public void ScheduleInternalRedirect(string redirectPath, object ignoreTag = null) {
            if (RedirectHelper == null) {
                throw new ArgumentException("Internal redirection is not supported on this request.");
            }

            RedirectHelper.InternalRedirectPath = redirectPath;
            RedirectHelper.IgnoreTag = ignoreTag;
        }

        /// <summary>
        /// Clears the information that this request should be redirected. Called automatically by the rest server library. 
        /// </summary>
        public void ClearInternalRedirect() {
            if (RedirectHelper == null) {
                throw new ArgumentException("Internal redirection is not supported on this request.");
            }

            RedirectHelper.InternalRedirectPath = null;
            RedirectHelper.IgnoreTag = null;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Helper function to parse the request body to json.
        /// </summary>
        public T JsonBody<T>() {
            return JsonUtility.FromJson<T>(HttpRequest.Body);
        }

        #endregion

        #region Internal Debugging

        private void DebugLogSendAsync(string method, int status, string message = "") {
            if (!_logger.logEnabled) {
                return;
            }

            // var frame = (new System.Diagnostics.StackTrace()).GetFrame(3);
            // var type = frame.GetMethod().DeclaringType;
            // var caller = frame.GetMethod().Name;
            _logger.Log($"{method} with {status}.");
        }

        #endregion
    }
}
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace RestServer.Helper {
    
    /// <summary>
    /// Mostly used behaviour for examples to provide a debug view in game.
    /// </summary>
    public class RestServerDebugBehaviour : MonoBehaviour {
        private List<string> _log = new List<string>();
        private bool _newLogReceived = true;
        private bool _firstUpdate = true;

        public float widthPercent = 0.25f;

        public bool forceDebugLogging = true;

        [Header("References")]
        public RestServer restServer;

        [FormerlySerializedAs("scrollViewTransform")]
        [Header("Scroll View Info")]
        public RectTransform infoViewTransform;

        public ScrollRect infoViewScrollRect;
        public Text debugText;

        [Header("Scroll View Controls")]
        public RectTransform controlViewTransform;

        public Button startStopButton;
        public Button listenToAllInterfacesButton;

        [Header("Scroll View Logs")]
        public RectTransform logViewTransform;

        public ScrollRect logViewScrollRect;
        public Text logText;

        public void Start() {
            if (restServer == null) {
#if UNITY_2020_3_OR_NEWER && (UNITY_2020_3_10 || UNITY_2020_3_11 || UNITY_2020_3_12 || UNITY_2020_3_13 || UNITY_2020_3_14 || UNITY_2020_3_15 || UNITY_2020_3_16 || UNITY_2020_3_17 || UNITY_2020_3_18 || UNITY_2020_3_19 || UNITY_2020_3_20 || UNITY_2020_3_21 || UNITY_2020_3_22 || UNITY_2020_3_23 || UNITY_2020_3_24 || UNITY_2020_3_25 || UNITY_2020_3_26 || UNITY_2020_3_27 || UNITY_2020_3_28 || UNITY_2020_3_29 || UNITY_2020_3_30 || UNITY_2020_3_31 || UNITY_2020_3_32 || UNITY_2020_3_33 || UNITY_2020_3_34 || UNITY_2020_3_35 || UNITY_2020_3_36 || UNITY_2020_3_37 || UNITY_2020_3_38 || UNITY_2020_3_39 || UNITY_2020_3_40 || UNITY_2020_3_41 || UNITY_2020_3_42 || UNITY_2020_3_43 || UNITY_2020_3_44)
                restServer = FindObjectOfType<RestServer>();
#else
                restServer = FindFirstObjectByType<RestServer>();
#endif
            }

            if (forceDebugLogging) {
                restServer.DebugLog = true;
                restServer.SpecialHandlers.AccessLog = DefaultRequestHandlerImpl.DebugLogAccessLog;   
            }
        }

        public void Update() {
            UpdateUILayout();

            if (_firstUpdate) {
                infoViewScrollRect.normalizedPosition = new Vector2(1, 1);
                logViewScrollRect.normalizedPosition = new Vector2(1, 1);

                _firstUpdate = false;
            }

            // Update IF
            var t = "Started: " + restServer.IsStarted + "\n";
            if (restServer.IsStarted && restServer.EndpointCollection != null) {
                t += "Listening on: " + restServer.Server.Endpoint + "\n";
                t += "Possible IPs:\n";
                foreach (var ipInfo in NetworkHelper.GetPossibleListenIPs(restServer)) {
                    t += "  " + ipInfo + "\n";
                }

                t += "\n";
                t += "Endpoints:\n";
                foreach (var type in new[] { HttpMethod.GET, HttpMethod.POST, HttpMethod.PUT, HttpMethod.DELETE }) {
                    t += $" {type}:\n";
                    var endpoints = restServer.EndpointCollection.GetAllEndpoints(type);
                    if (endpoints == null)
                        continue;
                    foreach (var endpoint in endpoints) {
                        if (endpoint.EndpointRegex != null) {
                            t += "   - Regex: " + endpoint.EndpointRegex + "\n";
                        }
                        else if (endpoint.WebSocketUpgradeAllowed) {
                            t += "   - WebSocket: " + endpoint.EndpointString + "\n";
                        }
                        else {
                            t += "   - " + endpoint.EndpointString + "\n";
                        }
                    }
                }
            }

            debugText.text = t;
            if (_newLogReceived) {
                logText.text = string.Join("\n", _log.ToArray().Reverse().ToArray());
            }
        }

        private void UpdateUILayout() {
            var width = Screen.width * widthPercent;
            var height01 = Screen.height * 0.15f;
            var height02 = Screen.height * 0.30f;
            var height03 = Screen.height - height01 - height02;
            controlViewTransform.sizeDelta = new Vector2(width, height01);
            controlViewTransform.anchoredPosition = new Vector2(0, 0);

            infoViewTransform.sizeDelta = new Vector2(width, height02);
            infoViewTransform.anchoredPosition = new Vector2(0, -height01);

            logViewTransform.sizeDelta = new Vector2(width, height03);
            logViewTransform.anchoredPosition = new Vector2(0, -height01 - height02);
        }

        public void ActionStartStopButton() {
            if (restServer.IsStarted) {
                restServer.StopServer();
            }
            else {
                restServer.StartServer();
            }
        }

        public void ActionListenToAllInterfacesButton() {
            if (restServer.IsStarted) {
                restServer.StopServer();
            }

            restServer.ListenAddressUnity = ListenAddressUnity.Any;
            restServer.StartServer();
        }

        void OnEnable() {
            Application.logMessageReceived += HandleLog;
        }

        void OnDisable() {
            Application.logMessageReceived -= HandleLog;
        }

        void HandleLog(string logString, string stackTrace, LogType type) {
            var l = $"{System.DateTime.Now} {type}: {logString}";
            if (_log.Count > 100) {
                _log.RemoveAt(0);
            }

            _log.Add(l);
            _newLogReceived = true;
        }
    }
}
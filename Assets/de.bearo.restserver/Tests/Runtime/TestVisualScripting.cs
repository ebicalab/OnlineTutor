#if RESTSERVER_VISUALSCRIPTING
using System.Collections;
using System.Linq;
using System.Net;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace de.bearo.restserver.Tests.Runtime {
    public class TestVisualScripting {
        [UnityTest]
        public IEnumerator Test_Get_Path() {
            SceneManager.LoadScene("VisualScriptingTestScene", LoadSceneMode.Single);

            yield return new WaitForFixedUpdate();
            
            using var th = new TestHelper(restServer: FindRestServer());
            yield return th.DoStartup();

            yield return th.HttpAsyncGet("/?a=muhmuh");

            var r = th.LastAsyncResponse;
            Assert.AreEqual(HttpStatusCode.OK, r.StatusCode);

            var responseContent = r.Content.ReadAsStringAsync().Result;
            Assert.AreEqual("muhmuh", responseContent);

            Assert.AreEqual("DoNotRemove", r.Headers.GetValues("AddCustom").First());
            Assert.AreEqual("SetHeaderCustom", r.Headers.GetValues("SetHeaderCustom").First());
            Assert.AreEqual("SetHeaderETag", r.Headers.GetValues("ETag").First());
        }

        [UnityTest]
        public IEnumerator Test_Post_Path() {
            SceneManager.LoadScene("VisualScriptingTestScene", LoadSceneMode.Single);

            yield return new WaitForFixedUpdate();
            
            using var th = new TestHelper(restServer: FindRestServer());
            yield return th.DoStartup();

            const string content = "please send this back";
            yield return th.HttpAsyncPost("/", content);

            var r = th.LastAsyncResponse;
            Assert.AreEqual(HttpStatusCode.Created, r.StatusCode);

            var responseContent = r.Content.ReadAsStringAsync().Result;
            Assert.AreEqual(content, responseContent);

            Assert.True(r.Headers.Contains("Host"));
            Assert.AreEqual("localhost:8080", r.Headers.GetValues("Host").First());
        }

        [UnityTest]
        public IEnumerator Test_SimpleParameters_01() {
            SceneManager.LoadScene("VisualScriptingTestScene", LoadSceneMode.Single);

            yield return new WaitForFixedUpdate();
            
            using var th = new TestHelper(restServer: FindRestServer());
            yield return th.DoStartup();

            const string content = "Unused";
            yield return th.HttpAsyncGet($"/simpleParameters_01?a={content}");

            var r = th.LastAsyncResponse;
            Assert.AreEqual(HttpStatusCode.OK, r.StatusCode);

            var responseContent = r.Content.ReadAsStringAsync().Result;
            Assert.AreEqual(content, responseContent);
        }
        
        [UnityTest]
        public IEnumerator Test_TransformAutoEndpoint_01() {
            SceneManager.LoadScene("VisualScriptingTestScene", LoadSceneMode.Single);

            yield return new WaitForFixedUpdate();
            
            using var th = new TestHelper(restServer: FindRestServer());
            yield return th.DoStartup();

            yield return th.HttpAsyncGet("/transformAutoEndpoint");

            var r = th.LastAsyncResponse;
            var responseStr = r.Content.ReadAsStringAsync().Result;
            
            Assert.AreEqual("{\"position\":{\"x\":1.0,\"y\":2.0,\"z\":3.0},\"rotation\":{\"x\":0.0,\"y\":0.0,\"z\":0.0},\"scale\":{\"x\":1.0,\"y\":1.0,\"z\":1.0}}", responseStr);
        }
        
        private RestServer.RestServer FindRestServer() {
#if UNITY_2020_3_OR_NEWER && (UNITY_2020_3_10 || UNITY_2020_3_11 || UNITY_2020_3_12 || UNITY_2020_3_13 || UNITY_2020_3_14 || UNITY_2020_3_15 || UNITY_2020_3_16 || UNITY_2020_3_17 || UNITY_2020_3_18 || UNITY_2020_3_19 || UNITY_2020_3_20 || UNITY_2020_3_21 || UNITY_2020_3_22 || UNITY_2020_3_23 || UNITY_2020_3_24 || UNITY_2020_3_25 || UNITY_2020_3_26 || UNITY_2020_3_27 || UNITY_2020_3_28 || UNITY_2020_3_29 || UNITY_2020_3_30 || UNITY_2020_3_31 || UNITY_2020_3_32 || UNITY_2020_3_33 || UNITY_2020_3_34 || UNITY_2020_3_35 || UNITY_2020_3_36 || UNITY_2020_3_37 || UNITY_2020_3_38 || UNITY_2020_3_39 || UNITY_2020_3_40 || UNITY_2020_3_41 || UNITY_2020_3_42 || UNITY_2020_3_43 || UNITY_2020_3_44)
            return Object.FindObjectOfType<RestServer.RestServer>();
#else
            return Object.FindFirstObjectByType<RestServer.RestServer>();
#endif
        }
        // [UnityTest]
        // public IEnumerator Test_MaterialAutoEndpoint_01() {
        //     SceneManager.LoadScene("VisualScriptingTestScene", LoadSceneMode.Single);
        //
        //     yield return new WaitForFixedUpdate();
        //     using var th = new TestHelper(false);
        //
        //     yield return th.HttpAsyncGet("/materialAutoEndpoint");
        //
        //     var r = th.LastAsyncResponse;
        //     var responseStr = r.Content.ReadAsStringAsync().Result;
        //     
        //     Assert.AreEqual("{\"colors\":[{\"name\":\"_Color\",\"value\":{\"r\":0.0,\"g\":0.0,\"b\":1.0,\"a\":1.0}}],\"floats\":[],\"ints\":[],\"matrixs\":[],\"vectors\":[],\"textureOffsets\":[],\"textureScales\":[]}", responseStr);
        // }
    }
}

#endif
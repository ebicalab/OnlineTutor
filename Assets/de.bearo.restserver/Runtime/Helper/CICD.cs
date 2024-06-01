using UnityEngine;

namespace RestServer.Helper {
    public class CICD {
        public static YieldInstruction SafeWaitForEndOfFrame() {
#if UNITY_6000_0_OR_NEWER
            // In Unity 6 WaitForSeconds stops working in unit tests at all (without error), so we just wait "a frame".
            return new WaitForSeconds(1.0f/60.0f);
#else
            // In batch mode there is no WaitForEndOfFrame (as there is no graphic device), so we have to use a different yield instruction
            if (Application.isBatchMode) {
                return new WaitForSeconds(0.02f);
            }

            return new WaitForEndOfFrame();
#endif
        }
    }
}
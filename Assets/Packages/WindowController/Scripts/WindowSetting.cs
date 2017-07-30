using UnityEngine;
using System.Collections;
using System;


namespace WindowController {
    [AddComponentMenu("Window Controller/Window Setting")]
    public class WindowSetting : MonoBehaviour {
        [Serializable]
        public class CommandLineKeys {
            public string posX = "--pos-x";
            public string posY = "--pos-y";
            public string topmost = "--topmost";
        }

        [Serializable]
        public class LoopSettingTopMost {
            public bool Validity = true;
            public float Interval = 1.0f;
        }
        
        public CommandLineKeys keys = new CommandLineKeys();
        public LoopSettingTopMost LoopTopMost = new LoopSettingTopMost();

        private Coroutine coroutine = null;


        void Awake() {
#if !UNITY_EDITOR
            this.Setting();
#endif
        }

        void OnDestroy() {
            if(this.coroutine != null) {
                StopCoroutine(this.coroutine);
            }
        }

        private void Setting() {
            if(Application.isEditor || Screen.fullScreen || !(Application.platform == RuntimePlatform.WindowsPlayer)) { return; }

            int x = 0, y = 0;
            CommandLineHelper.GetIntValue(this.keys.posX, out x);
            CommandLineHelper.GetIntValue(this.keys.posY, out y);
            WindowController.TryMoveWindow(x, y, 1000, 60);

            var topmost = false;
            if(CommandLineHelper.GetBoolValue(this.keys.topmost, out topmost) && topmost) {
                WindowController.SetToTopMost();

                this.coroutine = StartCoroutine(this.SetTopMost());
            }
        }

        private IEnumerator SetTopMost() {
            while(true) {
                yield return new WaitForSeconds(this.LoopTopMost.Interval);

                if(this.LoopTopMost.Validity) {
                    WindowController.SetToTopMost();
                }
            }
        }
    }
}

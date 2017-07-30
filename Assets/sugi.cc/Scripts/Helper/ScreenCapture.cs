using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif
namespace sugi.cc
{
    public class ScreenCapture : MonoBehaviour
    {
        static ScreenCapture Instance
        {
            get
            {
                if (_Instance == null) _Instance = FindObjectOfType<ScreenCapture>();
                if (_Instance == null) _Instance = new GameObject("capture").AddComponent<ScreenCapture>();
                return _Instance;
            }
        }
        static ScreenCapture _Instance;

        public int fps = 30;
        public int superSize = 0;
        public int stopFramecount = 300;

        bool recording;
        int framecount;
        string runTimeName = "";

        string fpsStr;
        string superSizeStr;
        string stopFramecountStr;
        bool showGUI;


        void Init()
        {
            System.IO.Directory.CreateDirectory("Capture");
            Time.captureFramerate = fps;
            framecount = -1;
            runTimeName = Application.productName + System.DateTime.Now.ToString();
            runTimeName = runTimeName.Replace('/', '_').Replace(' ', '_').Replace(':', '_').Replace('\\', '_');
            fpsStr = fps.ToString();
            superSizeStr = superSize.ToString();
            stopFramecountStr = stopFramecount.ToString();
        }

        void Start()
        {
            fpsStr = fps.ToString();
            superSizeStr = superSize.ToString();
            stopFramecountStr = stopFramecount.ToString();
            SettingManager.AddExtraGuiFunc(DrawControllGUI);
        }

        // Update is called once per frame
        void Update()
        {
            if (!recording)
                return;

            if (framecount > 0)
            {
                var name = "Capture/" + runTimeName + "_frame" + Time.frameCount.ToString("00000") + ".png";
                UnityEngine.ScreenCapture.CaptureScreenshot(name, superSize);
            }
            framecount++;
            if (0 < stopFramecount && stopFramecount < framecount)
                recording = false;
        }

        void StartRecording()
        {
            Init();
            recording = true;
            SettingManager.Instance.HideGUI();
        }

        public static void DrawControllGUI()
        {
            Instance.recording = false;
            GUILayout.BeginVertical("box");
            if (Instance.showGUI = GUILayout.Toggle(Instance.showGUI, "show ScreenCapture GUI"))
            {
                IntField("Capture FPS:", ref Instance.fpsStr, ref Instance.fps);
                IntField("super size", ref Instance.superSizeStr, ref Instance.superSize);
                IntField("stop frame count", ref Instance.stopFramecountStr, ref Instance.stopFramecount);

                GUILayout.BeginHorizontal();
                if (Instance.recording)
                {
                    if (GUILayout.Button(string.Format("Stop Recording({0}/{1})", Instance.framecount.ToString("00000"), Instance.stopFramecount.ToString("00000"))))
                        Instance.recording = false;
                }
                else if (GUILayout.Button("Start Recording"))
                    Instance.StartRecording();
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }
        static void IntField(string label, ref string strVal, ref int targetInt)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label);
            strVal = GUILayout.TextField(strVal);
            GUILayout.EndHorizontal();

            var newInt = targetInt;
            if (int.TryParse(strVal, out newInt))
                targetInt = newInt;
        }

#if UNITY_EDITOR
        [MenuItem("ScreenCapture/Shot")]
        public static void CaptureScreenShot()
        {
            System.IO.Directory.CreateDirectory("ScreenShot");
            var name = "ScreenShot/Capture.png";
            UnityEngine.ScreenCapture.CaptureScreenshot(name);
        }

        [MenuItem("ScreenCapture/Start/FPS:10")]
        public static void StartRecording10()
        {
            Instance.fps = 10;
            Instance.StartRecording();
        }

        [MenuItem("ScreenCapture/Start/FPS:30")]
        public static void StartRecording30()
        {
            Instance.fps = 30;
            Instance.StartRecording();
        }

        [MenuItem("ScreenCapture/Start/FPS:60")]
        public static void StartRecording60()
        {
            Instance.fps = 60;
            Instance.StartRecording();
        }

        [MenuItem("ScreenCapture/Stop")]
        public static void StopRecording()
        {
            Instance.recording = false;
        }
#endif
    }
}
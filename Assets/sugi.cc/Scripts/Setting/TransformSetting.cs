using UnityEngine;
using mattatz.TransformControl;

namespace sugi.cc
{
    public class TransformSetting : MonoBehaviour
    {
        [Header("sync materialProperties server to client")]
        public bool sync;
        public Space space = Space.Self;
        [SerializeField]
        string settingFilePath = "TransformSetting/someObject.json";
        [SerializeField]
        Setting setting;

        void Start()
        {
            setting = new Setting(transform, space);
            SettingManager.AddSettingMenu(setting, settingFilePath);
            if (sync)
                setting.SetSyncable();
        }

        [System.Serializable]
        public class Setting : SettingManager.Setting
        {
            public Vector3 position;
            public Vector3 rotation;
            public Vector3 scale;

            Transform target;
            Space space;
            TransformControl control;
            string[] modeSelect = new[] { "None", "Transrate", "Rotate", "Scale" };

            public Setting(Transform transform, Space space)
            {
                target = transform;
                this.space = space;
                if (space == Space.Self)
                {
                    position = transform.localPosition;
                    rotation = transform.localRotation.eulerAngles;
                    scale = transform.localScale;
                }
                else
                {
                    position = transform.position;
                    rotation = transform.rotation.eulerAngles;
                    scale = transform.localScale;//lossyScale(readOnly)
                }
                control = target.gameObject.AddComponent<TransformControl>();
            }
            protected override void OnLoad()
            {
                base.OnLoad();
                target.hasChanged = false;
                ApplySetting();
                control.mode = TransformControl.TransformMode.None;
            }
            public override void OnGUIFunc()
            {
                control.global = GUILayout.Toggle(control.global, "Control Space in Global");
                control.mode = (TransformControl.TransformMode)GUILayout.SelectionGrid((int)control.mode, modeSelect, 4);
                base.OnGUIFunc();
                target.hasChanged = false;
                control.Control();
                ApplySetting();
            }

            protected override void OnClose()
            {
                base.OnClose();
                control.mode = TransformControl.TransformMode.None;
            }

            void ApplySetting()
            {
                if (target.hasChanged)
                {
                    if (space == Space.Self)
                    {
                        position = target.localPosition;
                        rotation = target.localRotation.eulerAngles;
                        scale = target.localScale;
                    }
                    else
                    {
                        position = target.position;
                        rotation = target.rotation.eulerAngles;
                        scale = target.localScale;
                    }
                    if (dataEditor != null)
                        dataEditor.Load();
                }
                else
                {
                    if (space == Space.Self)
                    {
                        target.localPosition = position;
                        target.localRotation = Quaternion.Euler(rotation);
                        target.localScale = scale;
                    }
                    else
                    {
                        target.position = position;
                        target.rotation = Quaternion.Euler(rotation);
                        target.localScale = scale;
                    }
                }
            }
        }
    }
}
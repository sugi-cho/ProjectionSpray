using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Linq;
using DataUI;
using FileUtility;

using NetSystem = UnityEngine.Networking.NetworkSystem;

namespace sugi.cc
{
    public class SettingManager : MonoBehaviour
    {

        public static Material drawLineMat { get { if (_mat == null) _mat = new Material(Shader.Find("Particles/Alpha Blended")); return _mat; } }
        static Material _mat;

        public static void AddSettingMenu(Setting setting, string filePath)
        {
            setting.LoadSettingFromFile(filePath);
            setting.dataEditor = new FieldEditor(setting);
            if (!Instance.settings.Contains(setting))
            {
                Instance.settings.Add(setting);
                Instance.settings = Instance.settings.OrderBy(b => b.filePath).ToList();
            }
            Instance.BuildSettingTree();
        }
        public static void AddExtraGuiFunc(System.Action func)
        {
            if (!Instance.extraGuiFuncList.Contains(func))
            {
                Instance.extraGuiFunc += func;
                Instance.extraGuiFuncList.Add(func);
            }
        }


        #region instance

        public static SettingManager Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new GameObject("SettingManager").AddComponent<SettingManager>();
                return _Instance;
            }
        }

        static SettingManager _Instance;

        #endregion

        public static KeyCode EditKey = KeyCode.E;
        List<SettingTreeNode> settingTree;

        List<Setting> settings = new List<Setting>();
        Setting currentSetting;
        bool edit;
        Rect windowRect = Rect.MinMaxRect(0, 0, Mathf.Min(Screen.width, 1024f), Mathf.Min(Screen.height, 768f));
        Vector2 scroll;
        System.Action extraGuiFunc;
        List<System.Action> extraGuiFuncList = new List<System.Action>();


        public void HideGUI()
        {
            edit = false;
            Cursor.visible = false;
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(EditKey))
                edit = !edit;
            Cursor.visible = edit;
        }

        void OnGUI()
        {
            if (!edit)
                return;
            windowRect = GUI.Window(GetInstanceID(), windowRect, OnWindow, string.Format("Settings:({0})", Application.persistentDataPath));
        }

        void BuildSettingTree()
        {
            var splitedSettingPathList = settings.Select(b => new { paths = b.filePath.Split('/'), settig = b }).ToList();
            var treeRootList = new List<SettingTreeNode>();
            splitedSettingPathList.ForEach(pathSettings =>
            {
                var paths = pathSettings.paths;
                var setting = pathSettings.settig;
                var currentNodeList = treeRootList;
                for (var i = 0; i < paths.Length; i++)
                {
                    var path = paths[i];
                    var nextNodeList = IsContainsPath(currentNodeList, path);
                    if (nextNodeList == null)
                    {
                        var newNode = new SettingTreeNode(path);
                        if (path == paths.Last())
                            newNode.setting = setting;
                        else
                            newNode.children = new List<SettingTreeNode>();
                        currentNodeList.Add(newNode);
                        nextNodeList = newNode.children;
                    }
                    currentNodeList = nextNodeList;
                }
            });
            settingTree = treeRootList;
        }
        List<SettingTreeNode> IsContainsPath(List<SettingTreeNode> nodeList, string path)
        {
            var node = nodeList.Where(b => b.path == path).FirstOrDefault();
            if (node == null)
                return null;
            else
                return node.children;
        }
        class SettingTreeNode
        {
            public bool open;
            public string path;
            public List<SettingTreeNode> children;
            public Setting setting;
            public SettingTreeNode(string path) { this.path = path; }
        }

        void OnWindow(int id)
        {
            scroll = GUILayout.BeginScrollView(scroll);

            if (GUILayout.Button("open SettingFolder"))
                OpenInFileBrowser.Open(Application.persistentDataPath);
            GUILayout.Space(16f);
            GUILayout.Label("Settings:");

            if (settingTree != null)
                settingTree.ForEach(node =>
                {
                    ShowSettingNodeGUI(node);
                });

            GUI.contentColor = Color.white;
            if (extraGuiFunc != null)
            {
                GUILayout.Space(16);
                extraGuiFunc.Invoke();
            }


            GUILayout.EndScrollView();
            GUI.DragWindow();
        }
        void ShowSettingNodeGUI(SettingTreeNode node)
        {
            GUI.contentColor = Color.white;
            GUILayout.BeginVertical("box");
            if (node.setting != null)
                ShowSettingGUI(node.setting, node.path);
            else if (node.open = GUILayout.Toggle(node.open, node.path + "/"))
            {
                node.children.Where(b => b.setting == null).ToList().ForEach(child => { ShowSettingNodeGUI(child); });
                GUILayout.Space(8);
                node.children.Where(b => b.setting != null).ToList().ForEach(child => { ShowSettingNodeGUI(child); });
            }
            GUILayout.EndVertical();
        }
        void ShowSettingGUI(Setting setting, string path)
        {
            var preEdit = setting.edit;
            GUI.contentColor = Color.yellow;
            if (setting.edit = GUILayout.Toggle(setting.edit, path))
            {
                GUILayout.Space(16);
                GUI.contentColor = Color.white;

                GUILayout.BeginHorizontal();
                GUILayout.Space(16f);

                GUILayout.BeginVertical();
                setting.OnGUIFunc();
                GUILayout.EndVertical();

                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Save and Close"))
                    setting.SaveAndClose();
                if (NetworkServer.active && setting.canSync)
                    if (GUILayout.Button("Sync Setting"))
                        setting.SyncSetting();
                if (GUILayout.Button("Cancel"))
                    setting.CancelAndClose();
                GUILayout.EndHorizontal();

                GUILayout.Space(16);
            }
            if (preEdit && setting.edit != preEdit)
                setting.CancelAndClose();
        }

        void OnRenderObject()
        {
            settings.ForEach(setting =>
            {
                setting.OnRenderObjectFunc(Camera.current);
            });
        }

        [System.Serializable]
        public abstract class Setting
        {
            public FieldEditor dataEditor { get; set; }

            public string filePath { get; set; }

            public bool edit { get; set; }

            public void SetSyncable()
            {
                sync = true;
                NetworkMessageManager.AddHandler(LoadSettingFromJson);
                NetworkManager.Instance.onServerConnect.AddListener(SyncSettingToClient);
            }
            bool sync;
            public bool canSync { get { return sync; } }

            //use this Function as intisialize.
            public void LoadSettingFromFile(string path)
            {
                filePath = path;
                Helper.LoadJsonFile(this, filePath);
                OnLoad();
            }

            public void LoadSettingFromJson(string json)
            {
                JsonUtility.FromJsonOverwrite(json, this);
                dataEditor.Load();
                Save();//save to json
                OnLoad();//re initializing
            }
            void LoadSettingFromJson(NetworkMessage netMsg)
            {
                var msg = netMsg.ReadMessage<NetSystem.StringMessage>();
                LoadSettingFromJson(msg.value);
            }
            void SyncSettingToClient(NetworkConnection conn)
            {
                var json = JsonUtility.ToJson(this);
                NetworkMessageManager.SendNetworkMessage(conn, LoadSettingFromJson, NetworkMessageManager.GetStringMessage(json));
            }

            public void Save()
            {
                Helper.SaveJsonFile(this, filePath);
            }

            public void SaveAndClose()
            {
                Save();
                edit = false;
                OnClose();
            }

            public void SyncSetting()
            {
                var json = JsonUtility.ToJson(this);
                if (NetworkServer.active)
                    NetworkMessageManager.SendNetworkMessageToAll(LoadSettingFromJson, NetworkMessageManager.GetStringMessage(json));
            }

            public void CancelAndClose()
            {
                Helper.LoadJsonFile(this, filePath);
                dataEditor = new FieldEditor(this);
                edit = false;
                OnClose();
            }

            public virtual void OnGUIFunc() { dataEditor.OnGUI(); }

            public virtual void OnRenderObjectFunc(Camera cam) { }

            protected virtual void OnLoad() { }

            protected virtual void OnClose() { }
        }
    }
}
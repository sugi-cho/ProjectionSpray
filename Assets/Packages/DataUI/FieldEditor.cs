using UnityEngine;
using System.Reflection;
using System.Text;
using System.Collections.Generic;

namespace DataUI {
	public class FieldEditor {
		public enum FieldKindEnum { Int, Float, Bool, Vector2, Vector3, Vector4, Matrix, Color, Enum, String, Unknown }
		public const BindingFlags BINDING = BindingFlags.Public | BindingFlags.Instance;

		public readonly System.Object data;
		public readonly List<BaseGUIField> GuiFields = new List<BaseGUIField>();

		FieldInfo[] _fieldInfos;

		public FieldEditor(System.Object data) {
			this.data = data;
			_fieldInfos = data.GetType().GetFields(BINDING);
			for (var i = 0; i < _fieldInfos.Length; i++)
				GuiFields.Add(GenerateGUI(_fieldInfos[i]));
		}

		public void OnGUI() {
			var limit = GuiFields.Count;
			for (var i = 0; i < limit; i++)
				GuiFields[i].OnGUI();
		}
		public void Load() {
			var limit = GuiFields.Count;
			for (var i = 0; i < limit; i++)
				GuiFields[i].Load();
		}

		public BaseGUIField GenerateGUI(FieldInfo fi) {
			var fieldKind = EstimateFieldKind(fi);
			switch (fieldKind) {
			case FieldKindEnum.Int:
				return new GUIInt(data, fi);
			case FieldKindEnum.Float:
				return new GUIFloat (data, fi);
			case FieldKindEnum.Vector2:
				return new GUIVector2(data, fi, 2);
			case FieldKindEnum.Vector3:
				return new GUIVector3(data, fi, 3);
			case FieldKindEnum.Vector4:
				return new GUIVector4(data, fi, 4);
            case FieldKindEnum.Matrix:
				return new GUIMatrix (data, fi);
			case FieldKindEnum.Color:
				return new GUIColor (data, fi);
			case FieldKindEnum.Bool:
				return new GUIBool (data, fi);
			case FieldKindEnum.Enum:
				return new GUIEnum (data, fi);
            case FieldKindEnum.String:
                return new GUIText (data, fi);
			default:
				return new GUIUnsupported (data, fi);
			}
		}

		public FieldKindEnum EstimateFieldKind(FieldInfo fi) {
			var fieldType = fi.FieldType;
			if (fieldType.IsPrimitive) {
				if (fieldType == typeof(int))
					return FieldKindEnum.Int;
				if (fieldType == typeof(float))
					return FieldKindEnum.Float;
				if (fieldType == typeof(bool))
					return FieldKindEnum.Bool;
				return FieldKindEnum.Unknown;
			}
			if (fieldType.IsEnum)
				return FieldKindEnum.Enum;
			if (fieldType.IsValueType) {
				if (fieldType == typeof(Color))
					return FieldKindEnum.Color;
				if (fieldType == typeof(Vector2))
					return FieldKindEnum.Vector2;
				if (fieldType == typeof(Vector3))
					return FieldKindEnum.Vector3;
				if (fieldType == typeof(Vector4))
					return FieldKindEnum.Vector4;
                if (fieldType == typeof(Matrix4x4))
                    return FieldKindEnum.Matrix;
			}
            if (fieldType == typeof(string))
                return FieldKindEnum.String;

			return FieldKindEnum.Unknown;
		}

		public abstract class BaseGUIField {
			public readonly System.Object Data;
			public readonly FieldInfo Fi;

			protected System.Action _onGUI;

			public BaseGUIField(System.Object data, FieldInfo fi) {
				this.Data = data;
				this.Fi = fi;
			}
			public virtual void OnGUI() {
				_onGUI ();
			}
			public abstract void Load();
			public abstract void Save();
		}
		public class GUIInt : BaseGUIField {
			public readonly TextInt TextInt;

			public GUIInt(System.Object data, FieldInfo fi) : base(data, fi) {
                TextInt = new TextInt((int)fi.GetValue(data));
                _onGUI = () => {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(string.Format("{0} ", fi.Name), GUILayout.ExpandWidth(false));
                    TextInt.StrValue = GUILayout.TextField(TextInt.StrValue, GUILayout.ExpandWidth(true), GUILayout.MinWidth(30f));
                    GUILayout.EndHorizontal();
					Save ();
                };
            }

			public override void Load() {
				TextInt.Value = (int)Fi.GetValue (Data);
            }
			public override void Save () {
				Fi.SetValue (Data, TextInt.Value);
			}
        }
		public class GUIFloat : BaseGUIField {
			public readonly TextFloat TextFloat;

			public GUIFloat(System.Object data, FieldInfo fi) : base(data, fi) {
				TextFloat = new TextFloat((float)fi.GetValue(data));
				_onGUI = () => {
					GUILayout.BeginHorizontal();
					GUILayout.Label(string.Format("{0} ", fi.Name), GUILayout.ExpandWidth(false));
					TextFloat.StrValue = GUILayout.TextField(TextFloat.StrValue, GUILayout.ExpandWidth(true), GUILayout.MinWidth(30f));
					GUILayout.EndHorizontal();
					Save();
				};		
			}
			public override void Load() {
				TextFloat.Value = (float)Fi.GetValue(Data);
			}
			public override void Save () {
				Fi.SetValue(Data, TextFloat.Value);
			}
		}
		public abstract class BaseGUIVector : BaseGUIField {
			public readonly TextVector TextVector;

			public BaseGUIVector(System.Object data, FieldInfo fi, int dimention) : base(data, fi) {
				TextVector = GetTextVector(data, fi);
				_onGUI = () => {
					GUILayout.BeginHorizontal();
					GUILayout.Label(string.Format("{0} ", fi.Name), GUILayout.ExpandWidth(false));
					for (var i = 0; i < dimention; i++)
						TextVector[i] = GUILayout.TextField(TextVector[i], GUILayout.ExpandWidth(true), GUILayout.MinWidth(30f));
					GUILayout.EndHorizontal();
					Save();
				};
			}
			public abstract TextVector GetTextVector (System.Object data, FieldInfo fi);
		}
		public class GUIVector2 : BaseGUIVector {
			public GUIVector2(System.Object data, FieldInfo fi, int dimention) : base(data, fi, dimention){}
			#region implemented abstract members of BaseGUIVector
			public override TextVector GetTextVector (object data, FieldInfo fi) {
				return new TextVector ((Vector2)fi.GetValue (data));
			}
			public override void Load() {
				TextVector.Value = (Vector2)Fi.GetValue (Data);
			}
			public override void Save () {
				Fi.SetValue(Data, (Vector2)TextVector.Value);
			}
			#endregion
		}
		public class GUIVector3 : BaseGUIVector {
			public GUIVector3(System.Object data, FieldInfo fi, int dimention) : base(data, fi, dimention){}
			#region implemented abstract members of BaseGUIVector
			public override TextVector GetTextVector (object data, FieldInfo fi) {
				return new TextVector ((Vector3)fi.GetValue (data));
			}
			public override void Load() {
				TextVector.Value = (Vector3)Fi.GetValue (Data);
			}
			public override void Save () {
				Fi.SetValue(Data, (Vector3)TextVector.Value);
			}
			#endregion
		}
		public class GUIVector4 : BaseGUIVector {
			public GUIVector4(System.Object data, FieldInfo fi, int dimention) : base(data, fi, dimention){}
			#region implemented abstract members of BaseGUIVector
			public override TextVector GetTextVector (object data, FieldInfo fi) {
				return new TextVector ((Vector4)fi.GetValue (data));
			}
			public override void Load() {
				TextVector.Value = (Vector4)Fi.GetValue (Data);
			}
			public override void Save () {
				Fi.SetValue(Data, TextVector.Value);
			}
			#endregion
		}
		public class GUIColor : BaseGUIField {
			public readonly TextVector TextVector;

			public GUIColor(System.Object data, FieldInfo fi) : base(data, fi) {
				TextVector = new TextVector((Color)fi.GetValue(data));
				_onGUI = () => {
					var c = (Color)TextVector.Value;
					GUILayout.BeginVertical();
					GUILayout.BeginHorizontal();
					GUILayout.Label(string.Format("{0} ", fi.Name), GUILayout.ExpandWidth(false));
					var prevColor = GUI.color;
					GUI.color = new Color(c.r, c.g, c.b);
					GUILayout.Label("■■■■■■", GUILayout.ExpandWidth(false));
					GUI.color = new Color(c.a, c.a, c.a);
					GUILayout.Label("■■", GUILayout.ExpandWidth(false));
					GUI.color = prevColor;
					GUILayout.EndHorizontal();
					GUILayout.BeginHorizontal();
					for (var i = 0; i < 4; i++)
						TextVector[i] = GUILayout.TextField(TextVector[i], GUILayout.ExpandWidth(true), GUILayout.MinWidth(30f));
					GUILayout.EndHorizontal();
					GUILayout.EndVertical();
					Save();
				};				
			}
			public override void Load () {
				TextVector.Value = (Color)Fi.GetValue (Data);
			}
			public override void Save () {
				Fi.SetValue (Data, (Color)TextVector.Value);
			}
		}
		public class GUIMatrix : BaseGUIField {
			public readonly TextMatrix TextMatrix;

			public GUIMatrix(System.Object data, FieldInfo fi) : base(data, fi) {
				TextMatrix = new TextMatrix((Matrix4x4)fi.GetValue(data));
				_onGUI = () => {
					GUILayout.BeginHorizontal();
					GUILayout.Label(string.Format("{0} ", fi.Name), GUILayout.ExpandWidth(false));
					GUILayout.BeginVertical();
					for (var y = 0; y < 4; y++) {
						GUILayout.BeginHorizontal();
						for (var x = 0; x < 4; x++) {
							TextMatrix[x + y * 4] = GUILayout.TextField(
								TextMatrix[x + y * 4], GUILayout.ExpandWidth(true), GUILayout.MinWidth(30f));
						}
						GUILayout.EndHorizontal();
					}
					GUILayout.EndVertical();
					GUILayout.EndHorizontal();
					Save();
				};		
			}
			public override void Load () {
				TextMatrix.Value = (Matrix4x4)Fi.GetValue (Data);
			}
			public override void Save() {
				Fi.SetValue (Data, (Matrix4x4)TextMatrix.Value);
			}
		}
		public class GUIBool : BaseGUIField {
			bool _toggle;

			public GUIBool(System.Object data, FieldInfo fi) : base(data, fi) {
				_toggle = (bool)fi.GetValue(data);
				_onGUI = () => {
					GUILayout.BeginHorizontal();
					_toggle = GUILayout.Toggle(_toggle, string.Format("{0} ", fi.Name));
					GUILayout.EndHorizontal();
					Save();
				};
			}
			public override void Load () {
				_toggle = (bool)Fi.GetValue (Data);
			}
			public override void Save() {
				Fi.SetValue (Data, _toggle);
			}
		}
		public class GUIEnum : BaseGUIField {
			public readonly TextInt TextInt;

			public GUIEnum(System.Object data, FieldInfo fi) : base(data, fi) {
				var enumType = fi.FieldType;
				var list = new StringBuilder();
				foreach (var selection in System.Enum.GetValues(enumType))
					list.AppendFormat("{0}({1}) ", selection, (int)selection);
				TextInt = new TextInt((int)fi.GetValue(data));
				_onGUI = () => {
					GUILayout.BeginHorizontal();
					GUILayout.Label(string.Format("{0} ", fi.Name), GUILayout.ExpandWidth(false));
					TextInt.StrValue = GUILayout.TextField(TextInt.StrValue, GUILayout.ExpandWidth(true), GUILayout.MinWidth(30f));
					GUILayout.Label(string.Format("{0}({1})", GetEnumValue(), TextInt.Value));
					GUILayout.EndHorizontal();
					GUILayout.Label(list.ToString());
					Save();
				};
			}
			public override void Load() {
				TextInt.Value = (int)Fi.GetValue (Data);
			}
			public override void Save () {
				Fi.SetValue (Data, GetEnumValue());
			}
			public System.Object GetEnumValue() {
				return System.Enum.ToObject (Fi.FieldType, TextInt.Value);
			}
		}
        public class GUIText : BaseGUIField {
            public string text;

            public GUIText(System.Object data, FieldInfo fi) : base(data, fi) {
                Load();
                _onGUI = () => {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(string.Format("{0} ", fi.Name), GUILayout.ExpandWidth(false));
                    text = GUILayout.TextField(text, GUILayout.ExpandWidth(true), GUILayout.MinWidth(30f));
                    GUILayout.EndHorizontal();
                    Save ();
                };
            }

            public override void Load() {
                text = (string)Fi.GetValue (Data);
            }
            public override void Save () {
                Fi.SetValue (Data, text);
            }
        }
		public class GUIUnsupported : BaseGUIField {
			public GUIUnsupported(System.Object data, FieldInfo fi) : base(data, fi) {
				_onGUI = () => {
					GUILayout.BeginHorizontal();
					GUILayout.Label(string.Format("Unsupported Field : {0} of {1}", fi.Name, fi.FieldType.Name));
					GUILayout.EndHorizontal();
				};		
			}
			public override void Load () {}
			public override void Save () {}
		}
	}
}

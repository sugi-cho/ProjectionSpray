using UnityEngine;
using System.Collections;

namespace DataUI {
	public class TextFloat {
		float _value = 0;
		string _strValue = "0";
		bool _changed = true;
		
		public TextFloat(float exisitingValue) {
			Value = exisitingValue;
		}
		
		public string StrValue {
			get {
				return _strValue;
			}
			set {
				if (value != null && value != _strValue) {
					_strValue = value;
					_changed = true;
				}
			}
		}
		public float Value {
			get {
				if (_changed && float.TryParse(_strValue, out _value))
					_changed = false;
				return _value;
			}
			set {
				if (_value != value) {
					_value = value;
					_strValue = _value.ToString();
					_changed = false;
				}
			}
		}
	}
	
	public class TextInt {
		int _value = 0;
		string _strValue = "0";
		bool _changed = true;
		
		public TextInt(int existing) {
			Value = existing;
		}
		
		public string StrValue {
			get {
				return _strValue;
			}
			set {
				_strValue = value;
				_changed = true;
			}
		}
		
		public int Value {
			get {
				if (_changed && int.TryParse(_strValue, out _value))
					_changed = false;
				return _value;
			}
			set {
				if (_value != value) {
					_value = value;
					_strValue = _value.ToString();
					_changed = false;
				}
			}
		}
	}
	
	public class TextVector {
		TextFloat[] _texts = new TextFloat[]{ new TextFloat(0), new TextFloat(0), new TextFloat(0), new TextFloat(0) };
		Vector4 _value = Vector4.zero;
		
		public TextVector(Vector4 existing) {
			Value = existing;
		}
		
		public string this[int index] {
			get { return _texts[index].StrValue; }
			set { _texts[index].StrValue = value; }
		}
		
		public Vector4 Value {
			get {
				_value.Set(_texts[0].Value, _texts[1].Value, _texts[2].Value, _texts[3].Value);
				return _value;
			}
			set {
				_value = value;
				_texts[0].Value = _value.x;
				_texts[1].Value = _value.y;
				_texts[2].Value = _value.z;
				_texts[3].Value = _value.w;
			}
		}
    }  

    public class TextMatrix {
        TextFloat[] _texts = new TextFloat[16];
        Matrix4x4 _value = Matrix4x4.zero;

        public TextMatrix(Matrix4x4 existing) {
            for (var i = 0; i < _texts.Length; i++)
                _texts[i] = new TextFloat(0);
            Value = existing;
        }

        public string this[int index] {
            get { return _texts[index].StrValue; }
            set { _texts[index].StrValue = value; }
        }

        public Matrix4x4 Value {
            get {
                for (var y = 0; y < 4; y++)
                    for (var x = 0; x < 4; x++)
                        _value [x + y * 4] = _texts [y + x * 4].Value;
                return _value;
            }
            set {
                _value = value;
                for (var y = 0; y < 4; y++)
                    for (var x = 0; x < 4; x++)
                        _texts [x + y * 4].Value = _texts [y + x * 4].Value;
            }
        }
    }
}
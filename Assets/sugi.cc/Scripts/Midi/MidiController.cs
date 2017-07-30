using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using MidiJack;
using System.Linq;

namespace sugi.cc
{
    public class MidiController : MonoBehaviour
    {
        public static MidiController Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = FindObjectOfType<MidiController>();
                    if (_Instance == null)
                        _Instance = new GameObject("MidiController").AddComponent<MidiController>();
                }
                return _Instance;
            }
        }
        static MidiController _Instance;
        public static void AddCcAction(MidiDriver.KnobDelegate knobAction, MidiCcEventInfo info)
        {
            MidiMaster.knobDelegate += knobAction;
            Instance.eventList.Add(info);
            Instance.eventList.OrderBy(b => b.channel.ToString() + b.knobNumber).ToList();
        }

        [SerializeField]
        List<MidiCcEventInfo> eventList;

        public MidiCcTriger triger;
        void Start()
        {
            triger.Apply();
        }
    }
    [System.Serializable]
    public abstract class MidiCcBase
    {
        public string knobName;
        public MidiChannel channel = MidiChannel.All;
        public int knobNumber;
        public MidiCcEventInfo info;

        public abstract void Apply();
    }
    [System.Serializable]
    public class MidiCcTriger : MidiCcBase
    {
        public UnityEvent onTriger;
        override public void Apply()
        {
            MidiDriver.KnobDelegate action = (MidiChannel ch, int knob, float val) =>
            {
                Debug.Log(ch.ToString() + "." + knob + "." + val);
                if ((ch == channel || channel == MidiChannel.All) && knob == knobNumber && 0.5f < val)
                    onTriger.Invoke();
            };

            info = new MidiCcEventInfo()
            {
                knobName = knobName,
                channel = channel,
                knobNumber = knobNumber,
                methods = Enumerable.Range(0, onTriger.GetPersistentEventCount())
                .Select(idx => onTriger.GetPersistentTarget(idx).ToString() + "." + onTriger.GetPersistentMethodName(idx))
                .ToArray()
            };
            MidiController.AddCcAction(action, info);
        }
    }
    [System.Serializable]
    public class MidiCcKnob : MidiCcBase
    {
        public FloatEvent onKnob;
        public override void Apply()
        {
            MidiDriver.KnobDelegate action = (MidiChannel ch, int knob, float val) =>
            {
                if ((ch == channel || channel == MidiChannel.All) && knob == knobNumber)
                    onKnob.Invoke(val);
            };
            info = new MidiCcEventInfo()
            {
                knobName = knobName,
                channel = channel,
                knobNumber = knobNumber,
                methods = Enumerable.Range(0, onKnob.GetPersistentEventCount())
                .Select(idx => onKnob.GetPersistentTarget(idx).ToString() + "." + onKnob.GetPersistentMethodName(idx))
                .ToArray()
            };
            MidiController.AddCcAction(action, info);
        }
    }

    [System.Serializable]
    public struct MidiCcEventInfo
    {
        public string knobName;
        public MidiChannel channel;
        public int knobNumber;
        public string[] methods;
    }
}
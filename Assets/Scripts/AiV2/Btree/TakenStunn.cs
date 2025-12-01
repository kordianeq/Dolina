using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "Behavior/Event Channels/TakenStunn")]
#endif
[Serializable, GeneratePropertyBag]
[EventChannelDescription(name: "TakenStunn", message: "[stunned]", category: "Npc_Events", id: "9a8f89b580bd794590c3c965b0240cd1")]
public sealed partial class TakenStunn : EventChannel<bool> { }


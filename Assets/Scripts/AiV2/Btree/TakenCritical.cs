using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "Behavior/Event Channels/TakenCritical")]
#endif
[Serializable, GeneratePropertyBag]
[EventChannelDescription(name: "TakenCritical", message: "Critical dmg", category: "NPC_Base", id: "ddb54f918708f026296cab90f926fdcf")]
public sealed partial class TakenCritical : EventChannel { }


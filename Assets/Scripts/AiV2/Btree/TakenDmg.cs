using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "Behavior/Event Channels/TakenDmg")]
#endif
[Serializable, GeneratePropertyBag]
[EventChannelDescription(name: "TakenDmg", message: "[npc] taken dmg", category: "NPC_Base", id: "bad4301a988287a61776aade46693590")]
public sealed partial class TakenDmg : EventChannel<NpcCoreBase> { }


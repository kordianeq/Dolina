using System;
using Unity.Behavior;

[BlackboardEnum]
public enum NpcRotationModes
{
	disabled,
    LookAtTarget,
	LookAtMoveDirection,
	LookAtVelocity,
	LookAtVelocityReversed
}

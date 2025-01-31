using Robust.Shared.Serialization;

namespace Content.Shared._White.TargetDoll;

public enum BodyPart
{
    Head,
    Chest,
    Groin,
    RightArm,
    RightHand,
    LeftArm,
    LeftHand,
    RightLeg,
    RightFoot,
    LeftLeg,
    LeftFoot,
    Eyes,
    Mouth,
    Tail,
}

[Serializable, NetSerializable]
public sealed class TargetDollChangeEvent(NetEntity uid, BodyPart bodyPart) : EntityEventArgs
{
    public NetEntity Uid { get; } = uid;
    public BodyPart BodyPart { get; } = bodyPart;
}

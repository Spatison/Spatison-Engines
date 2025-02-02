using Robust.Shared.Serialization;

namespace Content.Shared._White.TargetDoll;

[Flags]
public enum BodyPart
{
    Other = 0,
    Head = 1,
    Chest = 1 << 1,
    Groin = 1 << 2,
    RightArm = 1 << 3,
    RightHand = 1 << 4,
    MiddleArm = 1 << 5,
    MiddleHand = 1 << 6,
    LeftArm = 1 << 7,
    LeftHand = 1 << 8,
    RightLeg = 1 << 9,
    RightFoot = 1 << 10,
    LeftLeg = 1 << 11,
    LeftFoot = 1 << 12,
    Eyes = 1 << 13,
    Mouth = 1 << 14,
    Tail = 1 << 15,

    Torso = Chest | Groin,
    Arms = RightArm | MiddleArm | LeftArm,
    Hands = RightHand | MiddleHand | LeftHand,
    Legs = RightLeg | LeftLeg,
}

[Serializable, NetSerializable]
public sealed class TargetDollChangeEvent(NetEntity uid, BodyPart bodyPart) : EntityEventArgs
{
    public NetEntity Uid { get; } = uid;
    public BodyPart BodyPart { get; } = bodyPart;
}

using Content.Shared._White.Body;
using Robust.Shared.Serialization;

namespace Content.Shared._White.TargetDoll;

[Serializable, NetSerializable]
public sealed class TargetDollChangeEvent(NetEntity uid, BodyPart bodyPart) : EntityEventArgs
{
    public NetEntity Uid { get; } = uid;
    public BodyPart BodyPart { get; } = bodyPart;
}

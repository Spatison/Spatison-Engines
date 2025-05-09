using Robust.Shared.Serialization;

namespace Content.Shared._White.Medical.Wound;

[Serializable, NetSerializable]
public enum WoundType
{
    Other,
    Bone,
    Skin,
    Organ,
}

using Content.Shared._White.Body.Components;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared._White.Body;

#region BodyPart

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

[DataRecord]
public sealed record BodyPartData(
    Dictionary<string, BodyPart> Connections,
    Dictionary<(string, Organ), EntProtoId?> Organs,
    Dictionary<(string, Bone), BoneData> Bones);

[DataDefinition, Serializable, NetSerializable]
public sealed partial class BodyPartSlot
{
    public BodyPartSlot() { }

    public BodyPartSlot(BodyPartSlot other)
    {
        Type = other.Type;
        ContainerSlot = other.ContainerSlot;
        StartingBodyPart = other.StartingBodyPart;
    }

    [DataField]
    public BodyPart Type = BodyPart.Other;

    [DataField]
    public HashSet<string> ChildBodyPart = new();

    [DataField]
    public Dictionary<string, BoneSlot> BonesData = new();

    [DataField]
    public Dictionary<string, OrganSlot> OrgansData = new();

    [ViewVariables, NonSerialized]
    public ContainerSlot? ContainerSlot;

    [DataField(readOnly: true, customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>)), NonSerialized]
    public string? StartingBodyPart;

    public string? Id => ContainerSlot?.ID;
    public bool HasBodyPart => ContainerSlot?.ContainedEntity != null;
    public EntityUid? BodyPartUid => ContainerSlot?.ContainedEntity;
}

/// <summary>
/// Raised when a body part is attached to body.
/// </summary>
/// <param name="Part">The attached body part.</param>
/// <param name="Body">The body to which the body part was attached.</param>
/// <param name="SlotId">Container ID of Part.</param>
public readonly record struct BodyPartAddedEvent(
    Entity<BodyPartComponent> Part,
    Entity<BodyComponent>? Body,
    string SlotId);

/// <summary>
/// Raised when a body part is detached from body.
/// </summary>
/// <param name="Part">The detached body part.</param>
/// <param name="Body">The body from which the body part was detached.</param>
/// <param name="SlotId">Container ID of Part.</param>
public readonly record struct BodyPartRemovedEvent(
    Entity<BodyPartComponent> Part,
    Entity<BodyComponent>? Body,
    string SlotId);

#endregion

#region Bone

[Flags]
public enum Bone
{
    Other = 0,
    Сranium = 1,
    Thorax = 1 << 1,
    Coxae = 1 << 2,
    Humerus = 1 << 3,
    Antebrachii = 1 << 4,
    Manus = 1 << 5,
    Femur = 1 << 6,
    Crus = 1 << 7,
    Pedis = 1 << 8,
}

public enum BoneSeverity
{
    Intact,
    Broken,
}

[DataRecord]
public sealed record BoneData(Dictionary<string, string> Organs);

[DataDefinition, Serializable, NetSerializable]
public sealed partial class BoneSlot
{
    public BoneSlot() { }

    public BoneSlot(BoneSlot other)
    {
        Type = other.Type;
        ContainerSlot = other.ContainerSlot;
        StartingBone = other.StartingBone;
    }

    public void Copy(BoneSlot other)
    {
        Type = other.Type != Bone.Other ? other.Type : Type;
        ContainerSlot ??= other.ContainerSlot;
        StartingBone ??= other.StartingBone;
        foreach (var (organId, organSlot) in other.OrgansData)
        {
            if (!OrgansData.TryGetValue(organId, out var originalOrganSlot))
            {
                OrgansData[organId] = organSlot;
                continue;
            }

            originalOrganSlot.Copy(organSlot);
            organSlot.Copy(originalOrganSlot);
        }
    }

    [DataField]
    public Bone Type = Bone.Other;

    [DataField]
    public Dictionary<string, OrganSlot> OrgansData = new();

    [ViewVariables, NonSerialized]
    public ContainerSlot? ContainerSlot;

    [DataField(readOnly: true, customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>)), NonSerialized]
    public string? StartingBone;

    public string? Id => ContainerSlot?.ID;
    public bool HasBone => ContainerSlot?.ContainedEntity != null;
    public EntityUid? BoneUid => ContainerSlot?.ContainedEntity;
}

public readonly record struct BoneAddedEvent;

public readonly record struct BoneRemovedEvent;

#endregion

#region Organ

[Flags]
public enum Organ
{
    Other = 0,
    Brain = 1,
    Heart = 1 << 1,
    Eyes = 1 << 2,
    Tongue = 1 << 3,
    Appendix = 1 << 4,
    Ears = 1 << 5,
    Lungs = 1 << 6,
    Stomach = 1 << 7,
    Liver = 1 << 8,
    Kidneys = 1 << 9,
}

public enum OrganSeverity
{
    Healed,
    Moderate,
    Severe,
    Critical,
    Dead,
}

[DataDefinition, Serializable, NetSerializable]
public sealed partial class OrganSlot
{
    public OrganSlot() { }

    public OrganSlot(OrganSlot other)
    {
        Type = other.Type;
        ContainerSlot = other.ContainerSlot;
        StartingOrgan = other.StartingOrgan;
    }

    public void Copy(OrganSlot other)
    {
        Type = other.Type != Organ.Other ? other.Type : Type;
        ContainerSlot ??= other.ContainerSlot;
        StartingOrgan ??= other.StartingOrgan;
    }

    [DataField(customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>)), NonSerialized]
    public string? StartingOrgan;

    [DataField]
    public Organ Type = Organ.Other;

    [ViewVariables, NonSerialized]
    public ContainerSlot? ContainerSlot;

    public string? Id => ContainerSlot?.ID;
    public bool HasOrgan => ContainerSlot?.ContainedEntity != null;
    public EntityUid? OrganUid => ContainerSlot?.ContainedEntity;
}

/// <summary>
/// Raised when an organ attaches to body.
/// </summary>
/// <param name="Body">The body to which the organ is attached.</param>
public readonly record struct OrganAddedEvent(Entity<BodyComponent>? Body);

/// <summary>
/// Raised when an organ detaches from body.
/// </summary>
/// <param name="Body">The body from which the organ is detached.</param>
public readonly record struct OrganRemovedEvent(Entity<BodyComponent>? Body);

#endregion

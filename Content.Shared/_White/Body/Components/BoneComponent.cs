using Content.Shared.FixedPoint;
using Robust.Shared.Containers;
using Robust.Shared.GameStates;

namespace Content.Shared._White.Body.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class BoneComponent : Component
{
    [DataField, AutoNetworkedField]
    public Bone BoneType = Bone.Other;

    /// <summary>
    /// Relevant body this bone is attached to.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? Body;

    /// <summary>
    /// Parent body part for this bone.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? ParentPart;

    [DataField, AutoNetworkedField]
    public FixedPoint2 Integrity = 125;

    [DataField, AutoNetworkedField]
    public FixedPoint2 Strength = 15;

    /*[DataField(required: true)]
    public SortedDictionary<FixedPoint2, BoneSeverity> Thresholds = new();*/

    [DataField]
    public Dictionary<string, OrganSlot> Organs = new();

    [ViewVariables]
    public BoneSeverity Severity = BoneSeverity.Intact;
}

using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;

namespace Content.Shared._White.Body.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class OrganComponent : Component
{
    [DataField, AutoNetworkedField]
    public Organ OrganType = Organ.Other;

    /// <summary>
    /// Relevant body this organ is attached to.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? Body;

    /// <summary>
    /// Parent body part for this organ.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? ParentPart;

    /*[DataField(required: true)]
    public SortedDictionary<FixedPoint2, OrganSeverity> Thresholds = new();*/

    [DataField, AutoNetworkedField]
    public FixedPoint2 Integrity;

    [DataField, AutoNetworkedField]
    public FixedPoint2 BloodAmount;

    [DataField, AutoNetworkedField]
    public FixedPoint2 BloodThreshold;
}

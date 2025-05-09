using Robust.Shared.GameStates;

namespace Content.Shared._White.Body.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class BodyPartComponent : Component
{
    /// <summary>
    /// The type of this body part
    /// </summary>
    [DataField, AutoNetworkedField]
    public BodyPart PartType = BodyPart.Other;

    /// <summary>
    /// Child body parts attached to this body part.
    /// </summary>
    [DataField]
    public Dictionary<string, BodyPartSlot> Children = new();

    /// <summary>
    /// Bones attached to this body part.
    /// </summary>
    [DataField]
    public Dictionary<string, BoneSlot> Bones = new();

    /// <summary>
    /// Organs attached to this body part.
    /// </summary>
    [DataField]
    public Dictionary<string, OrganSlot> Organs = new();

    /// <summary>
    /// Parent body for this part.
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public EntityUid? Body;

    /// <summary>
    /// Parent body part for this part.
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public EntityUid? ParentPart;
}

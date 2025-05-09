using Content.Shared._White.Body.Prototypes;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._White.Body.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class BodyComponent : Component
{
    /// <summary>
    /// Relevant template to spawn for this body.
    /// </summary>
    [DataField, AutoNetworkedField]
    public ProtoId<BodyPrototype> Prototype;

    [DataField]
    public Dictionary<string, BodyPartSlot> BodyParts = new();

    [DataField/*(required:true)*/]
    public string RootBodyPartId;

    [DataField, AutoNetworkedField]
    public SoundSpecifier GibSound = new SoundCollectionSpecifier("gib");

    /// <summary>
    /// The amount of legs required to move at full speed.
    /// If 0, then legs do not impact speed.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int RequiredLegs;
}

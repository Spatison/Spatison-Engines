using Content.Shared.Damage.Prototypes;
using Content.Shared.FixedPoint;
using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared._White.Medical.Wound.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class WoundManagerComponent : Component
{
    /// <summary>
    ///     This <see cref="DamageContainerPrototype"/> specifies what damage types are supported by this component.
    ///     If null, all damage types will be supported.
    /// </summary>
    [DataField(customTypeSerializer: typeof(PrototypeIdSerializer<DamageContainerPrototype>))]
    public string? DamageContainer;

    /// <summary>
    /// Integrity points of this woundable.
    /// </summary>
    [DataField, AutoNetworkedField]
    public FixedPoint2 Integrity;

    /// <summary>
    /// Container potentially holding wounds.
    /// </summary>
    [ViewVariables]
    public Container Wounds;
}

using Content.Shared.Damage.Prototypes;
using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared._White.Medical.Wound.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class WoundComponent : Component
{
    /// <summary>
    /// 'Parent' of wound. Basically the entity to which the wound was applied.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid Parent;

    /// <summary>
    /// Actually, severity of the wound. The more the worse.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public FixedPoint2 WoundSeverityPoint;

    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public WoundType WoundType;

    /// <summary>
    /// Damage type of this wound.
    /// </summary>
    [DataField(customTypeSerializer: typeof(PrototypeIdSerializer<DamageTypePrototype>)), ViewVariables(VVAccess.ReadOnly)]
    public string DamageType = "Blunt";
}

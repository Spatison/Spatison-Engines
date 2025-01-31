using Robust.Shared.GameStates;

namespace Content.Shared._White.TargetDoll;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class TargetDollComponent : Component
{
    [ViewVariables, AutoNetworkedField]
    public BodyPart Target = BodyPart.Chest;
}

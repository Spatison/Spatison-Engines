using Content.Shared._White.TargetDoll;
using Robust.Client.Player;
using Robust.Shared.Input.Binding;
using Robust.Shared.Player;
using static Content.Shared.Input.ContentKeyFunctions;

namespace Content.Client._White.TargetDoll;

public sealed class TargetDollSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _playerManager = default!;

    public event Action<TargetDollComponent>? TargetDollStartup;
    public event Action? TargetDollShutdown;

    public event Action<BodyPart>? TargetChange;

    public override void Initialize()
    {
        SubscribeLocalEvent<TargetDollComponent, ComponentStartup>(OnTargetingStartup);
        SubscribeLocalEvent<TargetDollComponent, ComponentShutdown>(OnTargetingShutdown);
        SubscribeLocalEvent<TargetDollComponent, LocalPlayerAttachedEvent>(HandlePlayerAttached);
        SubscribeLocalEvent<TargetDollComponent, LocalPlayerDetachedEvent>(HandlePlayerDetached);

        #region Binds

        CommandBinds.Builder
            .Bind(TargetDollHead, InputCmdHandler.FromDelegate(session =>HandleTargetChange(session, BodyPart.Head)))
            .Bind(TargetDollChest, InputCmdHandler.FromDelegate(session => HandleTargetChange(session, BodyPart.Chest, BodyPart.Groin)))
            .Bind(TargetDollGroin, InputCmdHandler.FromDelegate(session => HandleTargetChange(session, BodyPart.Groin)))
            .Bind(TargetDollRightArm, InputCmdHandler.FromDelegate(session => HandleTargetChange(session, BodyPart.RightArm, BodyPart.RightHand)))
            .Bind(TargetDollRightHand, InputCmdHandler.FromDelegate(session => HandleTargetChange(session, BodyPart.RightHand)))
            .Bind(TargetDollLeftArm, InputCmdHandler.FromDelegate(session => HandleTargetChange(session, BodyPart.LeftArm, BodyPart.LeftHand)))
            .Bind(TargetDollLeftHand, InputCmdHandler.FromDelegate(session => HandleTargetChange(session, BodyPart.LeftHand)))
            .Bind(TargetDollRightLeg, InputCmdHandler.FromDelegate(session => HandleTargetChange(session, BodyPart.RightLeg, BodyPart.RightFoot)))
            .Bind(TargetDollRightFoot, InputCmdHandler.FromDelegate(session => HandleTargetChange(session, BodyPart.RightFoot)))
            .Bind(TargetDollLeftLeg, InputCmdHandler.FromDelegate(session => HandleTargetChange(session, BodyPart.LeftLeg, BodyPart.LeftFoot)))
            .Bind(TargetDollLeftFoot, InputCmdHandler.FromDelegate(session => HandleTargetChange(session, BodyPart.LeftFoot)))
            .Bind(TargetDollEyes, InputCmdHandler.FromDelegate(session => HandleTargetChange(session, BodyPart.Eyes)))
            .Bind(TargetDollMouth, InputCmdHandler.FromDelegate(session => HandleTargetChange(session, BodyPart.Mouth)))
            .Bind(TargetDollTail, InputCmdHandler.FromDelegate(session => HandleTargetChange(session, BodyPart.Tail)))
            .Register<TargetDollSystem>();

        #endregion
    }

    private void OnTargetingStartup(EntityUid uid, TargetDollComponent component, ComponentStartup args)
    {
        if (_playerManager.LocalEntity == uid)
            TargetDollStartup?.Invoke(component);
    }

    private void OnTargetingShutdown(EntityUid uid, TargetDollComponent component, ComponentShutdown args)
    {
        if (_playerManager.LocalEntity == uid)
            TargetDollShutdown?.Invoke();
    }

    private void HandlePlayerAttached(EntityUid uid, TargetDollComponent component, LocalPlayerAttachedEvent args) => TargetDollStartup?.Invoke(component);

    private void HandlePlayerDetached(EntityUid uid, TargetDollComponent component, LocalPlayerDetachedEvent args) => TargetDollShutdown?.Invoke();

    private void HandleTargetChange(ICommonSession? session, BodyPart bodyPart, BodyPart? alreadyTargetingBodyPart = null)
    {
        if (session is not { AttachedEntity: { } uid, }
            || !TryComp<TargetDollComponent>(uid, out var targeting)
            || targeting.Target == bodyPart)
        {
            if (alreadyTargetingBodyPart != null)
                TargetChange?.Invoke(alreadyTargetingBodyPart.Value);

            return;
        }

        TargetChange?.Invoke(bodyPart);
    }
}

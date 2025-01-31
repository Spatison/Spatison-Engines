using Content.Shared._White.TargetDoll;

namespace Content.Server._White.TargetDoll;

public sealed class TargetDollSystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeNetworkEvent<TargetDollChangeEvent>(OnTargetChange);
    }

    private void OnTargetChange(TargetDollChangeEvent message, EntitySessionEventArgs args)
    {
        var uid = GetEntity(message.Uid);
        if (!TryComp<TargetDollComponent>(uid, out var target))
            return;

        target.Target = message.BodyPart;
        Dirty(uid, target);
    }
}

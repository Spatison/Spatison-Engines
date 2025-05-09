using System.Numerics;
using Content.Server.Body.Components;
using Content.Shared._White.Body.Components;
using Content.Shared.Gibbing.Components;
using Robust.Shared.Audio;

namespace Content.Server._White.Body.Systems;

public sealed partial class BodySystem
{
    #region Public Api

    public override HashSet<EntityUid> GibBody(
        EntityUid bodyUid,
        bool gibOrgans = false,
        BodyComponent? body = null,
        GibbableComponent? gibbable = null,
        bool launchGibs = true,
        Vector2? splatDirection = null,
        float splatModifier = 1,
        Angle splatCone = default,
        SoundSpecifier? gibSoundOverride = null
    )
    {
        if (!Resolve(bodyUid, ref body, logMissing: false)
            || TerminatingOrDeleted(bodyUid)
            || EntityManager.IsQueuedForDeletion(bodyUid))
            return new HashSet<EntityUid>();

        var xform = Transform(bodyUid);
        if (xform.MapUid is null)
            return new HashSet<EntityUid>();

        var gibs = base.GibBody(
            bodyUid,
            gibOrgans,
            body,
            launchGibs: launchGibs,
            splatDirection: splatDirection,
            splatModifier: splatModifier,
            splatCone:splatCone);

        var ev = new BeingGibbedEvent(gibs);
        RaiseLocalEvent(bodyUid, ref ev);

        QueueDel(bodyUid);

        return gibs;
    }

    #endregion
}

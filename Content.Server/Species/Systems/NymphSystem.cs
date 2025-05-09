using Content.Server.Mind;
using Content.Shared.Species.Components;
using Content.Shared.Zombies;
using Content.Server.Zombies;
using Content.Shared._White.Body;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Server.Species.Systems;

public sealed partial class NymphSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _protoManager= default!;
    [Dependency] private readonly MindSystem _mindSystem = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly ZombieSystem _zombie = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NymphComponent, OrganRemovedEvent>(OnRemovedFromPart);
    }

    private void OnRemovedFromPart(EntityUid uid, NymphComponent comp, ref OrganRemovedEvent args)
    {
        if (!_timing.IsFirstTimePredicted)
            return;

        if (TerminatingOrDeleted(uid) || TerminatingOrDeleted(args.Body))
            return;

        if (!_protoManager.TryIndex<EntityPrototype>(comp.EntityPrototype, out var entityProto))
            return;

        // Get the organs' position & spawn a nymph there
        var coords = Transform(uid).Coordinates;
        var nymph = EntityManager.SpawnAtPosition(entityProto.ID, coords);

        if (HasComp<ZombieComponent>(args.Body)) // Zombify the new nymph if old one is a zombie
            _zombie.ZombifyEntity(nymph);

        // Move the mind if there is one and it's supposed to be transferred
        if (comp.TransferMind == true && args.Body.HasValue && _mindSystem.TryGetMind(args.Body.Value, out var mindId, out var mind)) // WD EDIT
            _mindSystem.TransferTo(mindId, nymph, mind: mind);

        // Delete the old organ
        QueueDel(uid);
    }
}

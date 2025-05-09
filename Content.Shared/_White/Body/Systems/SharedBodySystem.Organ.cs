using System.Diagnostics.CodeAnalysis;
using Content.Shared._White.Body.Components;
using Robust.Shared.Containers;

namespace Content.Shared._White.Body.Systems;

public abstract partial class SharedBodySystem
{
    private void InitOrgan()
    {
        SubscribeLocalEvent<OrganComponent, EntGotInsertedIntoContainerMessage>(OnOrganGotInserted);
        SubscribeLocalEvent<OrganComponent, EntGotRemovedFromContainerMessage>(OnOrganGotRemoved);
    }

    #region Event Handling

    private void OnOrganGotInserted(EntityUid uid, OrganComponent component, ref EntGotInsertedIntoContainerMessage args)
    {
        component.ParentPart = args.Container.Owner;

        if (TryComp<BodyPartComponent>(component.ParentPart, out var bodyPartComponent))
            component.Body = bodyPartComponent.Body;
        else if (TryComp<BoneComponent>(component.ParentPart, out var boneComponent))
            component.Body = boneComponent.Body;

        if (!component.Body.HasValue || !TryComp<BodyComponent>(component.Body, out var bodyComponent))
            return;

        RaiseLocalEvent(component.Body.Value, new OrganAddedEvent((component.Body.Value, bodyComponent)));
    }

    private void OnOrganGotRemoved(EntityUid uid, OrganComponent component, ref EntGotRemovedFromContainerMessage args)
    {
        var parent = args.Container.Owner;
        var body = component.Body;

        component.Body = null;
        component.ParentPart = null;

        if (!body.HasValue || !HasComp<BodyPartComponent>(parent) && !HasComp<BoneComponent>(parent))
            return;

        RaiseLocalEvent(body.Value, new OrganRemovedEvent());
    }

    #endregion

    #region Private API

    private void SetupOrgans(EntityUid parentUid, List<OrganSlot> organSlots)
    {
        foreach (var organSlot in organSlots)
        {
            if (organSlot.HasOrgan || string.IsNullOrEmpty(organSlot.StartingOrgan))
                continue;

            var organ = EntityManager.SpawnEntity(
                organSlot.StartingOrgan,
                EntityManager.GetComponent<TransformComponent>(parentUid).Coordinates);

            if (organSlot.ContainerSlot != null
                && TryComp<OrganComponent>(organ, out var organComponent)
                && (organComponent.OrganType & organSlot.Type) == 0
                && _container.Insert(organ, organSlot.ContainerSlot))
                continue;

            _sawmill.Error($"Couldn't insert {ToPrettyString(organ)} to {ToPrettyString(parentUid)}");
            QueueDel(organ);
        }
    }

    private void SetOrgansBody(EntityUid body, BodyComponent bodyComponent, IEnumerable<(EntityUid Uid, OrganComponent Component)> organs)
    {
        foreach (var organ in organs)
        {
            organ.Component.Body = body;
            RaiseLocalEvent(body, new OrganAddedEvent((body, bodyComponent)));
        }
    }

    #endregion

    #region Public API

    public IEnumerable<(EntityUid Uid, OrganComponent Component)> GetOrgans(EntityUid uid, BodyComponent? component = null)
    {
        if (!Resolve(uid, ref component, logMissing: false))
            yield break;

        foreach (var (_, bodyPartSlot) in component.BodyParts)
        {
            if (!bodyPartSlot.BodyPartUid.HasValue)
                continue;

            var bodyPartComponent = Comp<BodyPartComponent>(bodyPartSlot.BodyPartUid.Value);
            foreach (var organ in GetOrgans(bodyPartSlot.BodyPartUid.Value, bodyPartComponent))
                yield return organ;
        }
    }

    public IEnumerable<(EntityUid Uid, OrganComponent Component)> GetOrgans(EntityUid uid, BodyPartComponent? component = null)
    {
        if (!Resolve(uid, ref component, logMissing: false))
            yield break;

        foreach (var organSlot in component.Organs.Values)
        {
            if (!organSlot.OrganUid.HasValue)
                continue;

            var organComponent = Comp<OrganComponent>(organSlot.OrganUid.Value);
            yield return (organSlot.OrganUid.Value, organComponent);
        }

        foreach (var boneSlot in component.Bones.Values)
        {
            if (!boneSlot.BoneUid.HasValue)
                continue;

            var boneComponent = Comp<BoneComponent>(boneSlot.BoneUid.Value);
            foreach (var organ in GetOrgans(boneSlot.BoneUid.Value, boneComponent))
                yield return organ;
        }
    }

    public IEnumerable<(EntityUid Uid, OrganComponent Component)> GetOrgans(EntityUid uid, BoneComponent? component = null)
    {
        if (!Resolve(uid, ref component, logMissing: false))
            yield break;

        foreach (var organSlot in component.Organs.Values)
        {
            if (!organSlot.OrganUid.HasValue)
                continue;

            var organComponent = Comp<OrganComponent>(organSlot.OrganUid.Value);
            yield return (organSlot.OrganUid.Value, organComponent);
        }
    }

    public bool AddOrganToFirstValidSlot(
        EntityUid partId,
        EntityUid organId,
        BodyPartComponent? part = null,
        OrganComponent? organ = null)
    {
        if (!Resolve(partId, ref part, logMissing: false)
            || !Resolve(organId, ref organ, logMissing: false))
            return false;

        foreach (var slotId in part.Organs.Keys)
        {
            InsertOrgan(partId, organId, slotId, part, organ);
            return true;
        }

        return false;
    }

    public bool InsertOrgan(
        EntityUid partId,
        EntityUid organId,
        string slotId,
        BodyPartComponent? part = null,
        OrganComponent? organ = null)
    {
        if (!Resolve(organId, ref organ, logMissing: false)
            || !Resolve(partId, ref part, logMissing: false)
            || !CanInsertOrgan(partId, slotId, part))
            return false;

        return _container.TryGetContainer(partId, slotId, out var container)
            && _container.Insert(organId, container);
    }

    public bool CanInsertOrgan(
        EntityUid partId,
        string slotId,
        BodyPartComponent? part = null)
    {
        return Resolve(partId, ref part) && part.Organs.ContainsKey(slotId);
    }

    public List<(T Comp, OrganComponent Organ)> GetBodyOrganComponents<T>(
        EntityUid uid,
        BodyComponent? body = null)
        where T : IComponent
    {
        if (!Resolve(uid, ref body))
            return new List<(T Comp, OrganComponent Organ)>();

        var query = GetEntityQuery<T>();
        var list = new List<(T Comp, OrganComponent Organ)>(3);
        foreach (var organ in GetOrgans(uid, body))
        {
            if (query.TryGetComponent(organ.Uid, out var comp))
                list.Add((comp, organ.Component));
        }

        return list;
    }

    public bool TryGetBodyOrganComponents<T>(
        EntityUid uid,
        [NotNullWhen(true)] out List<(T Comp, OrganComponent Organ)>? comps,
        BodyComponent? body = null)
        where T : IComponent
    {
        if (!Resolve(uid, ref body))
        {
            comps = null;
            return false;
        }

        comps = GetBodyOrganComponents<T>(uid, body);

        if (comps.Count != 0)
            return true;

        comps = null;
        return false;
    }

    #endregion
}

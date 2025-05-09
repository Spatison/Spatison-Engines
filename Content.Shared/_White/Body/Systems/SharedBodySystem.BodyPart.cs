using System.Linq;
using Content.Shared._White.Body.Components;
using Robust.Shared.Containers;

namespace Content.Shared._White.Body.Systems;

public abstract partial class SharedBodySystem
{
    private void InitBodyPart()
    {
        SubscribeLocalEvent<BodyPartComponent, ComponentInit>(OnBodyPartInit);
        SubscribeLocalEvent<BodyPartComponent, MapInitEvent>(OnBodyPartMapInit);
        SubscribeLocalEvent<BodyPartComponent, EntGotInsertedIntoContainerMessage>(OnBodyPartGotInserted);
        SubscribeLocalEvent<BodyPartComponent, EntGotRemovedFromContainerMessage>(OnBodyPartGotRemoved);
    }

    #region Event Handling

    private void OnBodyPartInit(EntityUid uid, BodyPartComponent component, ComponentInit args)
    {
        foreach (var (bodyPartId, bodyPartSlot) in component.Children)
            bodyPartSlot.ContainerSlot = _container.EnsureContainer<ContainerSlot>(uid, bodyPartId);

        foreach (var (organId, organSlot) in component.Organs)
            organSlot.ContainerSlot = _container.EnsureContainer<ContainerSlot>(uid, organId);

        foreach (var (boneId, boneSlot) in component.Bones)
            boneSlot.ContainerSlot = _container.EnsureContainer<ContainerSlot>(uid, boneId);
    }

    private void OnBodyPartMapInit(EntityUid uid, BodyPartComponent component, MapInitEvent args)
    {
        SetupOrgans(uid, component.Organs.Values.ToList());

        SetupBones(uid, component.Bones.Values.ToList());

        SetupBodyParts(uid, component.Children.Values.ToList());
    }

    private void OnBodyPartGotInserted(EntityUid uid, BodyPartComponent component, ref EntGotInsertedIntoContainerMessage args)
    {
        component.ParentPart = args.Container.Owner;

        if (TryComp<BodyComponent>(component.ParentPart, out var bodyComponent))
            component.Body = component.ParentPart;
        else if (TryComp<BodyPartComponent>(component.ParentPart, out var parentBodyPartComponent))
            component.Body = parentBodyPartComponent.Body;

        if (!component.Body.HasValue || !Resolve(component.Body.Value, ref bodyComponent))
            return;

        foreach (var bones in GetBone(uid, component))
        {
            bones.Component.Body = component.Body;

            SetOrgansBody(component.Body.Value, bodyComponent, GetOrgans(bones.Uid, bones.Component));

            RaiseLocalEvent(component.Body.Value, new BoneAddedEvent());
        }

        SetOrgansBody(component.Body.Value, bodyComponent, GetOrgans(uid, component));

        var ev = new BodyPartAddedEvent(
            (uid, component),
            (component.Body.Value, bodyComponent),
            args.Container.ID);
        RaiseLocalEvent(component.Body.Value, ev);
    }

    private void OnBodyPartGotRemoved(EntityUid uid, BodyPartComponent component, ref EntGotRemovedFromContainerMessage args)
    {
        var body = component.Body;

        component.Body = null;
        component.ParentPart = null;

        if (!TryComp<BodyComponent>(body, out var bodyComponent))
            return;

        var ev = new BodyPartRemovedEvent(
            (uid, component),
            (body.Value, bodyComponent),
            args.Container.ID);
        RaiseLocalEvent(body.Value, ev);
    }

    #endregion

    #region Private Api

    private bool TryCreateBodyPartSlot(
        EntityUid bodyPart,
        string slotId,
        BodyPart partType,
        BodyPartComponent? bodyPartComponent = null
    )
    {
        if (!Resolve(bodyPart, ref bodyPartComponent, logMissing: false) || bodyPartComponent.Body is null)
            return false;

        var bodyComponent = Comp<BodyComponent>(bodyPartComponent.Body.Value);
        var container = _container.EnsureContainer<ContainerSlot>(bodyPart, slotId);
        var bodyPartSlot = new BodyPartSlot();

        if (!bodyComponent.BodyParts.TryAdd(slotId, bodyPartSlot))
            return false;

        if (!bodyPartComponent.Children.TryAdd(slotId, bodyPartSlot))
        {
            bodyComponent.BodyParts.Remove(slotId);
            return false;
        }

        Dirty(bodyPart, bodyPartComponent);
        return true;
    }

    private bool CanAttachBodyPart(
        EntityUid parentBodyPart,
        string slotId,
        EntityUid childBodyPart,
        BodyPartComponent? parentBodyPartComponent = null,
        BodyPartComponent? childBodyPartComponent = null
    ) =>
        Resolve(parentBodyPart, ref parentBodyPartComponent, false)
        && Resolve(childBodyPart, ref childBodyPartComponent, false)
        && parentBodyPartComponent.Children.TryGetValue(slotId, out var parentSlot)
        && parentSlot.ContainerSlot is not null
        && (parentSlot.Type & childBodyPartComponent.PartType) != 0
        && _container.CanInsert(childBodyPart, parentSlot.ContainerSlot);

    private void SetupBodyParts(EntityUid parentUid, List<BodyPartSlot> bodyPartSlots)
    {
        foreach (var bodyPartSlot in bodyPartSlots)
        {
            if (bodyPartSlot.HasBodyPart || string.IsNullOrEmpty(bodyPartSlot.StartingBodyPart))
                continue;

            var bodyPart = EntityManager.SpawnEntity(
                bodyPartSlot.StartingBodyPart,
                EntityManager.GetComponent<TransformComponent>(parentUid).Coordinates);

            if (bodyPartSlot.ContainerSlot == null
                || !TryComp<BodyPartComponent>(bodyPart, out var bodyPartComponent)
                || (bodyPartComponent.PartType & bodyPartSlot.Type) == 0
                || !_container.Insert(bodyPart, bodyPartSlot.ContainerSlot))
            {
                _sawmill.Error($"Couldn't insert {ToPrettyString(bodyPart)} to {ToPrettyString(parentUid)}");
                QueueDel(bodyPart);
                continue;
            }

            SetupBones(bodyPart, bodyPartSlot.BonesData.Values.ToList());
            SetupOrgans(bodyPart, bodyPartSlot.OrgansData.Values.ToList());
        }
    }

    #endregion

    #region Public API

    /// <summary>
    /// Gets body parts of this entity.
    /// </summary>
    public IEnumerable<(EntityUid Uid, BodyPartComponent Component)> GetBodyParts(
        EntityUid uid,
        BodyComponent? component = null,
        BodyPart? type = null
        )
    {
        if (!Resolve(uid, ref component, logMissing: false))
            yield break;

        foreach (var bodyPartSlot in component.BodyParts.Values)
        {
            if (!TryComp<BodyPartComponent>(bodyPartSlot.BodyPartUid, out var bodyPartComponent)
                || (type & bodyPartComponent.PartType) == 0)
                continue;

            yield return (bodyPartSlot.BodyPartUid.Value, bodyPartComponent);
        }
    }

    public IEnumerable<BodyPartSlot> GetBodyPartSlots(
        EntityUid uid,
        BodyPart type,
        BodyComponent? component = null,
        bool getEmpty = false
        )
    {
        if (!Resolve(uid, ref component, logMissing: false))
            yield break;

        foreach (var (_, bodyPartSlot) in component.BodyParts)
        {
            if ((type & bodyPartSlot.Type) == 0)
                continue;

            if (getEmpty && bodyPartSlot.HasBodyPart)
                continue;

            yield return bodyPartSlot;
        }
    }

    public bool BodyHasBodyPart(EntityUid body, EntityUid bodyPart, BodyComponent? bodyComponent = null)
    {
        if (!Resolve(body, ref bodyComponent, false))
            return false;

        foreach (var (_, bodyPartSlot) in bodyComponent.BodyParts)
            if (bodyPartSlot.BodyPartUid == bodyPart)
                return true;

        return false;
    }

    public bool TryCreateBodyPartSlotAndAttach(
        EntityUid parentId,
        string slotId,
        EntityUid childId,
        BodyPart partType,
        BodyPartComponent? parent = null,
        BodyPartComponent? child = null
        ) =>
        TryCreateBodyPartSlot(parentId, slotId, partType, parent)
        && AttachBodyPart(parentId, slotId, childId, parent, child);

    public bool IsRootBodyPart(
        EntityUid body,
        EntityUid bodyPart,
        BodyComponent? bodyComponent = null,
        BodyPartComponent? bodyPartComponentart = null
        ) =>
        Resolve(bodyPart, ref bodyPartComponentart)
        && Resolve(body, ref bodyComponent)
        && _container.TryGetContainingContainer(body, bodyPart, out var container)
        && bodyComponent.BodyParts.ContainsKey(container.ID);

    public bool AttachBodyPart(
        EntityUid parentBodyPart,
        string slotId,
        EntityUid childBodyPart,
        BodyPartComponent? parentBodyPartComponent = null,
        BodyPartComponent? childBodyPartComponent = null
        )
    {
        if (!Resolve(parentBodyPart, ref parentBodyPartComponent, false)
            || !Resolve(childBodyPart, ref childBodyPartComponent, false)
            || !CanAttachBodyPart(parentBodyPart, slotId, childBodyPart, parentBodyPartComponent, childBodyPartComponent)
            || !parentBodyPartComponent.Children.TryGetValue(slotId, out var child)
            || child.ContainerSlot is null)
            return false;

        return _container.Insert(childBodyPart, child.ContainerSlot);
    }

    #endregion
}

using System.Linq;
using Content.Shared._White.Body.Components;
using Robust.Shared.Containers;

namespace Content.Shared._White.Body.Systems;

public abstract partial class SharedBodySystem
{
    private void InitBone()
    {
        SubscribeLocalEvent<BoneComponent, ComponentInit>(OnBoneInit);
        SubscribeLocalEvent<BoneComponent, MapInitEvent>(OnBoneMapInit);
        SubscribeLocalEvent<BoneComponent, EntGotInsertedIntoContainerMessage>(OnBoneGotInserted);
        SubscribeLocalEvent<BoneComponent, EntGotRemovedFromContainerMessage>(OnBoneGotRemoved);
    }

    # region Event Handling

    private void OnBoneInit(EntityUid uid, BoneComponent component, ComponentInit args)
    {
        foreach (var (organContainerId, organSlot) in component.Organs)
            organSlot.ContainerSlot = _container.EnsureContainer<ContainerSlot>(uid, organContainerId);
    }

    private void OnBoneMapInit(EntityUid uid, BoneComponent component, MapInitEvent args) =>
        SetupOrgans(uid, component.Organs.Values.ToList());

    private void OnBoneGotInserted(EntityUid uid, BoneComponent component, ref EntGotInsertedIntoContainerMessage args)
    {
        component.ParentPart = args.Container.Owner;
        if (!TryComp<BodyPartComponent>(component.ParentPart, out var bodyPartComponent)
            || bodyPartComponent.Body is null
            || !TryComp<BodyComponent>(bodyPartComponent.Body, out var bodyComponent))
            return;

        component.Body = bodyPartComponent.Body;

        SetOrgansBody(component.Body.Value, bodyComponent, GetOrgans(uid, component));

        RaiseLocalEvent(component.Body.Value, new BoneAddedEvent());
    }

    private void OnBoneGotRemoved(EntityUid uid, BoneComponent component, ref EntGotRemovedFromContainerMessage args)
    {
        var parent = args.Container.Owner;

        component.Body = null;
        component.ParentPart = null;

        if (!TryComp<BodyPartComponent>(parent, out var bodyPartComponent) || bodyPartComponent.Body is null)
            return;

        foreach (var organs in GetOrgans(uid, component))
        {
            organs.Component.Body = null;
            RaiseLocalEvent(bodyPartComponent.Body.Value, new OrganRemovedEvent());
        }

        RaiseLocalEvent(bodyPartComponent.Body.Value, new BoneRemovedEvent());
    }

    # endregion

    #region Private API

    private void SetupBones(EntityUid parentUid, List<BoneSlot> boneSlots)
    {
        foreach (var boneSlot in boneSlots)
        {
            EntityUid? bone = null;
            BoneComponent? boneComponent = null;

            if (boneSlot.BoneUid.HasValue && Resolve(boneSlot.BoneUid.Value, ref boneComponent))
            {
                bone = boneSlot.BoneUid;
            }
            else if (!string.IsNullOrEmpty(boneSlot.StartingBone))
            {
                bone = EntityManager.SpawnEntity(
                    boneSlot.StartingBone,
                    EntityManager.GetComponent<TransformComponent>(parentUid).Coordinates);

                if (boneSlot.ContainerSlot == null
                    || !Resolve(bone.Value, ref boneComponent)
                    || (boneComponent.BoneType & boneSlot.Type) == 0
                    || !_container.Insert(bone.Value, boneSlot.ContainerSlot))
                {
                    _sawmill.Error($"Couldn't insert {ToPrettyString(bone)} to {ToPrettyString(parentUid)}");
                    QueueDel(bone);
                    continue;
                }
            }

            if (boneComponent == null || bone == null)
                continue;

            foreach (var (organId, organSlot) in boneSlot.OrgansData)
            {
                if (!boneComponent.Organs.TryGetValue(organId, out var originalOrganSlot))
                {
                    boneComponent.Organs[organId] = organSlot;
                    continue;
                }

                originalOrganSlot.Copy(organSlot);
            }

            boneSlot.OrgansData = boneComponent.Organs;
            SetupOrgans(bone.Value, boneSlot.OrgansData.Values.ToList());
        }
    }

    private IEnumerable<(EntityUid Uid, BoneComponent Component)> GetBone(EntityUid uid, BodyComponent? component = null)
    {
        if (!Resolve(uid, ref component, logMissing: false))
            yield break;

        foreach (var bodyPartSlot in component.BodyParts.Values)
        {
            if (!TryComp<BodyPartComponent>(bodyPartSlot.BodyPartUid, out var bodyPartComponent))
                continue;

            foreach (var bone in GetBone(bodyPartSlot.BodyPartUid.Value, bodyPartComponent))
                yield return bone;
        }
    }

    private IEnumerable<(EntityUid Uid, BoneComponent Component)> GetBone(EntityUid uid, BodyPartComponent? component = null)
    {
        if (!Resolve(uid, ref component, logMissing: false))
            yield break;

        foreach (var boneSlot in component.Bones.Values)
        {
            if (!TryComp<BoneComponent>(boneSlot.BoneUid, out var boneComponent))
                continue;

            yield return (boneSlot.BoneUid.Value, boneComponent);
        }
    }

    #endregion
}

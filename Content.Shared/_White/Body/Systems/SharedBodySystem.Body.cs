using System.Linq;
using System.Numerics;
using Content.Shared._White.Body.Components;
using Content.Shared.Gibbing.Components;
using Content.Shared.Gibbing.Events;
using Content.Shared.Inventory;
using Robust.Shared.Audio;
using Robust.Shared.Containers;

namespace Content.Shared._White.Body.Systems;

public abstract partial class SharedBodySystem
{
    private void InitBody()
    {
        SubscribeLocalEvent<BodyComponent, ComponentInit>(OnBodyInit);
        SubscribeLocalEvent<BodyComponent, MapInitEvent>(OnBodyMapInit);
    }

    #region Event Handling

    private void OnBodyInit(EntityUid uid, BodyComponent component, ref ComponentInit args)
    {
        if (!component.BodyParts.TryGetValue(component.RootBodyPartId, out var bodyPartSlot))
        {
            _sawmill.Error($"Body dont have parts with ID: {component.RootBodyPartId}");
            return;
        }

        bodyPartSlot.ContainerSlot = _container.EnsureContainer<ContainerSlot>(uid, component.RootBodyPartId);
    }

    private void OnBodyMapInit(EntityUid uid, BodyComponent component, ref MapInitEvent args)
    {
        var queue = new Queue<(string BodyPartId, EntityUid BodyPartParent)>();
        queue.Enqueue((component.RootBodyPartId, uid));

        while (queue.TryDequeue(out var id))
        {
            if (!component.BodyParts.TryGetValue(id.BodyPartId, out var bodyPartSlot) || bodyPartSlot.HasBodyPart)
                continue;

            bodyPartSlot.ContainerSlot = _container.EnsureContainer<ContainerSlot>(id.BodyPartParent, id.BodyPartId);

            var bodyPart = EntityManager.SpawnEntity(
                bodyPartSlot.StartingBodyPart,
                EntityManager.GetComponent<TransformComponent>(uid).Coordinates);

            if (!TryComp<BodyPartComponent>(bodyPart, out var bodyPartComponent)
                || (bodyPartComponent.PartType & bodyPartSlot.Type) == 0
                || !_container.Insert(bodyPart, bodyPartSlot.ContainerSlot))
            {
                _sawmill.Error($"Couldn't insert {ToPrettyString(bodyPart)} to {ToPrettyString(id.BodyPartParent)}");
                QueueDel(bodyPart);
                continue;
            }

            foreach (var (organId, organSlot) in bodyPartSlot.OrgansData)
            {
                if (!bodyPartComponent.Organs.TryGetValue(organId, out var originalOrganSlot))
                {
                    bodyPartComponent.Organs[organId] = organSlot;
                    continue;
                }

                originalOrganSlot.Copy(organSlot);
            }

            bodyPartSlot.OrgansData = bodyPartComponent.Organs;
            SetupOrgans(bodyPart, bodyPartSlot.OrgansData.Values.ToList());

            foreach (var (boneId, boneSlot) in bodyPartSlot.BonesData)
            {
                if (!bodyPartComponent.Bones.TryGetValue(boneId, out var originalBoneSlot))
                {
                    bodyPartComponent.Bones[boneId] = boneSlot;
                    continue;
                }

                originalBoneSlot.Copy(boneSlot);
            }

            bodyPartSlot.BonesData = bodyPartComponent.Bones;
            SetupBones(bodyPart, bodyPartSlot.BonesData.Values.ToList());

            foreach (var childBodyPart in bodyPartSlot.ChildBodyPart)
                queue.Enqueue((childBodyPart, bodyPart));
        }
    }

    #endregion

    #region Public API

    // TODO: Is this really not supposed to be in GibbingSystem?
    public virtual HashSet<EntityUid> GibBody(
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
        var gibs = new HashSet<EntityUid>();

        if (!Resolve(bodyUid, ref body, logMissing: false))
            return gibs;

        if (Resolve(bodyUid, ref gibbable, logMissing: false))
            gibSoundOverride ??= gibbable.GibSound;

        var parts = GetBodyParts(bodyUid, body).ToArray();
        gibs.EnsureCapacity(parts.Length);

        foreach (var part in parts)
        {
            _gibbing.TryGibEntityWithRef(
                bodyUid,
                part.Uid,
                GibType.Gib,
                GibContentsOption.Skip,
                ref gibs,
                playAudio: false,
                launchGibs:true,
                launchDirection:splatDirection,
                launchImpulse: GibletLaunchImpulse * splatModifier,
                launchImpulseVariance:GibletLaunchImpulseVariance,
                launchCone: splatCone);

            if (!gibOrgans)
                continue;

            foreach (var (_, organContainer) in part.Component.Organs)
            {
                if (organContainer.OrganUid is null)
                    continue;

                _gibbing.TryGibEntityWithRef(
                    bodyUid,
                    organContainer.OrganUid.Value,
                    GibType.Drop,
                    GibContentsOption.Skip,
                    ref gibs,
                    playAudio: false,
                    launchImpulse: GibletLaunchImpulse * splatModifier,
                    launchImpulseVariance:GibletLaunchImpulseVariance,
                    launchCone: splatCone);
            }
        }

        if (HasComp<InventoryComponent>(bodyUid))
        {
            foreach (var item in _inventory.GetHandOrInventoryEntities(bodyUid))
            {
                _transform.AttachToGridOrMap(item);
                gibs.Add(item);
            }
        }

        _audio.PlayPredicted(gibSoundOverride, Transform(bodyUid).Coordinates, null);
        return gibs;
    }

    #endregion
}

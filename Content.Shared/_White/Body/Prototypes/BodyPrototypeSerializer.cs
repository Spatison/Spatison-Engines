using System.Linq;
using Content.Shared._White.Body.Components;
using Content.Shared.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.Manager;
using Robust.Shared.Serialization.Markdown.Mapping;
using Robust.Shared.Serialization.Markdown.Sequence;
using Robust.Shared.Serialization.Markdown.Validation;
using Robust.Shared.Serialization.Markdown.Value;
using Robust.Shared.Serialization.TypeSerializers.Interfaces;

namespace Content.Shared._White.Body.Prototypes;

[TypeSerializer]
public sealed class BodyPrototypeSerializer : ITypeReader<BodyPrototype, MappingDataNode>
{
    public ValidationNode Validate(
        ISerializationManager serializationManager,
        MappingDataNode node,
        IDependencyCollection dependencies,
        ISerializationContext? context = null
    )
    {
        var nodes = new List<ValidationNode>();

        if (!node.TryGet("root", out ValueDataNode? root))
        {
            nodes.Add(new ErrorNode(node, "No root value data node found"));
            return new ValidatedSequenceNode(nodes);
        }

        if (!node.TryGet("slots", out MappingDataNode? slots))
        {
            nodes.Add(new ErrorNode(node, "No slots mapping data node found"));
            return new ValidatedSequenceNode(nodes);
        }

        if (!slots.TryGet(root.Value, out MappingDataNode? _))
        {
            nodes.Add(new ErrorNode(slots, $"No slot found with id {root.Value}"));
            return new ValidatedSequenceNode(nodes);
        }

        foreach (var (key, value) in slots)
        {
            if (key is not ValueDataNode)
            {
                nodes.Add(new ErrorNode(key, $"Key is not a value data node"));
                continue;
            }

            if (value is not MappingDataNode slot)
            {
                nodes.Add(new ErrorNode(value, $"Slot is not a mapping data node"));
                continue;
            }

            var result = ValidateSlot(slot, dependencies);
            nodes.Add(result.Node);

            foreach (var connection in result.Connections)
                if (!slots.TryGet(connection, out MappingDataNode? _))
                    nodes.Add(new ErrorNode(slots, $"No slot found with id {connection}"));
        }

        return new ValidatedSequenceNode(nodes);
    }

    public BodyPrototype Read(
        ISerializationManager serializationManager,
        MappingDataNode node,
        IDependencyCollection dependencies,
        SerializationHookContext hookCtx,
        ISerializationContext? context = null,
        ISerializationManager.InstantiationDelegate<BodyPrototype>? instanceProvider = null
        )
    {
        var id = node.Get<ValueDataNode>("id").Value;
        var name = node.Get<ValueDataNode>("name").Value;
        var root = node.Get<ValueDataNode>("root").Value;
        var slotNodes = node.Get<MappingDataNode>("slots");
        var allConnections = new Dictionary<string, (string? Part, HashSet<string>? Connections, Dictionary<string, string>? Organs, Dictionary<string, BoneSlot>? Bones)>();

        foreach (var (keyNode, valueNode) in slotNodes)
        {
            var slotId = ((ValueDataNode) keyNode).Value;
            var slot = (MappingDataNode) valueNode;

            string? part = null;
            if (slot.TryGet<ValueDataNode>("part", out var value))
                part = value.Value;

            HashSet<string>? connections = null;
            if (slot.TryGet("connections", out SequenceDataNode? slotConnectionsNode))
            {
                connections = new HashSet<string>();

                foreach (var connection in slotConnectionsNode.Cast<ValueDataNode>())
                    connections.Add(connection.Value);
            }

            Dictionary<string, BoneSlot>? bones = null;
            if (slot.TryGet("bones", out MappingDataNode? slotBonesNode))
            {
                bones = new Dictionary<string, BoneSlot>();

                foreach (var (boneKeyNode, boneValueNode) in slotBonesNode)
                {
                    var mappingBoneData = (MappingDataNode) boneValueNode;
                    var boneSlot = new BoneSlot(null, new Dictionary<string, string>());

                    if (mappingBoneData.TryGet("bone", out ValueDataNode? boneSlotBoneNode))
                        boneSlot = boneSlot with { Bone = boneSlotBoneNode.Value, };

                    if (!mappingBoneData.TryGet("organs", out MappingDataNode? boneSlotOrgansNode))
                        continue;

                    foreach (var (organKeyNode, organValueNode) in boneSlotOrgansNode)
                        boneSlot.Organs.Add(((ValueDataNode) organKeyNode).Value, ((ValueDataNode) organValueNode).Value);

                    bones.Add(((ValueDataNode) boneKeyNode).Value, boneSlot);
                }
            }

            Dictionary<string, string>? organs = null;
            if (slot.TryGet("organs", out MappingDataNode? slotOrgansNode))
            {
                organs = new Dictionary<string, string>();

                foreach (var (organKeyNode, organValueNode) in slotOrgansNode)
                    organs.Add(((ValueDataNode) organKeyNode).Value, ((ValueDataNode) organValueNode).Value);
            }

            allConnections.Add(slotId, (part, connections, organs, bones));
        }

        foreach (var (slotId, (_, connections, _, _)) in allConnections)
        {
            if (connections == null)
                continue;

            foreach (var connection in connections)
            {
                var other = allConnections[connection];
                other.Connections ??= new HashSet<string>();
                other.Connections.Add(slotId);
                allConnections[connection] = other;
            }
        }

        var slots = new Dictionary<string, BodyPrototypeSlot>();

        foreach (var (slotId, (part, connections, organs, bones)) in allConnections)
        {
            var slot = new BodyPrototypeSlot(part, connections ?? new HashSet<string>(), organs ?? new Dictionary<string, string>(), bones ?? new Dictionary<string, BoneSlot>());
            slots.Add(slotId, slot);
        }

        return new BodyPrototype(id, name, root, slots);
    }

    private (ValidationNode Node, List<string> Connections) ValidateSlot(MappingDataNode slot, IDependencyCollection dependencies)
    {
        var nodes = new List<ValidationNode>();
        var connections = new List<string>();

        var prototypes = dependencies.Resolve<IPrototypeManager>();
        var factory = dependencies.Resolve<IComponentFactory>();

        if (slot.TryGet("connections", out SequenceDataNode? connectionsNode))
        {
            foreach (var node in connectionsNode)
            {
                if (node is not ValueDataNode connection)
                {
                    nodes.Add(new ErrorNode(node, $"Connection is not a value data node"));
                    continue;
                }

                connections.Add(connection.Value);
            }
        }

        if (slot.TryGet("bones", out MappingDataNode? bonesNode))
        {
            foreach (var (key, value) in bonesNode)
            {
                if (key is not ValueDataNode)
                {
                    nodes.Add(new ErrorNode(key, $"Key is not a value data node"));
                    continue;
                }

                if (value is not ValueDataNode bone)
                {
                    nodes.Add(new ErrorNode(value, $"Value is not a value data node"));
                    continue;
                }

                if (!prototypes.TryIndex(bone.Value, out EntityPrototype? organPrototype))
                {
                    nodes.Add(new ErrorNode(value, $"No bone entity prototype found with id {bone.Value}"));
                    continue;
                }

                if (!organPrototype.HasComponent<BoneComponent>(factory))
                {
                    nodes.Add(new ErrorNode(value, $"Bone {bone.Value} does not have a bone component"));
                    continue;
                }

                if (!bonesNode.TryGet("organs", out MappingDataNode? organs))
                {
                    nodes.Add(new ErrorNode(bonesNode, "No organs mapping data node found"));
                    continue;
                }

                nodes.Add(OrganValidate(organs, dependencies));
            }
        }

        nodes.Add(OrganValidate(slot, dependencies));
        var validation = new ValidatedSequenceNode(nodes);
        return (validation, connections);
    }

    private ValidationNode OrganValidate(MappingDataNode slot, IDependencyCollection dependencies)
    {
        var nodes = new List<ValidationNode>();

        var prototypes = dependencies.Resolve<IPrototypeManager>();
        var factory = dependencies.Resolve<IComponentFactory>();

        if (!slot.TryGet("organs", out MappingDataNode? organsNode))
            return new ValidatedSequenceNode(nodes);

        foreach (var (key, value) in organsNode)
        {
            if (key is not ValueDataNode)
            {
                nodes.Add(new ErrorNode(key, $"Key is not a value data node"));
                continue;
            }

            if (value is not ValueDataNode organ)
            {
                nodes.Add(new ErrorNode(value, $"Value is not a value data node"));
                continue;
            }

            if (!prototypes.TryIndex(organ.Value, out EntityPrototype? organPrototype))
            {
                nodes.Add(new ErrorNode(value, $"No organ entity prototype found with id {organ.Value}"));
                continue;
            }

            if (!organPrototype.HasComponent<OrganComponent>(factory))
                nodes.Add(new ErrorNode(value, $"Organ {organ.Value} does not have a organ component"));
        }

        return new ValidatedSequenceNode(nodes);
    }
}

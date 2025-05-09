using Content.Shared.Gibbing.Systems;
using Content.Shared.Inventory;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;

namespace Content.Shared._White.Body.Systems;

public abstract partial class SharedBodySystem : EntitySystem
{
    /// <summary>
    /// Container ID prefix for any body parts.
    /// </summary>
    public const string BodyPartIdPrefix = "body_part_";

    /// <summary>
    /// Container ID prefix for any body organs.
    /// </summary>
    public const string OrganIdPrefix = "body_organ_";

    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly GibbingSystem _gibbing = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    private const float GibletLaunchImpulse = 8;
    private const float GibletLaunchImpulseVariance = 3;

    private ISawmill _sawmill = default!;

    public override void Initialize()
    {
        base.Initialize();

        _sawmill = Logger.GetSawmill("body");

        InitBody();
        InitBodyPart();
        InitBone();
        InitOrgan();
    }
}

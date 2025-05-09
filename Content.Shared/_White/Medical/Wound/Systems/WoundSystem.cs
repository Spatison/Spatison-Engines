using Robust.Shared.Containers;

namespace Content.Shared._White.Medical.Wound.Systems;

[Virtual]
public partial class WoundSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _container = default!;

    private ISawmill _sawmill = default!;

    public override void Initialize()
    {
        base.Initialize();

        _sawmill = Logger.GetSawmill("wound");

        InitWounding();
    }

    /*public override void Update(float frameTime)
    {
        base.Update(frameTime);

        UpdateHealing(frameTime);
    }*/
}

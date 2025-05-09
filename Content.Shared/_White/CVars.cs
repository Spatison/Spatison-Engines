using Robust.Shared.Configuration;

namespace Content.Shared._White;

[CVarDefs]
public sealed class WhiteCVars
{
    #region Wounding

    /// <summary>
    /// How many times per second do we want to heal wounds.
    /// </summary>
    public static readonly CVarDef<float> MedicalHealingTickrate =
        CVarDef.Create("medical.heal_tickrate", 0.5f, CVar.SERVER | CVar.REPLICATED);

    #endregion
}

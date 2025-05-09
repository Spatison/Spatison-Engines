using Content.Shared._White.Body;
using Content.Shared._White.Body.Components;


namespace Content.Shared.Humanoid
{
    public static class HumanoidVisualLayersExtension
    {
        public static bool HasSexMorph(HumanoidVisualLayers layer)
        {
            return layer switch
            {
                HumanoidVisualLayers.Chest => true,
                HumanoidVisualLayers.Head => true,
                _ => false
            };
        }

        public static string GetSexMorph(HumanoidVisualLayers layer, Sex sex, string id)
        {
            if (!HasSexMorph(layer) || sex == Sex.Unsexed)
                return id;

            return $"{id}{sex}";
        }

        /// <summary>
        ///     Sublayers. Any other layers that may visually depend on this layer existing.
        ///     For example, the head has layers such as eyes, hair, etc. depending on it.
        /// </summary>
        /// <param name="layer"></param>
        /// <returns>Enumerable of layers that depend on that given layer. Empty, otherwise.</returns>
        /// <remarks>This could eventually be replaced by a body system implementation.</remarks>
        public static IEnumerable<HumanoidVisualLayers> Sublayers(HumanoidVisualLayers layer)
        {
            switch (layer)
            {
                case HumanoidVisualLayers.Head:
                    yield return HumanoidVisualLayers.Head;
                    yield return HumanoidVisualLayers.Eyes;
                    yield return HumanoidVisualLayers.HeadSide;
                    yield return HumanoidVisualLayers.HeadTop;
                    yield return HumanoidVisualLayers.Hair;
                    yield return HumanoidVisualLayers.FacialHair;
                    yield return HumanoidVisualLayers.Snout;
                    break;
                case HumanoidVisualLayers.LArm:
                    yield return HumanoidVisualLayers.LArm;
                    yield return HumanoidVisualLayers.LHand;
                    break;
                case HumanoidVisualLayers.RArm:
                    yield return HumanoidVisualLayers.RArm;
                    yield return HumanoidVisualLayers.RHand;
                    break;
                case HumanoidVisualLayers.LLeg:
                    yield return HumanoidVisualLayers.LLeg;
                    yield return HumanoidVisualLayers.LFoot;
                    break;
                case HumanoidVisualLayers.RLeg:
                    yield return HumanoidVisualLayers.RLeg;
                    yield return HumanoidVisualLayers.RFoot;
                    break;
                case HumanoidVisualLayers.Chest:
                    yield return HumanoidVisualLayers.Chest;
                    yield return HumanoidVisualLayers.Tail;
                    break;
                default:
                    yield break;
            }
        }

        public static HumanoidVisualLayers? ToHumanoidLayers(this BodyPartComponent part)
        {
            // WD EDIT START
            switch (part.PartType)
            {
                case BodyPart.Other:
                    break;
                case BodyPart.Head:
                    return HumanoidVisualLayers.Head;
                case BodyPart.Chest:
                    return HumanoidVisualLayers.Chest;
                case BodyPart.Groin:
                    return HumanoidVisualLayers.Groin;
                case BodyPart.RightArm:
                    return HumanoidVisualLayers.RArm;
                case BodyPart.RightHand:
                    return HumanoidVisualLayers.RHand;
                case BodyPart.LeftArm:
                    return HumanoidVisualLayers.LArm;
                case BodyPart.LeftHand:
                    return HumanoidVisualLayers.LHand;
                case BodyPart.RightLeg:
                    return HumanoidVisualLayers.RLeg;
                case BodyPart.RightFoot:
                    return HumanoidVisualLayers.RFoot;
                case BodyPart.LeftLeg:
                    return HumanoidVisualLayers.LLeg;
                case BodyPart.LeftFoot:
                    return HumanoidVisualLayers.LFoot;
                case BodyPart.Tail:
                    return HumanoidVisualLayers.Tail;
            }
            // WD EDIT END

            return null;
        }
    }
}

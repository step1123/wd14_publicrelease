using Content.Shared.Damage.Prototypes;
using Content.Shared.FixedPoint;
using Robust.Client.AutoGenerated;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Prototypes;

namespace Content.Client.White.Medical.BodyScanner
{
    [GenerateTypedNameReferences]
    public sealed partial class GroupDamageCardComponent : Control
    {
        public GroupDamageCardComponent(string title, string damageGroupID, IReadOnlyDictionary<string, FixedPoint2> damagePerType)
        {
            RobustXamlLoader.Load(this);

            DamageGroupTitle.Text = title;

            HashSet<string> shownTypes = new();
            var protos = IoCManager.Resolve<IPrototypeManager>();
            var group = protos.Index<DamageGroupPrototype>(damageGroupID);

            // Show the damage for each type in that group.
            foreach (var type in group.DamageTypes)
            {
                if (damagePerType.TryGetValue(type, out var typeAmount))
                {
                    // If damage types are allowed to belong to more than one damage group, they may appear twice here. Mark them as duplicate.
                    if (!shownTypes.Contains(type))
                    {
                        shownTypes.Add(type);

                        Label damagePerTypeLabel = new()
                        {
                            Text = Loc.GetString("health-analyzer-window-damage-type-" + type, ("amount", typeAmount)),
                        };

                        SetColorLabel(damagePerTypeLabel, typeAmount.Float());

                        DamageLabelsContainer.AddChild(damagePerTypeLabel);
                    }
                }
            }
        }

        /// <summary>
        /// Sets the color of a label depending on the damage.
        /// </summary>
        /// <param name="label"></param>
        /// <param name="damage"></param>
        private void SetColorLabel(Label label, float damage)
        {
            var startColor = Color.White;
            var critColor = Color.Yellow;
            var endColor = Color.Red;

            var startDamage = 0f;
            var critDamage = 30f;
            var endDamage = 100f;

            if (damage <= startDamage)
            {
                label.FontColorOverride = startColor;
            }
            else if (damage >= endDamage)
            {
                label.FontColorOverride = endColor;
            }
            else if (damage >= startDamage && damage <= critDamage)
            {
                // We need a number from 0 to 100.
                damage *= 100f / (critDamage - startDamage);
                label.FontColorOverride = GetColorLerp(startColor, critColor, damage);
            }
            else if (damage >= critDamage && damage <= endDamage)
            {
                // We need a number from 0 to 100.
                damage *= 100f / (endDamage - critDamage);
                label.FontColorOverride = GetColorLerp(critColor, endColor, damage);
            }
        }

        /// <summary>
        /// Smooth transition from one color to another depending on the percentage.
        /// </summary>
        /// <param name="startColor"></param>
        /// <param name="endColor"></param>
        /// <param name="percentage"></param>
        /// <returns></returns>
        private Color GetColorLerp(Color startColor, Color endColor, float percentage)
        {
            var t = percentage / 100f;
            var r = MathHelper.Lerp(startColor.R, endColor.R, t);
            var g = MathHelper.Lerp(startColor.G, endColor.G, t);
            var b = MathHelper.Lerp(startColor.B, endColor.B, t);
            var a = MathHelper.Lerp(startColor.A, endColor.A, t);

            return new Color(r, g, b, a);
        }
    }
}

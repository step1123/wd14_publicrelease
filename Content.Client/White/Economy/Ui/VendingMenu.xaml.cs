using System.Numerics;
using Content.Shared.VendingMachines;
using Robust.Client.AutoGenerated;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.UserInterface.CustomControls;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Prototypes;

namespace Content.Client.White.Economy.Ui;

[GenerateTypedNameReferences]
public sealed partial class VendingMenu : DefaultWindow
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    public event Action<int>? OnItemSelected;
    public Action<VendingMachineWithdrawMessage>? OnWithdraw;

    public VendingMenu()
    {
        MinSize = SetSize = new Vector2(250, 150);
        RobustXamlLoader.Load(this);
        IoCManager.InjectDependencies(this);
    }

    /// <summary>
    /// Populates the list of available items on the vending machine interface
    /// and sets icons based on their prototypes
    /// </summary>
    public void Populate(List<VendingMachineEntry> inventory, double priceMultiplier, int credits)
    {
        CreditsLabel.Text = Loc.GetString("vending-ui-credits-amount", ("credits", credits));
        WithdrawButton.Disabled = credits == 0;
        WithdrawButton.OnPressed += _ =>
        {
            if (credits == 0)
                return;
            OnWithdraw?.Invoke(new VendingMachineWithdrawMessage());
        };
        VendingContents.RemoveAllChildren();
        if (inventory.Count == 0)
        {
            OutOfStockLabel.Visible = true;
            SetSizeAfterUpdate(OutOfStockLabel.Text?.Length ?? 0);
            return;
        }
        OutOfStockLabel.Visible = false;

        var longestEntry = string.Empty;
        var spriteSystem = IoCManager.Resolve<IEntitySystemManager>().GetEntitySystem<SpriteSystem>();

        for (var i = 0; i < inventory.Count; i++)
        {
            var entry = inventory[i];

            var itemName = string.Empty;
            var amount = 0;
            Texture? icon = null;
            switch (entry)
            {
                case VendingMachineInventoryEntry inventoryEntry:
                {
                    itemName = inventoryEntry.ID;
                    amount = (int) inventoryEntry.Amount;
                    if (_prototypeManager.TryIndex<EntityPrototype>(inventoryEntry.ID, out var prototype))
                    {
                        itemName = prototype.Name;
                        icon = spriteSystem.GetPrototypeIcon(prototype).Default;
                    }

                    break;
                }
                case VendingMachineEntityEntry entityEntry:
                    itemName = entityEntry.Name;
                    amount = entityEntry.Entities.Count;
                    if (amount > 0)
                    {
                        var entity = entityEntry.Entities[amount - 1];
                        var entityManager = IoCManager.Resolve<IEntityManager>();
                        if (entityManager.TryGetComponent(entity, out SpriteComponent? sprite) && sprite.Icon != null)
                        {
                            icon = sprite.Icon switch
                            {
                                Texture texture => texture,
                                RSI.State state => state.Frame0,
                                _ => icon
                            };
                        }
                    }
                    break;
            }

            if (itemName.Length > longestEntry.Length)
                longestEntry = itemName;

            var price = (int) (entry.Price * priceMultiplier);
            var vendingItem = new VendingItem($"{itemName} [{amount}]", $"{price} ¢", icon);

            var j = i;
            vendingItem.VendingItemBuyButton.OnPressed += _ =>
            {
                OnItemSelected?.Invoke(j);
            };

            VendingContents.AddChild(vendingItem);
        }

        SetSizeAfterUpdate(longestEntry.Length);
    }

    private void SetSizeAfterUpdate(int longestEntryLength)
    {
        SetSize = new Vector2(Math.Clamp((longestEntryLength + 10) * 12, 250, 700),
            Math.Clamp(VendingContents.ChildCount * 50, 150, 400));
    }
}

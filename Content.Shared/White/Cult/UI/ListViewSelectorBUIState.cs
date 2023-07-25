using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.White.Cult.UI;

[Serializable, NetSerializable]
public enum ListViewSelectorUiKey
{
    Key
}

[Serializable, NetSerializable]
public class ListViewBUIState : BoundUserInterfaceState
{
    public List<string> Items { get; set; }
    public bool IsUsingPrototypes { get; set; }

    public ListViewBUIState(List<string> items, bool isUsingPrototypes)
    {
        Items = items;
        IsUsingPrototypes = isUsingPrototypes;
    }
}

[Serializable, NetSerializable]
public class ListViewItemSelectedMessage : BoundUserInterfaceMessage
{
    public string SelectedItem { get; private set; }
    public int Index { get; private set; }

    public ListViewItemSelectedMessage(string selectedItem, int index)
    {
        SelectedItem = selectedItem;
        Index = index;
    }
}

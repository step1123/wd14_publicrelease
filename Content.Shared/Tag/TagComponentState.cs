#nullable enable
using System;
using Robust.Shared.GameObjects;
using Robust.Shared.Serialization;

namespace Content.Shared.Tag
{
    [Serializable, NetSerializable]
    public class TagComponentState : ComponentState
    {
        public TagComponentState(string[] tags)
        {
            Tags = tags;
        }

        public string[] Tags { get; }
    }
}

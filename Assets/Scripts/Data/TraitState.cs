// Assets/Scripts/Data/TraitState.cs
using System;

namespace PickMeUp.Data
{
    /// <summary>
    /// Represents the runtime state of a trait attached to a specific HeroInstance.
    /// </summary>
    [Serializable]
    public class TraitState
    {
        public string TraitDefId;
        public bool IsActive;
        public int Stacks;

        public TraitState() { }

        public TraitState(TraitDefinition definition)
        {
            TraitDefId = definition.TraitId;
            IsActive = true;
            Stacks = 0;
        }
    }
}
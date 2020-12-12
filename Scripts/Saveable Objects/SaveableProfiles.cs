using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaveableProfile
{
    public interface ISaveableProfile
    {
        string id { get; set; }
        void ReplaceProfile(ISaveableProfile newProfile);
    }

    public class CharacterProfile : ISaveableProfile
    {
        public void ReplaceProfile(ISaveableProfile newProfile)
        {
            if(newProfile is CharacterProfile)
            {
                CharacterProfile prof = (CharacterProfile) newProfile;
                acceptableItems = prof.acceptableItems;
                itemAcceptanceConditions = prof.itemAcceptanceConditions;
                currentItemType = prof.currentItemType;
            }
            else
            {
                Debug.LogError("[CharacterInventoryProfile] Tried to save an incompatible profile type");
            }
        }

        public string id { get; set; }
        public List<ItemType> acceptableItems;
        public List<InventoryAcceptancePair> itemAcceptanceConditions;
        public ItemType? currentItemType;
    }
}
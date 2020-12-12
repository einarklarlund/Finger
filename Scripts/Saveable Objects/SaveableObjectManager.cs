using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Signals;

namespace SaveableProfile
{
    public class SaveableProfileManager : MonoBehaviour
    {
        private List<ISaveableProfile> _profiles;

        private void Start()
        {
            _profiles = new List<ISaveableProfile>();
        }

        public ISaveableProfile GetSavedProfile(ISaveable saveable)
        {
            return _profiles.Find(profile => profile.id == saveable.GetID());
        }   

        public void OnProfileSaved(ProfileSavedSignal signal)
        {
            ISaveableProfile match = _profiles.Find(prof => prof.id == signal.profile.id);
            
            if(match == default(ISaveableProfile))
            {
                _profiles.Add(signal.profile);
            }
            else
            {
                match.ReplaceProfile(signal.profile);
            }
        }

        public void OnDreamTransition(DreamTransitionSignal signal)
        {
            if(signal.clearProfiles)
            {
                _profiles = new List<ISaveableProfile>();
            }
        }

        public void OnDreamEnded(DreamEndingSignal signal)
        {
            _profiles = new List<ISaveableProfile>();
        }
    }
}
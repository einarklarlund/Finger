using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaveableProfile
{
    public interface ISaveable
    {
        string GetID();
        void Save();
        void Load();
    }
}
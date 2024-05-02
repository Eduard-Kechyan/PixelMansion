using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    [CreateAssetMenu(fileName = "ConvoData", menuName = "ScriptableObject/ConvoData")]
    public class ConvoData : ScriptableObject
    {
        public ConvoUIHandler.ConvoGroup[] convoGroups;

        void OnValidate()
        {
            for (int i = 0; i < convoGroups.Length; i++)
            {
                convoGroups[i].name = convoGroups[i].id;
            }
        }
    }
}

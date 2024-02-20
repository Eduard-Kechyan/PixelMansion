using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    [CreateAssetMenu(fileName = "KeystoreData", menuName = "ScriptableObject/KeystoreData")]
    public class KeystoreData : ScriptableObject
    {
        // The asset made from this ScriptableObject is included in the .gitignore file and is also hidden in VS Code

        public string keystorePass = "";
        [Space(10)]
        public string keyaliasName = "";
        public string keyaliasPass = "";
    }
}

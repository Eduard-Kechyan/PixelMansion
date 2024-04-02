using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

// 

/*
    ! - This is script templated and ins't meant to be used 

    Us the script as a template to build the other scripts with
    All comments with * should be included unless not used

    ! Methods don't need the "private" prefix, only the "public" prefix when needed 
*/

namespace Merge
{
    public class ScriptTemplate : MonoBehaviour
    {
        // * Variables
        /*
            - Here are all the public and private variables placed
            - All private variables need the "private" prefix
            - All private variables come after the public ones
            - Related variables should be grouped
        */
        public bool validate;

        private string examplePrivateVariable;

        // * References
        /*
            Here go all the references we get by:
            - GetComponent<>()
            - Find()
            - Instances
            - And any other way we get them

            The all need to be private.
            No default types, only other scripts, classes and so on.
            The variable's name needs to be the same as the type
        */
        private GameData gameData;

        // * UI
        /*
           - Here is where our UI variables and references go
           - The root comes first;
        */
        private VisualElement root;

        private VisualElement exampleElement;


        // * Instance
        /*
            In case we use an instance, here is where it goes capitalized
        */
        public static ScriptTemplate Instance;

        void Awake()
        {
            /*
                Used when we need an instance of the class, a singleton and similar things
            */
        }

        void Start()
        {
            Debug.LogWarning("ScriptTemplate should be removed from " + gameObject.name);

            // * Cache
            /*
                Here is where we get our references
            */
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            /*
                - Only used for debugging
                - Always includes the compilation flags (#if UNITY_EDITOR, #endif)
            */

            // Use this as a trigger button in the inspector, the "validate" variable needs to be public 
            if (validate)
            {
                validate = false;

                //  Code goes here
            }

            // Some code should be wrapped in the following snippet blow for in case there is some warning in the console
            Glob.Validate(() =>
            {
                // Code goes here
            }, this);
        }
#endif
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Merge
{
    public class NoteDotHandler : MonoBehaviour
    {
        // Variables
        [HideInInspector]
        public int taskNoteDotAmount = 0;

        [HideInInspector]
        public bool isWorld = true;

        // References
        private ErrorManager errorManager;
        private WorldUI worldUI;
        private MergeUI mergeUI;

        void Start()
        {
            // Cache
            errorManager = ErrorManager.Instance;
            worldUI = GetComponent<WorldUI>();
            mergeUI = GetComponent<MergeUI>();

            if (worldUI == null)
            {
                isWorld = false;
            }
        }

        // Check if we need to show the note dot on the given button
        public void ToggleButtonNoteDot(UIButtons.Button button, bool show, int amount = 0, bool useBloop = false)
        {
            VisualElement buttonNoteDot = new();

            if (useBloop)
            {
                StopCoroutine(BloopNoteDot(buttonNoteDot));
            }

            switch (button)
            {
                case UIButtons.Button.Play:
                    buttonNoteDot = worldUI.playButtonNoteDot;
                    break;
                case UIButtons.Button.Home:
                    buttonNoteDot = mergeUI.homeButtonNoteDot;
                    break;
                case UIButtons.Button.Inventory:
                    buttonNoteDot = mergeUI.inventoryButtonNoteDot;
                    break;
                case UIButtons.Button.Shop:
                    if (isWorld)
                    {
                        buttonNoteDot = worldUI.shopButtonNoteDot;
                    }
                    else
                    {
                        buttonNoteDot = mergeUI.shopButtonNoteDot;
                    }
                    break;
                case UIButtons.Button.Task:
                    if (isWorld)
                    {
                        buttonNoteDot = worldUI.taskButtonNoteDot;

                        if (amount > 0)
                        {
                            worldUI.taskButtonNoteDotLabel.text = amount.ToString();
                        }
                    }
                    else
                    {
                        buttonNoteDot = mergeUI.taskButtonNoteDot;

                        if (amount > 0)
                        {
                            mergeUI.taskButtonNoteDotLabel.text = amount.ToString();
                        }
                    }

                    taskNoteDotAmount = amount;

                    break;
                default:
                    // ERROR
                    errorManager.ThrowWarning(ErrorManager.ErrorType.Code, GetType().ToString(), "Wrong button given: " + button);
                    break;
            }

            buttonNoteDot.RemoveFromClassList("note_dot_bloop");

            buttonNoteDot.style.visibility = show
                ? Visibility.Visible
                : Visibility.Hidden;
            buttonNoteDot.style.opacity = show ? 1 : 0;

            if (useBloop)
            {
                StartCoroutine(BloopNoteDot(buttonNoteDot));
            }
        }

        // Bloop the note dot to catch attention
        IEnumerator BloopNoteDot(VisualElement buttonNoteDot)
        {
            yield return new WaitForSeconds(0.2f);

            buttonNoteDot.AddToClassList("note_dot_bloop");

            yield return new WaitForSeconds(0.2f);

            buttonNoteDot.RemoveFromClassList("note_dot_bloop");
        }
    }
}

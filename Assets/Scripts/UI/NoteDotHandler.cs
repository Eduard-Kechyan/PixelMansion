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
        public bool isHub = true;

        // References
        private HubUI hubUI;
        private GameplayUI gameplayUI;

        void Start()
        {
            hubUI = GetComponent<HubUI>();
            gameplayUI = GetComponent<GameplayUI>();

            if (hubUI == null)
            {
                isHub = false;
            }
        }

        // Check if we need to show the note dot on the given button
        public void ToggleButtonNoteDot(string buttonName, bool show, int amount = 0, bool useBloop = false)
        {
            VisualElement buttonNoteDot = new();

            if (useBloop)
            {
                StopCoroutine(BloopNoteDot(buttonNoteDot));
            }

            switch (buttonName)
            {
                case "settings":
                    buttonNoteDot = hubUI.settingsButtonNoteDot;
                    break;
                case "play":
                    buttonNoteDot = hubUI.playButtonNoteDot;
                    break;
                case "home":
                    buttonNoteDot = gameplayUI.homeButtonNoteDot;
                    break;
                case "inventory":
                    buttonNoteDot = gameplayUI.inventoryButtonNoteDot;
                    break;
                case "shop":
                    if (isHub)
                    {
                        buttonNoteDot = hubUI.shopButtonNoteDot;
                    }
                    else
                    {
                        buttonNoteDot = gameplayUI.shopButtonNoteDot;
                    }
                    break;
                case "task":
                    if (isHub)
                    {
                        buttonNoteDot = hubUI.taskButtonNoteDot;

                        if (amount > 0)
                        {
                            hubUI.taskButtonNoteDotLabel.text = amount.ToString();
                        }
                    }
                    else
                    {
                        buttonNoteDot = gameplayUI.taskButtonNoteDot;

                        if (amount > 0)
                        {
                            gameplayUI.taskButtonNoteDotLabel.text = amount.ToString();
                        }
                    }

                    taskNoteDotAmount = amount;


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

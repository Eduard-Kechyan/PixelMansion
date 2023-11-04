using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

namespace Merge
{
    public class TutorialManager : MonoBehaviour
    {
        // Variables
        public bool skipTutorial = false;
        public TutorialData tutorialData;
        public ProgressManager progressManager;
        public ConvoUIHandler convoUIHandler;

        private string tutorialStep = "First";

        // References
        private ValuesUI valuesUI;
        private HubUI hubUI;
        private GameplayUI gameplayUI;
        private DataManager dataManager;

        // UI
        private VisualElement root;

        private VisualElement storyContainer;

        private VisualElement pointer;

        private VisualElement skipButton;
        private Label skipLabel;

        void Awake()
        {
            if (PlayerPrefs.HasKey("tutorialFinished") || skipTutorial || (Glob.lastSceneName != "" && Glob.lastSceneName != SceneManager.GetActiveScene().name))
            {
                progressManager.SetInitialData(0, true);

                Destroy(this);
            }

            if (PlayerPrefs.HasKey("tutorialStep"))
            {
                tutorialStep = PlayerPrefs.GetString("tutorialStep");
            }
            else
            {
                PlayerPrefs.SetString("tutorialStep", "First");

                PlayerPrefs.Save();
            }
        }

        void Start()
        {
            // Cache
            valuesUI = GameRefs.Instance.valuesUI;
            hubUI = GameRefs.Instance.hubUI;
            gameplayUI = GameRefs.Instance.gameplayUI;
            dataManager = DataManager.Instance;

            // UI
            root = GetComponent<UIDocument>().rootVisualElement;

            storyContainer = root.Q<VisualElement>("StoryContainer");

            pointer = root.Q<VisualElement>("Pointer");

            if (tutorialStep == "First")
            {
                skipButton = root.Q<VisualElement>("SkipButtonContainer");
                skipLabel = skipButton.Q<Label>("Label");

                skipButton.AddManipulator(new Clickable(evt =>
                {
                    EndStory();
                }));
            }

            StartCoroutine(WaitForLoading());
        }

        IEnumerator WaitForLoading()
        {
            while (!dataManager.loaded || !valuesUI.loaded || (hubUI != null && !hubUI.loaded) || (gameplayUI != null && !gameplayUI.loaded))
            {
                yield return null;
            }

            Init();
        }

        public void Init()
        {
            valuesUI.DisableButtons();

            if (hubUI != null)
            {
                hubUI.DisableButtons();
            }
            else
            {
                gameplayUI.DisableButtons();
            }

            HandleStep();
        }

        void HandleStep()
        {
            for (int i = 0; i < tutorialData.steps.Length; i++)
            {
                if (tutorialData.steps[i].id == tutorialStep)
                {
                    switch (tutorialData.steps[i].type)
                    {
                        case Types.TutorialStepType.Task:
                            if (tutorialData.steps[i].addTask)
                            {
                                progressManager.SetInitialData(tutorialData.steps[i].taskOrder);
                            }

                            HandleTask();
                            break;
                        case Types.TutorialStepType.Convo:
                            HandleConvo("Tutorial" + tutorialData.steps[i].id);
                            break;
                        case Types.TutorialStepType.Story:
                            HandleStory();
                            break;
                    }

                    break;
                }
            }
        }

        void NextStep()
        {
            for (int i = 0; i < tutorialData.steps.Length; i++)
            {
                if (tutorialData.steps[i].id == tutorialStep)
                {
                    if (tutorialData.steps[i + 1] != null) // Next
                    {
                        tutorialStep = tutorialData.steps[i + 1].id;

                        PlayerPrefs.SetString("tutorialStep", tutorialStep);

                        PlayerPrefs.Save();

                        HandleStep();
                    }
                    else // Last
                    {
                        HandleLast();
                    }

                    break;
                }
            }
        }

        void HandleTask()
        {
            // progressManager.SetInitialData();

            // TODO - Handle task here

            //pointer

            NextStep();
        }

        void HandleConvo(string convoId)
        {
            convoUIHandler.Converse(convoId, false, () =>
            {
                Glob.SetTimeout(() =>
                {
                    NextStep();
                }, 0.3f);
            });
        }

        void HandleStory()
        {
            storyContainer.AddToClassList("no_transition");
            skipButton.AddToClassList("no_transition");

            storyContainer.style.display = DisplayStyle.Flex;
            skipButton.style.display = DisplayStyle.Flex;

            storyContainer.style.opacity = 1;
            skipButton.style.opacity = 1;

            // TODO - Play initial story here

            Glob.SetTimeout(() =>
            {
                EndStory();
            }, 2f);
        }

        void EndStory()
        {
            storyContainer.RemoveFromClassList("no_transition");
            skipButton.RemoveFromClassList("no_transition");

            storyContainer.style.opacity = 0;
            skipButton.style.opacity = 0;

            Glob.SetTimeout(() =>
            {
                storyContainer.style.display = DisplayStyle.None;
                skipButton.style.display = DisplayStyle.None;

                NextStep();
            }, 0.3f);
        }

        void HandleLast()
        {
            valuesUI.EnableButtons();

            if (hubUI != null)
            {
                hubUI.EnableButtons();
            }
            else
            {
                gameplayUI.EnableButtons();
            }

            progressManager.SetInitialData(0, true);

            PlayerPrefs.SetInt("tutorialFinished", 1);

            PlayerPrefs.Save();
        }
    }
}

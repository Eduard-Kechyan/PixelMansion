using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using Unity.Mathematics;

namespace Merge
{
    public class TutorialManager : MonoBehaviour
    {
        // Variables
        public bool skipTutorial = false;
        public TutorialData tutorialData;
        public ProgressManager progressManager;
        public ConvoUIHandler convoUIHandler;
        public ShineSprites[] shineSprites;

        private string tutorialStep = "First";
        private bool pointsShone = false;

        [Serializable]
        public class ShineSprites
        {
            public Sprite[] sprites;
            public float speed;
        }

        // References
        private ValuesUI valuesUI;
        private HubUI hubUI;
        private UIDocument hubGameUIDoc;
        private GameplayUI gameplayUI;
        private UIButtons uiButtons;
        private DataManager dataManager;
        private InputMenu inputMenu;
        private PointerHandler pointerHandler;

        // UI
        private VisualElement root;

        private VisualElement storyContainer;

        private VisualElement skipButton;
        private Label skipLabel;

        private VisualElement convoBackground;

        void Awake()
        {
            string sceneName = SceneManager.GetActiveScene().name;

            if (PlayerPrefs.HasKey("tutorialFinished") || skipTutorial || (Glob.lastSceneName != "" && Glob.lastSceneName == sceneName))
            {
                progressManager.SetInitialData(0, true);

                HandleLast();

                Destroy(pointerHandler);
                Destroy(this);
            }
            else
            {
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
        }

        void Start()
        {
            // Cache
            valuesUI = GameRefs.Instance.valuesUI;
            hubUI = GameRefs.Instance.hubUI;
            hubGameUIDoc = GameRefs.Instance.hubGameUIDoc;
            gameplayUI = GameRefs.Instance.gameplayUI;
            dataManager = DataManager.Instance;
            inputMenu = GameRefs.Instance.inputMenu;
            uiButtons = GameData.Instance.GetComponent<UIButtons>();
            pointerHandler = GetComponent<PointerHandler>();

            // UI
            root = GetComponent<UIDocument>().rootVisualElement;

            storyContainer = root.Q<VisualElement>("StoryContainer");

            if (hubGameUIDoc != null)
            {
                convoBackground = hubGameUIDoc.GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>("ConvoBackground");
            }

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
            while (!dataManager.loaded || !valuesUI.loaded || (convoUIHandler != null && !convoUIHandler.loaded) || (hubUI != null && !hubUI.loaded) || (gameplayUI != null && !gameplayUI.loaded))
            {
                yield return null;
            }

            Init();
        }

        public void Init()
        {
            progressManager.SetInitialData(0, false);

            HandleStep();
        }

        public void CheckConvoBackground(bool alt = false)
        {
            if (convoBackground != null)
            {
                if ((tutorialStep == "First" || tutorialStep == "PlayerName" || tutorialStep == "Part1") && !alt)
                {
                    List<TimeValue> nullTransition = new() { new TimeValue(0.0f, TimeUnit.Second) };

                    convoBackground.style.transitionDuration = new StyleList<TimeValue>(nullTransition);

                    convoBackground.style.opacity = 1;
                    convoBackground.style.display = DisplayStyle.Flex;

                    if (!pointsShone)
                    {
                        ShinePoints();
                    }
                }
                else
                {
                    List<TimeValue> fullTransition = new() { new TimeValue(0.3f, TimeUnit.Second) };

                    convoBackground.style.transitionDuration = new StyleList<TimeValue>(fullTransition);

                    convoBackground.style.opacity = 0;

                    Glob.SetTimeout(() =>
                    {
                        convoBackground.style.display = DisplayStyle.None;
                    }, 0.3f);
                }
            }
        }

        void ShinePoints()
        {
            foreach (var point in convoBackground.Children())
            {
                switch (point.name)
                {
                    case "ShineA":
                        StartCoroutine(ShinePoint(point, 0));
                        break;
                    case "ShineB":
                        StartCoroutine(ShinePoint(point, 1));
                        break;
                    case "ShineC":
                        StartCoroutine(ShinePoint(point, 2));
                        break;
                    case "ShineD":
                        StartCoroutine(ShinePoint(point, 3));
                        break;
                }
            }

            pointsShone = true;
        }

        IEnumerator ShinePoint(VisualElement point, int shineSpriteOrder)
        {
            int count = UnityEngine.Random.Range(0, shineSprites[shineSpriteOrder].sprites.Length - 1);
            WaitForSeconds wait = new(shineSprites[shineSpriteOrder].speed);

            while (convoBackground.resolvedStyle.display == DisplayStyle.Flex)
            {
                Sprite currentSprite = shineSprites[shineSpriteOrder].sprites[count];

                point.style.backgroundImage = new StyleBackground(currentSprite);

                point.style.width = currentSprite.rect.width;
                point.style.height = currentSprite.rect.height;

                if (count == shineSprites[shineSpriteOrder].sprites.Length - 1)
                {
                    count = 0;
                }
                else
                {
                    count++;
                }

                yield return wait;
            }

            yield return null;
        }

        void HandleStep()
        {
            CheckConvoBackground();

            for (int i = 0; i < tutorialData.steps.Length; i++)
            {
                if (tutorialData.steps[i].id == tutorialStep)
                {
                    switch (tutorialData.steps[i].type)
                    {
                        case Types.TutorialStepType.Task:
                            HandleTask(tutorialData.steps[i]);
                            break;
                        case Types.TutorialStepType.Convo:
                            HandleConvo(tutorialData.steps[i]);
                            break;
                        case Types.TutorialStepType.Story:
                            HandleStory();
                            break;
                        case Types.TutorialStepType.Input:
                            HandleInput(tutorialData.steps[i].id);
                            break;
                    }

                    break;
                }
            }
        }

        void NextStep(bool handleNestStep = true)
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

                        if (handleNestStep)
                        {
                            HandleStep();
                        }
                    }
                    else // Last
                    {
                        if (handleNestStep)
                        {
                            HandleLast();
                        }
                    }

                    break;
                }
            }
        }

        void HandleTask(Types.TutorialStep step)
        {
            switch (step.taskType)
            {
                case Types.TutorialStepTask.Press:
                    TaskPress(step);
                    break;
                case Types.TutorialStepTask.Menu:
                    pointerHandler.HandleMerge(step.taskRef, () =>
                    {
                        NextStep();
                    });
                    break;
                case Types.TutorialStepTask.Merge:
                    break;
                case Types.TutorialStepTask.Gen:
                    break;
            }

            // NextStep();
        }

        void TaskPress(Types.TutorialStep step)
        {
            if (step.scene == Types.TutorialStepScene.Hub)
            {
                if (step.taskRef == "Play")
                {
                    hubUI.ShowButton("play");

                    Glob.SetTimeout(() =>
                    {
                        pointerHandler.HandlePress(uiButtons.hubPlayButtonPos, "Play", () =>
                        {
                            NextStep(false);
                        });
                    }, 0.5f);
                }
            }
            else
            {
                if (step.taskRef == "Task")
                {
                    gameplayUI.ShowButton("task");

                    pointerHandler.HandlePress(uiButtons.gameplayTaskButtonPos, "task", () =>
                    {
                        NextStep(false);
                    });
                }

                if (step.taskRef == "Home")
                {
                    gameplayUI.ShowButton("home");

                    pointerHandler.HandlePress(uiButtons.gameplayHomeButtonPos, "Home", () =>
                    {
                        NextStep(false);
                    });
                }
            }
        }

        void HandleConvo(Types.TutorialStep step)
        {
            convoUIHandler.Converse("Tutorial" + step.id, true, !step.keepConvoOpen, false, () =>
            {
                Glob.SetTimeout(() =>
                {
                    NextStep();
                }, 0.4f);
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

        void HandleInput(string inputId)
        {
            if (inputId == "PlayerName")
            {
                inputMenu.Open(inputId, (string inputResult) =>
                {
                    GameData.Instance.playerName = inputResult;

                    PlayerPrefs.SetString("playerName", inputResult);

                    PlayerPrefs.Save();

                    NextStep();
                });
            }
            else
            {
                Debug.LogWarning("Wrong inputId given: " + inputId);
            }
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
            hubUI.ShowButtons();
            gameplayUI.ShowButtons();

            PlayerPrefs.SetInt("tutorialFinished", 1);

            PlayerPrefs.Save();
        }
    }
}

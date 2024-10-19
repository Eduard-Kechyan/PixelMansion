using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Merge
{
    public class TutorialManager : MonoBehaviour
    {
        // Variables
        public bool startFromStep = false;
        public string stepToStartFrom = "";
        [SortingLayer]
        public string livingRoomSortingLayer;
        public SceneLoader sceneLoader;
        public TutorialData tutorialData;
        public ShineSprites[] shineSprites;
        public PreMansionHandler preMansionHandler;

        [HideInInspector]
        public bool ready = false;

        private string tutorialStep = "";
        private bool pointsShone = false;

        private bool destroyingTutorial = false;

        private bool tutorialStarted = false;

        private bool referencesSet = false;

        // Enums
        public enum TutorialStepType
        {
            Task,
            Convo,
            Input,
            Story,
        };

        public enum TutorialStepTask
        {
            Press,
            Merge,
            Gen,
            Menu,
            Unlock
        };

        // Classes
        [Serializable]
        public class TutorialStep
        {
            [HideInInspector]
            public string name;
            public string id;
            public SceneLoader.SceneType scene;
            public TutorialStepType type;
            public TutorialStepTask taskType;
            public UIButtons.Button taskButton;
            public Sprite taskSprite;
            public Item.Group itemGroup;
            public Sprite genSprite;
            public int taskOrder;
            public bool keepConvoOpen;
        }

        [Serializable]
        public class ShineSprites
        {
            public Sprite[] sprites;
            public float speed;
        }

        // References
        private GameRefs gameRefs;
        private ValuesUI valuesUI;
        private WorldUI worldUI;
        private UIDocument worldGameUIDoc;
        private MergeUI mergeUI;
        private UIButtons uiButtons;
        private DataManager dataManager;
        private GameData gameData;
        private InputMenu inputMenu;
        private PointerHandler pointerHandler;
        private TutorialCycle tutorialCycle;
        private InfoBox infoBox;
        private CloudSave cloudSave;
        private AnalyticsManager analyticsManager;
        private ProgressManager progressManager;
        private ConvoUIHandler convoUIHandler;
        private BoardManager boardManager;
        private ValuePop valuePop;
        private CharMove charMove;
        private DoorManager doorManager;

        // UI
        private VisualElement convoBackground;

        void Awake()
        {
            if (PlayerPrefs.HasKey("tutorialFinished"))
            {
                Destroy(gameObject);
            }
            else if (tutorialData.skipTutorial)
            {
                StartCoroutine(WaitForReferences(() =>
                {
                    if (!PlayerPrefs.HasKey("tutorialFinished"))
                    {
                        progressManager.SetInitialData(0);

                        PlayerPrefs.SetInt("tutorialFinished", 1);

                        PlayerPrefs.DeleteKey("tutorialStep");

                        PlayerPrefs.Save();
                    }
                }));
            }
            else
            {
                if (startFromStep && stepToStartFrom != "" && !PlayerPrefs.HasKey("tutorialStep"))
                {
                    tutorialStep = stepToStartFrom;

                    PlayerPrefs.SetString("tutorialStep", stepToStartFrom);

                    PlayerPrefs.Save();
                }
                else
                {
                    if (PlayerPrefs.HasKey("tutorialStep"))
                    {
                        tutorialStep = PlayerPrefs.GetString("tutorialStep");
                    }
                    else
                    {
                        tutorialStep = tutorialData.steps[0].id; // Initial step

                        tutorialStarted = true;

                        PlayerPrefs.SetString("tutorialStep", "First");

                        PlayerPrefs.Save();
                    }
                }
            }
        }

        void Start()
        {
            if (tutorialData.skipTutorial || PlayerPrefs.HasKey("tutorialFinished"))
            {
                progressManager = GameRefs.Instance.progressManager;
                dataManager = DataManager.Instance;

                referencesSet = true;
            }
            else
            {
                // Cache
                gameRefs = GameRefs.Instance;
                valuesUI = gameRefs.valuesUI;
                worldUI = gameRefs.worldUI;
                worldGameUIDoc = gameRefs.worldGameUIDoc;
                mergeUI = gameRefs.mergeUI;
                dataManager = DataManager.Instance;
                gameData = GameData.Instance;
                inputMenu = gameRefs.inputMenu;
                uiButtons = GameData.Instance.GetComponent<UIButtons>();
                pointerHandler = GetComponent<PointerHandler>();
                tutorialCycle = GetComponent<TutorialCycle>();
                infoBox = gameRefs.infoBox;
                cloudSave = Services.Instance.GetComponent<CloudSave>();
                analyticsManager = AnalyticsManager.Instance;
                convoUIHandler = gameRefs.convoUIHandler;
                boardManager = gameRefs.boardManager;
                progressManager = gameRefs.progressManager;
                valuePop = gameRefs.valuePop;

                referencesSet = true;

                // UI
                if (worldGameUIDoc != null)
                {
                    convoBackground = worldGameUIDoc.GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>("ConvoBackground");

                    charMove = CharMain.Instance.GetComponent<CharMove>();
                    doorManager = gameRefs.doorManager;
                }

                if (tutorialStep == "")
                {
                    Debug.LogWarning("tutorialStep is empty!");
                }

                if (tutorialStarted)
                {
                    analyticsManager.FireTutorialEvent("", true);

                    tutorialStarted = false;
                }

                cloudSave.SaveDataAsync("tutorialStep", PlayerPrefs.GetString("tutorialStep"));

                StartCoroutine(WaitForLoading());
            }
        }

        IEnumerator WaitForLoading()
        {
            while (tutorialData.skipTutorial || destroyingTutorial || !dataManager.loaded || !valuesUI.loaded || (convoUIHandler != null && !convoUIHandler.loaded) || (worldUI != null && !worldUI.loaded) || (mergeUI != null && !mergeUI.loaded))
            {
                yield return null;
            }

            Init();
        }

#if UNITY_EDITOR
        void Init()
        {
            CheckForMenuStepOnStart(() =>
            {
                if (CheckScene())
                {
                    SetUI();

                    progressManager.SetInitialData(0);

                    StartCoroutine(WaitForDataOrBoard());
                }
            });
        }

        bool CheckScene()
        {
            SceneLoader.SceneType currentScene = sceneLoader.GetScene();

            if (!PlayerPrefs.HasKey("taskToComplete"))
            {
                for (int i = 0; i < tutorialData.steps.Length; i++)
                {
                    if (tutorialData.steps[i].id == tutorialStep)
                    {
                        if (tutorialData.steps[i].scene == SceneLoader.SceneType.Merge && currentScene != SceneLoader.SceneType.Merge)
                        {
                            // Merge scene
                            sceneLoader.Load(SceneLoader.SceneType.Merge);

                            return false;
                        }

                        if (tutorialData.steps[i].scene == SceneLoader.SceneType.World && currentScene != SceneLoader.SceneType.World)
                        {
                            // World scene
                            sceneLoader.Load(SceneLoader.SceneType.World);

                            return false;
                        }

                        break;
                    }
                }

                return true;
            }

            return true;
        }
#else
        void Init()
        {
            CheckForMenuStepOnStart(() =>
            {
                SetUI();

                progressManager.SetInitialData(0);

                StartCoroutine(WaitForDataOrBoard());
            });
        }
#endif

        IEnumerator WaitForDataOrBoard()
        {
            while (!dataManager.loaded || (boardManager != null && !boardManager.boardSet))
            {
                yield return null;
            }

            ready = true;

            HandleStep();
        }

        public void CheckConvoBackground(bool alt = false)
        {
            if (convoBackground != null)
            {
                if ((tutorialStep == "Part1" || tutorialStep == "Part2" || tutorialStep == "PlayerName") && !alt)
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

        public bool CheckIfNextStepIsConvo()
        {
            for (int i = 0; i < tutorialData.steps.Length; i++)
            {
                if (tutorialData.steps[i].id == tutorialStep)
                {
                    if (tutorialData.steps[i].type == TutorialStepType.Convo)
                    {
                        return true;
                    }

                    break;
                }
            }

            return false;
        }

        public void CheckForMenuStepOnStart(Action callback)
        {
            bool found = false;

            if (gameData.lastScene == SceneLoader.SceneType.None)
            {
                for (int i = 0; i < tutorialData.steps.Length; i++)
                {
                    if (tutorialData.steps[i].id == tutorialStep)
                    {
                        if (tutorialData.steps[i].type == TutorialStepType.Task && tutorialData.steps[i].taskType == TutorialStepTask.Menu)
                        {
                            tutorialStep = tutorialData.steps[i - 1].id;

                            PlayerPrefs.SetString("tutorialStep", tutorialStep);

                            PlayerPrefs.Save();
                        }

                        found = true;

                        callback?.Invoke();

                        break;
                    }
                }
            }

            if (!found)
            {
                callback?.Invoke();
            }
        }

        void HandleStep()
        {
            HideButtons();

            CheckConvoBackground();

            for (int i = 0; i < tutorialData.steps.Length; i++)
            {
                if (tutorialData.steps[i].id == tutorialStep)
                {
                    switch (tutorialData.steps[i].type)
                    {
                        case TutorialStepType.Task:
                            HandleTask(tutorialData.steps[i]);
                            break;
                        case TutorialStepType.Convo:
                            HandleConvo(tutorialData.steps[i]);
                            break;
                        case TutorialStepType.Story:
                            HandleStory();
                            break;
                        case TutorialStepType.Input:
                            HandleInput(tutorialData.steps[i].id);
                            break;
                    }

                    break;
                }
            }
        }

        public void NextStep(bool handleNextStep = true, Action callback = null)
        {
            if (worldUI)
            {
                worldUI.CloseTextBox();
            }

            for (int i = 0; i < tutorialData.steps.Length; i++)
            {
                if (tutorialData.steps[i].id == tutorialStep)
                {
                    if (i + 1 < tutorialData.steps.Length) // Check if the next one exists
                    {
                        tutorialStep = tutorialData.steps[i + 1].id;

                        PlayerPrefs.SetString("tutorialStep", tutorialStep);

                        PlayerPrefs.Save();

                        analyticsManager.FireTutorialEvent(tutorialStep);

                        if (handleNextStep)
                        {
                            HandleStep();
                        }
                    }
                    else // Last
                    {
                        if (handleNextStep)
                        {
                            HandleLast();
                        }
                    }

                    break;
                }
            }

            callback?.Invoke();
        }

        public void HideButtons()
        {
            SceneLoader.SceneType currentScene = sceneLoader.GetScene();

            if (currentScene == SceneLoader.SceneType.World)
            {
                worldUI.HideButton(UIButtons.Button.Play, true);
                worldUI.HideButton(UIButtons.Button.Task, true);

                return;
            }

            if (currentScene == SceneLoader.SceneType.Merge)
            {
                mergeUI.HideButton(UIButtons.Button.Home, true);
                mergeUI.HideButton(UIButtons.Button.Task, true);

                return;
            }
        }

        void HandleTask(TutorialStep step)
        {
            if (infoBox)
            {
                infoBox.SetTutorialData(step.id);
            }

            if (worldUI)
            {
                worldUI.OpenTextBox(step.id);
            }

            switch (step.taskType)
            {
                case TutorialStepTask.Press:
                    TaskPress(step);
                    break;
                case TutorialStepTask.Menu:
                    pointerHandler.HandlePress(Vector2.zero, UIButtons.Button.TaskMenu, () =>
                    {
                        NextStep();
                    });
                    break;
                case TutorialStepTask.Merge:
                    pointerHandler.HandleMerge(step.taskSprite, () =>
                    {
                        NextStep();
                    });
                    break;
                case TutorialStepTask.Gen:
                    pointerHandler.HandleGen(step.genSprite, step.taskSprite, step.itemGroup, () =>
                    {
                        NextStep();
                    });
                    break;
                case TutorialStepTask.Unlock:
                    doorManager.GetPosition(livingRoomSortingLayer, (Vector2 livingRoomDoorLocation) =>
                    {
                        charMove.SetDestination(livingRoomDoorLocation, false, false, () =>
                        {
                            preMansionHandler.Remove(() =>
                            {
                                NextStep();
                            });
                        }, true);
                    });

                    break;
            }
        }

        void TaskPress(TutorialStep step)
        {
            if (step.scene == SceneLoader.SceneType.World)
            {
                if (step.taskButton == UIButtons.Button.Play)
                {
                    worldUI.ShowButton(UIButtons.Button.Play);
                    worldUI.HideButton(UIButtons.Button.Task, true);

                    Glob.SetTimeout(() =>
                    {
                        pointerHandler.HandlePress(uiButtons.worldPlayButtonPos, UIButtons.Button.Play, () =>
                        {
                            NextStep(false, () =>
                            {
                                sceneLoader.Load(SceneLoader.SceneType.Merge);
                            });
                        });
                    }, 0.5f);
                }

                if (step.taskButton == UIButtons.Button.Task)
                {
                    worldUI.HideButton(UIButtons.Button.Play, true);
                    worldUI.ShowButton(UIButtons.Button.Task);

                    Glob.SetTimeout(() =>
                    {
                        pointerHandler.HandlePress(uiButtons.worldTaskButtonPos, UIButtons.Button.Task, () =>
                        {
                            NextStep();
                        });
                    }, 0.5f);
                }
            }
            else // SceneLoader.SceneType.Merge
            {
                if (step.taskButton == UIButtons.Button.Task)
                {
                    mergeUI.HideButton(UIButtons.Button.Home, true);
                    mergeUI.ShowButton(UIButtons.Button.Task);

                    pointerHandler.HandlePress(uiButtons.mergeTaskButtonPos, UIButtons.Button.Task, () =>
                    {
                        NextStep();
                    });
                }

                if (step.taskButton == UIButtons.Button.Home)
                {
                    mergeUI.ShowButton(UIButtons.Button.Home);
                    mergeUI.HideButton(UIButtons.Button.Task, true);

                    pointerHandler.HandlePress(uiButtons.mergeHomeButtonPos, UIButtons.Button.Home, () =>
                    {
                        NextStep(false, () =>
                        {
                            sceneLoader.Load(SceneLoader.SceneType.World);
                        });
                    });
                }
            }
        }

        void HandleConvo(TutorialStep step)
        {
            Glob.WaitForSelectable(() =>
            {
                StartCoroutine(WaitForPop(() =>
                {
                    convoUIHandler.Converse("Tutorial" + step.id, true, !step.keepConvoOpen, false, () =>
                    {
                        Glob.SetTimeout(() =>
                        {
                            NextStep();
                        }, 0.4f);
                    });
                }));
            });
        }

        IEnumerator WaitForPop(Action callback)
        {
            while (valuePop.popping || valuePop.mightPop)
            {
                yield return null;
            }

            callback();
        }

        void HandleStory()
        {
            // Play initial story here if it exists

            /*  storyContainer.AddToClassList("no_transition");
              skipButton.AddToClassList("no_transition");

              storyContainer.style.display = DisplayStyle.Flex;
              skipButton.style.display = DisplayStyle.Flex;

              storyContainer.style.opacity = 1;
              skipButton.style.opacity = 1;*/

            Debug.LogWarning("A Story bit of the tutorial should have been plating here. But, it isn't implemented yet or, there shouldn't be a Story bit here!");

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

                    cloudSave.SaveDataAsync("playerName", inputResult);

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
            /*   storyContainer.RemoveFromClassList("no_transition");
               skipButton.RemoveFromClassList("no_transition");

               storyContainer.style.opacity = 0;
               skipButton.style.opacity = 0;*/

            Glob.SetTimeout(() =>
            {
                // storyContainer.style.display = DisplayStyle.None;
                // skipButton.style.display = DisplayStyle.None;

                NextStep();
            }, 0.3f);
        }

        void HandleLast()
        {
            PlayerPrefs.SetInt("tutorialFinished", 1);

            PlayerPrefs.DeleteKey("tutorialStep");

            PlayerPrefs.Save();

            cloudSave.SaveDataAsync("tutorialFinished", 1);

            analyticsManager.FireTutorialEvent("", false, true);

            ResetUI();

            tutorialCycle.ShowCycle(() =>
            {
                Destroy(gameObject);
            });
        }

        void SetUI()
        {
            // valuesUI.SetSortingOrder(4);

            valuesUI.HideButtons();

            if (worldUI != null)
            {
                worldUI.HideButtons();
                worldUI.CheckButtons();
            }

            if (mergeUI != null)
            {
                mergeUI.HideButtons();
                mergeUI.CheckButtons();
            }
        }

        void ResetUI()
        {
            // valuesUI.SetSortingOrder(10);

            valuesUI.ShowButtons();

            if (worldUI != null)
            {
                worldUI.ShowButtons(true);
            }

            if (mergeUI != null)
            {
                mergeUI.ShowButtons();
            }
        }

        IEnumerator WaitForReferences(Action callback)
        {
            destroyingTutorial = true;

            while (!referencesSet)
            {
                yield return null;
            }

            while (!dataManager.loaded)
            {
                yield return null;
            }

            callback();

            Destroy(gameObject);
        }
    }
}

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
        public bool skipTutorial = false;
        public bool startFromStep = false;
        public string stepToStartFrom = "";
        public TutorialData tutorialData;
        public ShineSprites[] shineSprites;

        private string tutorialStep = "First";
        private bool pointsShone = false;

        private bool destroyingTutorial = false;

        private bool tutorialStarted = false;

        private bool referencesSet = false;

        [Serializable]
        public class ShineSprites
        {
            public Sprite[] sprites;
            public float speed;
        }

        // References
        private ValuesUI valuesUI;
        private WorldUI worldUI;
        private UIDocument worldGameUIDoc;
        private MergeUI mergeUI;
        private UIButtons uiButtons;
        private DataManager dataManager;
        private GameData gameData;
        private InputMenu inputMenu;
        private PointerHandler pointerHandler;
        private InfoBox infoBox;
        private CloudSave cloudSave;
        private AnalyticsManager analyticsManager;
        private ProgressManager progressManager;
        private ConvoUIHandler convoUIHandler;
        private BoardManager boardManager;
        private SceneLoader sceneLoader;
        private ValuePop valuePop;

        // UI
        private VisualElement convoBackground;

        void Awake()
        {
            if (PlayerPrefs.HasKey("tutorialFinished"))
            {
                StartCoroutine(WaitForReferences(() =>
                {
                    progressManager.SetInitialData(0, true);
                }));
            }
            else if (skipTutorial || (Glob.lastScene != Types.Scene.None && Glob.lastScene == sceneLoader.GetScene()))
            {
                StartCoroutine(WaitForReferences(() =>
                {
                    progressManager.SetInitialData(0);

                    PlayerPrefs.SetInt("tutorialFinished", 1);

                    PlayerPrefs.DeleteKey("tutorialStep");

                    PlayerPrefs.Save();
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
                        tutorialStarted = true;

                        PlayerPrefs.SetString("tutorialStep", "First");

                        PlayerPrefs.Save();
                    }
                }
            }
        }

        void Start()
        {
            // Cache
            valuesUI = GameRefs.Instance.valuesUI;
            worldUI = GameRefs.Instance.worldUI;
            worldGameUIDoc = GameRefs.Instance.worldGameUIDoc;
            mergeUI = GameRefs.Instance.mergeUI;
            dataManager = DataManager.Instance;
            gameData = GameData.Instance;
            inputMenu = GameRefs.Instance.inputMenu;
            uiButtons = GameData.Instance.GetComponent<UIButtons>();
            pointerHandler = GetComponent<PointerHandler>();
            infoBox = GameRefs.Instance.infoBox;
            cloudSave = Services.Instance.GetComponent<CloudSave>();
            analyticsManager = AnalyticsManager.Instance;
            convoUIHandler = GameRefs.Instance.convoUIHandler;
            boardManager = GameRefs.Instance.boardManager;
            sceneLoader = GameRefs.Instance.sceneLoader;
            progressManager = GameRefs.Instance.progressManager;
            valuePop = GameRefs.Instance.valuePop;

            referencesSet = true;

            // UI
            if (worldGameUIDoc != null)
            {
                convoBackground = worldGameUIDoc.GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>("ConvoBackground");
            }

            if (tutorialStarted)
            {
                analyticsManager.FireTutorialEvent("", true);

                tutorialStarted = false;
            }

            cloudSave.SaveDataAsync("tutorialStep", PlayerPrefs.GetString("tutorialStep"));

            StartCoroutine(WaitForLoading());
        }

        IEnumerator WaitForLoading()
        {
            while (destroyingTutorial || !dataManager.loaded || !valuesUI.loaded || (convoUIHandler != null && !convoUIHandler.loaded) || (worldUI != null && !worldUI.loaded) || (mergeUI != null && !mergeUI.loaded))
            {
                yield return null;
            }

            Init();
        }

#if UNITY_EDITOR
        void Init()
        {
            if (CheckScene())
            {
                SetUI();

                progressManager.SetInitialData(0);

                StartCoroutine(WaitForDataOrBoard());
            }
        }

        bool CheckScene()
        {
            Types.Scene currentScene = sceneLoader.GetScene();

            if (!PlayerPrefs.HasKey("taskToComplete"))
            {
                for (int i = 0; i < tutorialData.steps.Length; i++)
                {
                    if (tutorialData.steps[i].id == tutorialStep)
                    {
                        if (tutorialData.steps[i].scene == Types.Scene.Merge && currentScene != Types.Scene.Merge)
                        {
                            // Merge scene
                            sceneLoader.Load(Types.Scene.Merge);

                            return false;
                        }

                        if (tutorialData.steps[i].scene == Types.Scene.World && currentScene != Types.Scene.World)
                        {
                            // World scene
                            sceneLoader.Load(Types.Scene.World);

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
            SetUI();

            progressManager.SetInitialData(0);

            StartCoroutine(WaitForDataOrBoard());
        }
#endif

        IEnumerator WaitForDataOrBoard()
        {
            while (!dataManager.loaded || (boardManager != null && !boardManager.boardSet))
            {
                yield return null;
            }

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

        public bool CheckIfNextStepIsConvo()
        {
            for (int i = 0; i < tutorialData.steps.Length; i++)
            {
                if (tutorialData.steps[i].id == tutorialStep)
                {
                    if (tutorialData.steps[i].type == Types.TutorialStepType.Convo)
                    {
                        return true;
                    }

                    break;
                }
            }

            return false;
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

        public void NextStep(bool handleNextStep = true, Action callback = null)
        {
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
            Types.Scene currentScene = sceneLoader.GetScene();

            if (currentScene == Types.Scene.World)
            {
                worldUI.HideButton(Types.Button.Play, true);
                worldUI.HideButton(Types.Button.Task, true);

                return;
            }

            if (currentScene == Types.Scene.Merge)
            {
                mergeUI.HideButton(Types.Button.Home, true);
                mergeUI.HideButton(Types.Button.Task, true);

                return;
            }
        }

        void HandleTask(Types.TutorialStep step)
        {
            if (infoBox != null)
            {
                infoBox.SetTutorialData(step.id);
            }

            switch (step.taskType)
            {
                case Types.TutorialStepTask.Press:
                    TaskPress(step);
                    break;
                case Types.TutorialStepTask.Menu:
                    pointerHandler.HandlePress(Vector2.zero, Types.Button.TaskMenu, () =>
                    {
                        NextStep();
                    });
                    break;
                case Types.TutorialStepTask.Merge:
                    pointerHandler.HandleMerge(step.taskSprite, () =>
                    {
                        NextStep();
                    });
                    break;
                case Types.TutorialStepTask.Gen:
                    pointerHandler.HandleGen(step.genSprite, step.taskSprite, step.itemGroup, () =>
                    {
                        NextStep();
                    });
                    break;
            }
        }

        void TaskPress(Types.TutorialStep step)
        {
            if (step.scene == Types.Scene.World)
            {
                if (step.taskButton == Types.Button.Play)
                {
                    worldUI.ShowButton(Types.Button.Play);
                    worldUI.HideButton(Types.Button.Task, true);

                    Glob.SetTimeout(() =>
                    {
                        pointerHandler.HandlePress(uiButtons.worldPlayButtonPos, Types.Button.Play, () =>
                        {
                            NextStep(false, () =>
                            {
                                sceneLoader.Load(Types.Scene.Merge);
                            });
                        });
                    }, 0.5f);
                }

                if (step.taskButton == Types.Button.Task)
                {
                    worldUI.HideButton(Types.Button.Play, true);
                    worldUI.ShowButton(Types.Button.Task);

                    Glob.SetTimeout(() =>
                    {
                        pointerHandler.HandlePress(uiButtons.worldTaskButtonPos, Types.Button.Task, () =>
                        {
                            NextStep();
                        });
                    }, 0.5f);
                }
            }
            else // Types.Scene.Merge
            {
                if (step.taskButton == Types.Button.Task)
                {
                    mergeUI.HideButton(Types.Button.Home, true);
                    mergeUI.ShowButton(Types.Button.Task);

                    pointerHandler.HandlePress(uiButtons.mergeTaskButtonPos, Types.Button.Task, () =>
                    {
                        NextStep();
                    });
                }

                if (step.taskButton == Types.Button.Home)
                {
                    mergeUI.ShowButton(Types.Button.Home);
                    mergeUI.HideButton(Types.Button.Task, true);

                    pointerHandler.HandlePress(uiButtons.mergeHomeButtonPos, Types.Button.Home, () =>
                    {
                        NextStep(false, () =>
                        {
                            sceneLoader.Load(Types.Scene.World);
                        });
                    });
                }
            }
        }

        void HandleConvo(Types.TutorialStep step)
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

            Destroy(gameObject);
        }

        void SetUI()
        {
            valuesUI.SetSortingOrder(4);

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
            valuesUI.SetSortingOrder(10);

            valuesUI.ShowButtons();

            if (worldUI != null)
            {
                worldUI.ShowButtons();
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

            while (gameData.boardData == null)
            {
                yield return null;
            }

            callback();

            Destroy(gameObject);
        }
    }
}

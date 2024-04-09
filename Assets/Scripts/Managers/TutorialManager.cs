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
        public bool startFromStep = false;
        public string stepToStartFrom = "";
        public TutorialData tutorialData;
        public ProgressManager progressManager;
        public ConvoUIHandler convoUIHandler;
        public SceneLoader sceneLoader;
        public ShineSprites[] shineSprites;

        private string tutorialStep = "First";
        private bool pointsShone = false;
        private Types.Scene currentScene;

        private bool handlingLast = false;

        private bool tutorialStarted = false;

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
        private InputMenu inputMenu;
        private PointerHandler pointerHandler;
        private InfoBox infoBox;
        private CloudSave cloudSave;
        private AnalyticsManager analyticsManager;

        // UI
        private VisualElement root;

        private VisualElement convoBackground;

        void Awake()
        {
            currentScene = Glob.ParseEnum<Types.Scene>(SceneManager.GetActiveScene().name);

            if (PlayerPrefs.HasKey("tutorialFinished") || skipTutorial || (Glob.lastScene != Types.Scene.None && Glob.lastScene == currentScene))
            {
                handlingLast = true;

                StartCoroutine(WaitForDataManager());
            }
            else
            {
                if (startFromStep && stepToStartFrom != "")
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
            inputMenu = GameRefs.Instance.inputMenu;
            uiButtons = GameData.Instance.GetComponent<UIButtons>();
            pointerHandler = GetComponent<PointerHandler>();
            infoBox = GameRefs.Instance.infoBox;
            cloudSave = Services.Instance.GetComponent<CloudSave>();
            //analyticsManager = Services.Instance.GetComponent<AnalyticsManager>();
            analyticsManager = AnalyticsManager.Instance;

            // UI
            root = GetComponent<UIDocument>().rootVisualElement;

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
            while (handlingLast || !dataManager.loaded || !valuesUI.loaded || (convoUIHandler != null && !convoUIHandler.loaded) || (worldUI != null && !worldUI.loaded) || (mergeUI != null && !mergeUI.loaded))
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
                valuesUI.SetSortingOrder(4);
                valuesUI.DisableButtonsAlt();

                progressManager.SetInitialData(0, false);

                HandleStep();
            }
        }

        bool CheckScene()
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

                    break;
                }
            }

            return true;
        }
#else
        void Init()
        {
            valuesUI.SetSortingOrder(4);
            valuesUI.DisableButtonsAlt();

            progressManager.SetInitialData(0, false);

            HandleStep();
        }
#endif

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

        void NextStep(bool handleNextStep = true, Action callback = null)
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
                    worldUI.ShowButton(Types.Button.Task);
                    worldUI.HideButton(Types.Button.Play, true);

                    Glob.SetTimeout(() =>
                    {
                        pointerHandler.HandlePress(uiButtons.worldTaskButtonPos, Types.Button.Task, () =>
                        {
                            NextStep();
                        });
                    }, 0.5f);
                }
            }
            else
            {
                if (step.taskButton == Types.Button.Task)
                {
                    mergeUI.ShowButton(Types.Button.Task);
                    mergeUI.HideButton(Types.Button.Home, true);

                    pointerHandler.HandlePress(uiButtons.mergeTaskButtonPos, Types.Button.Task, () =>
                    {
                        NextStep();
                    });
                }

                if (step.taskButton == Types.Button.Home)
                {
                    mergeUI.ShowButton(Types.Button.Home);

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

            Destroy(gameObject);
        }

        IEnumerator WaitForDataManager()
        {
            while (DataManager.Instance == null)
            {
                yield return null;
            }

            progressManager.SetInitialData(0, true);

            Destroy(gameObject);
        }

        IEnumerator WaitForGameReferences()
        {
            if (!skipTutorial)
            {
                while (GameRefs.Instance == null)
                {
                    yield return null;
                }

                while (!GameRefs.Instance.ready)
                {
                    yield return null;
                }

                valuesUI = GameRefs.Instance.valuesUI;
                worldUI = GameRefs.Instance.worldUI;
                mergeUI = GameRefs.Instance.mergeUI;

                /*  if (valuesUI != null)
                  {
                      valuesUI.SetSortingOrder(10);
                      valuesUI.EnableButtonsAlt();
                  }

                  if (worldUI != null)
                  {
                      worldUI.ShowButtons();
                  }

                  if (mergeUI != null)
                  {
                      mergeUI.ShowButtons();
                  }*/
            }

            Destroy(gameObject);
        }
    }
}

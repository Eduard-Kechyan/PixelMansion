using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Merge
{
    public class ConvoUIHandler : MonoBehaviour
    {
        // Variables
        public ConvoData convoData;
        public CharacterColor[] characterColors;
        [ReadOnly]
        public bool isConvoOpen = false;

        [Header("Debug")]
        public bool converse;

        [HideInInspector]
        public bool loaded = false;

        private Coroutine convoContainerTimeOut;

        private bool showText = false;

        private ConvoGroup currentConvoGroup;
        private int currentConvo;
        private string convoId = "";

        private bool canSkip = true;
        private Action callback;
        private bool closeAfter = true;
        private bool showUnderlay = true;

        private bool canHandleNext = false;

        private Sprite[] avatarsSprites;

        private Scale fullScale = new(new Vector2(1f, 1f));
        private Scale smallScale = new(new Vector2(0.8f, 0.8f));
        private Scale fullScaleFlipped = new(new Vector2(-1f, 1f));
        private Scale smallScaleFlipped = new(new Vector2(-0.8f, 0.8f));

        // Enums
        public enum Character
        {
            NONE,
            Julia,
            James,
        };

        public enum CharacterExpression
        {
            Natural,
            Happy,
            Surprised,
            Angry,
            Sad,
            Sleepy,
            Thinking
        };

        // Classes
        [Serializable]
        public class CharacterColor
        {
            [HideInInspector]
            public string name;
            public Character character;
            public Color accentColor = Color.black;
        }

        [Serializable]
        public class ConvoGroup
        {
            [HideInInspector]
            public string name;
            public string id;
            public bool hasTimeOut = true;
            public Character characterA = Character.Julia;
            public Character characterB = Character.NONE;
            public List<Convo> content;
        }

        [Serializable]
        public class Convo
        {
            public Character character;
            public CharacterExpression expression;
            public bool isRight;
            public bool isSide;
            public string convoExtra;
        }

        // References
        private GameRefs gameRefs;
        private WorldUI worldUI;
        private ValuesUI valuesUI;
        private I18n LOCALE;
        private CharMain charMain;
        private AddressableManager addressableManager;
        private TutorialManager tutorialManager;

        // UI
        private VisualElement root;
        private VisualElement convoContainer;
        private VisualElement convoUnderlay;
        private VisualElement convoBoxUnderlay;
        private VisualElement skipButton;
        private Label skipLabel;

        private VisualElement convoBox;
        private VisualElement nextButton;
        private Label nextLabel;
        private VisualElement namePlate;
        private VisualElement namePlateEdgeLeft;
        private VisualElement namePlateEdgeRight;
        private Label nameLabel;
        private Label convoLabel;

        private VisualElement avatarLeft;
        private VisualElement avatarRight;

        void Start()
        {
            // Cache
            gameRefs = GameRefs.Instance;
            worldUI = gameRefs.worldUI;
            valuesUI = gameRefs.valuesUI;
            LOCALE = I18n.Instance;
            charMain = CharMain.Instance;
            addressableManager = DataManager.Instance.GetComponent<AddressableManager>();
            tutorialManager = gameRefs.tutorialManager;

            // UI
            root = GetComponent<UIDocument>().rootVisualElement;

            convoContainer = root.Q<VisualElement>("ConvoContainer");
            convoUnderlay = convoContainer.Q<VisualElement>("ConvoUnderlay");

            convoBoxUnderlay = convoContainer.Q<VisualElement>("ConvoBoxUnderlay");

            convoBox = convoContainer.Q<VisualElement>("ConvoBox");
            nextButton = convoBox.Q<VisualElement>("NextButtonContainer");
            nextLabel = nextButton.Q<Label>("Label");
            namePlate = convoBox.Q<VisualElement>("NamePlate");
            namePlateEdgeLeft = namePlate.Q<VisualElement>("EdgeLeft");
            namePlateEdgeRight = namePlate.Q<VisualElement>("EdgeRight");
            nameLabel = namePlate.Q<Label>("NameLabel");
            convoLabel = convoBox.Q<Label>("ConvoLabel");

            avatarLeft = convoBox.Q<VisualElement>("AvatarLeft");
            avatarRight = convoBox.Q<VisualElement>("AvatarRight");

            // UI taps
            /*nextButton.AddManipulator(new Clickable(evt =>
            {
                HandleNext();
            }));*/

            convoContainer.AddManipulator(new Clickable(evt =>
            {
                HandleNext();
            }));

            StartCoroutine(WaitForInitialization());
        }

        IEnumerator WaitForInitialization()
        {
            while (!addressableManager.initialized)
            {
                yield return null;
            }

            Init();
        }

        async void Init()
        {
            nextLabel.text = LOCALE.Get("convo_next_button");

            nextButton.style.opacity = 0;

            canHandleNext = false;

            avatarsSprites = await addressableManager.LoadAssetAllArrayAsync<Sprite>("avatars");

            loaded = true;

            if (Debug.isDebugBuild)
            {
                skipButton = convoContainer.Q<VisualElement>("SkipButtonContainer");
                skipLabel = skipButton.Q<Label>("Label");

                skipButton.style.display = DisplayStyle.Flex;

                skipButton.AddManipulator(new Clickable(evt =>
                {
                    HandleSkip();
                }));

                skipLabel.text = LOCALE.Get("convo_skip_button");
            }
        }

        void OnValidate()
        {
            if (converse)
            {
                converse = false;

                Converse("Response"); // Dummy 
            }

            if (characterColors.Length > 0)
            {
                for (int i = 0; i < characterColors.Length; i++)
                {
                    if (characterColors[i].character != Character.NONE)
                    {
                        characterColors[i].name = characterColors[i].character.ToString();
                    }
                }
            }
        }

        public void Converse(string newConvoId, bool newCanSkip = true, bool newCloseAfter = true, bool newShowUnderlay = true, Action newCallback = null)
        {
            convoId = newConvoId;
            closeAfter = newCloseAfter;
            showUnderlay = newShowUnderlay;

            for (int i = 0; i < convoData.convoGroups.Length; i++)
            {
                if (convoData.convoGroups[i].id == newConvoId)
                {
                    currentConvoGroup = convoData.convoGroups[i];

                    currentConvo = 0;

                    SetAvatar(currentConvoGroup.content[0]);

                    isConvoOpen = true;

                    callback = newCallback;
                    canSkip = newCanSkip;

                    if (currentConvoGroup.hasTimeOut)
                    {
                        Glob.SetTimeout(() =>
                        {
                            UpdateConvo();
                        }, 0.2f);
                    }
                    else
                    {
                        UpdateConvo();
                    }

                    break;
                }
            }
        }

        void UpdateConvo()
        {
            List<TimeValue> nullDelay = new() { new TimeValue(0.0f) };
            List<TimeValue> halfDelay = new() { new TimeValue(0.3f) };
            List<TimeValue> fullDelay = new() { new TimeValue(0.6f) };

            if (isConvoOpen)
            {
                worldUI.CloseUI();

                valuesUI.CloseUI();

                charMain.Hide();

                convoLabel.text = "";

                Glob.StopTimeout(convoContainerTimeOut);

                SetNameAndText(currentConvo, true);

                convoContainer.style.display = DisplayStyle.Flex;

                convoUnderlay.style.opacity = 1;

                convoBox.style.bottom = 24;
                convoBox.style.transitionDelay = halfDelay;

                if (showUnderlay)
                {
                    convoBoxUnderlay.style.height = 58;
                    convoBoxUnderlay.style.transitionDelay = nullDelay;
                }

                if (canSkip)
                {
                    skipButton.style.opacity = 1;
                    skipButton.style.transitionDelay = halfDelay;
                }

                avatarLeft.style.left = 0;
                avatarLeft.style.transitionDelay = nullDelay;

                if (currentConvoGroup.characterB != Character.NONE)
                {
                    avatarRight.style.right = 4;
                    avatarRight.style.transitionDelay = nullDelay;
                }
            }
            else
            {
                if (closeAfter)
                {
                    worldUI.OpenUI();

                    valuesUI.OpenUI();

                    Glob.SetTimeout(() =>
                    {
                        charMain.Show();
                    }, 0.3f);
                }

                convoContainerTimeOut = Glob.SetTimeout(() =>
                {
                    convoContainer.style.display = DisplayStyle.None;
                }, 0.6f);

                convoUnderlay.style.opacity = 0;

                convoBox.style.bottom = -120f;
                convoBox.style.transitionDelay = halfDelay;

                convoBoxUnderlay.style.height = 0;
                convoBoxUnderlay.style.transitionDelay = halfDelay;

                skipButton.style.opacity = 0;
                skipButton.style.transitionDelay = halfDelay;

                avatarLeft.style.left = -96;
                avatarLeft.style.transitionDelay = fullDelay;
                avatarRight.style.right = -96;
                avatarRight.style.transitionDelay = fullDelay;

                canSkip = true;
                closeAfter = true;
            }
        }

        IEnumerator FadeInText(string text)
        {
            showText = true;

            while (showText)
            {
                for (int i = 0; i < text.Length; i++)
                {
                    convoLabel.text += text[i];

                    if (i == text.Length - 1)
                    {
                        nextButton.style.display = DisplayStyle.Flex;
                        nextButton.style.opacity = 1;

                        canHandleNext = true;

                        showText = false;
                    }

                    yield return new WaitForSeconds(0.01f); // Less than a millisecond
                }
            }
        }

        void HandleSkip()
        {
            showText = false;
            StopCoroutine(FadeInText(""));

            isConvoOpen = false;
            UpdateConvo();

            PlayerPrefs.DeleteKey("ProgressStep");

            PlayerPrefs.Save();

            callback?.Invoke();

            callback = null;
        }

        void HandleNext()
        {
            if (canHandleNext)
            {
                nextButton.style.display = DisplayStyle.None;
                nextButton.style.opacity = 0;

                canHandleNext = false;

                convoLabel.text = "";

                currentConvo++;

                if (convoId == "TutorialPart1" && currentConvo == 3)
                {
                    tutorialManager.CheckConvoBackground(true);
                }

                if (LOCALE.CheckIfExists("convo_" + currentConvoGroup.id + "_" + currentConvo))
                {
                    // Next
                    SetNameAndText(currentConvo);
                }
                else
                {
                    // Last
                    HandleSkip();
                }
            }
        }

        void SetNameAndText(int order, bool hasTimeout = false)
        {
            string text;

            if (currentConvoGroup.content[order].convoExtra == "")
            {
                text = LOCALE.Get("convo_" + currentConvoGroup.id + "_" + order);
            }
            else
            {
                string textExtra = "";

                if (currentConvoGroup.content[order].convoExtra == "PlayerName")
                {
                    textExtra = GameData.Instance.playerName;
                }

                text = LOCALE.Get("convo_" + currentConvoGroup.id + "_" + order, textExtra);
            }

            nameLabel.text = currentConvoGroup.content[order].character.ToString();

            Color namePlateColor = GetColor(currentConvoGroup.content[order].character);

            namePlate.style.unityBackgroundImageTintColor = namePlateColor;
            namePlateEdgeLeft.style.unityBackgroundImageTintColor = namePlateColor;
            namePlateEdgeRight.style.unityBackgroundImageTintColor = namePlateColor;

            if (currentConvoGroup.content[order].isRight)
            {
                namePlate.AddToClassList("name_plate_right");

                avatarLeft.style.scale = new StyleScale(smallScale);
                avatarRight.style.scale = new StyleScale(fullScaleFlipped);
            }
            else
            {
                namePlate.RemoveFromClassList("name_plate_right");

                avatarLeft.style.scale = new StyleScale(fullScale);
                avatarRight.style.scale = new StyleScale(smallScaleFlipped);
            }

            SetAvatar(currentConvoGroup.content[order]);

            showText = false;
            StopCoroutine(FadeInText(""));

            if (hasTimeout)
            {
                Glob.SetTimeout(() =>
                {
                    StartCoroutine(FadeInText(text));
                }, 0.6f);
            }
            else
            {
                StartCoroutine(FadeInText(text));
            }
        }

        void SetAvatar(Convo convo = null)
        {
            if (convo == null)
            {
                avatarLeft.style.backgroundImage = new StyleBackground(FindSprites(currentConvoGroup.characterA.ToString(), CharacterExpression.Natural, false));

                if (currentConvoGroup.characterB != Character.NONE)
                {
                    avatarRight.style.backgroundImage = new StyleBackground(FindSprites(currentConvoGroup.characterB.ToString(), CharacterExpression.Natural, false));
                }
            }
            else
            {
                avatarLeft.style.backgroundImage = new StyleBackground(FindSprites(currentConvoGroup.characterA.ToString(), convo.expression, convo.isSide));

                if (currentConvoGroup.characterB != Character.NONE)
                {
                    avatarRight.style.backgroundImage = new StyleBackground(FindSprites(currentConvoGroup.characterB.ToString(), convo.expression, convo.isSide));
                }
            }
        }

        Color GetColor(Character character)
        {
            for (int i = 0; i < characterColors.Length; i++)
            {
                if (characterColors[i].character == character)
                {
                    return characterColors[i].accentColor;
                }
            }

            return Color.black;
        }

        Sprite FindSprites(string characterName, CharacterExpression expression = CharacterExpression.Natural, bool isSide = false)
        {
            foreach (Sprite sprite in avatarsSprites)
            {
                if (isSide)
                {
                    if (sprite.name == "Avatar" + characterName + "Side" + ((int)expression + 1))
                    {
                        return sprite;
                    }
                }
                else
                {
                    if (sprite.name == "Avatar" + characterName + ((int)expression + 1))
                    {
                        return sprite;
                    }
                }
            }

            return null;
        }
    }
}

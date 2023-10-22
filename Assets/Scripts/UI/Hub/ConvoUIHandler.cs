using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Merge
{
    public class ConvoUIHandler : MonoBehaviour
    {
        // Variables
        public ProgressManager progressManager;
        public ConvoData convoData;
        public Types.EyeColor[] characterColors;
        [ReadOnly]
        public bool isConvoOpen = false;

        [Header("Debug")]
        public bool converse;

        private Coroutine convoContainerTimeOut;

        private bool showText = false;

        private Types.ConvoGroup currentConvoGroup;
        private int currentConvo;

        private Sprite[] avatarsSprites;
        private Sprite[] eyesSprites;
        private Sprite[] mouthsSprites;

        private Scale fullScale = new(new Vector2(1f, 1f));
        private Scale smallScale = new(new Vector2(0.8f, 0.8f));
        private Scale fullScaleFlipped = new(new Vector2(-1f, 1f));
        private Scale smallScaleFlipped = new(new Vector2(-0.8f, 0.8f));

        // References
        private HubUI hubUI;
        private ValuesUI valuesUI;
        private I18n LOCALE;
        private CharMain charMain;

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
        private VisualElement avatarLeftEyes;
        private VisualElement avatarLeftEyeColor;
        private VisualElement avatarLeftMouth;
        private VisualElement avatarRight;
        private VisualElement avatarRightEyes;
        private VisualElement avatarRightEyeColor;
        private VisualElement avatarRightMouth;

        void Start()
        {
            // Cache
            hubUI = GameRefs.Instance.hubUI;
            valuesUI = GameRefs.Instance.valuesUI;
            LOCALE = I18n.Instance;
            charMain = CharMain.Instance;

            // UI
            root = GetComponent<UIDocument>().rootVisualElement;

            convoContainer = root.Q<VisualElement>("ConvoContainer");
            convoUnderlay = convoContainer.Q<VisualElement>("ConvoUnderlay");

            convoBoxUnderlay = convoContainer.Q<VisualElement>("ConvoBoxUnderlay");
            skipButton = convoContainer.Q<VisualElement>("SkipButtonContainer");
            skipLabel = skipButton.Q<Label>("Label");

            convoBox = convoContainer.Q<VisualElement>("ConvoBox");
            nextButton = convoBox.Q<VisualElement>("NextButtonContainer");
            nextLabel = nextButton.Q<Label>("Label");
            namePlate = convoBox.Q<VisualElement>("NamePlate");
            namePlateEdgeLeft = namePlate.Q<VisualElement>("EdgeLeft");
            namePlateEdgeRight = namePlate.Q<VisualElement>("EdgeRight");
            nameLabel = namePlate.Q<Label>("NameLabel");
            convoLabel = convoBox.Q<Label>("ConvoLabel");

            avatarLeft = convoBox.Q<VisualElement>("AvatarLeft");
            avatarLeftEyes = avatarLeft.Q<VisualElement>("Eyes");
            avatarLeftEyeColor = avatarLeft.Q<VisualElement>("EyeColor");
            avatarLeftMouth = avatarLeft.Q<VisualElement>("Mouth");
            avatarRight = convoBox.Q<VisualElement>("AvatarRight");
            avatarRightEyes = avatarRight.Q<VisualElement>("Eyes");
            avatarRightEyeColor = avatarRight.Q<VisualElement>("EyeColor");
            avatarRightMouth = avatarRight.Q<VisualElement>("Mouth");

            // UI Taps
            skipButton.AddManipulator(new Clickable(evt =>
            {
                HandleSkip();
            }));

            nextButton.AddManipulator(new Clickable(evt =>
            {
                HandleNext();
            }));

            Init();
        }

        void Init()
        {
            skipLabel.text = LOCALE.Get("convo_skip_button");
            nextLabel.text = LOCALE.Get("convo_next_button");

            nextButton.style.opacity = 0;

            avatarsSprites = Resources.LoadAll<Sprite>("Sprites/Avatars/Main");
            eyesSprites = Resources.LoadAll<Sprite>("Sprites/Avatars/Eyes");
            mouthsSprites = Resources.LoadAll<Sprite>("Sprites/Avatars/Mouths");
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
                    if (characterColors[i].character != Types.Character.NONE)
                    {
                        characterColors[i].name = characterColors[i].character.ToString();
                    }
                }
            }
        }

        public void Converse(string convoId)
        {
            for (int i = 0; i < convoData.convoGroups.Length; i++)
            {
                if (convoData.convoGroups[i].id == convoId)
                {
                    currentConvoGroup = convoData.convoGroups[i];

                    currentConvo = 0;

                    SetAvatar();
                    SetAvatarFace(currentConvoGroup.content[0]);

                    isConvoOpen = true;

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
                hubUI.CloseUI();

                valuesUI.CloseUI();

                charMain.Hide();

                convoLabel.text = "";

                Glob.StopTimeout(convoContainerTimeOut);

                SetNameAndText(currentConvo, true);

                convoContainer.style.display = DisplayStyle.Flex;

                convoUnderlay.style.opacity = 1;

                convoBox.style.bottom = 4;
                convoBox.style.transitionDelay = halfDelay;

                convoBoxUnderlay.style.height = 58;
                convoBoxUnderlay.style.transitionDelay = nullDelay;
                skipButton.style.opacity = 1;
                skipButton.style.transitionDelay = halfDelay;

                avatarLeft.style.left = 4;
                avatarLeft.style.transitionDelay = nullDelay;

                if (currentConvoGroup.characterB != Types.Character.NONE)
                {
                    avatarRight.style.right = 4;
                    avatarRight.style.transitionDelay = nullDelay;
                }
            }
            else
            {
                hubUI.OpenUI();

                valuesUI.OpenUI();

                convoContainerTimeOut = Glob.SetTimeout(() =>
                {
                    convoContainer.style.display = DisplayStyle.None;
                }, 0.6f);

                Glob.SetTimeout(() =>
                {
                    charMain.Show();
                }, 0.3f);

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

                        showText = false;
                    }

                    yield return new WaitForSeconds(0.03f); // Less than a millisecond
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
        }

        void HandleNext()
        {
            nextButton.style.display = DisplayStyle.None;
            nextButton.style.opacity = 0;

            convoLabel.text = "";

            currentConvo++;

            if (LOCALE.CheckNext("convo_" + currentConvoGroup.id + "_" + currentConvo))
            {
                // Next
                SetNameAndText(currentConvo);
            }
            else
            {
                // Last
                isConvoOpen = false;

                UpdateConvo();

                PlayerPrefs.DeleteKey("ProgressStep");

                PlayerPrefs.Save();
            }
        }

        void SetNameAndText(int order, bool hasTimeout = false)
        {
            string text = LOCALE.Get("convo_" + currentConvoGroup.id + "_" + order);

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

            SetAvatarFace(currentConvoGroup.content[order]);

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

        void SetAvatar()
        {
            avatarLeft.style.backgroundImage = new StyleBackground(FindSprites("Avatar", Types.Expression.Normal, false, currentConvoGroup.characterA.ToString()));
            avatarLeftEyes.style.backgroundImage = new StyleBackground(FindSprites("Eyes"));
            avatarLeftEyeColor.style.unityBackgroundImageTintColor = GetColor(currentConvoGroup.characterA, true);
            avatarLeftMouth.style.backgroundImage = new StyleBackground(FindSprites("Mouth"));

            if (currentConvoGroup.characterB != Types.Character.NONE)
            {
                avatarRight.style.backgroundImage = new StyleBackground(FindSprites("Avatar", Types.Expression.Normal, false, currentConvoGroup.characterB.ToString()));
                avatarRightEyes.style.backgroundImage = new StyleBackground(FindSprites("Eyes"));
                avatarRightEyeColor.style.unityBackgroundImageTintColor = GetColor(currentConvoGroup.characterB, true);
                avatarRightMouth.style.backgroundImage = new StyleBackground(FindSprites("Mouth"));
            }
        }

        void SetAvatarFace(Types.Convo convo)
        {
            if (convo.isRight)
            {
                avatarRightEyes.style.backgroundImage = new StyleBackground(FindSprites("Eyes", convo.expression, convo.isSide));
                avatarRightMouth.style.backgroundImage = new StyleBackground(FindSprites("Mouth", convo.expression));
            }
            else
            {
                avatarLeftEyes.style.backgroundImage = new StyleBackground(FindSprites("Eyes", convo.expression, convo.isSide));
                avatarLeftMouth.style.backgroundImage = new StyleBackground(FindSprites("Mouth", convo.expression));
            }
        }

        Color GetColor(Types.Character character, bool eyeColor = false)
        {
            for (int i = 0; i < characterColors.Length; i++)
            {
                if (characterColors[i].character == character)
                {
                    if (eyeColor)
                    {
                        return characterColors[i].eyeColor;
                    }
                    else
                    {
                        return characterColors[i].accentColor;
                    }
                }
            }

            return Color.black;
        }

        Sprite FindSprites(string type, Types.Expression expression = Types.Expression.Normal, bool isSide = false, string characterName = "")
        {
            switch (type)
            {
                case "Avatar":
                    foreach (Sprite sprite in avatarsSprites)
                    {
                        if (sprite.name == "Avatar" + characterName)
                        {
                            return sprite;
                        }
                    }
                    break;
                case "Eyes":
                    foreach (Sprite sprite in eyesSprites)
                    {
                        if (isSide)
                        {
                            if (sprite.name == "EyesSide" + expression)
                            {
                                return sprite;
                            }
                        }
                        else
                        {
                            if (sprite.name == "Eyes" + expression)
                            {
                                return sprite;
                            }
                        }
                    }
                    break;
                case "Mouth":
                    foreach (Sprite sprite in mouthsSprites)
                    {
                        if (sprite.name == "Mouth" + expression)
                        {
                            return sprite;
                        }
                    }
                    break;
            }

            return null;
        }
    }
}

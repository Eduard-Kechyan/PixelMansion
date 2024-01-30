using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Merge
{
    public class EarthOptions : MonoBehaviour
    {
        // Variables
        public UIDocument uiDoc;
        public AudioSource sourceSound;
        public AudioSource sourceMusic;
        public RotationSpeed rotationSpeed = RotationSpeed.Normal;
        public Sprite musicOnSprite;
        public Sprite musicOffSprite;
        public Sprite speedNormalSprite;
        public Sprite speedFastSprite;
        public Sprite speedSlowSprite;
        public float timeOfDayLength = 2f;
        public float boxLength = 1f;

        [Header("Stats")]
        [ReadOnly]
        public bool musicOn = true;

        public enum RotationSpeed
        {
            Slow,
            Normal,
            Fast
        }

        private bool buttonBoxOpen = false;
        private bool showTimeOfDat = false;

        // References
        private EarthMaster earthMaster;

        // UI
        private VisualElement root;
        private Button tapperButton;
        private VisualElement box;
        private VisualElement buttonBox;
        private Button settingsButton;
        private Button musicButton;
        private VisualElement musicIcon;
        private Button speedButton;
        private VisualElement speedIcon;
        private Button dayButton;
        private Button duskButton;
        private Button nightButton;
        private Button dawnButton;
        private Label timeOfDayLabel;

        void Start()
        {
            // Cache
            earthMaster = GetComponent<EarthMaster>();

            // UI
            root = uiDoc.rootVisualElement;
            box = root.Q<VisualElement>("Box");
            tapperButton = root.Q<Button>("TapperButton");
            buttonBox = root.Q<VisualElement>("ButtonBox");

            settingsButton = root.Q<Button>("SettingsButton");
            musicButton = root.Q<Button>("MusicButton");
            musicIcon = musicButton.Q<VisualElement>("Icon");
            speedButton = root.Q<Button>("SpeedButton");
            speedIcon = speedButton.Q<VisualElement>("Icon");
            dayButton = root.Q<Button>("DayButton");
            duskButton = root.Q<Button>("DuskButton");
            nightButton = root.Q<Button>("NightButton");
            dawnButton = root.Q<Button>("DawnButton");

            timeOfDayLabel = root.Q<Label>("TimeOfDayLabel");

            tapperButton.clicked += () =>
            {
                ShowBox();
            };
            settingsButton.clicked += () =>
            {
                PlaySound();
                ToggleButtonBox();
            };
            musicButton.clicked += () =>
            {
                PlaySound();
                ToggleMusic();
            };
            speedButton.clicked += () =>
            {
                PlaySound();
                SetSpeed();
            };
            dayButton.clicked += () =>
            {
                PlaySound();
                earthMaster.mainRotation = 298;
            };
            duskButton.clicked += () =>
            {
                PlaySound();
                earthMaster.mainRotation = 46;
            };
            nightButton.clicked += () =>
            {
                PlaySound();
                earthMaster.mainRotation = 132;
            };
            dawnButton.clicked += () =>
            {
                PlaySound();
                earthMaster.mainRotation = 210;
            };
        }

        void PlaySound()
        {
            sourceSound.Play();
        }

        void ToggleMusic()
        {
            musicOn = !musicOn;

            sourceMusic.volume = musicOn ? 1 : 0;

            musicIcon.style.backgroundImage = new StyleBackground(musicOn ? musicOnSprite : musicOffSprite);
        }

        void SetSpeed()
        {
            if (rotationSpeed == RotationSpeed.Normal)
            {
                rotationSpeed = RotationSpeed.Fast;
                speedIcon.style.backgroundImage = new StyleBackground(speedFastSprite);
            }
            else if (rotationSpeed == RotationSpeed.Fast)
            {
                rotationSpeed = RotationSpeed.Slow;
                speedIcon.style.backgroundImage = new StyleBackground(speedSlowSprite);
            }
            else
            {
                rotationSpeed = RotationSpeed.Normal;
                speedIcon.style.backgroundImage = new StyleBackground(speedNormalSprite);
            }
        }

        void ShowBox()
        {
            StopCoroutine(HideBox());

            box.style.opacity = 1;
            box.style.visibility = Visibility.Visible;

            tapperButton.style.visibility = Visibility.Hidden;

            StartCoroutine(HideBox());
        }

        IEnumerator HideBox()
        {
            yield return new WaitForSeconds(boxLength);

            if(!buttonBoxOpen)
            {
                box.style.opacity = 0;
                box.style.visibility = Visibility.Hidden;

                tapperButton.style.visibility = Visibility.Visible;
            }
        }

        void ToggleButtonBox()
        {
            StopCoroutine(HideBox());
            
            buttonBoxOpen = !buttonBoxOpen;

            buttonBox.style.top = buttonBoxOpen ? 0 : 40;            

            if (!buttonBoxOpen)
            {
                StartCoroutine(HideBox());
            }
        }

        public void ShowTimeOfDay(string timeName)
        {
            showTimeOfDat = false;

            StopCoroutine(HideTimeOfDay());

            timeOfDayLabel.text = timeName;

            List<TimeValue> transitionB = new() { new TimeValue(0.2f, TimeUnit.Second) };

            timeOfDayLabel.style.transitionDuration = new StyleList<TimeValue>(transitionB);

            timeOfDayLabel.style.opacity = 1f;

            showTimeOfDat = true;

            StartCoroutine(HideTimeOfDay());
        }

        IEnumerator HideTimeOfDay()
        {
            if (showTimeOfDat)
            {
                yield return new WaitForSeconds(timeOfDayLength);

                List<TimeValue> transition = new() { new TimeValue(1.5f, TimeUnit.Second) };

                timeOfDayLabel.style.transitionDuration = new StyleList<TimeValue>(transition);

                timeOfDayLabel.style.opacity = 0f;

                showTimeOfDat = false;
            }
        }
    }
}

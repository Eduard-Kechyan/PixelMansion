using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Merge
{
    public class TutorialCycle : MonoBehaviour
    {
        // Variables
        public float startDelay = 0.5f;
        public float stepDuration = 1f;

        [Header("Debug")]
        public bool show = false;
        public bool hide = false;

        private bool readyToFinish = false;

        private Action finishCallback;

        private readonly List<TimeValue> transition = new();

        // References
        private I18n LOCALE;
        private SoundManager soundManager;

        // UI
        private VisualElement tutorialCycle;

        private Label title;
        private Button close;

        private VisualElement step1;
        private VisualElement step2;
        private VisualElement step3;
        private VisualElement step4;

        private Label step1Label;
        private Label step2Label;
        private Label step3Label;
        private Label step4Label;

        private VisualElement arrow1;
        private VisualElement arrow2;
        private VisualElement arrow3;
        private VisualElement arrow4;

        void Start()
        {
            // Cache
            LOCALE = I18n.Instance;
            soundManager = SoundManager.Instance;

            // UI
            tutorialCycle = GameRefs.Instance.valuesUIDoc.rootVisualElement.Q<VisualElement>("TutorialCycle");

            title = tutorialCycle.Q<Label>("Title");
            close = tutorialCycle.Q<Button>("Close");

            step1 = tutorialCycle.Q<VisualElement>("Step1");
            step2 = tutorialCycle.Q<VisualElement>("Step2");
            step3 = tutorialCycle.Q<VisualElement>("Step3");
            step4 = tutorialCycle.Q<VisualElement>("Step4");

            step1Label = step1.Q<Label>("StepLabel");
            step2Label = step2.Q<Label>("StepLabel");
            step3Label = step3.Q<Label>("StepLabel");
            step4Label = step4.Q<Label>("StepLabel");

            arrow1 = tutorialCycle.Q<VisualElement>("Arrow1");
            arrow2 = tutorialCycle.Q<VisualElement>("Arrow2");
            arrow3 = tutorialCycle.Q<VisualElement>("Arrow3");
            arrow4 = tutorialCycle.Q<VisualElement>("Arrow4");

            close.clicked += () => SoundManager.Tap(HideCycle);
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (show)
            {
                show = false;

                ShowCycle();
            }

            if (hide)
            {
                hide = false;

                HideCycle();
            }
        }
#endif

        public void ShowCycle(Action callback = null)
        {
            finishCallback = callback;

            // Set texts
            title.text = LOCALE.Get("tutorial_cycle_title");
            step1Label.text = LOCALE.Get("tutorial_cycle_step_1");
            step2Label.text = LOCALE.Get("tutorial_cycle_step_2");
            step3Label.text = LOCALE.Get("tutorial_cycle_step_3");
            step4Label.text = LOCALE.Get("tutorial_cycle_step_4");

            // Set transitions
            transition.Add(new TimeValue(stepDuration - 0.1f, TimeUnit.Second));

            step1.style.transitionDuration = new StyleList<TimeValue>(transition);
            step2.style.transitionDuration = new StyleList<TimeValue>(transition);
            step3.style.transitionDuration = new StyleList<TimeValue>(transition);
            step4.style.transitionDuration = new StyleList<TimeValue>(transition);

            arrow1.style.transitionDuration = new StyleList<TimeValue>(transition);
            arrow2.style.transitionDuration = new StyleList<TimeValue>(transition);
            arrow3.style.transitionDuration = new StyleList<TimeValue>(transition);
            arrow4.style.transitionDuration = new StyleList<TimeValue>(transition);

            // Show tutorial cycle
            Glob.SetTimeout(() =>
            {
                tutorialCycle.style.display = DisplayStyle.Flex;
                tutorialCycle.style.opacity = 1;

                StartCoroutine(FadeInStepsAndArrows());
            }, startDelay);
        }

        IEnumerator FadeInStepsAndArrows()
        {
            WaitForSeconds wait = new WaitForSeconds(stepDuration);

            yield return new WaitForSeconds(startDelay);

            // Title
            title.style.opacity = 1;

            yield return wait;

            // 1
            step1.style.opacity = 1;

            soundManager.PlaySound(SoundManager.SoundType.Experience); // TODO - Add proper sound here 

            yield return wait;

            // 2
            arrow1.style.opacity = 1;

            step2.style.opacity = 1;

            soundManager.PlaySound(SoundManager.SoundType.Experience); // TODO - Add proper sound here 

            yield return wait;

            // 3
            arrow2.style.opacity = 1;

            step3.style.opacity = 1;

            soundManager.PlaySound(SoundManager.SoundType.Experience); // TODO - Add proper sound here 

            yield return wait;

            // 4
            arrow3.style.opacity = 1;

            step4.style.opacity = 1;

            soundManager.PlaySound(SoundManager.SoundType.Experience); // TODO - Add proper sound here 

            yield return wait;

            arrow4.style.opacity = 1;

            yield return wait;

            // Show close button
            close.style.display = DisplayStyle.Flex;
            close.style.opacity = 1;

            yield return wait;

            // Finish the tutorial cycle
            readyToFinish = true;
        }

        void HideCycle()
        {
            if (readyToFinish)
            {
                readyToFinish = false;

                tutorialCycle.style.display = DisplayStyle.None;
                tutorialCycle.style.opacity = 0;

                step1.style.opacity = 0;
                step2.style.opacity = 0;
                step3.style.opacity = 0;
                step4.style.opacity = 0;

                arrow1.style.opacity = 0;
                arrow2.style.opacity = 0;
                arrow3.style.opacity = 0;
                arrow4.style.opacity = 0;

                close.style.display = DisplayStyle.None;
                close.style.opacity = 0;

                Glob.SetTimeout(() =>
                {
                    finishCallback?.Invoke();
                }, 0.3f);
            }
        }
    }
}

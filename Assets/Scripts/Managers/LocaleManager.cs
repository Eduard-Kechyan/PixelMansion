using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Merge
{
    public class LocaleManager : MonoBehaviour
    {
        // Variables
        public Font hyFont;
        public Font jpFont;
        public Font krFont;
        public Font cnFont;
        public Font enFont;
        public Font titleFont;

        // References
        private GameRefs gameRefs;

        private List<VisualElement> wrappers = new();
        private VisualElement menuLocaleWrapper;
        private VisualElement hubLocaleWrapper;
        private VisualElement gamePlayLocaleWrapper;

        public void Init(Types.Scene scene)
        {
            gameRefs = GameRefs.Instance;

            // Get menu locale wrapper if it's null
            if (menuLocaleWrapper == null)
            {
                menuLocaleWrapper = gameRefs.menuUI.GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>("LocaleWrapper");

                wrappers.Add(menuLocaleWrapper);
            }

            // Get hub locale wrapper
            if (scene == Types.Scene.Hub)
            {
                hubLocaleWrapper = gameRefs.hubUI.GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>("LocaleWrapper");

                wrappers.Add(hubLocaleWrapper);
            }
            // Get game play locale wrapper
            else
            {
                gamePlayLocaleWrapper = gameRefs.gamePlayUI.GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>("LocaleWrapper");

                wrappers.Add(gamePlayLocaleWrapper);
            }

            SetLocaleWrappers(Settings.Instance.currentLocale);
        }

        public void UpdateUILocale(Types.Locale newLocale, bool useTimeout = false)
        {
            if (useTimeout)
            {
                StartCoroutine(SetTimeOut(newLocale));
            }
            else
            {
                SetLocaleWrappers(newLocale);
            }
        }

        IEnumerator SetTimeOut(Types.Locale newLocale)
        {
            yield return new WaitForSeconds(0.1f);

            SetLocaleWrappers(newLocale);
        }

        void SetLocaleWrappers(Types.Locale newLocale)
        {
            for (int i = 0; i < wrappers.Count; i++)
            {
                VisualElement wrapper = wrappers[i];

                wrapper.RemoveFromClassList("locale_hy");
                wrapper.RemoveFromClassList("locale_jp");
                wrapper.RemoveFromClassList("locale_kr");
                wrapper.RemoveFromClassList("locale_cn");
                wrapper.RemoveFromClassList("locale_en");

                switch (newLocale)
                {
                    case Types.Locale.Armenian:
                        wrapper.AddToClassList("locale_hy");
                        break;
                    case Types.Locale.Japanese:
                        wrapper.AddToClassList("locale_jp");
                        break;
                    case Types.Locale.Korean:
                        wrapper.AddToClassList("locale_kr");
                        break;
                    case Types.Locale.Chinese:
                        wrapper.AddToClassList("locale_cn");
                        break;
                    default: // Types.Locale.English:
                        wrapper.AddToClassList("locale_en");
                        break;
                }
            }
        }
    }
}
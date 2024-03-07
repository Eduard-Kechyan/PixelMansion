using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Merge
{
    public class LocaleManager : MonoBehaviour
    {
        // Varaibles
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
                wrappers[i].RemoveFromClassList("locale_hy");
                wrappers[i].RemoveFromClassList("locale_jp");
                wrappers[i].RemoveFromClassList("locale_kr");
                wrappers[i].RemoveFromClassList("locale_cn");
                wrappers[i].RemoveFromClassList("locale_en");

                switch (newLocale)
                {
                    case Types.Locale.Armenian:
                        wrappers[i].AddToClassList("locale_hy");
                        break;
                    case Types.Locale.Japanese:
                        wrappers[i].AddToClassList("locale_jp");
                        break;
                    case Types.Locale.Korean:
                        wrappers[i].AddToClassList("locale_kr");
                        break;
                    case Types.Locale.Chinese:
                        wrappers[i].AddToClassList("locale_cn");
                        break;
                    default:
                        wrappers[i].AddToClassList("locale_en");
                        break;
                }
            }
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.TextCore.Text;

namespace Merge
{
    public class LocaleManager : MonoBehaviour
    {
        // Variables
        public FontAsset hyFont;
        public FontAsset jpFont;
        public FontAsset krFont;
        public FontAsset cnFont;
        public FontAsset enFont;
        public FontAsset titleFont;

        // References
        private GameRefs gameRefs;

        private List<VisualElement> wrappers = new();
        private VisualElement menuLocaleWrapper;
        private VisualElement hubLocaleWrapper;
        private VisualElement gamePlayLocaleWrapper;

        public void Init(Types.Scene scene)
        {
            gameRefs = GameRefs.Instance;

            Reset();

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
            else if (scene == Types.Scene.GamePlay)
            {
                gamePlayLocaleWrapper = gameRefs.gamePlayUI.GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>("LocaleWrapper");

                wrappers.Add(gamePlayLocaleWrapper);
            }

            SetLocaleWrappers(Settings.Instance.currentLocale);
        }

        void SetLocaleWrappers(Types.Locale newLocale)
        {
            for (int i = 0; i < wrappers.Count; i++)
            {
                VisualElement wrapper = wrappers[i];

                wrapper.RemoveFromClassList("locale_" + Types.Locale.Armenian);
                wrapper.RemoveFromClassList("locale_" + Types.Locale.Japanese);
                wrapper.RemoveFromClassList("locale_" + Types.Locale.Korean);
                wrapper.RemoveFromClassList("locale_" + Types.Locale.Chinese);

                wrapper.AddToClassList("locale_" + newLocale);
            }
        }

        void Reset()
        {
            wrappers.Clear();

            menuLocaleWrapper = null;
            hubLocaleWrapper = null;
            gamePlayLocaleWrapper = null;
        }
    }
}
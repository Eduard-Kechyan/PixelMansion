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

        // UI
        private List<VisualElement> wrappers = new();
        private VisualElement menuLocaleWrapper;
        private VisualElement worldLocaleWrapper;
        private VisualElement mergeLocaleWrapper;

        public void Init(SceneLoader.SceneType scene)
        {
            gameRefs = GameRefs.Instance;

            Reset();

            // Get menu locale wrapper if it's null
            if (menuLocaleWrapper == null)
            {
                menuLocaleWrapper = gameRefs.menuUI.GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>("LocaleWrapper");

                wrappers.Add(menuLocaleWrapper);
            }

            // Get world locale wrapper
            if (scene == SceneLoader.SceneType.World)
            {
                worldLocaleWrapper = gameRefs.worldUI.GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>("LocaleWrapper");

                wrappers.Add(worldLocaleWrapper);
            }
            // Get merge locale wrapper
            else if (scene == SceneLoader.SceneType.Merge)
            {
                mergeLocaleWrapper = gameRefs.mergeUI.GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>("LocaleWrapper");

                wrappers.Add(mergeLocaleWrapper);
            }

            SetLocaleWrappers(Settings.Instance.currentLocale);
        }

        void SetLocaleWrappers(I18n.Locale newLocale)
        {
            for (int i = 0; i < wrappers.Count; i++)
            {
                VisualElement wrapper = wrappers[i];

                wrapper.RemoveFromClassList("locale_" + I18n.Locale.Armenian);
                wrapper.RemoveFromClassList("locale_" + I18n.Locale.Japanese);
                wrapper.RemoveFromClassList("locale_" + I18n.Locale.Korean);
                wrapper.RemoveFromClassList("locale_" + I18n.Locale.Chinese);

                wrapper.AddToClassList("locale_" + newLocale);
            }
        }

        void Reset()
        {
            wrappers.Clear();

            menuLocaleWrapper = null;
            worldLocaleWrapper = null;
            mergeLocaleWrapper = null;
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    [CreateAssetMenu(fileName = "MenuOptionsData", menuName = "ScriptableObject/MenuOptionsData")]
    public class MenuOptionsData : ScriptableObject
    {
        public MenuUI.MenuOptions[] data = new MenuUI.MenuOptions[Enum.GetValues(typeof(MenuUI.Menu)).Length];

        void OnValidate()
        {
            if (data != null)
            {
                int count = 0;

                foreach (MenuUI.Menu menu in Enum.GetValues(typeof(MenuUI.Menu)))
                {
                    data[count].menuType = menu;
                    data[count].name = menu.ToString();

                    count++;
                }
            }
        }
    }
}

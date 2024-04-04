using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Globalization;
using UnityEditor;
using UnityEngine.UIElements;
using CI.QuickSave;
using System.IO;
using Newtonsoft.Json;

namespace Merge
{
    public class UtilitiesEditor : EditorWindow
    {
        // Variables

        [MenuItem("Window/Utilities")]
        public static void OpenWindow()
        {
            EditorWindow window = GetWindow<UtilitiesEditor>();
            window.titleContent = new GUIContent("Utilities");
        }

        public void CreateGUI()
        {
            VisualElement buttonsBlock = new();
            buttonsBlock.style.flexDirection = FlexDirection.Row;
            buttonsBlock.style.justifyContent = Justify.Center;
            buttonsBlock.style.marginBottom = 10f;

            ///////////////

            Button findNonUniformButton = new() { text = "Find Non-Uniform Sprites" };

            findNonUniformButton.style.marginRight = 10f;
            findNonUniformButton.style.marginLeft = 10f;
            findNonUniformButton.style.marginTop = 10f;
            findNonUniformButton.style.marginBottom = 10f;

            findNonUniformButton.clicked += () => FindNonUniformSprites();

            buttonsBlock.Add(findNonUniformButton);

            rootVisualElement.Add(buttonsBlock);

            ///////////////
        }

        void FindNonUniformSprites()
        {
            string folderPath = "Assets/Sprites";

            if (Directory.Exists(folderPath))
            {
                List<string> spritePaths = GetSpritePathsInFolder(folderPath);

                Debug.Log(spritePaths.Count + " Non-Uniform Sprites Found");

                for (int i = 0; i < spritePaths.Count; i++)
                {
                    Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePaths[i]);

                    if (sprite != null && sprite.textureRect != null)
                    {
                        if (sprite.textureRect.width % 2 != 0 || sprite.textureRect.height % 2 != 0)
                        {
                            Debug.Log(spritePaths[i].Replace("Assets/Sprites\\", "").Replace(".png", "") + " " + sprite.textureRect.width + "x" + sprite.textureRect.height);
                        }
                    }
                }
            }
        }

        List<string> GetSpritePathsInFolder(string folderPath)
        {
            List<string> spritePaths = new();

            string[] preSpritePaths = Directory.GetFiles(folderPath, "*.png");

            spritePaths.AddRange(preSpritePaths);

            string[] subFolders = Directory.GetDirectories(folderPath);

            foreach (var subFolder in subFolders)
            {
                List<string> subFolderSpritePaths = GetSpritePathsInFolder(subFolder);

                spritePaths.AddRange(subFolderSpritePaths);
            }

            return spritePaths;
        }
    }
}

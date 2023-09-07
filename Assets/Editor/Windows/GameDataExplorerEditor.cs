using System.Collections;
using System.Globalization;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using CI.QuickSave;
using System.IO;
using Newtonsoft.Json;

namespace Merge
{
    public class GameDataExplorerEditor : EditorWindow
    {
        // Variables
        public QuickSaveReader reader;

        private VisualElement dataBlock;

        private List<Color> colors;
        private Color colorBlue = Color.blue;
        private Color colorCyan = Color.cyan;
        private Color colorGreen = Color.green;
        private Color colorYellow = Color.yellow;
        private Color colorRed = Color.red;

        [MenuItem("Window/Game Data Explorer")]
        public static void OpenWindow()
        {
            EditorWindow window = GetWindow<GameDataExplorerEditor>();
            window.titleContent = new GUIContent("Game Data Explorer");
        }

        public void CreateGUI()
        {
            colors = new();

            colorBlue = FromHEX("71A0F6");
            colorCyan = FromHEX("55CBB3");
            colorGreen = FromHEX("3EC37B");
            colorYellow = FromHEX("FEDA75");
            colorRed = FromHEX("EC737F");

            colors.Add(colorBlue);
            colors.Add(colorCyan);
            colors.Add(colorGreen);
            colors.Add(colorYellow);
            colors.Add(colorRed);

            SetDataPre();
        }

        void SetDataPre()
        {
            rootVisualElement.Clear();

            AddColors();

            AddReloadButton();

            string folderPath = Application.persistentDataPath + "/QuickSave";

            string[] filePaths = Directory.GetFiles(folderPath);

            if (filePaths.Length > 0)
            {
                VisualElement fileNameBlock = new();
                VisualElement fileOpenBlock = new();

                fileNameBlock.style.flexDirection = FlexDirection.Row;
                fileNameBlock.style.justifyContent = Justify.Center;
                fileNameBlock.style.marginBottom = 10f;

                fileOpenBlock.style.flexDirection = FlexDirection.Row;
                fileOpenBlock.style.justifyContent = Justify.Center;
                fileOpenBlock.style.marginBottom = 10f;

                string firstFileName = "";

                for (int i = 0; i < filePaths.Length; i++)
                {
                    string fileName = filePaths[i]
                        .Replace(folderPath + "\\", "")
                        .Replace(".json", "");

                    if (i == 0)
                    {
                        firstFileName = fileName;
                    }

                    Button fileNameButton = new() { text = fileName };
                    Button fileOpenButton = new() { text = "Open " + fileName };

                    string pathName = filePaths[i];

                    fileNameButton.clicked += () => SetData(fileName);
                    fileOpenButton.clicked += () => OpenFile(pathName);

                    fileNameBlock.Add(fileNameButton);
                    fileOpenBlock.Add(fileOpenButton);
                }

                rootVisualElement.Add(fileNameBlock);
                rootVisualElement.Add(fileOpenBlock);

                SetData(firstFileName);
            }
            else
            {
                Label emptyLabel = new() { text = "No Data" };

                emptyLabel.style.color = Color.red;
                emptyLabel.style.unityTextAlign = TextAnchor.MiddleCenter;

                rootVisualElement.Add(emptyLabel);
            }
        }

        void SetData(string fileName)
        {
            if (dataBlock == null)
            {
                dataBlock = new();

                dataBlock.style.paddingLeft = 10f;
                dataBlock.style.paddingRight = 10f;
                dataBlock.style.flexDirection = FlexDirection.Row;
                dataBlock.style.paddingTop = 25f;
            }
            else
            {
                dataBlock.Clear();
            }

            reader = QuickSaveReader.Create(fileName);

            Label chosenLabel = new() { text = fileName + ":" };

            chosenLabel.style.position = Position.Absolute;
            chosenLabel.style.top = 0;
            chosenLabel.style.left = 0;
            chosenLabel.style.right = 0;
            chosenLabel.style.width = Length.Percent(100);
            chosenLabel.style.height = 20f;
            chosenLabel.style.color = Color.white;
            chosenLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            chosenLabel.style.fontSize = 16f;

            dataBlock.Add(chosenLabel);

            VisualElement blockLeft = new();
            VisualElement blockRight = new();

            blockRight.style.paddingLeft = 10f;

            foreach (var item in reader.Get())
            {
                Label keyLabel = new() { text = item.Key + ":" };

                keyLabel.style.color = Color.white;
                keyLabel.style.marginRight = 10f;
                keyLabel.style.fontSize = 14f;

                blockLeft.Add(keyLabel);
                blockRight.Add(HandleValue(item.Value));

                dataBlock.Add(blockLeft);
                dataBlock.Add(blockRight);
            }

            rootVisualElement.Add(dataBlock);
        }

        Label HandleValue(Newtonsoft.Json.Linq.JToken newValue)
        {
            Label valueLabel = new();
            string stringValue = newValue.ToString();

            valueLabel.style.fontSize = 14f;

            switch (newValue.Type)
            {
                case Newtonsoft.Json.Linq.JTokenType.String:
                    valueLabel.style.color = colorYellow;

                    if (stringValue.StartsWith("[") && stringValue.Length > 2)
                    {
                        valueLabel.text = "[Full Array]";
                    }
                    else
                    {
                        valueLabel.text = stringValue;
                    }
                    break;

                case Newtonsoft.Json.Linq.JTokenType.Boolean:
                    if (stringValue == "True")
                    {
                        valueLabel.style.color = colorGreen;
                    }
                    else
                    {
                        valueLabel.style.color = colorRed;
                    }

                    valueLabel.text = stringValue;
                    break;

                case Newtonsoft.Json.Linq.JTokenType.Float:
                    valueLabel.style.color = colorCyan;

                    valueLabel.text = stringValue;
                    break;

                case Newtonsoft.Json.Linq.JTokenType.Integer:
                    valueLabel.style.color = colorBlue;

                    valueLabel.text = stringValue;
                    break;

                default:
                    valueLabel.text = stringValue;
                    break;
            }

            return valueLabel;
        }

        void OpenFile(string filePath)
        {
            /*string fielData =File.ReadAllText(filePath);

            if (fileStream != null)
            {
                Application.OpenURL()
            }*/

            Application.OpenURL(filePath);
        }

        void AddReloadButton()
        {
            Button reloadButton = new() { text = "Reload Data" };

            reloadButton.style.marginRight = StyleKeyword.Auto;
            reloadButton.style.marginLeft = StyleKeyword.Auto;
            reloadButton.style.marginTop = 10f;
            reloadButton.style.marginBottom = 10f;

            reloadButton.clicked += () => SetDataPre();

            rootVisualElement.Add(reloadButton);
        }

        void AddColors()
        {
            VisualElement colorBox = new();

            colorBox.style.flexDirection = FlexDirection.Row;
            colorBox.style.justifyContent = Justify.Center;
            colorBox.style.marginTop = 10f;

            for (int i = 0; i < colors.Count; i++)
            {
                VisualElement colorBlueItem = new();
                colorBlueItem.style.width = 20f;
                colorBlueItem.style.height = 20f;
                colorBlueItem.style.marginLeft = 5f;
                colorBlueItem.style.marginRight = 5f;
                colorBlueItem.style.backgroundColor = colors[i];

                colorBox.Add(colorBlueItem);
            }

            rootVisualElement.Add(colorBox);
        }

        Color FromHEX(string hex)
        {
            Color newColor = Color.white;

            if (hex.Length >= 6)
            {
                var r = hex.Substring(0, 2);
                var g = hex.Substring(2, 2);
                var b = hex.Substring(4, 2);
                var a = "FF";

                if (hex.Length == 8)
                {
                    a = hex.Substring(6, 2);
                }

                newColor = new Color(
                    (int.Parse(r, NumberStyles.HexNumber) / 255f),
                    (int.Parse(g, NumberStyles.HexNumber) / 255f),
                    (int.Parse(b, NumberStyles.HexNumber) / 255f),
                    (int.Parse(a, NumberStyles.HexNumber) / 255f)
                );
            }

            return newColor;
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using HtmlAgilityPack;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

namespace Merge
{
    public class HtmlHandler : MonoBehaviour
    {
        // Classes
        [Serializable]
        public class HtmlTag
        {
            public string name;
            public string content;
        }

        // References
        private ErrorManager errorManager;

#if UNITY_EDITOR
        public TextAsset dummyTermsHtml;
        private ScrollView htmlScrollView;

        void Start()
        {
            errorManager = ErrorManager.Instance;

            if (SceneManager.GetActiveScene().name == "DummyScene")
            {
                htmlScrollView = GetComponent<UIDocument>().rootVisualElement.Q<ScrollView>("HtmlScrollView");

                DebugHtml(dummyTermsHtml.text);
            }
        }

        void DebugHtml(string htmlString)
        {
            List<Label> labels = ConvertHtmlToUI(htmlString);

            foreach (Label newLabel in labels)
            {
                htmlScrollView.Add(newLabel);
            }
        }
#else
        void Start()
        {
            // Cache
            errorManager = ErrorManager.Instance;
        }
#endif

        public List<Label> ConvertHtmlToUI(string htmlString)
        {
            string convertedHtml = htmlString.Replace("<p><br></p>", "<br>").Replace("<p><hr></p>", "<hr>");

            HtmlDocument doc = new();

            doc.LoadHtml(convertedHtml);

            List<Label> labels = HandleNode(doc.DocumentNode.ChildNodes);

            return labels;
        }

        List<Label> HandleNode(HtmlNodeCollection nodes, bool combine = false, bool isList = false)
        {
            List<Label> labels = new();

            foreach (HtmlNode node in nodes)
            {
                Label newLabel = new();
                string labelName = "";
                string stringContent = "";
                bool addNewLabel = true;

                if (node.NodeType == HtmlNodeType.Element || (node.NodeType == HtmlNodeType.Text && (!string.IsNullOrWhiteSpace(node.InnerText) || !string.IsNullOrWhiteSpace(node.InnerHtml))))
                {
                    switch (node.Name)
                    {
                        // Text
                        case "#text":
                            labelName = "HtmlParagraph";
                            stringContent += node.InnerText;
                            newLabel.AddToClassList("html_paragraph");
                            break;
                        case "p":
                            if (node.InnerText == "")
                            {
                                labelName = "HtmlParagraph";
                                stringContent += node.InnerText;
                                newLabel.AddToClassList("html_paragraph");
                            }
                            else
                            {
                                addNewLabel = false;
                                labels.AddRange(HandleNode(node.ChildNodes, true));
                            }
                            break;
                        case "strong":
                            stringContent += $"<b>{node.InnerText}</b>";

                            if (labelName == "")
                            {
                                labelName = "HtmlParagraph";
                            }
                            break;
                        case "b":
                            stringContent += $"<b>{node.InnerText}</b>";

                            if (labelName == "")
                            {
                                labelName = "HtmlParagraph";
                            }
                            break;
                        case "i":
                            stringContent += $"<i>{node.InnerText}</i>";

                            if (labelName == "")
                            {
                                labelName = "HtmlParagraph";
                            }
                            break;
                        // Headings
                        case "h1":
                            labelName = "HtmlHeader1";
                            stringContent = node.InnerText;
                            newLabel.AddToClassList("html_header");
                            break;
                        case "h2":
                            labelName = "HtmlHeader2";
                            stringContent = node.InnerText;
                            newLabel.AddToClassList("html_header");
                            newLabel.AddToClassList("html_header_2");
                            break;
                        case "h3":
                            labelName = "HtmlHeader3";
                            stringContent = node.InnerText;
                            newLabel.AddToClassList("html_header");
                            newLabel.AddToClassList("html_header_3");
                            break;
                        case "h4":
                            labelName = "HtmlHeader4";
                            stringContent = node.InnerText;
                            newLabel.AddToClassList("html_header");
                            newLabel.AddToClassList("html_header_4");
                            break;
                        case "h5":
                            labelName = "HtmlHeader5";
                            stringContent = node.InnerText;
                            newLabel.AddToClassList("html_header");
                            newLabel.AddToClassList("html_header_5");
                            break;
                        case "h6":
                            labelName = "HtmlHeader6";
                            stringContent = node.InnerText;
                            newLabel.AddToClassList("html_header");
                            newLabel.AddToClassList("html_header_6");
                            break;
                        // Lists
                        case "ol":
                            addNewLabel = false;
                            labels.AddRange(HandleNode(node.ChildNodes, false, true));
                            break;
                        case "ul":
                            addNewLabel = false;
                            labels.AddRange(HandleNode(node.ChildNodes, false, true));
                            break;
                        case "li":
                            labelName = "HtmlListItem";
                            stringContent = "• " + node.InnerText;
                            newLabel.AddToClassList("html_list_item");
                            break;
                        // Other
                        case "a":
                            string href = node.GetAttributeValue("href", "");

                            href = href.Replace("www.", "").Replace("http://", "").Replace("https://", "");

                            href = "https://" + href;

                            stringContent += $"<color=#2d92ff><a href='{href}'>{node.InnerText}</a></color>";
                            break;
                        case "br":
                            labelName = "HtmlBreak";
                            stringContent = "BR";
                            newLabel.AddToClassList("html_br");
                            break;
                        case "hr":
                            labelName = "HtmlHr";
                            stringContent = "HR";
                            newLabel.AddToClassList("html_hr");
                            break;
                        default:
                            addNewLabel = false;

                            // ERROR
                            // errorManager.ThrowWarning(ErrorManager.ErrorType.Code, GetType().ToString(), "Tag: " + node.Name + " not implemented!");
                            break;
                    }
                }
                else
                {
                    addNewLabel = false;
                }

                if (addNewLabel)
                {
                    if (stringContent != "")
                    {
                        newLabel.text = Regex.Replace(stringContent.Replace("\r\n", " ").Replace("\n", " "), @"\s+", " ");
                        newLabel.name = labelName;

                        labels.Add(newLabel);
                    }
                }
            }

            if (combine)
            {
                List<Label> combinedLabels = new();
                Label newCombinedLabel = new();
                string combinedText = "";
                int combinedCount = 0;

                foreach (Label label in labels)
                {
                    if (combinedCount == 0)
                    {
                        newCombinedLabel.name = label.name;

                        foreach (string labelClass in label.GetClasses())
                        {
                            newCombinedLabel.AddToClassList(labelClass);
                        }
                    }

                    combinedText += label.text;

                    combinedCount++;
                }

                newCombinedLabel.text = combinedText;

                combinedLabels.Add(newCombinedLabel);

                return combinedLabels;
            }
            else
            {
                if (isList)
                {
                    labels[0].AddToClassList("html_list_item_first");
                    labels[labels.Count - 1].AddToClassList("html_list_item_last");
                }

                return labels;
            }
        }

        int CalculateGCD(int a, int b)
        {
            while (b != 0)
            {
                int temp = b;
                b = a % b;
                a = temp;
            }

            return a;
        }
    }
}

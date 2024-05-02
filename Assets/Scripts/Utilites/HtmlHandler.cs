using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UIElements;

namespace Merge
{
    public class HtmlHandler : MonoBehaviour
    {
        // Variables
        [Serializable]
        public class HtmlTag
        {
            public string name;
            public string content;
        }

        // References
        private ErrorManager errorManager;

        void Start()
        {
            // Cache
            errorManager = ErrorManager.Instance;
        }

        public List<VisualElement> ConvertHtmlToUI(string html)
        {
            List<HtmlTag> tags = ParseHtml(html);

            List<VisualElement> elements = new();

            int count = 0;

            foreach (HtmlTag tag in tags)
            {
                Label newLabel = new();

                switch (tag.name)
                {
                    case "h1":
                        newLabel.name = "Header1_" + count;
                        newLabel.text = tag.content;

                        newLabel.AddToClassList("html_paragraph");
                        newLabel.AddToClassList("html_header");
                        break;
                    case "h2":
                        newLabel.name = "Header2_" + count;
                        newLabel.text = tag.content;

                        newLabel.AddToClassList("html_paragraph");
                        newLabel.AddToClassList("html_header");
                        newLabel.AddToClassList("html_header_2");
                        break;
                    case "h3":
                        newLabel.name = "Header3_" + count;
                        newLabel.text = tag.content;

                        newLabel.AddToClassList("html_paragraph");
                        newLabel.AddToClassList("html_header");
                        newLabel.AddToClassList("html_header_3");
                        break;
                    case "h4":
                        newLabel.name = "Header4_" + count;
                        newLabel.text = tag.content;

                        newLabel.AddToClassList("html_paragraph");
                        newLabel.AddToClassList("html_header");
                        newLabel.AddToClassList("html_header_4");
                        break;
                    case "h5":
                        newLabel.name = "Header5_" + count;
                        newLabel.text = tag.content;

                        newLabel.AddToClassList("html_paragraph");
                        newLabel.AddToClassList("html_header");
                        newLabel.AddToClassList("html_header_5");
                        break;
                    case "h6":
                        newLabel.name = "Header6_" + count;
                        newLabel.text = tag.content;

                        newLabel.AddToClassList("html_paragraph");
                        newLabel.AddToClassList("html_header");
                        newLabel.AddToClassList("html_header_6");
                        break;
                    case "p":
                        newLabel.name = "Paragraph_" + count;
                        newLabel.text = tag.content;

                        newLabel.AddToClassList("html_paragraph");
                        break;
                    default:
                        // ERROR
                        errorManager.ThrowWarning(ErrorManager.ErrorType.Code, GetType().ToString(), "Tag: " + tag.name + " not implemented!");
                        break;
                }

                elements.Add(newLabel);

                count++;
            }

            return elements;
        }

        List<HtmlTag> ParseHtml(string htmlString)
        {
            string convertedHtmlString = htmlString.Replace("<strong>", "<b>").Replace("</strong>", "</b>");

            List<HtmlTag> tags = new();

            string regexPattern = @"<(\w+)[^>]*>(.*?)</\1>";

            MatchCollection matches = Regex.Matches(convertedHtmlString, regexPattern);

            foreach (Match match in matches)
            {
                string convertedContent = match.Groups[2].Value.Replace("</br>", "\n");

                HtmlTag newTag = new()
                {
                    name = match.Groups[1].Value,
                    content = convertedContent
                };

                tags.Add(newTag);
            }

            return tags;
        }
    }
}

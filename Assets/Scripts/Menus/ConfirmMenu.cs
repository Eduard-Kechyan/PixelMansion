using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Locale;

public class ConfirmMenu : MonoBehaviour
{
    // References
    private MenuUI menuUI;
    private I18n LOCALE;

    private bool denyTapped = false;
    private Action callback;
    private Action callbackAlt;

    // UI
    private VisualElement root;
    private VisualElement confirmMenu;
    private VisualElement menuContent;

    private Label confirmLabel;
    private Button confirmButton;
    private Button denyButton;

    void Start()
    {
        // Cache
        menuUI = GetComponent<MenuUI>();
        LOCALE = I18n.Instance;

        // Cache UI
        root = GetComponent<UIDocument>().rootVisualElement;

        confirmMenu = root.Q<VisualElement>("ConfirmMenu");
        menuContent = confirmMenu.Q<VisualElement>("Content");

        confirmLabel = menuContent.Q<Label>("Label");
        confirmButton = menuContent.Q<Button>("ConfirmButton");
        denyButton = menuContent.Q<Button>("DenyButton");

        confirmButton.clicked += () => ConfirmButtonClicked();
        denyButton.clicked += () => DenyButtonClicked();

        Init();
    }

    void Init()
    {
        // Make sure the menu is closed
        confirmMenu.style.display = DisplayStyle.None;
        confirmMenu.style.opacity = 0;
    }

    public void Open(string preFix, Action newCllback, Action newCallbackAlt = null, bool alt = false, bool closeAll = false)
    {
        callback = newCllback;

        if (!denyTapped)
        {
            if (newCallbackAlt != null)
            {
                callbackAlt = newCallbackAlt;
            }

            denyTapped = true;

            Glob.SetTimeout(() =>
            {
                denyTapped = false;
            }, 0.35f);
        }

        // Set the title
        string title = LOCALE.Get("confirm_title_" + preFix);

        confirmLabel.text = LOCALE.Get("confirm_label_" + preFix);

        if (alt)
        {
            confirmButton.text = LOCALE.Get("confirm_button_" + preFix);
            denyButton.text = LOCALE.Get("confirm_deny_button_" + preFix);
        }
        else
        {
            confirmButton.text = LOCALE.Get("confirm_button");
            denyButton.text = LOCALE.Get("confirm_deny_button");
        }

        // Open menu
        menuUI.OpenMenu(confirmMenu, title, false, closeAll);
    }

    void ConfirmButtonClicked()
    {
        callback?.Invoke();
    }

    void DenyButtonClicked()
    {
        if (callbackAlt != null)
        {
            callbackAlt();
        }
        else
        {
            Close();
        }
    }

    public void Close()
    {
        menuUI.CloseMenu(confirmMenu.name);
    }
}

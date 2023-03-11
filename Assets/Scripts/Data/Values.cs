using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Values : MonoBehaviour
{
    public UIDocument uiDoc;

    // UI
    private VisualElement root;

    private VisualElement topBox;

    private Button levelButton;
    private Label levelValue;

    private Button energyButton;
    private VisualElement energyPlus;

    private Button goldButton;
    private VisualElement goldPlus;

    private Button gemsButton;
    private VisualElement gemsPlus;

    void Start()
    {
        // Cache UI
        root = uiDoc.rootVisualElement;

        topBox = root.Q<VisualElement>("TopBox");

        levelButton = topBox.Q<Button>("LevelButton");
        levelValue = levelButton.Q<Label>("Value");

        energyButton = topBox.Q<Button>("EnergyButton");
        energyPlus = energyButton.Q<VisualElement>("Plus");

        goldButton = topBox.Q<Button>("GoldButton");
        goldPlus = goldButton.Q<VisualElement>("Plus");

        gemsButton = topBox.Q<Button>("GemsButton");
        gemsPlus = gemsButton.Q<VisualElement>("Plus");

        InitializeValues();
    }

    void InitializeValues()
    {
        levelButton.SetEnabled(false);

        energyButton.SetEnabled(false);
        energyButton.SetEnabled(false);
        energyButton.SetEnabled(false);

        // TODO - Add a check for the plusses
        energyPlus.style.display = DisplayStyle.None;
        goldPlus.style.display = DisplayStyle.None;
        gemsPlus.style.display = DisplayStyle.None;

        UpdateValues();
    }

    public void UpdateValues()
    {
        levelButton.text = GameData.experience.ToString(); // TODO -  Change this to a fill

        levelValue.text = GameData.level.ToString();
        energyButton.text = GameData.energy.ToString();
        goldButton.text = GameData.gold.ToString();
        gemsButton.text = GameData.gems.ToString();
    }
}

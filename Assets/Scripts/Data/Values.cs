using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Values : MonoBehaviour
{
    [HideInInspector]
    public bool set;

    private GameData gameData;

    private UIDocument uiDoc;

    private VisualElement root;

    private VisualElement topBox;

    private Button levelButton;
    private Label levelValue;

    private Button energyButton;
    private VisualElement energyPlus;
    [HideInInspector]
    public Label energyTimer;

    private Button goldButton;
    private VisualElement goldPlus;

    private Button gemsButton;
    private VisualElement gemsPlus;

    void Start()
    {
        gameData = GameData.Instance;
    }

    public void InitializeValues()
    {
        // Cache UI
        uiDoc = GameObject.Find("GamePlayUI").GetComponent<UIDocument>();

        root = uiDoc.rootVisualElement;

        topBox = root.Q<VisualElement>("TopBox");

        levelButton = topBox.Q<Button>("LevelButton");
        levelValue = levelButton.Q<Label>("Value");

        energyButton = topBox.Q<Button>("EnergyButton");
        energyPlus = energyButton.Q<VisualElement>("Plus");
        energyTimer = energyButton.Q<Label>("EnergyTimer");

        energyTimer.style.display = DisplayStyle.None;

        goldButton = topBox.Q<Button>("GoldButton");
        goldPlus = goldButton.Q<VisualElement>("Plus");

        gemsButton = topBox.Q<Button>("GemsButton");
        gemsPlus = gemsButton.Q<VisualElement>("Plus");

        set = true;

        SetValues();
    }

    void SetValues()
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
        levelButton.text = gameData.experience.ToString(); // TODO -  Change this to a fill

        levelValue.text = gameData.level.ToString();
        energyButton.text = gameData.energy.ToString();
        goldButton.text = gameData.gold.ToString();
        gemsButton.text = gameData.gems.ToString();
    }
}

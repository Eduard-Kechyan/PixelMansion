using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ValuesUI : MonoBehaviour
{
    public Sprite[] levelUpIndicatorSprites = new Sprite[0];

    // Variables
    private bool levelEnabled = false;
    private bool energyEnabled = false;
    private bool goldEnabled = false;
    private bool gemsEnabled = false;

    private bool energyPlusEnabled = false;
    private bool goldPlusEnabled = false;
    private bool gemsPlusEnabled = false;

    private bool enabledSet = false;

    // References
    private LevelMenu levelMenu;
    private EnergyMenu energyMenu;
    private ShopMenu shopMenu;

    // Instances
    private GameData gameData;

    // UI
    private VisualElement root;

    private VisualElement valuesBox;

    public Button levelButton;
    private VisualElement levelFill;
    private Label levelValue;
    private VisualElement levelUpIndicator;

    public Button energyButton;
    private VisualElement energyPlus;

    public Label energyTimer;

    public Button goldButton;
    private VisualElement goldPlus;

    public Button gemsButton;
    private VisualElement gemsPlus;

    void Start()
    {
        // Cache
        levelMenu = GameRefs.Instance.levelMenu;
        energyMenu = GameRefs.Instance.energyMenu;
        shopMenu = GameRefs.Instance.shopMenu;

        // Cache instances
        gameData = GameData.Instance;

        // Cache UI
        root = GetComponent<UIDocument>().rootVisualElement;

        valuesBox = root.Q<VisualElement>("ValuesBox");

        levelButton = valuesBox.Q<Button>("LevelButton");
        levelFill = levelButton.Q<VisualElement>("Fill");
        levelValue = levelButton.Q<Label>("Value");
        levelUpIndicator = valuesBox.Q<VisualElement>("Indicator");

        energyButton = valuesBox.Q<Button>("EnergyButton");
        energyPlus = energyButton.Q<VisualElement>("Plus");
        energyTimer = energyButton.Q<Label>("EnergyTimer");

        energyTimer.style.display = DisplayStyle.None;

        goldButton = valuesBox.Q<Button>("GoldButton");
        goldPlus = goldButton.Q<VisualElement>("Plus");

        gemsButton = valuesBox.Q<Button>("GemsButton");
        gemsPlus = gemsButton.Q<VisualElement>("Plus");

        SetValues();

        CheckForTaps();
    }

    void SetValues()
    {
        // TODO - Remove comments and set to false
        //levelButton.SetEnabled(false);
        //energyButton.SetEnabled(false);
        //goldButton.SetEnabled(false);
        //gemsButton.SetEnabled(false);

        // TODO - Add a check for the plusses
        // energyPlus.style.display = DisplayStyle.None;
        //goldPlus.style.display = DisplayStyle.None;
        //gemsPlus.style.display = DisplayStyle.None;

        UpdateValues();
    }

    void CheckForTaps()
    {
        levelButton.clicked += () => levelMenu.Open();
        energyButton.clicked += () => energyMenu.Open();
        goldButton.clicked += () => shopMenu.Open("Gold");
        gemsButton.clicked += () => shopMenu.Open("Gems");
    }

    public void UpdateValues()
    {
        levelFill.style.width = CalcLevelFill();

        levelValue.text = gameData.level.ToString();
        energyButton.text = gameData.energy.ToString();
        goldButton.text = gameData.gold.ToString();
        gemsButton.text = gameData.gems.ToString();
    }

    public void UpdateLevel()
    {
        levelValue.text = gameData.level.ToString();

        StartCoroutine(UpdateEnergyFullAfter());
    }

    IEnumerator UpdateEnergyFullAfter()
    {
        yield return new WaitForSeconds(0.3f);

        List<TimeValue> durationsZero = new List<TimeValue>();
        durationsZero.Add(new TimeValue(0f, TimeUnit.Second));

        levelFill.style.transitionDuration = new StyleList<TimeValue>(durationsZero);

        levelFill.style.width = Length.Percent(0);

        List<TimeValue> durationsFull = new List<TimeValue>();
        durationsFull.Add(new TimeValue(0.3f, TimeUnit.Second));

        levelFill.style.transitionDuration = new StyleList<TimeValue>(durationsFull);

        gameData.experience = gameData.leftoverExperience;

        DataManager.Instance.writer.Write("experience", gameData.experience).Commit();

        gameData.leftoverExperience = 0;

        levelFill.style.width = CalcLevelFill();
    }

    public void ToggleLevelUp(bool canLevelUp)
    {
        if (canLevelUp)
        {
            levelUpIndicator.style.opacity = 1;
            levelUpIndicator.style.visibility = Visibility.Visible;

            StartCoroutine("BlipLevelUpIndicator");
        }
        else
        {
            levelUpIndicator.style.opacity = 0;
            levelUpIndicator.style.visibility = Visibility.Hidden;

            StopCoroutine("BlipLevelUpIndicator");
        }
    }

    IEnumerator BlipLevelUpIndicator()
    {
        int count = 0;

        while (true)
        {
            yield return new WaitForSeconds(0.2f);

            levelUpIndicator.style.backgroundImage = new StyleBackground(levelUpIndicatorSprites[count]);

            if (count == 5)
            {
                count = 0;
            }
            else
            {
                count++;
            }
        }
    }

    public Length CalcLevelFill()
    {
        if (gameData == null)
        {
            gameData = GameData.Instance;
        }

        float fillPercent;

        if (gameData.experience < gameData.maxExperience)
        {
            fillPercent = (100f / (float)gameData.maxExperience) * (float)gameData.experience;
        }
        else
        {
            fillPercent = 100f;
        }

        return Length.Percent(fillPercent);
    }

    public void DisableButtons()
    {
        if (!enabledSet)
        {
            levelEnabled = levelButton.enabledSelf;
            energyEnabled = energyButton.enabledSelf;
            goldEnabled = goldButton.enabledSelf;
            gemsEnabled = gemsButton.enabledSelf;

            levelButton.SetEnabled(false);
            energyButton.SetEnabled(false);
            goldButton.SetEnabled(false);
            gemsButton.SetEnabled(false);

            energyPlusEnabled = energyPlus.resolvedStyle.visibility == Visibility.Visible;
            goldPlusEnabled = goldPlus.resolvedStyle.visibility == Visibility.Visible;
            gemsPlusEnabled = gemsPlus.resolvedStyle.visibility == Visibility.Visible;

            energyPlus.style.visibility = Visibility.Hidden;
            energyPlus.style.opacity = 0f;
            goldPlus.style.visibility = Visibility.Hidden;
            goldPlus.style.opacity = 0f;
            gemsPlus.style.visibility = Visibility.Hidden;
            gemsPlus.style.opacity = 0f;

            enabledSet = true;
        }
    }

    public void EnableButtons()
    {
        levelButton.SetEnabled(levelEnabled);
        energyButton.SetEnabled(energyEnabled);
        goldButton.SetEnabled(goldEnabled);
        gemsButton.SetEnabled(gemsEnabled);

        if (energyPlusEnabled)
        {
            energyPlus.style.visibility = Visibility.Visible;
            energyPlus.style.opacity = 1f;
        }

        if (goldPlusEnabled)
        {
            goldPlus.style.visibility = Visibility.Visible;
            goldPlus.style.opacity = 1f;
        }

        if (gemsPlusEnabled)
        {
            gemsPlus.style.visibility = Visibility.Visible;
            gemsPlus.style.opacity = 1f;
        }

        enabledSet = false;
    }

    public void SetSortingOrder(int order)
    {
        GetComponent<UIDocument>().sortingOrder = order;
    }
}

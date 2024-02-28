using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


namespace Merge
{
    public class EnergyMenu : MonoBehaviour
    {
        // Variables
        public int energyBuyAmount = 50;
        public int gemsCost = 5;

        // References
        private GameData gameData;
        private I18n LOCALE;
        private MenuUI menuUI;
        private ShopMenu shopMenu;
        private ValuePop valuePop;
        private AdsManager adsManager;
        private NoteMenu noteMenu;

        // UI
        private VisualElement root;
        private VisualElement energyMenu;
        private Label energyLabelA;
        private Label energyLabelB;
        private Label energyBuyLabel;
        private Label energyWatchLabel;
        private Button buyButton;
        private Button watchButton;

        void Start()
        {
            // Cache
            gameData = GameData.Instance;
            LOCALE = I18n.Instance;
            menuUI = GetComponent<MenuUI>();
            shopMenu = GetComponent<ShopMenu>();
            valuePop = GetComponent<ValuePop>();
            adsManager = Services.Instance.GetComponent<AdsManager>();
            noteMenu = GetComponent<NoteMenu>();

            // UI
            root = GetComponent<UIDocument>().rootVisualElement;

            energyMenu = root.Q<VisualElement>("EnergyMenu");

            energyLabelA = energyMenu.Q<Label>("EnergyLabelA");
            energyLabelB = energyMenu.Q<Label>("EnergyLabelB");

            energyWatchLabel = energyMenu.Q<VisualElement>("EnergyBoxes").Q<Label>("WatchLabel");
            energyBuyLabel = energyMenu.Q<VisualElement>("EnergyBoxes").Q<Label>("BuyLabel");

            watchButton = energyMenu.Q<VisualElement>("EnergyBoxes").Q<Button>("WatchButton");
            buyButton = energyMenu.Q<VisualElement>("EnergyBoxes").Q<Button>("BuyButton");

            watchButton.clicked += () => WatchAdHandle();
            buyButton.clicked += () => BuyEnergyHandler();

            Init();
        }

        void Init()
        {
            // Make sure the menu is closed
            energyMenu.style.display = DisplayStyle.None;
            energyMenu.style.opacity = 0;

            energyLabelA.text = LOCALE.Get("energy_menu_label_a");
            energyLabelB.text = LOCALE.Get("energy_menu_label_b");

            if (gameData.energy == 0)
            {
                energyLabelB.style.display = DisplayStyle.Flex;
            }
            else
            {
                energyLabelB.style.display = DisplayStyle.None;
            }

            energyBuyLabel.text = "+" + energyBuyAmount;

            watchButton.text = LOCALE.Get("energy_menu_watch_ad");
            buyButton.text = gemsCost.ToString();
        }

        public void Open()
        {
            // Title
            string title = LOCALE.Get("energy_menu_title");

            energyWatchLabel.text = "+" + adsManager.energyRewardAmount;

            // Open menu
            menuUI.OpenMenu(energyMenu, title, true);
        }

        // Add energy after successfully watching an ad
        void WatchAdHandle()
        {
            adsManager.WatchAd(Types.AdType.Energy, (int newEnergyAmount) =>
            {
                valuePop.PopValue(newEnergyAmount, Types.CollGroup.Energy, watchButton.worldBound.center, false, true);
            }, () =>
            {
                noteMenu.Open("note_menu_energy_ad_error_title", new List<string>() { "note_menu_energy_ad_error_1", "note_menu_energy_ad_error_2" });
            });
        }

        // Add energy after buying it
        void BuyEnergyHandler()
        {
            // Check if we have enough energy
            if (gameData.gems >= gemsCost)
            {
                gameData.UpdateValue(-gemsCost, Types.CollGroup.Gems, false, true); // Note the -

                valuePop.PopValue(energyBuyAmount, Types.CollGroup.Energy, buyButton.worldBound.center, false, true);
            }
            else
            {
                shopMenu.Open("Gems");
            }
        }
    }
}
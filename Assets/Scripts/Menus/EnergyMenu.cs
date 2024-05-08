using System;
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

        private bool waitingForAd = false;

        private MenuUI.Menu menuType = MenuUI.Menu.Energy;

        // References
        private GameData gameData;
        private I18n LOCALE;
        private MenuUI menuUI;
        private ShopMenu shopMenu;
        private ValuePop valuePop;
        private AdsManager adsManager;
        private AnalyticsManager analyticsManager;
        private NoteMenu noteMenu;
        private UIData uiData;

        // UI
        private VisualElement content;
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
            analyticsManager = AnalyticsManager.Instance;
            noteMenu = GetComponent<NoteMenu>();
            uiData = GameData.Instance.GetComponent<UIData>();

            DataManager.Instance.CheckLoaded(() =>
            {
                // UI
                content = uiData.GetMenuAsset(menuType);

                energyLabelA = content.Q<Label>("EnergyLabelA");
                energyLabelB = content.Q<Label>("EnergyLabelB");

                energyWatchLabel = content.Q<VisualElement>("EnergyBoxes").Q<Label>("WatchLabel");
                energyBuyLabel = content.Q<VisualElement>("EnergyBoxes").Q<Label>("BuyLabel");

                watchButton = content.Q<VisualElement>("EnergyBoxes").Q<Button>("WatchButton");
                buyButton = content.Q<VisualElement>("EnergyBoxes").Q<Button>("BuyButton");

                watchButton.clicked += () => SoundManager.Tap(WatchAdHandle);
                buyButton.clicked += () => SoundManager.Tap(BuyEnergyHandler);
            });
        }

        public void Open()
        {
            // Check menu
            if (menuUI.IsMenuOpen(menuType))
            {
                return;
            }

            // Set menu content
            SetUI();

            energyWatchLabel.text = "+" + adsManager.energyRewardAmountInner;

            // Open menu
            menuUI.OpenMenu(content, menuType);
        }

        void SetUI()
        {
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

            buyButton.text = gemsCost.ToString();

            if (adsManager.enableAds)
            {
                watchButton.style.display = DisplayStyle.Flex;

                watchButton.text = LOCALE.Get("energy_menu_watch_ad");

                energyLabelA.text = LOCALE.Get("energy_menu_label_a");
            }
            else
            {
                watchButton.style.display = DisplayStyle.None;

                energyLabelA.text = LOCALE.Get("energy_menu_label_a_alt");
            }
        }

        // Add energy after successfully watching an ad
        void WatchAdHandle()
        {
            if (!waitingForAd)
            {
                waitingForAd = true;

                menuUI.ShowMenuOverlay(() =>
                {
                    adsManager.WatchAd(AdsManager.AdType.Energy, (int newEnergyAmount) =>
                    {
                        menuUI.HideMenuOverlay(() =>
                        {
                            waitingForAd = false;

                            analyticsManager.FireEnergyBoughtEvent(gameData.level, gameData.energy, gameData.gems, true);

                            valuePop.PopValue(newEnergyAmount, Item.CollGroup.Energy, watchButton.worldBound.center, false, true);
                        });
                    }, () =>
                    {
                        menuUI.HideMenuOverlay(() =>
                        {
                            waitingForAd = false;

                            noteMenu.Open("note_menu_energy_ad_error_title", new List<string>() { "note_menu_energy_ad_error_1", "note_menu_energy_ad_error_2" });
                        }, true);
                    });
                }, true);
            }
        }

        // Add energy after buying it
        void BuyEnergyHandler()
        {
            // Check if we have enough energy
            if (gameData.gems >= gemsCost)
            {
                analyticsManager.FireEnergyBoughtEvent(gameData.level, gameData.energy, gameData.gems, false);

                gameData.UpdateValue(-gemsCost, Item.CollGroup.Gems, false, true); // Note the -

                valuePop.PopValue(energyBuyAmount, Item.CollGroup.Energy, buyButton.worldBound.center, false, true);
            }
            else
            {
                shopMenu.Open("Gems");
            }
        }
    }
}
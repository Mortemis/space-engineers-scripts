using System;
using System.Collections.Generic;
using VRageMath;
using VRage.Game;
using VRage.Library;
using System.Text;
using Sandbox.ModAPI.Interfaces;
using Sandbox.ModAPI.Ingame;
using Sandbox.Common;
using Sandbox.Game;
using VRage.Collections;
using VRage.Game.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRage.Game.ObjectBuilders.Definitions;
using System.Linq;

namespace storage
{
    class Program
    {
        IMyGridTerminalSystem GridTerminalSystem = null;
        IMyGridProgramRuntimeInfo Runtime = null;
        Action<string> Echo = null;
        IMyTerminalBlock Me = null;
        //======================================================
        //////////////////////////START/////////////////////////
        //======================================================

        const string PANEL_NAME = "storage-panel";
        const string INGOT_STORAGE_GROUP = "ingot-containers";

        List<IMyCargoContainer> ingots = new List<IMyCargoContainer>();
        Dictionary<string, string> LOCALES_RU = new Dictionary<string, string>();
        IMyTextPanel panel;



        public Program()
        {
            InitLocales();

            IMyBlockGroup ingotStorageGroup = GridTerminalSystem.GetBlockGroupWithName(INGOT_STORAGE_GROUP);
            ingotStorageGroup.GetBlocksOfType(ingots);

            panel = GridTerminalSystem.GetBlockWithName(PANEL_NAME) as IMyTextPanel;
        }

        void Main(string arg)
        {
            UpdateDisplay();
        }

        public class StorageData
        {
            public int volume;
            public int amount;
            public Dictionary<string, float> items = new Dictionary<string, float>();

            public StorageData(int volume, int amount)
            {
                this.volume = volume;
                this.amount = amount;
            }
            public void AddItem(string name, float mass)
            {
                items.Add(name, mass);
            }
        }

        StorageData GetIngotsData()
        {
            float currentVolume = 0;
            float maxVolume = 0;

            int amount = 0;
            List<IMyInventory> inventories = new List<IMyInventory>();
            foreach (var ingotContainer in ingots)
            {
                inventories.Add(ingotContainer.GetInventory());
                amount++;
                currentVolume += (float)ingotContainer.GetInventory().CurrentVolume;
                maxVolume += (float)ingotContainer.GetInventory().MaxVolume;
            }
            int volume = (int)(currentVolume / maxVolume * 100);
            StorageData data = new StorageData(volume, amount);

            foreach (var inventory in inventories)
            {
                List<MyInventoryItem> items = new List<MyInventoryItem>();
                inventory.GetItems(items);
                foreach (var item in items)
                {
                    string itemName = item.Type.ToString();
                    itemName = itemName.Remove(0, itemName.IndexOf('/') + 1);
                    
                    if (data.items.ContainsKey(itemName))
                    {

                        data.items[itemName] += (float)item.Amount;
                    }
                    else
                    {
                        data.AddItem(itemName, (float)item.Amount);
                    }
                }
            }
            
            return data;
        }

        void UpdateDisplay()
        {
            StorageData ingotData = GetIngotsData();
            var ingots = from entry in ingotData.items orderby entry.Value descending select entry;

            panel.WriteText("Слитки:\n");
            foreach (var ingot in ingots)
            {
                panel.WriteText($"{GetTranslation(ingot.Key)}: {NormalizeFloat(ingot.Value)}\n", true);
            }

        }

        string NormalizeFloat(float number)
        {
            if (number > 1000000)
            {
                number /= 1000000;
                return number.ToString("0.#") + "kk";
            }
            if (number > 1000)
            {
                number /= 1000;
                return number.ToString("0.#") + "k";
            }
            return number.ToString("0");
        }

        void InitLocales()
        {
            LOCALES_RU.Add("Stone", "Гравий");
            LOCALES_RU.Add("Silicon", "Кремний");
            LOCALES_RU.Add("Iron", "Железо");
            LOCALES_RU.Add("Nickel", "Никель");
            LOCALES_RU.Add("Magnesium", "Магний");
            LOCALES_RU.Add("Cobalt", "Кобальт");
            LOCALES_RU.Add("Silver", "Серебро");
            LOCALES_RU.Add("Gold", "Золото");
            LOCALES_RU.Add("Platinum", "Платина");
            LOCALES_RU.Add("Uranium", "Уран");
        }

        string GetTranslation(string word)
        {
            if (LOCALES_RU.ContainsKey(word))
            {
                return LOCALES_RU[word];
            }
            return word;
        }

        //======================================================
        //////////////////////////END///////////////////////////
        //======================================================
    }
}



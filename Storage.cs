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
        IMyTextPanel panel;


        public Program()
        {
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
                    //data.AddItem(item.Type.ToString(), item.Amount.ToString());
                    if (data.items.ContainsKey(item.Type.ToString()))
                    {
                        data.items[item.Type.ToString()] += (float)item.Amount;
                    } 
                    else
                    {
                        
                        data.AddItem(item.Type.ToString(), (float)item.Amount);
                    }
                }
            }

            return data;
        }
        
        void UpdateDisplay()
        {
            StorageData ingotData = GetIngotsData();
            panel.WriteText("Ingots\n");
            foreach (var ingot in ingotData.items)
            {
                panel.WriteText($"{ingot.Key}: {ingot.Value}\n", true);
            }

        }




        //======================================================
        //////////////////////////END///////////////////////////
        //======================================================
    }
}



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

namespace power
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

        const string REACTORS_GRP = "reactors";
        const string BATTERIES_GRP = "batteries";
        const string PANEL_NAME = "power-panel";

        List<IMyReactor> reactors = new List<IMyReactor>();
        List<IMyBatteryBlock> batteries = new List<IMyBatteryBlock>();
        IMyTextPanel panel;

        bool autoMode;

        public Program()
        {
            autoMode = false;

            IMyBlockGroup reactorGroup = GridTerminalSystem.GetBlockGroupWithName(REACTORS_GRP);
            reactorGroup.GetBlocksOfType(reactors);

            IMyBlockGroup batteryGroup = GridTerminalSystem.GetBlockGroupWithName(BATTERIES_GRP);
            batteryGroup.GetBlocksOfType(batteries);

            panel = GridTerminalSystem.GetBlockWithName(PANEL_NAME) as IMyTextPanel;
        }

        void Main(string arg)
        {
            ReactorData reactorData = GetReactorData();
            BatteryData batteryData = GetBatteryData();
            if (arg == "auto") autoMode = !autoMode;
            if (autoMode)
            {
                if (batteryData.percentage < 10 && reactorData.status == false)
                {
                    SwitchReactors(true);
                }
                if (batteryData.percentage > 90 && reactorData.status == true)
                {
                    SwitchReactors(false);
                }
            }
            UpdateDisplay(reactorData, batteryData);
        }

        public class BatteryData
        {
            public int percentage;
            public bool isCharging;
            public int powerRate;
            public int count;
            public BatteryData(int percentage, bool isCharging, int powerRate, int count)
            {
                this.percentage = percentage;
                this.isCharging = isCharging;
                this.powerRate = powerRate;
                this.count = count;
            }
        }

        public class ReactorData
        {
            public bool status;
            public int power;
            public int count;
            public float uraniumAmount;

            public ReactorData(bool status, int power, int count, float uranium)
            {
                this.status = status;
                this.power = power;
                this.count = count;
                this.uraniumAmount = uranium;
            }
        }


        /*
        bool GetReactorStatus()
        {
            bool isEnabled = false;
            foreach (var reactor in reactors)
            {
                isEnabled |= reactor.Enabled;
            }
            return isEnabled;
        }

        int GetReactorPower()
        {
            float power = 0;
            foreach (var reactor in reactors)
            {
                power += reactor.CurrentOutput;
            }
            return (int)power;
        }*/
        ReactorData GetReactorData()
        {
            bool isEnabled = false;
            float power = 0;
            int count = 0;
            float uraniumAmount = 0;
            foreach (var reactor in reactors)
            {
                if (reactor.IsFunctional)
                {
                    isEnabled |= reactor.Enabled;
                    power += reactor.CurrentOutput;
                    count++;

                    uraniumAmount += (float) reactor.GetInventory(0).CurrentMass;
                }
            }
            return new ReactorData(isEnabled, (int)power, count, uraniumAmount);
        }

        BatteryData GetBatteryData()
        {
            float currentCharge = 0;
            float maxCharge = 0;
            float currentOutput = 0;
            float maxOutput = 0;
            float currentInput = 0;
            float maxInput = 0;
            bool isCharging = false;
            foreach (var battery in batteries)
            {
                isCharging = battery.IsCharging;
                currentCharge += battery.CurrentStoredPower;
                maxCharge += battery.MaxStoredPower;
                currentOutput += battery.CurrentOutput;
                maxOutput += battery.MaxOutput;
                currentInput += battery.CurrentInput;
                maxInput += battery.MaxInput;
            }


            int chargePercentage = (int)(currentCharge / maxCharge * 100);
            int powerRate;
            if (isCharging)
            {
                powerRate = (int)(currentInput / maxInput * 100);
            }
            else
            {
                powerRate = (int)(currentOutput / maxOutput * 100);
            }

            return new BatteryData(chargePercentage, isCharging, powerRate, batteries.Count);

        }

        void UpdateDisplay(ReactorData reactorData, BatteryData batteryData)
        {
            panel.WriteText($"Реакторы: {(reactorData.status ? "вкл" : "выкл")}\n");
            panel.WriteText($"Кол-во реакторов:  {reactorData.count}\n", true);
            panel.WriteText($"Мощность:  {reactorData.power} МВт\n", true);
            panel.WriteText($"U в реакторах:        {reactorData.uraniumAmount:0.##} кг\n", true);
            panel.WriteText($"Авто-режим:       {(autoMode ? "вкл" : "выкл")}\n", true);
            panel.WriteText("-----------------------\n", true);
            panel.WriteText($"Заряд: {batteryData.percentage}%\n", true);
            panel.WriteText($"Кол-во:  {batteryData.count}\n", true);
            panel.WriteText($"Статус: {(batteryData.isCharging ? "зарядка" : "разрядка")}\n", true);
            panel.WriteText($"{(batteryData.isCharging ? "Эффективность заряда:" : "Эффективность разряда: ")}     { batteryData.powerRate}%\n", true);
        }

        void SwitchReactors(bool state)
        {
            foreach(var reactor in reactors)
            {
                reactor.Enabled = state;
            }
        }

        //======================================================
        //////////////////////////END///////////////////////////
        //======================================================
    }
}


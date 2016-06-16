using System;
using System.IO;
using UnityEngine;
using VicisFCEMod.Mod;
using VicisFCEMod.Util;

namespace VicisFCEMod.Machines {
    class MassCrateModulePoweredMk1 : MassCrateModule, PowerConsumerInterface {

        public const string VALUE_NAME = "Vici.MassCrateModulePoweredMk1";

        protected float ippps; // Items per Power per Second
        protected float maxPower;
        protected float currentPower;
        protected float lastPowerConsumption;
        protected float powerConsumptionFreq;
        protected float maxPowerMultiplier;

        public MassCrateModulePoweredMk1(ModCreateSegmentEntityParameters parameters) : base(parameters) {
            cubeColor = Color.red;

            currentPower = 0;
            maxPower = 1;
            lastPowerConsumption = 0;
            powerConsumptionFreq = 0.5f;
            maxPowerMultiplier = 10;

            ippps = 20;

            maxBins = 1;
            maxBinSize = 0;
            maxItems = 0;
        }

        public override void LowFrequencyUpdate() {
            base.LowFrequencyUpdate();
            lastPowerConsumption -= LowFrequencyThread.mrPreviousUpdateTimeStep;
            if (lastPowerConsumption > 0) return;
            lastPowerConsumption += powerConsumptionFreq;

            float reqPower = getReqPower();
            float power = Math.Min(reqPower, currentPower);

            maxPower = Math.Max(maxPowerMultiplier * reqPower, 1); // We can store up to 2 seconds of power, max
            maxBinSize = (int)(currentPower * ippps);
            maxItems = maxBinSize;

            currentPower -= power;
        }

        protected virtual float getReqPower() {
            return ItemBaseUtil.getItemCount(items) / ippps * powerConsumptionFreq;
        }

        public override void Write(BinaryWriter writer) {
            base.Write(writer);
            writer.Write(currentPower);
        }

        public override void Read(BinaryReader reader, int entityVersion) {
            base.Read(reader, entityVersion);
            VicisMod.VicisModVersion version = (VicisMod.VicisModVersion)entityVersion;
            switch (version) {
                case VicisMod.VicisModVersion.Version1:
                case VicisMod.VicisModVersion.Version2:
                case VicisMod.VicisModVersion.Version3:
                    break;
                default:
                    currentPower = reader.ReadSingle();
                    break;
            }
        }

        public bool DeliverPower(float amount) {
            VicisMod.log(getPrefix(), "Attempting to receive power of " + amount + ", I have " + currentPower + " / " + maxPower);
            if (amount > GetRemainingPowerCapacity()) {
                VicisMod.log(getPrefix(), "Returning false for power delivery");
                return false;
            }
            currentPower += amount;
            VicisMod.log(getPrefix(), "Now have " + currentPower);
            return true;
        }

        public float GetMaximumDeliveryRate() {
            return float.MaxValue;
        }

        public float GetMaxPower() {
            return maxPower;
        }

        public override string getPrefix() {
            return VALUE_NAME;
        }

        public float GetRemainingPowerCapacity() {
            return maxPower - currentPower;
        }

        public bool WantsPowerFromEntity(SegmentEntity entity) {
            return true;
        }

        public override string GetPopupText() {
            return "Power: " + currentPower + " / " + maxPower + "\nPPS: " + (getReqPower() / powerConsumptionFreq) + "\n" + base.GetPopupText();
        }
    }

}
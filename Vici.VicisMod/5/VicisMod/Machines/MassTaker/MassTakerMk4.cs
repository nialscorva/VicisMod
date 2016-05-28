using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VicisFCEMod.Mod;

namespace VicisFCEMod.Machines {
    public class MassTakerMk4 : MassTaker, PowerConsumerInterface {

        public const string VALUE_NAME = "Vici.MassTakerMk4";
        public const string FRIENDLY_NAME = "Mass Taker Mk4";
        protected float currentPower;
        protected float maxPower;
        protected float powerPerJump;


        public MassTakerMk4(ModCreateSegmentEntityParameters parameters) : base(parameters) {
            maxItems = 10;
            droneSpeed = 5;
            batch = 10;

            drone = new DroneComputer(droneSpeed);
            droneColor = new Color(248 / 256f, 164 / 256f, 42 / 256f);
            droneSize = new Vector3(1f, 1f, 1f);

            currentPower = 0;
            powerPerJump = 256;
            maxPower = powerPerJump * 10; // 2 seconds
        }

        protected override string getFriendlyName() {
            return FRIENDLY_NAME;
        }

        protected override bool FinishGiveItem() {
            if (carriedItems.Count == 0) return false;

            VicisMod.log(getPrefix(), "Attempting to give item " + carriedItems[0].GetDisplayString());
            if (headTo.AttemptGiveItem(carriedItems[0], getCarriedItemCount())) {
                carriedItems.RemoveAt(0);
                mbCarriedCubeNeedsConfiguring = true;
                return true;
            }
            return false;
        }

        public bool DeliverPower(float amount) {
            if (amount > GetRemainingPowerCapacity()) {
                return false;
            }
            currentPower += amount;
            return true;
        }

        public float GetMaximumDeliveryRate() {
            return float.MaxValue;
        }

        public float GetMaxPower() {
            return maxPower;
        }

        public float GetRemainingPowerCapacity() {
            return maxPower - currentPower;
        }

        public bool WantsPowerFromEntity(SegmentEntity entity) {
            return true;
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

        protected override string getPrefix() {
            return VALUE_NAME;
        }

        protected override void retrieveDrone(Vector3 coords, float timeJump) {
            drone.flyToUnity(coords, timeJump);
        }

        protected override void sendDrone(Vector3 coords, float timeJump) {
            if (coords != drone.getPos() && currentPower > powerPerJump) {
                currentPower -= powerPerJump;
                drone.goToUnity(coords);
            }
        }

        public override string GetPopupText() {
            string ret = "Power: " + currentPower + " / " + maxPower;
            return base.GetPopupText() + "\n" + ret;
        }
    }
}
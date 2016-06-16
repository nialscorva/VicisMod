using UnityEngine;
using System.IO;
using VicisFCEMod.Mod;

namespace VicisFCEMod.Machines {
    public class MassGiverMk4 : MassGiver, PowerConsumerInterface {

        public const string VALUE_NAME = "Vici.MassGiverMk4";
        public const string FRIENDLY_NAME = "Mass Giver Mk4";
        protected float currentPower;
        protected float maxPower;
        protected float powerPerJump;

        public MassGiverMk4(ModCreateSegmentEntityParameters parameters) : base(parameters) {
            maxItems = 10;
            droneSpeed = 5;
            batch = 10;

            drone = new DroneComputer(droneSpeed);
            droneColor = new Color(102 / 256f, 50 / 256f, 159 / 256f);
            droneSize = new Vector3(1f, 1f, 1f);

            currentPower = 0;
            powerPerJump = 256;
            maxPower = powerPerJump * 10;
        }

        protected override string getFriendlyName() {
            return FRIENDLY_NAME;
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
            return VALUE_NAME + myId;
        }

        protected override void retrieveDrone(Vector3 coords, float timeJump) {
            drone.flyToUnity(mUnityDroneRestPos, timeJump);
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
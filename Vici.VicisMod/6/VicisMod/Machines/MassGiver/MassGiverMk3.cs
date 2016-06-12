using UnityEngine;

namespace VicisFCEMod.Machines {
    public class MassGiverMk3 : MassGiver {

        public const string VALUE_NAME = "Vici.MassGiverMk3";
        public const string FRIENDLY_NAME = "Mass Giver Mk3";

        public MassGiverMk3(ModCreateSegmentEntityParameters parameters) : base(parameters) {
            maxItems = 30;
            droneSpeed = 2;
            batch = 30;

            drone = new DroneComputer(droneSpeed);
            droneColor = new Color(20 / 256f, 42 / 256f, 204 / 256f);
            droneSize = new Vector3(1.4f, 1.4f, 1.4f);
        }

        protected override string getFriendlyName() {
            return FRIENDLY_NAME;
        }

        protected override string getPrefix() {
            return VALUE_NAME + myId;
        }

        protected override void retrieveDrone(Vector3 coords, float timeJump) {
            drone.flyToUnity(mUnityDroneRestPos, timeJump);
        }

        protected override void sendDrone(Vector3 coords, float timeJump) {
            drone.flyToUnity(targetCoords, timeJump);
        }
    }
}
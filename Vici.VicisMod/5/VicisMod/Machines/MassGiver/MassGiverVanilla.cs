using UnityEngine;
using VicisFCEMod.Mod;

namespace VicisFCEMod.Machines {
    public class MassGiverVanilla : MassGiver {

        public const string VALUE_NAME = "Vici.MassGiverVanilla";
        public const string FRIENDLY_NAME = "Mass Giver Vanilla";

        public MassGiverVanilla(ModCreateSegmentEntityParameters parameters) : base(parameters) {
            maxItems = 1;
            droneSpeed = 1;
            batch = 1;

            drone = new DroneComputer(droneSpeed);
            droneColor = Color.white;
        }

        protected override bool AttemptTakeItem() {
            if ((batch + getStoredItemsCount()) > maxItems) return false;

            VicisMod.log(getPrefix(), "Attempting to get item " + chosen.GetDisplayString());
            ItemBase item = headTo.AttemptTakeItem(chosen, batch);
            if (item != null) {
                carriedItems.Add(item);
                mbCarriedCubeNeedsConfiguring = true;
                return true;
            }
            return false;
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
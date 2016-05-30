using UnityEngine;
using VicisFCEMod.Mod;

namespace VicisFCEMod.Machines { 
public class MassTakerMk1 : MassTaker {

        public const string VALUE_NAME = "Vici.MassTakerMk1";
        public const string FRIENDLY_NAME = "Mass Taker Mk1";

        public MassTakerMk1(ModCreateSegmentEntityParameters parameters) : base(parameters) {
            maxItems = 5;
            droneSpeed = 1;
            batch = 5;

            drone = new DroneComputer(droneSpeed);
            droneColor = new Color(77 / 256f, 214 / 256f, 31 / 256f);
            droneSize = new Vector3(1.2f, 1.2f, 1.2f);
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

        protected override string getFriendlyName() {
            return FRIENDLY_NAME;
        }

        protected override string getPrefix() {
            return VALUE_NAME + myId;
        }

        protected override void retrieveDrone(Vector3 coords, float timeJump) {
            drone.flyToUnity(coords, timeJump);
        }

        protected override void sendDrone(Vector3 coords, float timeJump) {
            drone.flyToUnity(coords, timeJump);
        }
    }
}
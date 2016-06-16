﻿using UnityEngine;
using VicisFCEMod.Mod;

namespace VicisFCEMod.Machines {
    public class MassTakerVanilla : MassTaker {

        public const string VALUE_NAME = "Vici.MassTakerVanilla";
        public const string FRIENDLY_NAME = "Mass Taker Vanilla";

        public MassTakerVanilla(ModCreateSegmentEntityParameters parameters) : base(parameters) {
            maxItems = 1;
            droneSpeed = 1;
            batch = 1;

            drone = new DroneComputer(droneSpeed);
            droneColor = Color.white;
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
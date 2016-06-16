﻿using UnityEngine;
using VicisFCEMod.Mod;

namespace VicisFCEMod.Machines {
    public class MassTakerMk2 : MassTaker {

        public const string VALUE_NAME = "Vici.MassTakerMk2";
        public const string FRIENDLY_NAME = "Mass Taker Mk2";

        public MassTakerMk2(ModCreateSegmentEntityParameters parameters) : base(parameters) {
            maxItems = 5;
            droneSpeed = 3;
            batch = 5;

            drone = new DroneComputer(droneSpeed);
            droneColor = new Color(182 / 256f, 222 / 256f, 124 / 256f);
            droneSize = new Vector3(1.1f, 1.1f, 1.1f);
        }

        protected override bool FinishGiveItem() {
            while(carriedItems.Count > 0 && carriedItems[0] == null) {
                carriedItems.RemoveAt(0);
            }

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
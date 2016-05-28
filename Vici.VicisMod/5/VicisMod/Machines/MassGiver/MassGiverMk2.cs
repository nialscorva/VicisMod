using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VicisFCEMod.Machines {
    public class MassGiverMk2 : MassGiver {

        public const string VALUE_NAME = "Vici.MassGiverMk2";
        public const string FRIENDLY_NAME = "Mass Giver Mk2";

        public MassGiverMk2(ModCreateSegmentEntityParameters parameters) : base(parameters) {
            maxItems = 5;
            droneSpeed = 3;
            batch = 5;

            drone = new DroneComputer(droneSpeed);
            droneColor = new Color(48 / 256f, 135 / 256f, 223 / 256f);
            droneSize = new Vector3(1.1f, 1.1f, 1.1f);
        }

        protected override string getFriendlyName() {
            return FRIENDLY_NAME;
        }

        protected override string getPrefix() {
            return VALUE_NAME;
        }

        protected override void retrieveDrone(Vector3 coords, float timeJump) {
            drone.flyToUnity(mUnityDroneRestPos, timeJump);
        }

        protected override void sendDrone(Vector3 coords, float timeJump) {
            drone.flyToUnity(targetCoords, timeJump);
        }
    }
}
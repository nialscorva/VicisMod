using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class MassGiverMk1 : MassGiver {

    public const string VALUE_NAME = "Vici.MassGiverMk1";
    public const string FRIENDLY_NAME = "Mass Giver Mk1";

    public MassGiverMk1(ModCreateSegmentEntityParameters parameters) : base(parameters) {
        maxItems = 5;
        droneSpeed = 1;
        batch = 5;

        drone = new DroneComputer(droneSpeed);
        droneColor = new Color(126 / 256f, 210 / 256f, 252 / 256f);
        droneSize = new Vector3(1.2f, 1.2f, 1.2f);
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
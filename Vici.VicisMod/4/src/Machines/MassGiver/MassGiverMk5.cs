using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.IO;

public class MassGiverMk5 : MassGiverMk4, PowerConsumerInterface {

    public const string VALUE_NAME = "Vici.MassGiverMk5";
    public const string FRIENDLY_NAME = "Mass Giver Mk5";

    public MassGiverMk5(ModCreateSegmentEntityParameters parameters) : base(parameters) {
        maxItems = 1;
        droneSpeed = 0;
        batch = 1;

        drone = new DroneComputer(droneSpeed);
        droneColor = new Color(223 / 256f, 48 / 256f, 213 / 256f);
        droneSize = new Vector3(0.6f, 0.6f, 0.6f);

        currentPower = 0;
        powerPerJump = 512;
        maxPower = powerPerJump * 10; // 2 seconds
    }

    protected override string getFriendlyName() {
        return FRIENDLY_NAME;
    }

    protected override string getPrefix() {
        return VALUE_NAME;
    }

    protected override void retrieveDrone(Vector3 coords, float timeJump) {
        if (coords != drone.getPos() && currentPower > powerPerJump) {
            currentPower -= powerPerJump;
            drone.goToUnity(coords);
        }
    }
}
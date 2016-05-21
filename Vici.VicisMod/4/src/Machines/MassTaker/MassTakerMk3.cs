using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class MassTakerMk3 : MassTaker {

    public const string VALUE_NAME = "Vici.MassTakerMk3";
    public const string FRIENDLY_NAME = "Mass Taker Mk3";

    public MassTakerMk3(ModCreateSegmentEntityParameters parameters) : base(parameters) {
        maxItems = 30;
        droneSpeed = 2;
        batch = 30;

        drone = new DroneComputer(droneSpeed);
        droneColor = new Color(235 / 256f, 248 / 256f, 42 / 256f);
        droneSize = new Vector3(1.4f, 1.4f, 1.4f);
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
        return VALUE_NAME;
    }

    protected override void retrieveDrone(Vector3 coords, float timeJump) {
        drone.flyToUnity(coords, timeJump);
    }

    protected override void sendDrone(Vector3 coords, float timeJump) {
        drone.flyToUnity(coords, timeJump);
    }
}
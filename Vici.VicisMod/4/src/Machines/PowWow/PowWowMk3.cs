using System;
using UnityEngine;

public class PowWowMk3 : PowWow {

    public const string VALUE_NAME = "Vici.PowWowMk3";

    public PowWowMk3(ModCreateSegmentEntityParameters param) : base(param) {
        transferCap = 320 * transferFrequency;
        radius = 5;
        maxPower = 1000;
        cubeColor = Color.magenta;
        scanFrequency = 10f;
    }

    public override string getPrefix() {
        return VALUE_NAME;
    }
}

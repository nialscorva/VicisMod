using System;
using UnityEngine;

public class PowWowMk4 : PowWow {

    public const string VALUE_NAME = "Vici.PowWowMk4";

    public PowWowMk4(ModCreateSegmentEntityParameters param) : base(param) {
        radius = 7;
        maxTransfer = float.MaxValue;
        cubeColor = Color.red;
        scanFrequency = 20f;
    }

    public override string getPrefix() {
        return VALUE_NAME;
    }
}

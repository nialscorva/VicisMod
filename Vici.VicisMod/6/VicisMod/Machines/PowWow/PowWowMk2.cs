using System;
using UnityEngine;

namespace VicisFCEMod.Machines {
    public class PowWowMk2 : PowWow {

        public const string VALUE_NAME = "Vici.PowWowMk2";

        public PowWowMk2(ModCreateSegmentEntityParameters param) : base(param) {
            transferCap = 40 * transferFrequency;
            radius = 4;
            maxPower = 500;
            cubeColor = Color.blue;
        }

        public override string getPrefix() {
            return VALUE_NAME;
        }
    }
}
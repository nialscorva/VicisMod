using System;
using UnityEngine;

namespace VicisFCEMod.Machines {
    public class PowWowMk1 : PowWow {

        public const string VALUE_NAME = "Vici.PowWowMk1";

        public PowWowMk1(ModCreateSegmentEntityParameters param) : base(param) {
            transferCap = 10 * transferFrequency;
            maxPower = 200;
            cubeColor = Color.green;
        }

        public override string getPrefix() {
            return VALUE_NAME;
        }
    }
}
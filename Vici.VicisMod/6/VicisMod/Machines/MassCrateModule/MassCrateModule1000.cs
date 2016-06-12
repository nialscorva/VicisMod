using UnityEngine;

namespace VicisFCEMod.Machines {
    class MassCrateModule1000 : MassCrateModule {

        public const string VALUE_NAME = "Vici.MassCrateModule1000";

        public MassCrateModule1000(ModCreateSegmentEntityParameters parameters) : base(parameters) {
            cubeColor = new Color(54 / 256f, 106 / 256f, 231 / 256f);

            maxBins = 1;
            maxBinSize = 1000;
            maxItems = 1000;
        }

        public override string getPrefix() {
            return VALUE_NAME;
        }
    }

}
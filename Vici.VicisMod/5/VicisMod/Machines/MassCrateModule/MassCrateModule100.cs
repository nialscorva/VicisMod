using UnityEngine;

namespace VicisFCEMod.Machines {
    class MassCrateModule100 : MassCrateModule {

        public const string VALUE_NAME = "Vici.MassCrateModule100";

        public MassCrateModule100(ModCreateSegmentEntityParameters parameters) : base(parameters) {
            cubeColor = new Color(238 / 256f, 252 / 256f, 39 / 256f);

            maxBins = 100;
            maxBinSize = 100;
            maxItems = 100;
        }

        public override string getPrefix() {
            return VALUE_NAME;
        }
    }

}
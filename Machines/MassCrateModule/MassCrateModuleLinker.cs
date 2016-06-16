using UnityEngine;

namespace VicisFCEMod.Machines {
    public class MassCrateModuleLinker : MassCrateModule {

        public const string VALUE_NAME = "Vici.MassCrateModuleLinker";

        public MassCrateModuleLinker(ModCreateSegmentEntityParameters parameters) : base(parameters) {
            cubeColor = Color.black;

            maxBins = 0;
            maxBinSize = 0;
            maxItems = 0;
            skip = true;
        }

        public override string getPrefix() {
            return VALUE_NAME;
        }
    }
}
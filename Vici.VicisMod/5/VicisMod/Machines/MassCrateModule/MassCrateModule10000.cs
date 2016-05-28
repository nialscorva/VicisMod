using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VicisFCEMod.Machines {
    class MassCrateModule10000 : MassCrateModule {

        public const string VALUE_NAME = "Vici.MassCrateModule10000";

        public MassCrateModule10000(ModCreateSegmentEntityParameters parameters) : base(parameters) {
            cubeColor = new Color(211 / 256f, 54 / 256f, 231 / 256f);

            maxBins = 1;
            maxBinSize = 10000;
            maxItems = 10000;
        }

        public override string getPrefix() {
            return VALUE_NAME;
        }
    }

}
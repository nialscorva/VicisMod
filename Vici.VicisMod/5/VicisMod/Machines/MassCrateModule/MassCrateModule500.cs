using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VicisFCEMod.Machines {
    class MassCrateModule500 : MassCrateModule {

        public const string VALUE_NAME = "Vici.MassCrateModule500";

        public MassCrateModule500(ModCreateSegmentEntityParameters parameters) : base(parameters) {
            cubeColor = new Color(54 / 256f, 231 / 256f, 181 / 256f);

            maxBins = 1;
            maxBinSize = 500;
            maxItems = 500;
        }

        public override string getPrefix() {
            return VALUE_NAME;
        }
    }

}
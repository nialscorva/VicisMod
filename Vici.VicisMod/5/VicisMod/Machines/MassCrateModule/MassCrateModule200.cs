using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VicisFCEMod.Machines {
    class MassCrateModule200 : MassCrateModule {

        public const string VALUE_NAME = "Vici.MassCrateModule200";

        public MassCrateModule200(ModCreateSegmentEntityParameters parameters) : base(parameters) {
            cubeColor = new Color(89 / 256f, 231 / 256f, 54 / 256f);

            maxBins = 1;
            maxBinSize = 200;
            maxItems = 200;
        }

        public override string getPrefix() {
            return VALUE_NAME;
        }
    }

}
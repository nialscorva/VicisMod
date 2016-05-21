using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class MassCrateModuleVanilla : MassCrateModule {

    public const string VALUE_NAME = "Vici.MassCrateModuleVanilla";
    
    public MassCrateModuleVanilla(ModCreateSegmentEntityParameters parameters) : base(parameters) {
        cubeColor = Color.white;

        maxBins = 25;
        maxBinSize = 25;
        maxItems = 25;
    }

    public override string getPrefix() {
        return VALUE_NAME;
    }
}


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class MassCrateModuleLinker : MassCrateModule {

    public const string VALUE_NAME = "Vici.MassCrateModuleLinker";

    public MassCrateModuleLinker(ModCreateSegmentEntityParameters parameters) : base(parameters) {
        cubeColor = Color.black;

        maxBins = 0;
        maxBinSize = 0;
        maxItems = 0;
    }

    public override string getPrefix() {
        return VALUE_NAME;
    }
}

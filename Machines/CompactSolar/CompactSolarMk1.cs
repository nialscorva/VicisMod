using UnityEngine;

class CompactSolarMk1 : CompactSolar {

    public const string VALUE_NAME = "Vici.CompactSolarMk1";

    public CompactSolarMk1(ModCreateSegmentEntityParameters param) : base(param) {
        // This machine represents the work of 8 Solar Panels. The base LFU will have
        // done the work of 1, however, so we only need to do the work of 7
        multiplier = 8 - 1;
        mrMaxPower *= 8;
        panelColor = new Color(0.5f, 0.5f, 0.5f);
    }

    public override string getPrefix() {
        return VALUE_NAME;
    }
}


using UnityEngine;

class CompactSolarMk3 : CompactSolar {

    public const string VALUE_NAME = "Vici.CompactSolarMk3";

    public CompactSolarMk3(ModCreateSegmentEntityParameters param) : base(param) {
        // This machine represents the work of 512 Solar Panels. The base LFU will have
        // done the work of 1, however, so we only need to do the work of 511
        this.multiplier = 512 - 1;
        this.mrMaxPower *= 512;
        this.panelColor = new Color(0.1f, 0.1f, 0.1f);
    }

    public override string getPrefix() {
        return VALUE_NAME;
    }
}

using UnityEngine;

class CompactSolarMk2 : CompactSolar {

    public const string VALUE_NAME = "Vici.CompactSolarMk2";

    public CompactSolarMk2(ModCreateSegmentEntityParameters param) : base(param) {
        // This machine represents the work of 64 Solar Panels. The base LFU will have
        // done the work of 1, however, so we only need to do the work of 63
        this.multiplier = 64 - 1;
        this.mrMaxPower *= 64;
        this.panelColor = new Color(0.3f, 0.3f, 0.3f);
    }

    public override string getPrefix() {
        return VALUE_NAME;
    }
}

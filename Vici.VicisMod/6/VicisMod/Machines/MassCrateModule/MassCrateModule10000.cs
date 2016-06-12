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

        public override string GetPopupText() {
            string ret = base.GetPopupText();
            ret += "\nPress (Q) to retrieve items";

            if (Input.GetButton("Extract") && items.Count > 0 && WorldScript.mLocalPlayer.mInventory.AddItem(items[0])) {
                FloatingCombatTextManager.instance.QueueText(mnX, mnY + 1L, mnZ, 1f, items[0].GetDisplayString(), Color.cyan, 1.5f);
                items.RemoveAt(0);
            }

            return ret;
        }
    }

}
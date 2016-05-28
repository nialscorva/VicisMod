using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VicisFCEMod.Mod;

namespace VicisFCEMod.Machines {
    public class MassCrateModuleManager {

        public const string LOGGER_PREFIX = "Vici.MassCrateModuleManager";

        // I feel dirty about this - Sets weren't added to C# until .NET 3.5, but I don't want to rely on 
        // others having their .NET framework up to date (not entirely sure if it's possible to have out of
        // date .NET frameworks with this game). Instead, I'll use a Dictionary to represent my Set for me.
        public Dictionary<MassCrateModule, MassCrateModule> modules = new Dictionary<MassCrateModule, MassCrateModule>();
        //public Dictionary<ushort, Dictionary<ushort, MassCrateModule>> locations = new Dictionary<ushort, Dictionary<ushort, MassCrateModule>>();

        public void Add(MassCrateModule mcm) {
            modules.Add(mcm, mcm);
            mcm.manager = this;
        }

        public void Remove(MassCrateModule mcm) {
            modules.Remove(mcm);

            if (modules.Count == 0) {
                VicisMod.log(LOGGER_PREFIX, "I'm All Alone. Goodbye.");
                return;
            }

            // Nothing elso to do
            if (mcm.neighbors.Count == 0) return;

            // Need to figure out if we need to split into two groups...
            VicisMod.log(LOGGER_PREFIX, "Getting ready for floodfill");
            List<List<MassCrateModule>> groups = new List<List<MassCrateModule>>();
            List<MassCrateModule> neighbors = mcm.neighbors;
            foreach (MassCrateModule neighbor in neighbors) {
                bool cont = false;
                foreach (List<MassCrateModule> group in groups) {
                    if (group.Contains(neighbor)) {
                        cont = true;
                        break;
                    }
                }
                if (cont) continue;
                List<MassCrateModule> floodFillResults = floodFillFun(neighbor);
                groups.Add(floodFillResults);
            }

            // We found only one group, don't need to spin off any more managers!
            if (groups.Count == 1 && groups[0].Count == modules.Count) return;
            if (groups.Count == 1) {
                VicisMod.log(LOGGER_PREFIX, "Found one group, but it has " + groups[0].Count + " crates and I know of " + modules.Count + " crates. What?");
                return;
            }

            VicisMod.log(LOGGER_PREFIX, "I'm now the lord of " + groups[0].Count + " modules (hopefully after I create the other managers");

            // Ok, now we need to create new managers for the extra groups.
            // First, remove the results from current modules
            for (int i = 1; i < groups.Count; ++i) {
                VicisMod.log(LOGGER_PREFIX, "Removing " + groups[i].Count + " from old from modules");
                foreach (MassCrateModule m in groups[i]) {
                    modules.Remove(m);
                }
                MassCrateModuleManager newManager = new MassCrateModuleManager();
                newManager.AddAll(groups[i]);
            }

            VicisMod.log(LOGGER_PREFIX, "I'm now the lord of " + modules.Count + " modules, really!");

            if (modules.Count == 0) {
                VicisMod.log(LOGGER_PREFIX, "Somehow, I have no more modules...");
            }
        }

        public List<MassCrateModule> floodFillFun(MassCrateModule start) {
            // Make sure we start of correct
            foreach (MassCrateModule m in modules.Keys) m.ping = false;

            // We've already pinged this guy.
            start.ping = true;

            List<MassCrateModule> floodFillResults = new List<MassCrateModule>();
            List<MassCrateModule> floodFill = new List<MassCrateModule>();
            floodFill.Add(start);
            VicisMod.log(LOGGER_PREFIX, "Starting floodfill with " + floodFill.Count + " guy");
            while (floodFill.Count > 0) {
                MassCrateModule m = floodFill[0];

                foreach (MassCrateModule n in m.neighbors) {
                    if (n.ping) continue;
                    n.ping = true;
                    floodFill.Add(n);
                }
                floodFillResults.Add(m);
                floodFill.RemoveAt(0);

                VicisMod.log(LOGGER_PREFIX, "Now have " + floodFill.Count + " things to check and " + floodFillResults.Count + " results");
            }
            VicisMod.log(LOGGER_PREFIX, "Returning " + floodFillResults.Count + " results");
            return floodFillResults;
        }

        public void AddAll(List<MassCrateModule> mcms) {
            foreach (MassCrateModule mcm in mcms) {
                modules.Add(mcm, mcm);
                mcm.manager = this;
            }
        }

        public bool AttemptGiveItem(ItemBase item) {
            foreach (MassCrateModule mcm in modules.Keys) {
                VicisMod.log(LOGGER_PREFIX, "Attempting to give " + item.GetDisplayString() + " to a module");
                if (mcm.AttemptGiveItem(item)) return true;
                VicisMod.log(LOGGER_PREFIX, "Failed, will attempt again");
            }
            VicisMod.log(LOGGER_PREFIX, "Could not give item " + item.GetDisplayString());
            return false;
        }

        public ItemBase AttemptTakeItem(ItemBase item) {
            foreach (MassCrateModule mcm in modules.Keys) {
                VicisMod.log(LOGGER_PREFIX, "Attempting to take " + item.GetDisplayString() + " to a module");
                ItemBase ret = mcm.AttemptTakeItem(item);
                if (ret != null) return ret;

                VicisMod.log(LOGGER_PREFIX, "Failed, will attempt again");
            }
            VicisMod.log(LOGGER_PREFIX, "Could not give take " + item.GetDisplayString());
            return null;
        }

        public MassCrateModule provideCrateDropoff(ItemBase item, MassTaker taker, int amount) {
            float dist = 999999999;
            MassCrateModule ret = null;
            foreach (MassCrateModule mcm in modules.Keys) {
                if (mcm.AttemptGiveItem(item, amount, false)) {
                    float tdist = calcDist(mcm, taker);
                    if (tdist < dist) {
                        ret = mcm;
                        dist = tdist;
                    }
                }
            }
            return ret;
        }

        public MassCrateModule provideCratePickup(ItemBase item, MassGiver giver, int amount) {
            float dist = 999999999;
            MassCrateModule ret = null;
            foreach (MassCrateModule mcm in modules.Keys) {
                if (mcm.AttemptTakeItem(item, amount, false) != null) {
                    float tdist = calcDist(mcm, giver);
                    if (tdist < dist) {
                        ret = mcm;
                        dist = tdist;
                    }
                }
            }
            return ret;
        }

        public static float calcDist(SegmentEntity a, SegmentEntity b) {
            return new Vector3(a.mnX - b.mnX, a.mnY - b.mnY, a.mnZ - b.mnZ).magnitude;
        }

        public static float calcDist(SegmentEntity a, Vector3 pos) {
            return new Vector3(a.mnX - pos.x, a.mnY - pos.y, a.mnZ - pos.z).magnitude;
        }

        public void Merge(MassCrateModuleManager mcmm) {
            // First, assign this MCMM as the manager for all modules controlled by the old manager
            foreach (MassCrateModule mcm in mcmm.modules.Keys) {
                Add(mcm);
            }

            mcmm.modules.Clear();
        }

        public int getNumItems() {
            int ret = 0;
            foreach (MassCrateModule m in modules.Keys) {
                ret += m.getNumItems();
            }
            return ret;
        }

        public int getMaxItems() {
            int ret = 0;
            foreach (MassCrateModule m in modules.Keys) {
                ret += m.getMaxItems();
            }
            return ret;
        }
    }

}
using System.Collections.Generic;
using UnityEngine;
using VicisFCEMod.Mod;

namespace VicisFCEMod.Machines {
    public class MassCrateModuleManager {

        public const string LOGGER_PREFIX = "Vici.MassCrateModuleManager";
        public List<MassCrateModule> modules = new List<MassCrateModule>();

        public void Add(MassCrateModule mcm) {
            if (modules.Contains(mcm)) return;
            modules.Add(mcm);
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
            for (int i = 0; i < neighbors.Count; ++i) {
                MassCrateModule neighbor = neighbors[i];
                bool cont = false;
                for (int j = 0; j < groups.Count; ++j) { 
                    List<MassCrateModule> group = groups[j];
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
                for (int j = 0; j < groups[i].Count; ++j) {
                    MassCrateModule m = groups[i][j];
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
            for (int i = 0; i < modules.Count; ++i) modules[i].ping = false;

            // We've already pinged this guy.
            start.ping = true;

            List<MassCrateModule> floodFillResults = new List<MassCrateModule>();
            List<MassCrateModule> floodFill = new List<MassCrateModule>();
            floodFill.Add(start);
            VicisMod.log(LOGGER_PREFIX, "Starting floodfill with " + floodFill.Count + " guy");
            while (floodFill.Count > 0) {
                MassCrateModule m = floodFill[0];

                for (int i = 0; i < m.neighbors.Count; ++i) {
                    MassCrateModule n = m.neighbors[i];
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
            for (int i = 0; i < mcms.Count; ++i) {
                modules.Add(mcms[i]);
                mcms[i].manager = this;
            }
        }

        /*
        public bool AttemptGiveItem(ItemBase item, int amount) {
            foreach (MassCrateModule mcm in modules) {
                VicisMod.log(LOGGER_PREFIX, "Attempting to give " + item.GetDisplayString() + " to a module");
                if (mcm.AttemptGiveItem(item, amount)) return true;
                VicisMod.log(LOGGER_PREFIX, "Failed, will attempt again");
            }
            VicisMod.log(LOGGER_PREFIX, "Could not give item " + item.GetDisplayString());
            return false;
        }

        public ItemBase AttemptTakeItem(ItemBase item) {
            foreach (MassCrateModule mcm in modules) {
                VicisMod.log(LOGGER_PREFIX, "Attempting to take " + item.GetDisplayString() + " to a module");
                ItemBase ret = mcm.AttemptTakeItem(item);
                if (ret != null) return ret;

                VicisMod.log(LOGGER_PREFIX, "Failed, will attempt again");
            }
            VicisMod.log(LOGGER_PREFIX, "Could not give take " + item.GetDisplayString());
            return null;
        }
        */

        public MassCrateModule provideCrateDropoff(ItemBase item, MassTaker taker, int amount) {
            float dist = float.MaxValue;
            MassCrateModule ret = null;
            bool itemIsClaimed = false;
            bool claimedCrateAccepted = false;
            for (int i = 0; i < modules.Count; ++i) {// (MassCrateModule mcm in modules) {
                if (modules[i].shouldSkip()) continue;
                MassCrateModule mcm = modules[i];
                bool thisCrateClaimed = mcm.HasClaimed(item);
                if(thisCrateClaimed && !itemIsClaimed) {
                    dist = float.MaxValue;
                    ret = null;
                    itemIsClaimed = true;
                }
                itemIsClaimed |= thisCrateClaimed;
                if (mcm.AttemptGiveItem(item, amount, false)) {
                    // Check if this crate has claimed this item. If so, need to disregard other successful searches
                    if(mcm.HasClaimed(item) && !claimedCrateAccepted) {
                        claimedCrateAccepted = true;
                        dist = calcDist(mcm, taker);
                        ret = mcm;
                        continue;
                    }

                    // If this item has been claimed by at least 1 crate, skip the rest of the searches.
                    if (!thisCrateClaimed && itemIsClaimed) continue;

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
            float dist = float.MaxValue;
            MassCrateModule ret = null;
            
            for (int i = 0; i < modules.Count; ++i) {// MassCrateModule mcm in modules) {
                if (modules[i].shouldSkip()) continue;
                MassCrateModule mcm = modules[i];
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
            return new Vector3(a.mnX - b.mnX, a.mnY - b.mnY, a.mnZ - b.mnZ).sqrMagnitude;
        }

        public static float calcDist(SegmentEntity a, Vector3 pos) {
            return new Vector3(a.mnX - pos.x, a.mnY - pos.y, a.mnZ - pos.z).sqrMagnitude;
        }

        public void Merge(MassCrateModuleManager mcmm) {
            // First, assign this MCMM as the manager for all modules controlled by the old manager
            for (int i = 0; i < mcmm.modules.Count; ++i) {
                Add(mcmm.modules[i]);
            }

            mcmm.modules.Clear();
        }

        public int getNumItems() {
            int ret = 0;
            
            for (int i = 0; i < modules.Count; ++i) {
                ret += modules[i].getNumItems();
            }

            return ret;
        }

        public int getMaxItems() {
            int ret = 0;
            
            for(int i = 0; i < modules.Count; ++i) {
                ret += modules[i].getMaxItems();
            }            
            
            return ret;
        }
    }

}
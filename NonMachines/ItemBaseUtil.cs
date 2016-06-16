﻿using System.Collections.Generic;
using VicisFCEMod.Mod;

namespace VicisFCEMod.Util {
    public static class ItemBaseUtil {

        public const string LOGGER_PREFIX = "Vici.ItemBaseUtil";

        public static bool compareBaseDeep(this ItemBase a, ItemBase b) {
            return a.mnItemID == b.mnItemID && a.mType == b.mType &&
                (compareCubeStack(a as ItemCubeStack, b as ItemCubeStack) ||
                 compareDurability(a as ItemDurability, b as ItemDurability) ||
                 compareStack(a as ItemStack, b as ItemStack) ||
                 compareSingle(a as ItemSingle, b as ItemSingle) ||
                 compareCharge(a as ItemCharge, b as ItemCharge) ||
                 compareLocation(a as ItemLocation, b as ItemLocation));
        }

        public static bool compareBase(this ItemBase a, ItemBase b) {
            return a.mnItemID == b.mnItemID && a.mType == b.mType;
        }

        public static bool compareCubeStack(this ItemCubeStack a, ItemCubeStack b) {
            // We'll ignore the item stacks for now. May revisit in the future
            return a != null && b != null && compareBase(a, b) && a.mCubeType == b.mCubeType && a.mCubeValue == b.mCubeValue;// && a.mnAmount == b.mnAmount;
        }

        public static bool compareDurability(this ItemDurability a, ItemDurability b) {
            return a != null && b != null && compareBase(a, b) && a.mnCurrentDurability == b.mnCurrentDurability && a.mnMaxDurability == b.mnMaxDurability;
        }

        public static bool compareStack(this ItemStack a, ItemStack b) {
            // Again, we'll ignore the size of the stack for now
            return a != null && b != null && compareBase(a, b);// && a.mnAmount == b.mnAmount;
        }

        public static bool compareSingle(this ItemSingle a, ItemSingle b) {
            return a != null && b != null && compareBase(a, b);
        }

        public static bool compareCharge(this ItemCharge a, ItemCharge b) {
            return a != null && b != null && compareBase(a, b) && a.mChargeLevel == b.mChargeLevel;
        }

        public static bool compareLocation(this ItemLocation a, ItemLocation b) {
            return a != null && b != null && compareBase(a, b) && a.mLocX == b.mLocX && a.mLocY == b.mLocY && a.mLocZ == b.mLocZ &&
                a.mLookVector.x == b.mLookVector.x && a.mLookVector.y == b.mLookVector.y && a.mLookVector.z == b.mLookVector.z;
        }

        public static bool isStackAndSame(this ItemBase a, ItemBase b) {
            return a != null && b != null && (compareCubeStack(a as ItemCubeStack, b as ItemCubeStack) || compareStack(a as ItemStack, b as ItemStack));
        }

        public static bool isStack(this ItemBase a) {
            return a != null && (a.mType == ItemType.ItemCubeStack || a.mType == ItemType.ItemStack);
        }

        public static void incrementStack(this ItemBase a, int amount) {
            if (a.mType == ItemType.ItemCubeStack) {
                (a as ItemCubeStack).mnAmount += amount;
            } else if (a.mType == ItemType.ItemStack) {
                (a as ItemStack).mnAmount += amount;
            }
        }

        public static void decrementStack(this ItemBase a, int amount) {
            if (a.mType == ItemType.ItemCubeStack) {
                (a as ItemCubeStack).mnAmount -= amount;
            } else if (a.mType == ItemType.ItemStack) {
                (a as ItemStack).mnAmount -= amount;
            }
        }

        public static void setAmount(this ItemBase a, int amount) {
            if (a.mType == ItemType.ItemCubeStack) {
                (a as ItemCubeStack).mnAmount = amount;
            } else if (a.mType == ItemType.ItemStack) {
                (a as ItemStack).mnAmount = amount;
            }
        }

        public static ItemBase newInstance(ItemBase a) {
            
            switch (a.mType) {
                case ItemType.ItemCubeStack:
                    ItemCubeStack ics = a as ItemCubeStack;
                    return new ItemCubeStack(ics.mCubeType, ics.mCubeValue, ics.mnAmount);
                case ItemType.ItemStack:
                    ItemStack its = a as ItemStack;
                    return new ItemStack(its.mnItemID, its.mnAmount);
                case ItemType.ItemCharge:
                    ItemCharge ic = a as ItemCharge;
                    return new ItemCharge(ic.mnItemID, (int)ic.mChargeLevel);
                case ItemType.ItemDurability:
                    ItemDurability id = a as ItemDurability;
                    return new ItemDurability(id.mnItemID, id.mnCurrentDurability, id.mnMaxDurability);
                case ItemType.ItemLocation:
                    ItemLocation il = a as ItemLocation;
                    return new ItemLocation(il.mnItemID, il.mLocX, il.mLocY, il.mLocZ, il.mLookVector);
                case ItemType.ItemSingle:
                    return new ItemSingle(a.mnItemID);
            }
            return null;
        }

        public static int getAmount(this ItemBase item) {
            if (item.mType == ItemType.ItemCubeStack) {
                ItemCubeStack a = item as ItemCubeStack;
                if (a != null) return a.mnAmount;
            } else if (item.mType == ItemType.ItemStack) {
                ItemStack a = item as ItemStack;
                if (a != null) return a.mnAmount;
            }
            return 1;
        }

        public static int getItemCount(this List<ItemBase> items) {
            if (items == null) return 0;
            int ret = 0;
            for (int i = 0; i < items.Count; ++i) {
                if (items[i] == null) continue;
                ret += getAmount(items[i]);
            }
            return ret;
        }
    }
}
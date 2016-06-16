using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VicisFCEMod.Mod;
using VicisFCEMod.Util;
using FortressCraft.Community;

namespace VicisFCEMod.Machines {
    public abstract class MassTaker : MachineEntity, CommunityItemInterface {

        protected static int id = 0;
        protected int myId;

        public const string CUBE_NAME = "Vici.MassTaker";

        public MassCrateModule mcm;

        public List<ItemBase> items = new List<ItemBase>();
        public List<ItemBase> carriedItems = new List<ItemBase>();
        protected MassCrateModule headTo;
        protected Vector3 targetCoords;
        protected MassCrateModule lastInteracted;
        protected ushort maxItems;
        protected int batch;

        protected Vector3 forwards;

        protected bool linkedToGo;
        protected GameObject CarryDrone;
        protected Color droneColor;
        protected Vector3 droneSize;
        protected DroneComputer drone;
        protected GameObject CarryDroneClamp;
        protected GameObject Thrust_Particles;
        protected GameObject HoloCubePreview;
        protected Vector3 mUnityDroneRestPos;
        protected bool mbCarriedCubeNeedsConfiguring;
        protected GameObject mCarriedObjectItem;
        protected bool mbHoloPreviewDirty;

        protected GameObject HoloPreview;
        protected float droneSpeed;

        public MassTaker(ModCreateSegmentEntityParameters parameters) :
            base(eSegmentEntity.Mod,
                SpawnableObjectEnum.MassStorageInputPort,
                parameters.X,
                parameters.Y,
                parameters.Z,
                parameters.Cube,
                parameters.Flags,
                parameters.Value,
                parameters.Position,
                parameters.Segment) {
            myId = id++;
            mbNeedsLowFrequencyUpdate = true;
            mbNeedsUnityUpdate = true;
            batch = 1;
            forwards = SegmentCustomRenderer.GetRotationQuaternion(parameters.Flags) * Vector3.forward;
            forwards.Normalize();
            drone = new DroneComputer(droneSpeed);
            droneSize = new Vector3(1, 1, 1);
            LookForAttachedModules();
        }

        protected virtual bool AttemptGiveItem() {
            if (items.Count == 0 || getDropOffSize() == 0) return false;

            ItemBase item = Util.ItemBaseUtil.newInstance(items[0]);
            int count = getDropOffSize();
            if (Util.ItemBaseUtil.isStack(items[0])) {
                // We have a stacked item, make sure to carry only what we need
                Util.ItemBaseUtil.decrementStack(items[0], count);
                if (Util.ItemBaseUtil.getAmount(items[0]) == 0) {
                    // Stack no longer exists, remove it.
                    // Don't need to set the amount on the item since it already had a =< batch size amount
                    items.RemoveAt(0);
                } else {
                    // Stack still exists, so we need to set the amount that we removed
                    Util.ItemBaseUtil.setAmount(item, count);
                }
            } else {
                // No stack, no problem!
                items.RemoveAt(0);
            }

            carriedItems.Add(item);
            return true;
        }

        protected abstract void sendDrone(Vector3 coords, float timeJump);

        protected abstract void retrieveDrone(Vector3 coords, float timeJump);

        protected abstract string getPrefix();

        protected abstract string getFriendlyName();

        protected abstract bool FinishGiveItem();

        protected virtual void LookForAttachedModules() {
            VicisMod.log(getPrefix(), "Looking for storage");
            bool ignore;
            List<MassCrateModule> list = VicisMod.checkSurrounding<MassCrateModule>(this, out ignore);
            if (list.Count > 0) {
                mcm = list[0];
                mcm.taker = this;
            }
        }

        public override void DropGameObject() {
            base.DropGameObject();
            linkedToGo = false;
        }

        public override void UnitySuspended() {
            CarryDrone = null;
            if (mCarriedObjectItem != null) {
                UnityEngine.Object.Destroy(mCarriedObjectItem);
            }
            if (HoloPreview != null) {
                UnityEngine.Object.Destroy(HoloPreview);
            }
            HoloPreview = null;
            mCarriedObjectItem = null;
            CarryDroneClamp = null;
            Thrust_Particles = null;
            drone.delete();
        }

        public override void UnityUpdate() {
            if (linkedToGo) {
                droneLogic(Time.deltaTime);
                return;
            }
            getGameObjects();
        }

        protected virtual void droneLogic(float timeJump) {
            if (drone.getPos() == mUnityDroneRestPos) {
                drone.faceTo(forwards);
            }
            if (mbCarriedCubeNeedsConfiguring) {
                mbCarriedCubeNeedsConfiguring = false;
                drone.giveItem(carriedItems.Count > 0 ? carriedItems[0] : null);
            }
            sendingDrone(timeJump);
            retrievingDrone(timeJump);
        }

        protected virtual void retrievingDrone(float timeJump) {
            if (headTo == null) {
                retrieveDrone(mUnityDroneRestPos, timeJump);
            }
        }

        protected virtual void sendingDrone(float timeJump) {
            if (carriedItems.Count > 0 && headTo != null) {

                getTargetCoords();

                if (targetCoords == Vector3.zero) {
                    return;
                }
                sendDrone(targetCoords, timeJump);
            }
        }

        protected virtual void getTargetCoords() {
            if (headTo != null && (targetCoords == null || targetCoords == Vector3.zero)) {
                targetCoords = drone.getUnityCoords(headTo.mnX, headTo.mnY, headTo.mnZ) + new Vector3(0.5f, 1.5f, 0.5f);
            }
        }

        protected virtual void getGameObjects() {
            if (linkedToGo || mWrapper == null || mWrapper.mGameObjectList == null || mWrapper.mGameObjectList[0].gameObject == null) return;
            CarryDrone = mWrapper.mGameObjectList[0].gameObject.transform.Search("CarryDrone").gameObject;
            CarryDrone.transform.localScale = droneSize;
            MeshRenderer mesh = CarryDrone.GetComponent<MeshRenderer>();
            mesh.material.SetColor("_Color", droneColor);
            CarryDroneClamp = mWrapper.mGameObjectList[0].gameObject.transform.Search("ClampPoint").gameObject;
            Thrust_Particles = mWrapper.mGameObjectList[0].gameObject.transform.Search("Thrust_Particles").gameObject;
            drone.setDrone(CarryDrone);
            drone.setThrust(Thrust_Particles);
            drone.setClamp(CarryDroneClamp);
            if (mUnityDroneRestPos == Vector3.zero) mUnityDroneRestPos = CarryDrone.transform.position;
            linkedToGo = true;
        }

        public override void LowFrequencyUpdate() {
            if (mcm == null) LookForAttachedModules();

            if (getStoredItemsCount() < maxItems) {
                VicisMod.log(getPrefix(), "Attempting to get item from surroundings");
                ItemBase item = this.TakeFromSurrounding();
                if (item != null) {
                    VicisMod.log(getPrefix(), "Got a " + item.GetDisplayString());
                    addItem(item);
                }
            }
            
            if (!linkedToGo) droneLogic(LowFrequencyThread.mrPreviousUpdateTimeStep);

            if (items.Count == 0 && carriedItems.Count == 0) return;

            if (mcm != null && items.Count > 0 && headTo == null) {
                if (items[0] == null) {
                    items.RemoveAt(0);
                    return;
                }
                VicisMod.log(getPrefix(), "LFU trying to find place to drop off " + items[0].GetDisplayString());
                headTo = mcm.manager.provideCrateDropoff(items[0], this, getDropOffSize());
                if (headTo != null) lastInteracted = headTo;
            }

            if (headTo != null && carriedItems.Count == 0) {
                float dist = (drone.getPos() - mUnityDroneRestPos).magnitude;
                VicisMod.log(getPrefix(), "LFU Attempting to give item to drone, drone is at " + drone.getPos() + ", dist = " + dist);
                if (dist <= 0.05f) {
                    if (AttemptGiveItem()) {
                        MarkDirtyDelayed();
                        return;
                    }
                }
            }

            if (headTo != null && carriedItems.Count > 0) {
                if(carriedItems[0] == null) {
                    carriedItems.RemoveAt(0);
                    return;
                }
                float dist = (drone.getPos() - targetCoords).magnitude;
                VicisMod.log(getPrefix(), "LFU drone is at " + drone.getPos() + ", dist = " + dist);
                if (dist <= 0.05f) {
                    VicisMod.log(getPrefix(), "LFU Finalising drop off");
                    if (!FinishGiveItem()) {
                        // Dammit, the crate was filled before the drone got there. Recollect, try again.
                        items.Insert(0, carriedItems[0]);
                        carriedItems.RemoveAt(0);
                        if (carriedItems.Count == 0) VicisMod.log(getPrefix(), "I'm still carrying something. What?");
                    } else {
                        MarkDirtyDelayed();
                    }
                }

            }

            if (headTo != null && carriedItems.Count == 0) {
                VicisMod.log(getPrefix(), "LFU resetting headTo and targetCoords");
                headTo = null;
                targetCoords = Vector3.zero;
            }

        }

        protected virtual bool addItem(ItemBase item) {
            if (item == null) return false;
            if (items == null) items = new List<ItemBase>();
            if ((getStoredItemsCount() + item.getAmount()) > maxItems) return false;
            if (item.isStack()) {
                for (int j = 0; j < items.Count; ++j) {
                    if(items[j] == null) {
                        items.RemoveAt(j);
                        --j;
                        continue;
                    }
                    // Check if we already have this type of stack
                    if (item.compareBaseDeep(items[j])) {
                        items[j].incrementStack(1);
                        MarkDirtyDelayed();
                        return true;
                    }
                }
            }
            items.Add(item);
            MarkDirtyDelayed();
            return true;
        }

        protected virtual int getDropOffSize() {
            if (items.Count == 0) return 0;
            return Math.Min(batch, Util.ItemBaseUtil.getAmount(items[0]));
        }

        protected virtual bool isConveyorFacingMe(ConveyorEntity conv) {
            long x = conv.mnX + (long)conv.mForwards.x;
            long y = conv.mnY + (long)conv.mForwards.y;
            long z = conv.mnZ + (long)conv.mForwards.z;
            return (x == mnX) &&
                (y == mnY) &&
                (z == mnZ);
        }


        public override void Read(BinaryReader reader, int entityVersion) {
            VicisMod.VicisModVersion version = (VicisMod.VicisModVersion)entityVersion;
            switch (version) {
                case VicisMod.VicisModVersion.Version1:
                case VicisMod.VicisModVersion.Version2:
                case VicisMod.VicisModVersion.Version3:
                    break;
                default:
                    mbHoloPreviewDirty = true;
                    try {
                        int numCarried = reader.ReadInt32();
                        for (int i = 0; i < numCarried; ++i) {
                            ItemBase item = ItemFile.DeserialiseItem(reader);
                            if (item == null) continue;
                            carriedItems.Add(item);
                        }
                        int numItems = reader.ReadInt32();
                        for (int i = 0; i < numItems; ++i) {
                            ItemBase item = ItemFile.DeserialiseItem(reader);
                            if (item == null) continue;
                            items.Add(item);
                        }
                    } catch (Exception e) {
                        // Damn, corruption. Nothing to do but move on
                    }

                    break;
            }
        }

        public override void Write(BinaryWriter writer) {
            writer.Write(carriedItems.Count);
            for (int i = 0; i < carriedItems.Count; ++i) {
                ItemFile.SerialiseItem(carriedItems[i], writer);
            }
            writer.Write(items.Count);
            for (int i = 0; i < items.Count; ++i) {
                ItemFile.SerialiseItem(items[i], writer);
            }
        }

        public override bool ShouldSave() {
            return true;
        }

        public override int GetVersion() {
            return (int)VicisMod.VicisModVersion.Version4;
        }

        public override string GetPopupText() {

            string ret = getFriendlyName() + "\nCurrently holding " + getStoredItemsCount() + " / " + maxItems + " items";
            ret += "\nDrone carrying " + getCarriedItemCount() + " / " + maxItems + " items";
            if (mcm == null) ret += "\nLooking for a module to connect to";
            else ret += "\nConnected to " + mcm.manager.modules.Count + " sized module group";
            ret += "\nDrone Speed: " + droneSpeed + ", Batch Size " + batch;
            return ret;
        }

        public int getStoredItemsCount() {
            if (items == null) items = new List<ItemBase>();
            return getItemCount(items);
        }

        public int getCarriedItemCount() {
            if (carriedItems == null) carriedItems = new List<ItemBase>();
            return getItemCount(carriedItems);
        }

        protected int getItemCount(List<ItemBase> items) {
            return Util.ItemBaseUtil.getItemCount(items);
        }

        public override void OnDelete() {
            for (int i = 0; i < items.Count; ++i) {
                if (items[i] == null) continue;
                ItemManager.instance.DropItem(items[i], mnX, mnY, mnZ, Vector3.zero);
            }
            for (int i = 0; i < carriedItems.Count; ++i) {
                if (carriedItems[i] == null) continue;
                ItemManager.instance.DropItem(carriedItems[i], mnX, mnY, mnZ, Vector3.zero);
            }
            items.Clear();
            carriedItems.Clear();
        }

        public bool HasItems() {
            return false;
        }

        public bool HasItem(ItemBase item) {
            return false;
        }

        public bool HasItems(ItemBase item, out int amount) {
            amount = 0;
            return false;
        }

        public bool HasFreeSpace(uint amount) {
            return GetFreeSpace() >= amount;
        }

        public int GetFreeSpace() {
            return maxItems - items.getItemCount();
        }

        public bool GiveItem(ItemBase item) {
            return addItem(item);
        }

        public ItemBase TakeItem(ItemBase item) {
            return null;
        }

        public ItemBase TakeAnyItem() {
            return null;
        }
    }

}
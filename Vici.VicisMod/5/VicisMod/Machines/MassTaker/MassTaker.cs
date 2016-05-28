using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VicisFCEMod.Mod;
using VicisFCEMod.Util;

namespace VicisFCEMod.Machines {
    public abstract class MassTaker : MachineEntity {

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

            ItemBase item = ItemBaseUtil.newInstance(items[0]);
            int count = getDropOffSize();
            if (ItemBaseUtil.isStack(items[0])) {
                // We have a stacked item, make sure to carry only what we need
                ItemBaseUtil.decrementStack(items[0], count);
                if (ItemBaseUtil.getAmount(items[0]) == 0) {
                    // Stack no longer exists, remove it.
                    // Don't need to set the amount on the item since it already had a =< batch size amount
                    items.RemoveAt(0);
                } else {
                    // Stack still exists, so we need to set the amount that we removed
                    ItemBaseUtil.setAmount(item, count);
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
                VicisMod.log(getPrefix(), "Retrieving drone");
                retrieveDrone(mUnityDroneRestPos, timeJump);
            }
        }

        protected virtual void sendingDrone(float timeJump) {
            if (carriedItems.Count > 0 && headTo != null) {
                VicisMod.log(getPrefix(), "Sending drone to drop off " + carriedItems[0].GetDisplayString() + " from location " +
                    VicisMod.getPosString(headTo.mnX, headTo.mnY, headTo.mnZ) + ", I'm at " + VicisMod.getPosString(mnX, mnY, mnZ) +
                    ", which is " + MassCrateModuleManager.calcDist(headTo, this) + "m away");

                getTargetCoords();

                VicisMod.log(getPrefix(), "Target coords are " + targetCoords + ", drone is at " + drone.getPos());

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

            if (mcm != null && items.Count > 0 && headTo == null) {
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

            if (!linkedToGo) droneLogic(LowFrequencyThread.mrPreviousUpdateTimeStep);

            if (getStoredItemsCount() >= maxItems) return;

            VicisMod.log(getPrefix(), "Looking for ConveyorEntities");

            pickUpFromConveyors();
        }

        protected virtual void pickUpFromConveyors() {
            bool ignore;
            List<ConveyorEntity> list = VicisMod.checkSurrounding<ConveyorEntity>(this, out ignore);
            VicisMod.log(getPrefix(), "Found " + list.Count + " ConveyorEntities");
            for (int i = 0; i < list.Count && getStoredItemsCount() < maxItems; ++i) {
                ConveyorEntity c = list[i];
                if (!isConveyorFacingMe(c)) continue;
                if (!c.mbReadyToConvey && c.mrCarryTimer <= 0.2f) {
                    ItemBase item = null;
                    if (c.mCarriedItem != null) {
                        item = c.mCarriedItem;
                    }
                    if (c.mCarriedCube != 0) {
                        item = ItemManager.SpawnCubeStack(c.mCarriedCube, c.mCarriedValue, 1);
                    }
                    // Wha... How?
                    if (item == null) continue;

                    // Check if I can add it to the items list
                    if (!addItem(item)) return;

                    c.mCarriedItem = null;
                    c.mCarriedCube = 0;
                    c.mCarriedValue = 0;
                    c.RemoveCube();
                    c.FinaliseOffloadingCargo();
                }
            }
        }

        protected virtual bool addItem(ItemBase item) {
            if (getStoredItemsCount() >= maxItems) return false;
            if (item.mType == ItemType.ItemCubeStack || item.mType == ItemType.ItemStack) {
                for (int j = 0; j < items.Count; ++j) {
                    // Check if we already have this type of stack
                    if (item.mnItemID == items[j].mnItemID && ItemBaseUtil.compareBaseDeep(item, items[j])) {
                        if (item.mType == ItemType.ItemCubeStack) ++(items[j] as ItemCubeStack).mnAmount;
                        else ++(items[j] as ItemStack).mnAmount;
                        return true;
                    }
                }
            }
            items.Add(item);
            return true;
        }

        protected virtual int getDropOffSize() {
            if (items.Count == 0) return 0;
            return Math.Min(batch, ItemBaseUtil.getAmount(items[0]));
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
                    int numCarried = reader.ReadInt32();
                    for (int i = 0; i < numCarried; ++i) {
                        carriedItems.Add(ItemFile.DeserialiseItem(reader));
                    }
                    int numItems = reader.ReadInt32();
                    for (int i = 0; i < numItems; ++i) {
                        items.Add(ItemFile.DeserialiseItem(reader));
                    }
                    break;
            }
        }

        public override void Write(BinaryWriter writer) {
            writer.Write(carriedItems.Count);
            foreach (ItemBase item in carriedItems) {
                ItemFile.SerialiseItem(item, writer);
            }
            writer.Write(items.Count);
            foreach (ItemBase item in items) {
                ItemFile.SerialiseItem(item, writer);
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
            return getItemCount(items);
        }

        public int getCarriedItemCount() {
            return getItemCount(carriedItems);
        }

        protected int getItemCount(List<ItemBase> items) {
            return ItemBaseUtil.getItemCount(items);
        }

        public override void OnDelete() {
            foreach (ItemBase it in items) {
                ItemManager.instance.DropItem(it, mnX, mnY, mnZ, Vector3.zero);
            }
            foreach (ItemBase it in carriedItems) {
                ItemManager.instance.DropItem(it, mnX, mnY, mnZ, Vector3.zero);
            }
        }
    }

}
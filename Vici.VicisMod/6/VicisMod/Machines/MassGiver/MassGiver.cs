using FortressCraft.Community;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VicisFCEMod.Mod;
using VicisFCEMod.Util;

namespace VicisFCEMod.Machines {
    public abstract class MassGiver : MachineEntity, CommunityItemInterface {

        protected static int id = 0;
        protected int myId;

        public const string CUBE_NAME = "Vici.MassGiver";

        public MassCrateModule mcm;

        protected ItemBase chosen;
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

        public MassGiver(ModCreateSegmentEntityParameters parameters) :
            base(eSegmentEntity.Mod,
                SpawnableObjectEnum.MassStorageOutputPort,
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
            LookForAttachedModules();
            droneSize = new Vector3(1, 1, 1);
        }


        protected virtual bool AttemptTakeItem() {
            if (getCarriedItemCount() > 0) return false;

            VicisMod.log(getPrefix(), "Attempting to get item " + chosen.GetDisplayString());
            ItemBase item = headTo.AttemptTakeItem(chosen, batch);
            if (item != null) {
                carriedItems.Add(item);
                mbCarriedCubeNeedsConfiguring = true;
                return true;
            }
            return false;
        }

        protected abstract void sendDrone(Vector3 coords, float timeJump);

        protected abstract void retrieveDrone(Vector3 coords, float timeJump);

        protected abstract string getPrefix();

        protected abstract string getFriendlyName();

        protected virtual bool FinishTakeItem() {
            if (carriedItems.Count == 0 || (getStoredItemsCount() != 0 && (getStoredItemsCount() + getCarriedItemCount()) > maxItems)) return false;

            for (int i = 0; i < carriedItems.Count; ++i) {
                items.Add(carriedItems[i]);
            }
            carriedItems.Clear();
            return true;
        }

        protected virtual void LookForAttachedModules() {
            VicisMod.log(getPrefix(), "Looking for storage");
            bool ignore;
            List<MassCrateModule> list = VicisMod.checkSurrounding<MassCrateModule>(this, out ignore);
            VicisMod.log(getPrefix(), "Found " + list.Count + " MassCrateModules");
            if (list.Count > 0) {
                mcm = list[0];
                mcm.giver = this;
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
                if (mbHoloPreviewDirty) {
                    renderHolo();
                }
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

        protected virtual void getGameObjects() {
            if (linkedToGo || mWrapper == null || mWrapper.mGameObjectList == null || mWrapper.mGameObjectList[0].gameObject == null) return;
            CarryDrone = mWrapper.mGameObjectList[0].gameObject.transform.Search("CarryDrone").gameObject;
            MeshRenderer mesh = CarryDrone.GetComponent<MeshRenderer>();
            CarryDrone.transform.localScale = droneSize;
            mesh.material.SetColor("_Color", droneColor);
            CarryDroneClamp = mWrapper.mGameObjectList[0].gameObject.transform.Search("ClampPoint").gameObject;
            Thrust_Particles = mWrapper.mGameObjectList[0].gameObject.transform.Search("Thrust_Particles").gameObject;
            drone.setDrone(CarryDrone);
            drone.setThrust(Thrust_Particles);
            drone.setClamp(CarryDroneClamp);
            HoloCubePreview = mWrapper.mGameObjectList[0].gameObject.transform.Search("HoloCube").gameObject;
            HoloCubePreview.SetActive(false);
            if (mUnityDroneRestPos == Vector3.zero) mUnityDroneRestPos = CarryDrone.transform.position;
            if (chosen != null) mbHoloPreviewDirty = true;
            linkedToGo = true;
        }

        protected virtual void renderHolo() {
            if (HoloPreview != null) {
                UnityEngine.Object.Destroy(HoloPreview);
            }
            if (chosen != null) {
                VicisMod.log(getPrefix(), "Creating new item holo for " + chosen.GetDisplayString() + " of type " + chosen.mType);
                if (chosen.mType == ItemType.ItemCubeStack) {
                    HoloCubePreview.SetActive(true);
                } else {
                    int @object = (int)ItemEntry.mEntries[chosen.mnItemID].Object;
                    GameObject original = SpawnableObjectManagerScript.instance.maSpawnableObjects[@object];
                    HoloPreview = (GameObject)UnityEngine.Object.Instantiate(original, mWrapper.mGameObjectList[0].gameObject.transform.position + new Vector3(0f, 1.5f, 0f), Quaternion.identity);
                    HoloPreview.transform.parent = mWrapper.mGameObjectList[0].gameObject.transform;
                    if (HoloPreview.GetComponent<Renderer>() != null) {
                        HoloPreview.GetComponent<Renderer>().material = PrefabHolder.instance.HoloPreviewMaterial;
                        HoloPreview.GetComponent<Renderer>().castShadows = false;
                        HoloPreview.GetComponent<Renderer>().receiveShadows = false;
                    }
                    HoloPreview.gameObject.AddComponent<RotateConstantlyScript>();
                    HoloPreview.gameObject.GetComponent<RotateConstantlyScript>().YRot = 1f;
                    HoloPreview.gameObject.GetComponent<RotateConstantlyScript>().XRot = 0.35f;
                    HoloPreview.SetActive(true);
                    HoloCubePreview.SetActive(false);
                }
            } else {
                HoloCubePreview.SetActive(false);
            }
            mbHoloPreviewDirty = false;
        }

        protected virtual void retrievingDrone(float timeJump) {
            if (headTo == null) {
                retrieveDrone(mUnityDroneRestPos, timeJump);
            }
        }

        protected virtual void sendingDrone(float timeJump) {
            if (carriedItems.Count == 0 && headTo != null && items.Count < maxItems) {

                getTargetCoords();

                if (targetCoords == Vector3.zero) {
                    return;
                }
                sendDrone(targetCoords, timeJump);
            }
        }

        protected virtual void getTargetCoords() {
            if (targetCoords == null || targetCoords == Vector3.zero) {
                targetCoords = drone.getUnityCoords(headTo.mnX, headTo.mnY, headTo.mnZ) + new Vector3(0.5f, 1.5f, 0.5f);
            }
        }

        public override void LowFrequencyUpdate() {
            if (mcm == null) LookForAttachedModules();

            if (chosen == null) return;

            if (mcm != null && chosen != null && headTo == null && carriedItems.Count == 0) {
                VicisMod.log(getPrefix(), "LFU Trying to find new target crate");
                headTo = mcm.manager.provideCratePickup(chosen, this, batch);
                if (headTo != null) lastInteracted = headTo;
            }

            if (headTo != null && carriedItems.Count == 0) {
                float dist = (drone.getPos() - targetCoords).magnitude;
                VicisMod.log(getPrefix(), "LFU Drone is at " + drone.getPos() + ", dist = " + dist);
                if (dist <= 0.05f) {
                    VicisMod.log(getPrefix(), "LFU Attempting to pick up item");
                    if (AttemptTakeItem()) {
                        MarkDirtyDelayed();
                        return;
                    } else if (getStoredItemsCount() == 0) {
                        // Well darn, someone picked this up before us. Better restart
                        headTo = null;
                        targetCoords = Vector3.zero;
                    }
                }
            }

            if (headTo != null && carriedItems.Count > 0) {
                VicisMod.log(getPrefix(), "LFU Carrying something, resetting headTo and targetCoords");
                headTo = null;
                targetCoords = Vector3.zero;
            }

            if (headTo == null && carriedItems.Count > 0) {
                float dist = (drone.getPos() - mUnityDroneRestPos).magnitude;
                VicisMod.log(getPrefix(), "LFU Drone is at " + drone.getPos() + ", dist = " + dist);
                if (dist <= 0.05f) {
                    VicisMod.log(getPrefix(), "LFU Attempting to drop off item");
                    FinishTakeItem();
                    MarkDirtyDelayed();
                }
            }

            if (!linkedToGo) droneLogic(LowFrequencyThread.mrPreviousUpdateTimeStep);

            if (items.Count == 0) {
                return;
            }
            
            if(getStoredItemsCount() > 0) {
                ItemBase item = TakeAnyItem();
                if (item != null) {
                    if (!this.GiveToSurrounding(item)) {
                        items.Insert(0, item);
                    }
                }
            }
        }

        protected virtual bool isConveyorNotFacingMe(ConveyorEntity conv) {
            long x = conv.mnX + (long)conv.mForwards.x;
            long y = conv.mnY + (long)conv.mForwards.y;
            long z = conv.mnZ + (long)conv.mForwards.z;
            return (x != mnX) ||
                (y != mnY) ||
                (z != mnZ);
        }

        public static ItemBase getCurrentHotBarItem() {
            if (SurvivalHotBarManager.instance == null) {
                VicisMod.log("MassGiver", "SurvivalHotBarManager.instance is null??");
                return null;
            }
            SurvivalHotBarManager.HotBarEntry currentHotBarEntry = SurvivalHotBarManager.instance.GetCurrentHotBarEntry();
            if (currentHotBarEntry == null) {
                return null;
            }
            if (currentHotBarEntry.state == SurvivalHotBarManager.HotBarEntryState.Empty) {
                return null;
            }
            if (currentHotBarEntry.cubeType != 0) {
                return ItemManager.SpawnCubeStack(currentHotBarEntry.cubeType, currentHotBarEntry.cubeValue, 1);
            }
            if (currentHotBarEntry.itemType >= 0) {
                return ItemManager.SpawnItem(currentHotBarEntry.itemType);
            }
            VicisMod.log("MassGiver", "No cube and no item in hotbar?");
            return null;
        }

        public override void Read(BinaryReader reader, int entityVersion) {
            VicisMod.VicisModVersion version = (VicisMod.VicisModVersion)entityVersion;
            switch (version) {
                case VicisMod.VicisModVersion.Version1:
                case VicisMod.VicisModVersion.Version2:
                case VicisMod.VicisModVersion.Version3:
                    break;
                default:
                    try {
                        chosen = ItemFile.DeserialiseItem(reader);
                        mbHoloPreviewDirty = true;
                        int numItems = reader.ReadInt32();
                        for (int i = 0; i < numItems; ++i) {
                            ItemBase item = ItemFile.DeserialiseItem(reader);
                            if (item == null) continue;
                            items.Add(item);
                        }
                        int numCarried = reader.ReadInt32();
                        for (int i = 0; i < numCarried; ++i) {
                            ItemBase item = ItemFile.DeserialiseItem(reader);
                            if (item == null) continue;
                            carriedItems.Add(item);
                        }
 
                    } catch (Exception e) {
                        // Damn, corruption. Need to clean up   
                    }

                    break;
            }
        }

        public override void Write(BinaryWriter writer) {
            ItemFile.SerialiseItem(chosen, writer);
            writer.Write(items.Count);
            for (int i = 0; i < items.Count; ++i) {
                if(items[i] == null) {
                    items.RemoveAt(0);
                    --i;
                    continue;
                }
                ItemFile.SerialiseItem(items[i], writer);
            }
            writer.Write(carriedItems.Count);
            for (int i = 0; i < carriedItems.Count; ++i) {
                if (carriedItems[i] == null) {
                    carriedItems.RemoveAt(0);
                    --i;
                    continue;
                }
                ItemFile.SerialiseItem(carriedItems[i], writer);
            }
        }

        public override bool ShouldSave() {
            return true;
        }

        public override int GetVersion() {
            return (int)VicisMod.VicisModVersion.Version4;
        }

        public override string GetPopupText() {

            if (Input.GetButton("Interact")) {
                if(Input.GetKey(KeyCode.LeftShift)) {
                    chosen = null;
                } else {
                    chosen = getCurrentHotBarItem();
                }
                if (carriedItems.Count == 0) headTo = null;
                mbHoloPreviewDirty = true;
                lastInteracted = null;
            }

            string ret = getFriendlyName() + "\nCurrently holding " + getStoredItemsCount() + " / " + maxItems + " items";
            ret += "\nDrone carrying " + getCarriedItemCount() + " / " + maxItems + " items";
            if (mcm == null) ret += "\nLooking for a module to connect to";
            else ret += "\nConnected to " + mcm.manager.modules.Count + " sized module group";
            ret += "\nDrone Speed: " + droneSpeed + ", Batch Size " + batch;

            if (chosen != null) ret += "\nLooking for " + chosen.GetDisplayString();
            else {
                ret += "\nInteract with me with an item in your hotbar for me to grab it";
            }

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

        protected virtual int getItemCount(List<ItemBase> items) {
            return Util.ItemBaseUtil.getItemCount(items);
        }

        public override void OnDelete() {
            for (int i = 0; i < items.Count; ++i) {
                if (items[i] == null) continue;
                ItemManager.instance.DropItem(items[i], mnX, mnY, mnZ, Vector3.zero);
            }
            for (int i = 0; i < carriedItems.Count; ++i) {
                if (items[i] == null) continue;
                ItemManager.instance.DropItem(carriedItems[i], mnX, mnY, mnZ, Vector3.zero);
            }
            items.Clear();
            carriedItems.Clear();
        }

        public bool HasItems() {
            return items.getItemCount() > 0;
        }

        public bool HasItem(ItemBase item) {
            for(int i = 0; i < items.Count; ++i) {
                if (item.compareBaseDeep(items[i])) return true;
            }
            return false;
        }

        public bool HasItems(ItemBase item, out int amount) {
            amount = 0;
            for(int i = 0; i < items.Count; ++i) {
                if(item.compareBase(items[i])) {
                    amount = items[i].getAmount();
                    return true;
                }
            }
            return false;
        }

        public bool HasFreeSpace(uint amount) {
            return GetFreeSpace() >= amount;
        }

        public int GetFreeSpace() {
            return maxItems - items.getItemCount();
        }

        public bool GiveItem(ItemBase item) {
            return false;
        }

        public ItemBase TakeItem(ItemBase item) {
            if (getStoredItemsCount() == 0) return null;
            for(int i = 0; i < items.Count; ++i) {
                if(item.compareBase(items[i])) {
                    ItemBase ret = Util.ItemBaseUtil.newInstance(items[i]);
                    ret.setAmount(1);
                    items[i].decrementStack(1);
                    if(items[i].getAmount() == 0 || !items[i].IsStack()) {
                        items.RemoveAt(i);
                    }
                    MarkDirtyDelayed();
                    return ret;
                }
            }
            return null;
        }

        public ItemBase TakeAnyItem() {
            if (getStoredItemsCount() == 0) return null;
            if (items[0] == null) { items.RemoveAt(0); return null; }
            ItemBase ret = Util.ItemBaseUtil.newInstance(items[0]);
            ret.setAmount(1);
            items[0].decrementStack(1);
            if (items[0].getAmount() == 0 || !items[0].IsStack()) {
                items.RemoveAt(0);
            }
            return ret;
        }
    }
}
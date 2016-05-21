using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public abstract class MassGiver : MachineEntity {

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
        //if ((batch + getStoredItemsCount()) > maxItems) return false;

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
        if (carriedItems.Count == 0 || (getStoredItemsCount() + getCarriedItemCount() > maxItems)) return false;

        foreach (ItemBase it in carriedItems) {
            items.Add(it);
        }
        carriedItems.Clear();
        return true;
    }

    protected virtual void LookForAttachedModules() {
        VicisMod.log(getPrefix(), "Looking for storage");
        bool ignore;
        List<MassCrateModule> list = VicisMod.checkSurrounding<MassCrateModule>(this, out ignore);
        VicisMod.log(getPrefix(), "Found " + list.Count + " MassCrateModules");
        if(list.Count > 0) {
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
        if(drone.getPos() == mUnityDroneRestPos) {
            drone.faceTo(forwards);
        }
        if(mbCarriedCubeNeedsConfiguring) {
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
        if(mUnityDroneRestPos == Vector3.zero) mUnityDroneRestPos = CarryDrone.transform.position;
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
            VicisMod.log(getPrefix(), "Retrieving drone");
            retrieveDrone(mUnityDroneRestPos, timeJump);
        }
    }

    protected virtual void sendingDrone(float timeJump) {
        if (carriedItems.Count == 0 && headTo != null && items.Count < maxItems) {
            VicisMod.log(getPrefix(), "Sending drone to pick up " + chosen.GetDisplayString() + " from location " + 
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
        if(targetCoords == null || targetCoords == Vector3.zero) {
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
        
        if(headTo != null && carriedItems.Count == 0) {
            float dist = (drone.getPos() - targetCoords).magnitude;
            VicisMod.log(getPrefix(), "LFU Drone is at " + drone.getPos() + ", dist = " + dist);
            if (dist <= 0.05f) {
                VicisMod.log(getPrefix(), "LFU Attempting to pick up item");
                if (AttemptTakeItem()) {
                    MarkDirtyDelayed();
                    return;
                } else if(getStoredItemsCount() == 0){
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

        if(headTo == null && carriedItems.Count > 0) {
            float dist = (drone.getPos() - mUnityDroneRestPos).magnitude;
            VicisMod.log(getPrefix(), "LFU Drone is at " + drone.getPos() + ", dist = " + dist);
            if (dist <= 0.05f) {
                VicisMod.log(getPrefix(), "LFU Attempting to drop off item");
                FinishTakeItem();
                MarkDirtyDelayed();
            }
        }

        if (!linkedToGo) droneLogic(LowFrequencyThread.mrPreviousUpdateTimeStep);

        if (items.Count == 0) return;
        dropOffToConveyors();
    }

    protected virtual void dropOffToConveyors() {
        bool ignore;
        List<ConveyorEntity> list = VicisMod.checkSurrounding<ConveyorEntity>(this, out ignore);
        VicisMod.log(getPrefix(), "Found " + list.Count + " ConveyorEntities");
        string msg = items.Count + ": ";
        foreach (ItemBase it in items) msg += it.GetDisplayString() + ", ";
        VicisMod.log(getPrefix(), "Currently storing " + msg);
        for (int i = 0; i < list.Count && items.Count > 0; ++i) {
            ConveyorEntity c = list[i] as ConveyorEntity;
            if (!isConveyorNotFacingMe(c)) {
                VicisMod.log(getPrefix(), "Conveyor is either not facing somewhere else: " + isConveyorNotFacingMe(c));
                continue;
            }
            if (c.mbReadyToConvey && c.mrLockTimer == 0f) {
                VicisMod.log(getPrefix(), "Ready to convey, will be giving " + items[0].GetDisplayString() + ", a " + items[0].mType);
                if (items[0].mType == ItemType.ItemCubeStack) {
                    ItemCubeStack a = items[0] as ItemCubeStack;
                    c.AddCube(a.mCubeType, a.mCubeValue, 1);
                    c.mItemForwards = forwards;
                    --a.mnAmount;
                    if (a.mnAmount == 0) {
                        VicisMod.log(getPrefix(), "Removing cube " + a.GetDisplayString() + " from items list");
                        items.RemoveAt(0);
                    }
                } else if (items[0].mType == ItemType.ItemStack) {
                    ItemStack a = ItemBaseUtil.newInstance(items[0]) as ItemStack;
                    a.mnAmount = 1;
                    ItemBaseUtil.decrementStack(items[0], 1);
                    c.AddItem(a);
                    c.mItemForwards = forwards;
                    if (ItemBaseUtil.getAmount(items[0]) == 0) {
                        VicisMod.log(getPrefix(), "Removing item " + a.GetDisplayString() + " from items list");
                        items.RemoveAt(0);
                    }
                } else {
                    c.AddItem(items[0]);
                    c.mrCarryTimer = 1f;
                    c.mrVisualCarryTimer = 1f;
                    c.mItemForwards = forwards;
                    items.RemoveAt(0);
                }
            } else {
                VicisMod.log(getPrefix(), "Conveyor is not ready to convey");
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
        switch(version) {
            case VicisMod.VicisModVersion.Version1:
            case VicisMod.VicisModVersion.Version2:
            case VicisMod.VicisModVersion.Version3:
                break;
            default:
                chosen = ItemFile.DeserialiseItem(reader);
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
        ItemFile.SerialiseItem(chosen, writer);
        writer.Write(carriedItems.Count);
        foreach(ItemBase item in carriedItems) {
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

        if (Input.GetButton("Interact")) {
            chosen = getCurrentHotBarItem();
            if (carriedItems.Count == 0) headTo = null;
            mbHoloPreviewDirty = true;
            lastInteracted = null;
        }

        string ret = getFriendlyName() + "\nCurrently holding " + getStoredItemsCount() + " / " + maxItems + " items";
        if (mcm == null) ret += "\nLooking for a module to connect to";
        else ret += "\nConnected to " + mcm.manager.modules.Count + " sized module group";
        ret += "\nDrone Speed: " + droneSpeed + ", Batch Size " + batch;

        if (chosen != null) ret += "\nLooking for " + chosen.GetDisplayString();

        return ret;
    }

    public int getStoredItemsCount() {
        return getItemCount(items);
    }

    public int getCarriedItemCount() {
        return getItemCount(carriedItems);
    }

    protected virtual int getItemCount(List<ItemBase> items) {
        return ItemBaseUtil.getItemCount(items);
    }

    public override void OnDelete() {
        foreach (ItemBase it in items) {
            ItemManager.instance.DropItem(it, mnX, mnY, mnZ, Vector3.zero);
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public abstract class PowWow : MachineEntity, PowerConsumerInterface {

    public const string CUBE_NAME = "Vici.PowWow";

    public const int ROLLING_AVG_LENGTH = 10;

    public float transferFrequency = 0.5f;
    public float scanFrequency = 5f;
    public float maxPower;
    public float maxTransfer = float.MaxValue;
    public float currentPower;
    public float totalTransfered;
    public SegmentEntity powerSource;

    public float lastSearch;
    public float lastTransfer1;
    public float lastTransfer2;
    public int radius;
    public float transferCap;
    public Color cubeColor;
    public GameObject gameObject;

    public float[] transfers = new float[ROLLING_AVG_LENGTH];
    public float[] transfersTime = new float[ROLLING_AVG_LENGTH];
    public int lifo = 0;
    private float sumTransfers = 0;
    private float sumTimes = 0;

    private bool linkedToGo;

    public List<PowerConsumerInterface> pcis = new List<PowerConsumerInterface>();

    public PowWow(ModCreateSegmentEntityParameters parameters) :
        base(eSegmentEntity.Mod, 
            SpawnableObjectEnum.ExperimentalAssembler, 
            parameters.X, 
            parameters.Y, 
            parameters.Z, 
            parameters.Cube, 
            parameters.Flags, 
            parameters.Value, 
            parameters.Position, 
            parameters.Segment) {

        currentPower = 0;
        totalTransfered = 0;
        radius = 3;
        transferCap = 10000;
        maxPower = 10000;
        for(int i = 0; i < ROLLING_AVG_LENGTH; ++i) {
            transfers[i] = 0;
        }

        // Trigger the searching of PCIs
        lastSearch = 0;
        lastTransfer1 = 0;
        lastTransfer2 = 0;
        powerSource = null;
        linkedToGo = false;
        mbNeedsLowFrequencyUpdate = true;
        mbNeedsUnityUpdate = true;
        this.cubeColor = Color.white;
        VicisMod.log(getPrefix(), "Created");
    }

    public abstract string getPrefix();

    public override void UnityUpdate() {
        if (!linkedToGo) {
            if(this.mWrapper != null && this.mWrapper.mGameObjectList != null) {
                this.gameObject = this.mWrapper.mGameObjectList[0].gameObject;
                MeshRenderer[] components = gameObject.GetComponentsInChildren<MeshRenderer>();
                if (components != null && components.Length > 0) {
                    foreach(MeshRenderer mesh in components) {
                        mesh.material.SetColor("_Color", cubeColor);
                    }
                    VicisMod.log(getPrefix(), "Successfully added cubeColor to " + components.Length + "MeshRenderers");
                } else {
                    VicisMod.log(getPrefix(), "Could not establish MeshRenderers for gameobject!");
                }
                linkedToGo = true;
            }
        }
    }

    public override void UnitySuspended() {
        this.gameObject = null;
    }

    public override void DropGameObject() {
        base.DropGameObject();
        linkedToGo = false;
    }

    public override void LowFrequencyUpdate() {
        recordStats(attemptTransferPower());
    }

    protected float attemptTransferPower() { 
        VicisMod.log(getPrefix(), "Running LFU");
        lastSearch -= LowFrequencyThread.mrPreviousUpdateTimeStep;
        // Nothing to do
        if (currentPower <= 0) return 0;
        lastTransfer1 -= LowFrequencyThread.mrPreviousUpdateTimeStep;
        if (lastTransfer1 > 0) return 0;
        lastTransfer1 += transferFrequency;
        VicisMod.log(getPrefix(), "Currently have " + currentPower + " Power");
        cullPCIs();
        if(lastSearch <= 0) {
            VicisMod.log(getPrefix(), "Finding PCIs");
            findPCIs();
            lastSearch = scanFrequency;
        }

        VicisMod.log(getPrefix(), "Aware of " + pcis.Count + " PCIs");
        // Now lets charge these guys up!
        float transferedThisTime = 0;
        for(int i = 0; i < pcis.Count; ++i) {
            PowerConsumerInterface pci = pcis[i];
            if(pci.WantsPowerFromEntity(this)) {
                float transfer = transferCap;
                transfer = Math.Min(transfer, currentPower);
                transfer = Math.Min(transfer, pci.GetMaximumDeliveryRate());
                transfer = Math.Min(transfer, pci.GetRemainingPowerCapacity());
                if(transfer > 0 && pci.DeliverPower(transfer)) {
                    VicisMod.log(getPrefix(), "Adding " + transfer + " to PCI " + pci.ToString());
                    currentPower -= transfer;
                    totalTransfered += transfer;
                    transferedThisTime += transfer;
                    // And we're done
                    if (currentPower == 0) return transferedThisTime;
                }
            }
        }
        return transferedThisTime;
    }

    protected void recordStats(float transfered) {
        lastTransfer2 += LowFrequencyThread.mrPreviousUpdateTimeStep;
        if (transfered == 0 && lastTransfer2 < transferFrequency) return;
        string msg = "current counts = [";
        for (int i = 0; i < ROLLING_AVG_LENGTH; ++i) {
            if (i > 0) msg += ", ";
            msg += transfers[i];
        }
        msg += "], Lifo = " + lifo + ", currentSum = " + sumTransfers + ", transfered now = " + transfered;
        VicisMod.log(getPrefix() + ".recordStats", msg);
        sumTransfers -= transfers[lifo];
        sumTimes -= transfersTime[lifo];
        transfersTime[lifo] = lastTransfer2;
        lastTransfer2 = 0;
        sumTimes += transfersTime[lifo];
        transfers[lifo] = transfered;
        sumTransfers += transfers[lifo++];
        lifo %= ROLLING_AVG_LENGTH;
    }

    protected void findPCIs() {
        for (int i = -1 * radius; i <= radius; ++i) {
            for (int j = -1 * radius; j <= radius; ++j) {
                for (int k = -1 * radius; k <= radius; ++k) {
                    // Skip if we're looking at ourselves
                    if (i == 0 && j == 0 && k == 0) continue;
                    // Skip if the point is out of our radius
                    if (i * i + j * j + k * k > radius * radius) continue;
                    // Check the segment
                    long x = this.mnX + i;
                    long y = this.mnY + j;
                    long z = this.mnZ + k;
                    Segment segment = base.AttemptGetSegment(x, y, z);
                    // Check if segment was generated (skip this point if it doesn't
                    if (segment == null) continue;
                    ushort cube = segment.GetCube(x, y, z);
                    // If this isn't an entity, skip it
                    if (!CubeHelper.HasEntity((int)cube)) continue;
                    PowerConsumerInterface pci = segment.SearchEntity(x, y, z) as PowerConsumerInterface;
                    if (pci == null) continue;
                    // Don't feed power to T4 or T5 batteries
                    if (pci is T4_Battery || pci is T5_Battery) continue;
                    // Also, don't feed the power source (infinite loop of power, like a laser pointed at it's PSB)
                    if (pci == powerSource) continue;
                    // Let's only keep track of PCIs that will accept power from the PowWow
                    if (!pci.WantsPowerFromEntity(this)) continue;
                    for(int l = pcis.Count - 1; l >= 0 && pci != null; --l) {
                        PowerConsumerInterface pci2 = pcis[l];
                        if(pci2 != null && !(pci2 as SegmentEntity).mbDelete && pci2 == pci) {
                            pci = null;
                        }
                    }
                    if (pci != null) pcis.Add(pci);
                }
            }
        }
    }

    protected void cullPCIs() {
        for(int i = pcis.Count - 1; i >= 0; --i) {
            PowerConsumerInterface pci = pcis[i];
            if(pci == null || (pci as SegmentEntity).mbDelete || pci == powerSource) {
                pcis.RemoveAt(i);
            }
        }
    }

    public override bool ShouldSave() {
        return true;
    }

    public override int GetVersion() {
        return (int)VicisMod.CURRENT_VERSION;
    }

    public override void WriteNetworkUpdate(System.IO.BinaryWriter writer) {
        this.Write(writer);
    }

    public override void Write(BinaryWriter writer) {
        writer.Write(this.currentPower);
        float value = 0f;
        writer.Write(value);
        writer.Write(value);
        writer.Write(value);
        writer.Write(value);
        writer.Write(value);
        writer.Write(value);
        writer.Write(value);
    }

    public override void ReadNetworkUpdate(System.IO.BinaryReader reader) {
        this.Read(reader, (int)VicisMod.CURRENT_VERSION);
    }

    public override void Read(BinaryReader reader, int entityVersion) {
        VicisMod.VicisModVersion version = (VicisMod.VicisModVersion)entityVersion;
        switch(version) {
            case VicisMod.VicisModVersion.Version1:
                // No serialization happened here
                break;
            // REMEMBER TO ADD CASE STATEMENTS FOR OLD VERSIONS
            default:
                this.currentPower = reader.ReadSingle();
                reader.ReadSingle();
                reader.ReadSingle();
                reader.ReadSingle();
                reader.ReadSingle();
                reader.ReadSingle();
                reader.ReadSingle();
                reader.ReadSingle();
                if (this.currentPower > this.maxPower) this.currentPower = this.maxPower;
                break;
        }
    }

    public bool DeliverPower(float amount) {
        VicisMod.log(getPrefix(), "receiving power of " + amount + " amount");
        if(amount > GetRemainingPowerCapacity()) {
            VicisMod.log(getPrefix(), "Rejected");
            return false;
        }
        currentPower += amount;
        VicisMod.log(getPrefix(), "Now have " + currentPower + " power");
        return true;
    }

    public float GetMaximumDeliveryRate() {
        return maxTransfer;
    }

    public float GetMaxPower() {
        return maxPower;
    }

    public float GetRemainingPowerCapacity() {
        return maxPower - currentPower;
    }

    public bool WantsPowerFromEntity(SegmentEntity entity) {
        // Completely assuming if the entity is asking, it wants to be the power source
        powerSource = entity;
        return true;
    }

    public override string GetPopupText() {
        return "Consumers : " + pcis.Count + "\nPower : " + currentPower + "\nAvg PPS : " + sumTransfers / sumTimes + "\nTotal : " + totalTransfered;
    }

    public override HoloMachineEntity CreateHolobaseEntity(Holobase holobase) {
        HolobaseEntityCreationParameters hecp = new HolobaseEntityCreationParameters(this);
        HolobaseVisualisationParameters hvp = hecp.AddVisualisation(holobase.mPreviewCube);
        hvp.Color = cubeColor;
        return holobase.CreateHolobaseEntity(hecp);
    }
}

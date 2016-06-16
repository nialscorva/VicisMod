using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VicisFCEMod.Mod;

namespace VicisFCEMod.Machines {
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
            for (int i = 0; i < ROLLING_AVG_LENGTH; ++i) {
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
            cubeColor = Color.white;
            VicisMod.log(getPrefix(), "Created");
        }

        public abstract string getPrefix();

        public override void UnityUpdate() {
            if (!linkedToGo) {
                if (mWrapper != null && mWrapper.mGameObjectList != null) {
                    gameObject = mWrapper.mGameObjectList[0].gameObject;
                    MeshRenderer[] components = gameObject.GetComponentsInChildren<MeshRenderer>();
                    if (components != null && components.Length > 0) {
                        foreach (MeshRenderer mesh in components) {
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
            gameObject = null;
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
            if (currentPower <= 0 || transferCap <= 0) return 0;
            lastTransfer1 -= LowFrequencyThread.mrPreviousUpdateTimeStep;
            if (lastTransfer1 > 0) return 0;
            lastTransfer1 += transferFrequency;
            if (lastTransfer1 <= 0) lastTransfer1 = transferFrequency;
            VicisMod.log(getPrefix(), "Currently have " + currentPower + " Power");
            cullPCIs();
            if (lastSearch <= 0) {
                VicisMod.log(getPrefix(), "Finding PCIs");
                findPCIs();
                lastSearch = scanFrequency;
            }

            float transferedThisTime = attemptGivePlayerPower();
            if (currentPower == 0) return transferedThisTime;

            VicisMod.log(getPrefix(), "Aware of " + pcis.Count + " PCIs");
            // Now lets charge these guys up!
            for (int i = 0; i < pcis.Count; ++i) {
                PowerConsumerInterface pci = pcis[i];
                if (pci.WantsPowerFromEntity(this)) {
                    float transfer = Math.Min(transferCap, currentPower);
                    transfer = Math.Min(transfer, pci.GetMaximumDeliveryRate());
                    transfer = Math.Min(transfer, pci.GetRemainingPowerCapacity());
                    if (transfer > 0 && pci.DeliverPower(transfer)) {
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

        protected float attemptGivePlayerPower() {
            float playerDist = getPlayerDistance();
            VicisMod.log(getPrefix(), "Is " + playerDist + "m away from player and my radius is " + radius);
            // A bit of a hack - the update player distance function above sometimes doesn't work, I suspect it's because the entity isn't loaded yet.
            // Check for 0 exactly since there's no way to be 0 distance from an object
            if (playerDist > radius || playerDist == 0) return 0;
            float transfer = Math.Min(transferCap, currentPower);
            float totalTransfered = 0;
            // Try and give the player some power
            if (SurvivalPowerPanel.mrSuitFreeCapacity > 0) {
                float transfer1 = Math.Min(transfer, SurvivalPowerPanel.mrSuitFreeCapacity);
                VicisMod.log(getPrefix(), "giving player " + transfer1 + " power");
                SurvivalPowerPanel.GivePower(transfer1);
                currentPower -= transfer1;
                totalTransfered += transfer1;
                transfer = Math.Min(transferCap, currentPower);
            }

            // Try and give ARTHER some power
            ARTHERPetSurvival pet = ARTHERPetSurvival.instance;
            if (pet.mrCurrentPower < pet.mrMaxPower) {
                float transfer1 = Math.Min(transfer, pet.mrMaxPower - pet.mrCurrentPower);
                VicisMod.log(getPrefix(), "Giving ARTHER " + transfer1 + " power, it currently has " + pet.mrCurrentPower + " power");
                pet.mrCurrentPower += transfer1;
                currentPower -= transfer1;
                totalTransfered += transfer1;
            }
            VicisMod.log(getPrefix(), "Gave " + totalTransfered + " power");
            return totalTransfered;
        }

        protected float getPlayerDistance() {
            if (!GameState.GameStarted) {
                return 0;
            }
            if (!GameState.PlayerSpawned) {
                return 0;
            }
            if (WorldScript.mLocalPlayer == null) {
                return 0;
            }
            long mnWorldX = WorldScript.mLocalPlayer.mnWorldX;
            long mnWorldY = WorldScript.mLocalPlayer.mnWorldY;
            long mnWorldZ = WorldScript.mLocalPlayer.mnWorldZ;
            Vector3 vec = new Vector3((float)(mnX - mnWorldX), (float)(mnY - mnWorldY), (float)(mnZ - mnWorldZ));
            return vec.magnitude;
        }

        protected void recordStats(float transfered) {
            lastTransfer2 += LowFrequencyThread.mrPreviousUpdateTimeStep;
            if (transfered == 0 && lastTransfer2 < transferFrequency) return;
            if (transfered > 0) MarkDirtyDelayed();
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
                        long x = mnX + i;
                        long y = mnY + j;
                        long z = mnZ + k;
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
                        for (int l = pcis.Count - 1; l >= 0 && pci != null; --l) {
                            PowerConsumerInterface pci2 = pcis[l];
                            if (pci2 != null && !(pci2 as SegmentEntity).mbDelete && pci2 == pci) {
                                pci = null;
                            }
                        }
                        if (pci != null) pcis.Add(pci);
                    }
                }
            }
        }

        protected void cullPCIs() {
            for (int i = pcis.Count - 1; i >= 0; --i) {
                PowerConsumerInterface pci = pcis[i];
                if (pci == null || (pci as SegmentEntity).mbDelete || pci == powerSource) {
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
            Write(writer);
        }

        public override void Write(BinaryWriter writer) {
            writer.Write(currentPower);
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
            Read(reader, (int)VicisMod.VicisModVersion.Version2);
        }

        public override void Read(BinaryReader reader, int entityVersion) {
            VicisMod.VicisModVersion version = (VicisMod.VicisModVersion)entityVersion;
            switch (version) {
                case VicisMod.VicisModVersion.Version1:
                    // No serialization happened here
                    break;
                // REMEMBER TO ADD CASE STATEMENTS FOR OLD VERSIONS
                default:
                    currentPower = reader.ReadSingle();
                    reader.ReadSingle();
                    reader.ReadSingle();
                    reader.ReadSingle();
                    reader.ReadSingle();
                    reader.ReadSingle();
                    reader.ReadSingle();
                    reader.ReadSingle();
                    if (currentPower > maxPower) currentPower = maxPower;
                    break;
            }
        }

        public bool DeliverPower(float amount) {
            VicisMod.log(getPrefix(), "receiving power of " + amount + " amount");
            if (amount > GetRemainingPowerCapacity()) {
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
            if (currentPower < maxPower) {
                // Completely assuming if the entity is asking, it wants to be the power source
                powerSource = entity;
                return true;
            }
            return false;
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
}
using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class CompactSolar : SolarPanel {

    public const string CUBE_NAME = "Vici.CompactSolar";

    protected float multiplier;
    protected float value;
    protected bool mbLinkedToGo = false;
    protected float lastScan;
    protected float scanFrequency = 5.0f;
    private GameObject[] maPanels;
    protected Color panelColor;
    protected List<PowerConsumerInterface> mk4AndMk5Bats = new List<PowerConsumerInterface>();

    public CompactSolar(ModCreateSegmentEntityParameters parameters) :
        base(parameters.Segment,
            parameters.X,
            parameters.Y,
            parameters.Z,
            parameters.Cube,
            parameters.Flags,
            0 /* normal solar panel */) {
        // might be useful sometime
        value = parameters.Value;
        mrTransferRate = float.MaxValue; // POWER OF THE SUN
        multiplier = 1;
        lastScan = 0;
        panelColor = Color.black;
    }

    public abstract string getPrefix();

    public override void LowFrequencyUpdate() {
        base.LowFrequencyUpdate();

        cullBats();
        lastScan -= LowFrequencyThread.mrPreviousUpdateTimeStep;

        // Did we didn't collect anything, don't try to collect anymore
        if (!mbCollecting) return;

        if (lastScan <= 0) {
            findMk4AndMk5Bats();
            lastScan = scanFrequency;
        }

        // Get the last amount power generated
        float newGenerated = mrLastPowerGain * LowFrequencyThread.mrPreviousUpdateTimeStep;
        VicisMod.log(getPrefix(), "Already collected " + newGenerated + " power");

        // Generate however more power we need
        newGenerated *= multiplier;
        // Make sure we have the space for it
        newGenerated = Math.Min(newGenerated, mrSparePowerCapacity);
        VicisMod.log(getPrefix(), "Now generated " + newGenerated + " power");
        // Add it together
        if(newGenerated > 0) {
            mrLastPowerGain += newGenerated / LowFrequencyThread.mrPreviousUpdateTimeStep;
            mrCurrentPower += newGenerated;
            mrNormalisedPower = mrCurrentPower / mrMaxPower;
            mrRemainingCapacity = mrMaxPower - mrCurrentPower;
            mrSparePowerCapacity = mrMaxPower - mrCurrentPower;
            VicisMod.log(getPrefix(), "Current Power now " + mrCurrentPower + " with " + mrRemainingCapacity + " capacity left");
            GameManager.PowerGenerated(newGenerated);
            GameManager.mrTotalSolarPower += newGenerated;
            if (PlayerStats.mbCreated) {
                PlayerStats.instance.AddPowerToStats(newGenerated);
            }
        }

        // If we still have power, attempt to give all of it to adjacent T4 and T5 batteries
        if(mrCurrentPower > 0) {
            attemptGiveMorePower();
        }
    }

    protected void attemptGiveMorePower() {
        VicisMod.log(getPrefix(), "Attempting to give more power to " + mk4AndMk5Bats.Count + " batteries with " + mrCurrentPower + " power left");
        foreach(PowerConsumerInterface pci in mk4AndMk5Bats) {
            if(pci.WantsPowerFromEntity(this)) {
                // We purposely ignore the max delivery rate for T4 and T5 batteries
                float transfer = Math.Min(mrCurrentPower, pci.GetRemainingPowerCapacity());
                VicisMod.log(getPrefix(), "Giving " + transfer + " power");
                if(transfer > 0 && pci.DeliverPower(transfer)) {
                    VicisMod.log(getPrefix(), "Success in giving " + transfer + " power");
                    mrCurrentPower -= transfer;
                    mrNormalisedPower = mrCurrentPower / mrMaxPower;
                    mrRemainingCapacity = mrMaxPower - mrCurrentPower;
                    mrSparePowerCapacity = mrMaxPower - mrCurrentPower;
                    MarkDirtyDelayed();
                }
            }
            if (mrCurrentPower <= 0) return;
        }
    }

    protected void findMk4AndMk5Bats() {
        VicisMod.log(getPrefix(), "Looking for T4 and T5 batteries");
        long[] coords = new long[3];
        for (int i = 0; i < 3; ++i) {
            for(int j = -1; j <= 1; j+=2) {
                Array.Clear(coords, 0, 3);
                coords[i] = j;

                long x = mnX + coords[0];
                long y = mnY + coords[1];
                long z = mnZ + coords[2];

                Segment segment = base.AttemptGetSegment(x, y, z);
                // Check if segment was generated (skip this point if it doesn't
                if (segment == null) continue;
                ushort cube = segment.GetCube(x, y, z);
                // If this isn't an entity, skip it
                if (!CubeHelper.HasEntity((int)cube)) continue;
                PowerConsumerInterface pci = segment.SearchEntity(x, y, z) as PowerConsumerInterface;
                if (pci == null) continue;
                // We're only looking for T4 and T5 batteries
                if (!(pci is T4_Battery) && !(pci is T5_Battery)) continue;
                VicisMod.log(getPrefix(), "Found one: " + pci.ToString());
                // Let's only keep track of PCIs that will accept power from the PowWow
                if (!pci.WantsPowerFromEntity(this)) continue;
                for (int l = mk4AndMk5Bats.Count - 1; l >= 0 && pci != null; --l) {
                    PowerConsumerInterface pci2 = mk4AndMk5Bats[l];
                    if (pci2 != null && !(pci2 as SegmentEntity).mbDelete && pci2 == pci) {
                        pci = null;
                    }
                }
                VicisMod.log(getPrefix(), "End, checking " + pci);
                if (pci != null) mk4AndMk5Bats.Add(pci);
            }
        }
    }

    protected void cullBats() {
        for (int i = mk4AndMk5Bats.Count - 1; i >= 0; --i) {
            PowerConsumerInterface pci = mk4AndMk5Bats[i];
            if (pci == null || (pci as SegmentEntity).mbDelete) {
                mk4AndMk5Bats.RemoveAt(i);
            }
        }
    }

    public override void DropGameObject() {
        base.DropGameObject();
        mbLinkedToGo = false;
    }

    public override void UnitySuspended() {
        maPanels = null;
    }

    public override void UnityUpdate() {
        base.UnityUpdate();
        if (!mbLinkedToGo) {
            if (mWrapper == null || !mWrapper.mbHasGameObject) {
                return;
            }
            if (mWrapper.mGameObjectList == null) {
                Debug.LogError("Solar missing game object #0?");
            }
            if (mWrapper.mGameObjectList[0].gameObject == null) {
                Debug.LogError("Solar missing game object #0 (GO)?");
            }
            CPH_SolarPanel[] componentsInChildren = mWrapper.mGameObjectList[0].gameObject.GetComponentsInChildren<CPH_SolarPanel>();
            maPanels = new GameObject[componentsInChildren.Length];
            for (int i = 0; i < componentsInChildren.Length; i++) {
                maPanels[i] = componentsInChildren[i].gameObject;
                MeshRenderer mesh = maPanels[i].GetComponentInChildren<MeshRenderer>();
                mesh.material.SetColor("_Color", panelColor);
            }

            mbLinkedToGo = true;
        }
    }

    public override string GetPopupText() {
        return "Collecting at rate of " +
                (SurvivalWeatherManager.mrSunAngle * 100f).ToString("F0") +
                "%\nCurrent Rate is " +
                mrLastPowerGain.ToString("F1") +
                "pps.";
    }
}

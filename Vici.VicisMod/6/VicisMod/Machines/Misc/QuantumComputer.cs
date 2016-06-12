using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using VicisFCEMod.Mod;

namespace VicisFCEMod.Machines {
    class QuantumComputer : MachineEntity, PowerConsumerInterface {

        public const string CUBE_NAME = "Vici.QuantumComputer";

        protected float time = 60; // seconds per iteration
        protected float currentTime = 0;
        protected int probability = 60; // 1 / probability each iteration to succeed
        protected int payout = 1; // research points
        protected float maxPower = 1000;
        protected float pps = 100;
        protected float currentPower = 0;
        protected static System.Random rand = new System.Random();

        protected Color cubeColor = Color.black;
        protected bool linkedToGo = false;
        protected GameObject go = null;
        protected int numTries = 0;
        protected int numSuccesses = 0;

        public QuantumComputer(ModCreateSegmentEntityParameters parameters) :
            base(eSegmentEntity.Mod,
                SpawnableObjectEnum.ResearchStation,
                parameters.X,
                parameters.Y,
                parameters.Z,
                parameters.Cube,
                parameters.Flags,
                parameters.Value,
                parameters.Position,
                parameters.Segment) {

            currentTime = time;
            mbNeedsLowFrequencyUpdate = true;
            mbNeedsUnityUpdate = true;
        }

        public override void DropGameObject() {
            base.DropGameObject();
            linkedToGo = false;
        }

        public override void UnitySuspended() {
             go = null;
        }

        public override void UnityUpdate() {
            if (!linkedToGo) {
                if (mWrapper == null || !mWrapper.mbHasGameObject) {
                    return;
                }
                if (mWrapper.mGameObjectList == null) {
                    Debug.LogError("QuantumComputer missing game object #0?");
                }
                if (mWrapper.mGameObjectList[0].gameObject == null) {
                    Debug.LogError("QuantumComputer missing game object #0 (GO)?");
                }
                go = mWrapper.mGameObjectList[0].gameObject;
                MeshRenderer[] meshes = go.GetComponentsInChildren<MeshRenderer>();
                for(int i = 0; i < meshes.Length; ++i) {
                    meshes[i].material.SetColor("_Color", cubeColor);
                }

                linkedToGo = true;
            }
        }

        public override void LowFrequencyUpdate() {
            if (currentPower < pps * LowFrequencyThread.mrPreviousUpdateTimeStep) return;
            currentPower -= pps * LowFrequencyThread.mrPreviousUpdateTimeStep;
            currentTime -= LowFrequencyThread.mrPreviousUpdateTimeStep;
            if(currentTime <= 0) {
                ++numTries;
                currentTime = time;
                int temp = rand.Next(0, probability);
                UnityEngine.Debug.Log("QuantumComputer: rand = " + temp + ", probability = " + probability + ", % = " + (temp % probability));
                if(temp == 0) {
                    ++numSuccesses;
                    WorldScript.mLocalPlayer.mResearch.GiveResearchPoints(payout);

                }
            }
        }

        public float GetMaxPower() {
            return maxPower;
        }

        public float GetRemainingPowerCapacity() {
            return maxPower - currentPower;
        }

        public float GetMaximumDeliveryRate() {
            return float.MaxValue;
        }

        public bool DeliverPower(float amount) {
            if(amount > GetRemainingPowerCapacity()) {
                return false;
            }
            currentPower += amount;
            return true;
        }

        public bool WantsPowerFromEntity(SegmentEntity entity) {
            return true;
        }

        public override string GetPopupText() {
            string ret = "Quantum Computer\nPower: " + currentPower + " / " + maxPower;
            if(currentPower == 0) {
                ret += "\nRequesting " + pps + " pps";
            }
            ret += "\nProgress: " + ((time - currentTime) / time) * 100 + "% Done";
            ret += "\nProbability Success each attempt: " + 1 / (float)probability * 100 + "%";
            ret += "\nResearch Point Payout: " + payout;
            ret += "\nSuccess / Tries: " + numSuccesses + " / " + numTries;
            return ret;
        }

        public override void Write(BinaryWriter writer) {
            writer.Write(currentPower);
            writer.Write(currentTime);
            writer.Write(numTries);
            writer.Write(numSuccesses);
        }

        public override void Read(BinaryReader reader, int entityVersion) {
            VicisMod.VicisModVersion version = (VicisMod.VicisModVersion)entityVersion;
            switch(version) {
                case VicisMod.VicisModVersion.Version1:
                case VicisMod.VicisModVersion.Version2:
                case VicisMod.VicisModVersion.Version3:
                case VicisMod.VicisModVersion.Version4:
                case VicisMod.VicisModVersion.Version5:
                default:
                    currentPower = reader.ReadSingle();
                    currentTime = reader.ReadSingle();
                    numTries = reader.ReadInt32();
                    numSuccesses = reader.ReadInt32();
                    break;
            }
        }

        public override int GetVersion() {
            return (int)VicisMod.VicisModVersion.Version6;
        }

        public override bool ShouldSave() {
            return true;
        }
    }
}

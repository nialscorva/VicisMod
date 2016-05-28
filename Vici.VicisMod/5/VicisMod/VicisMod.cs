using System;
using System.Collections.Generic;
using VicisFCEMod.Machines;

namespace VicisFCEMod.Mod {
    public class VicisMod : FortressCraftMod {

        public enum VicisModVersion {
            Version1,
            Version2,
            Version3,
            Version4
        }

        public const VicisModVersion CURRENT_VERSION = VicisModVersion.Version4;

        private const string LOGGER_PREFIX = "Vici.VicisMod";

        private ushort powWowCube;
        private ushort powWowMk1Val;
        private ushort powWowMk2Val;
        private ushort powWowMk3Val;
        private ushort powWowMk4Val;

        private ushort compactSolarCube;
        private ushort compactSolarMk1Val;
        private ushort compactSolarMk2Val;
        private ushort compactSolarMk3Val;

        private ushort massGiverCube;
        private ushort massGiverVanillaValue;
        private ushort massGiverMk1Value;
        private ushort massGiverMk2Value;
        private ushort massGiverMk3Value;
        private ushort massGiverMk4Value;
        private ushort massGiverMk5Value;

        private ushort massCrateModuleCube;
        private ushort massCrateModuleVanillaValue;
        private ushort massCrateModuleLinkerValue;
        private ushort massCrateModule100Value;
        private ushort massCrateModule200Value;
        private ushort massCrateModule500Value;
        private ushort massCrateModule1000Value;
        private ushort massCrateModule10000Value;
        private ushort massCrateModulePoweredMk1Value;

        private ushort massTakerCube;
        private ushort massTakerVanillaValue;
        private ushort massTakerMk1Value;
        private ushort massTakerMk2Value;
        private ushort massTakerMk3Value;
        private ushort massTakerMk4Value;
        private ushort massTakerMk5Value;

        private const bool DEBUG = true;

        public static void log(String prefix, String msg) {
            if (DEBUG) {
                UnityEngine.Debug.Log(prefix + ": " + msg);
            }
        }

        public override ModRegistrationData Register() {
            ModRegistrationData mrd = new ModRegistrationData();
            mrd.RegisterEntityHandler(PowWowMk1.VALUE_NAME);
            mrd.RegisterEntityHandler(PowWowMk2.VALUE_NAME);
            mrd.RegisterEntityHandler(PowWowMk3.VALUE_NAME);
            mrd.RegisterEntityHandler(PowWowMk4.VALUE_NAME);

            mrd.RegisterEntityHandler(CompactSolarMk1.VALUE_NAME);
            mrd.RegisterEntityHandler(CompactSolarMk2.VALUE_NAME);
            mrd.RegisterEntityHandler(CompactSolarMk3.VALUE_NAME);

            mrd.RegisterEntityHandler(MassGiverVanilla.VALUE_NAME);
            mrd.RegisterEntityHandler(MassGiverMk1.VALUE_NAME);
            mrd.RegisterEntityHandler(MassGiverMk2.VALUE_NAME);
            mrd.RegisterEntityHandler(MassGiverMk3.VALUE_NAME);
            mrd.RegisterEntityHandler(MassGiverMk4.VALUE_NAME);
            mrd.RegisterEntityHandler(MassGiverMk5.VALUE_NAME);

            mrd.RegisterEntityHandler(MassCrateModuleVanilla.VALUE_NAME);
            mrd.RegisterEntityHandler(MassCrateModuleLinker.VALUE_NAME);
            mrd.RegisterEntityHandler(MassCrateModule100.VALUE_NAME);
            mrd.RegisterEntityHandler(MassCrateModule200.VALUE_NAME);
            mrd.RegisterEntityHandler(MassCrateModule500.VALUE_NAME);
            mrd.RegisterEntityHandler(MassCrateModule1000.VALUE_NAME);
            mrd.RegisterEntityHandler(MassCrateModule10000.VALUE_NAME);
            mrd.RegisterEntityHandler(MassCrateModulePoweredMk1.VALUE_NAME);

            mrd.RegisterEntityHandler(MassTakerVanilla.VALUE_NAME);
            mrd.RegisterEntityHandler(MassTakerMk1.VALUE_NAME);
            mrd.RegisterEntityHandler(MassTakerMk2.VALUE_NAME);
            mrd.RegisterEntityHandler(MassTakerMk3.VALUE_NAME);
            mrd.RegisterEntityHandler(MassTakerMk4.VALUE_NAME);
            mrd.RegisterEntityHandler(MassTakerMk5.VALUE_NAME);

            powWowCube = getCubeValue(PowWow.CUBE_NAME);
            powWowMk1Val = getValueValue(PowWowMk1.VALUE_NAME);
            powWowMk2Val = getValueValue(PowWowMk2.VALUE_NAME);
            powWowMk3Val = getValueValue(PowWowMk3.VALUE_NAME);
            powWowMk4Val = getValueValue(PowWowMk4.VALUE_NAME);

            compactSolarCube = getCubeValue(CompactSolar.CUBE_NAME);
            compactSolarMk1Val = getValueValue(CompactSolarMk1.VALUE_NAME);
            compactSolarMk2Val = getValueValue(CompactSolarMk2.VALUE_NAME);
            compactSolarMk3Val = getValueValue(CompactSolarMk3.VALUE_NAME);

            massGiverCube = getCubeValue(MassGiver.CUBE_NAME);
            massGiverVanillaValue = getValueValue(MassGiverVanilla.VALUE_NAME);
            massGiverMk1Value = getValueValue(MassGiverMk1.VALUE_NAME);
            massGiverMk2Value = getValueValue(MassGiverMk2.VALUE_NAME);
            massGiverMk3Value = getValueValue(MassGiverMk3.VALUE_NAME);
            massGiverMk4Value = getValueValue(MassGiverMk4.VALUE_NAME);
            massGiverMk5Value = getValueValue(MassGiverMk5.VALUE_NAME);

            massCrateModuleCube = getCubeValue(MassCrateModule.CUBE_NAME);
            massCrateModuleVanillaValue = getValueValue(MassCrateModuleVanilla.VALUE_NAME);
            massCrateModuleLinkerValue = getValueValue(MassCrateModuleLinker.VALUE_NAME);
            massCrateModule100Value = getValueValue(MassCrateModule100.VALUE_NAME);
            massCrateModule200Value = getValueValue(MassCrateModule200.VALUE_NAME);
            massCrateModule500Value = getValueValue(MassCrateModule500.VALUE_NAME);
            massCrateModule1000Value = getValueValue(MassCrateModule1000.VALUE_NAME);
            massCrateModule10000Value = getValueValue(MassCrateModule10000.VALUE_NAME);
            massCrateModulePoweredMk1Value = getValueValue(MassCrateModulePoweredMk1.VALUE_NAME);

            massTakerCube = getCubeValue(MassTaker.CUBE_NAME);
            massTakerVanillaValue = getValueValue(MassTakerVanilla.VALUE_NAME);
            massTakerMk1Value = getValueValue(MassTakerMk1.VALUE_NAME);
            massTakerMk2Value = getValueValue(MassTakerMk2.VALUE_NAME);
            massTakerMk3Value = getValueValue(MassTakerMk3.VALUE_NAME);
            massTakerMk4Value = getValueValue(MassTakerMk4.VALUE_NAME);
            massTakerMk5Value = getValueValue(MassTakerMk5.VALUE_NAME);

            log(LOGGER_PREFIX, "Registered mod");

            return mrd;
        }

        public ushort getCubeValue(string cubeName) {
            log(LOGGER_PREFIX, "CubeName = " + cubeName);
            TerrainDataEntry terrainDataEntry;
            TerrainDataValueEntry terrainDataValueEntry;
            TerrainData.GetCubeByKey(cubeName, out terrainDataEntry, out terrainDataValueEntry);
            if (terrainDataEntry != null) {
                return terrainDataEntry.CubeType;
            }
            return 0;
        }

        public ushort getValueValue(string valueName) {
            log(LOGGER_PREFIX, "ValueName = " + valueName);
            TerrainDataEntry terrainDataEntry;
            TerrainDataValueEntry terrainDataValueEntry;
            TerrainData.GetCubeByKey(valueName, out terrainDataEntry, out terrainDataValueEntry);
            log(LOGGER_PREFIX, "Done getting value value");
            if (terrainDataValueEntry != null) {
                return terrainDataValueEntry.Value;
            }
            return 0;
        }

        public static List<T> checkSurrounding<T>(MachineEntity center, out bool encounteredNullSegment) where T : SegmentEntity {
            List<T> ret = new List<T>();
            long[] coords = new long[3];
            encounteredNullSegment = false;
            for (int i = 0; i < 3; ++i) {
                for (int j = -1; j <= 1; j += 2) {
                    Array.Clear(coords, 0, 3);
                    coords[i] = j;

                    long x = center.mnX + coords[0];
                    long y = center.mnY + coords[1];
                    long z = center.mnZ + coords[2];

                    Segment segment = center.AttemptGetSegment(x, y, z);
                    // Check if segment was generated (skip this point if it doesn't
                    if (segment == null) {
                        encounteredNullSegment = true;
                        continue;
                    }
                    T tmcm = segment.SearchEntity(x, y, z) as T;
                    if (tmcm != null && tmcm is T) {
                        ret.Add((T)tmcm);
                    }
                }
            }

            return ret;
        }

        public static string getPosString(long x, long y, long z) {
            return "[" + x + ", " + y + ", " + z + "]";
        }

        public override ModCreateSegmentEntityResults CreateSegmentEntity(ModCreateSegmentEntityParameters parameters) {
            log(LOGGER_PREFIX, "creating with params: " +
                "X = " + parameters.X + ", " +
                "Y = " + parameters.Y + ", " +
                "Z = " + parameters.Z + ", " +
                "Cube = " + parameters.Cube + ", " +
                "Value = " + parameters.Value + ", " +
                "Segment = " + parameters.Segment + ", " +
                "Type = " + parameters.Type + ", " +
                "Flags = " + parameters.Flags + ", " +
                "toString = \"" + parameters.ToString() + "\""
                );
            ModCreateSegmentEntityResults res = new ModCreateSegmentEntityResults();

            if (parameters.Cube == powWowCube) {
                if (parameters.Value == powWowMk1Val) res.Entity = new PowWowMk1(parameters);
                if (parameters.Value == powWowMk2Val) res.Entity = new PowWowMk2(parameters);
                if (parameters.Value == powWowMk3Val) res.Entity = new PowWowMk3(parameters);
                if (parameters.Value == powWowMk4Val) res.Entity = new PowWowMk4(parameters);
            }

            if (parameters.Cube == compactSolarCube) {
                if (parameters.Value == compactSolarMk1Val) res.Entity = new CompactSolarMk1(parameters);
                if (parameters.Value == compactSolarMk2Val) res.Entity = new CompactSolarMk2(parameters);
                if (parameters.Value == compactSolarMk3Val) res.Entity = new CompactSolarMk3(parameters);
            }

            if (parameters.Cube == massGiverCube) {
                if (parameters.Value == massGiverVanillaValue) res.Entity = new MassGiverVanilla(parameters);
                if (parameters.Value == massGiverMk1Value) res.Entity = new MassGiverMk1(parameters);
                if (parameters.Value == massGiverMk2Value) res.Entity = new MassGiverMk2(parameters);
                if (parameters.Value == massGiverMk3Value) res.Entity = new MassGiverMk3(parameters);
                if (parameters.Value == massGiverMk4Value) res.Entity = new MassGiverMk4(parameters);
                if (parameters.Value == massGiverMk5Value) res.Entity = new MassGiverMk5(parameters);
            }

            if (parameters.Cube == massCrateModuleCube) {
                if (parameters.Value == massCrateModuleVanillaValue) res.Entity = new MassCrateModuleVanilla(parameters);
                if (parameters.Value == massCrateModuleLinkerValue) res.Entity = new MassCrateModuleLinker(parameters);
                if (parameters.Value == massCrateModule100Value) res.Entity = new MassCrateModule100(parameters);
                if (parameters.Value == massCrateModule200Value) res.Entity = new MassCrateModule200(parameters);
                if (parameters.Value == massCrateModule500Value) res.Entity = new MassCrateModule500(parameters);
                if (parameters.Value == massCrateModule1000Value) res.Entity = new MassCrateModule1000(parameters);
                if (parameters.Value == massCrateModule10000Value) res.Entity = new MassCrateModule10000(parameters);
                if (parameters.Value == massCrateModulePoweredMk1Value) res.Entity = new MassCrateModulePoweredMk1(parameters);
            }

            if (parameters.Cube == massTakerCube) {
                if (parameters.Value == massTakerVanillaValue) res.Entity = new MassTakerVanilla(parameters);
                if (parameters.Value == massTakerMk1Value) res.Entity = new MassTakerMk1(parameters);
                if (parameters.Value == massTakerMk2Value) res.Entity = new MassTakerMk2(parameters);
                if (parameters.Value == massTakerMk3Value) res.Entity = new MassTakerMk3(parameters);
                if (parameters.Value == massTakerMk4Value) res.Entity = new MassTakerMk4(parameters);
                if (parameters.Value == massTakerMk5Value) res.Entity = new MassTakerMk5(parameters);
            }

            return res;
        }
    }
}
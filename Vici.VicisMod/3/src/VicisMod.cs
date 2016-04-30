using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class VicisMod : FortressCraftMod {

    public enum VicisModVersion {
        Version1,
        Version2
    }

    public const VicisModVersion CURRENT_VERSION = VicisModVersion.Version2;

    private String LOGGER_PREFIX = "Vici.VicisMod";

    private ushort powWowCube;
    private ushort powWowMk1Val;
    private ushort powWowMk2Val;
    private ushort powWowMk3Val;
    private ushort powWowMk4Val;

    private ushort compactSolarCube;
    private ushort compactSolarMk1Val;
    private ushort compactSolarMk2Val;
    private ushort compactSolarMk3Val;

    private const bool DEBUG = false;

    public static void log(String prefix, String msg) {
        if(DEBUG) {
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

        powWowCube = getCubeValue(PowWow.CUBE_NAME);
        powWowMk1Val = getValueValue(PowWowMk1.VALUE_NAME);
        powWowMk2Val = getValueValue(PowWowMk2.VALUE_NAME);
        powWowMk3Val = getValueValue(PowWowMk3.VALUE_NAME);
        powWowMk4Val = getValueValue(PowWowMk4.VALUE_NAME);

        compactSolarCube = getCubeValue(CompactSolar.CUBE_NAME);
        compactSolarMk1Val = getValueValue(CompactSolarMk1.VALUE_NAME);
        compactSolarMk2Val = getValueValue(CompactSolarMk2.VALUE_NAME);
        compactSolarMk3Val = getValueValue(CompactSolarMk3.VALUE_NAME);

        log(LOGGER_PREFIX, "Registered mod");

        return mrd;
    }

    private ushort getCubeValue(string cubeName) {
        TerrainDataEntry terrainDataEntry;
        TerrainDataValueEntry terrainDataValueEntry;
        TerrainData.GetCubeByKey(cubeName, out terrainDataEntry, out terrainDataValueEntry);
        if (terrainDataEntry != null) {
            return terrainDataEntry.CubeType;
        }
        // -1
        return ushort.MaxValue;
    }

    private ushort getValueValue(string valueName) {
        TerrainDataEntry terrainDataEntry;
        TerrainDataValueEntry terrainDataValueEntry;
        TerrainData.GetCubeByKey(valueName, out terrainDataEntry, out terrainDataValueEntry);
        if (terrainDataEntry != null) {
            return terrainDataValueEntry.Value;
        }
        // -1
        return ushort.MaxValue;
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

        return res;
    }
}

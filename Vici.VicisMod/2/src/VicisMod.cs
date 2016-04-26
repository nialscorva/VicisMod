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

        TerrainDataEntry terrainDataEntry;
        TerrainDataValueEntry terrainDataValueEntry;
        TerrainData.GetCubeByKey(PowWow.CUBE_NAME, out terrainDataEntry, out terrainDataValueEntry);
        if (terrainDataEntry != null) powWowCube = terrainDataEntry.CubeType;
        TerrainData.GetCubeByKey(PowWowMk1.VALUE_NAME, out terrainDataEntry, out terrainDataValueEntry);
        if (terrainDataValueEntry != null) powWowMk1Val = terrainDataValueEntry.Value;
        TerrainData.GetCubeByKey(PowWowMk2.VALUE_NAME, out terrainDataEntry, out terrainDataValueEntry);
        if (terrainDataValueEntry != null) powWowMk2Val = terrainDataValueEntry.Value;
        TerrainData.GetCubeByKey(PowWowMk3.VALUE_NAME, out terrainDataEntry, out terrainDataValueEntry);
        if (terrainDataValueEntry != null) powWowMk3Val = terrainDataValueEntry.Value;
        TerrainData.GetCubeByKey(PowWowMk4.VALUE_NAME, out terrainDataEntry, out terrainDataValueEntry);
        if (terrainDataValueEntry != null) powWowMk4Val = terrainDataValueEntry.Value;

        log(LOGGER_PREFIX, "Registered mod");

        return mrd;
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

        return res;
    }
}

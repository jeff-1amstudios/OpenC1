using System.Runtime.InteropServices;
[StructLayout(LayoutKind.Sequential)]
public struct VehicleDimensions
{
    public float VehicleMass;
    public float InverseWheelMass;
    public float Length;
    public float Width;
    public float Heigth;
    public float dFront;
    public float dBack;
    public float dTop;
    public float WheelWidth;
    public float FW_Radius;
    public float BW_Radius;
    public float FW_dPosFront;
    public float FW_dPosY;
    public float FW_dPosSide;
    public float BW_dPosBack;
    public float BW_dPosSide;
}

[StructLayout(LayoutKind.Sequential)]
public struct SuspensionSettings
{
    public float WheelSuspension;
    public float SpringRestitution;
    public float SpringDamping;
    public float SpringBias;
}

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct PhysXGroupIDs
{
    private const byte HEIGHTFIELD_GROUP_ID = 0;
    private const byte VEHICLE_GROUP_ID = 1;
    private const byte WHEELs_GROUP_ID = 2;
    private const byte ROCKET_GROUP_ID = 3;
    private const byte ITEM_GROUP_ID = 4;
    public byte HeightfieldGroupID
    {
        get
        {
            return 0;
        }
    }
    public byte VehicleGroupID
    {
        get
        {
            return 1;
        }
    }
    public byte WheelsGroupID
    {
        get
        {
            return 2;
        }
    }
    public byte RocketGroupID
    {
        get
        {
            return 3;
        }
    }
    public byte ItemGroupID
    {
        get
        {
            return 4;
        }
    }
}

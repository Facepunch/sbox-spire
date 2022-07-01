namespace Spire;

public partial class Utils
{
    public static Vector3 PlaneIntersectionWithZ( Vector3 pos, Vector3 dir, float z )
    {
        float a = (z - pos.z) / dir.z;
        return new( dir.x * a + pos.x, dir.y * a + pos.y, z );
    }
}

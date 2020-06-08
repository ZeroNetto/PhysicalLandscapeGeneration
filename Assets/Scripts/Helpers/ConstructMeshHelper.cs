namespace Helpers
{
    public class ConstructMeshHelper
    {
        public static bool IsInside(int mapSize, int x, int y) => x != mapSize - 1 && y != mapSize - 1;
        
        public static void ConstructTriangles(int x, int y, int mapSize, int meshMapIndex, int[] triangles)
        {
            var shift = (y * (mapSize - 1) + x) * 3 * 2;

            triangles[shift + 0] = meshMapIndex + mapSize;
            triangles[shift + 1] = meshMapIndex + mapSize + 1;
            triangles[shift + 2] = meshMapIndex;

            triangles[shift + 3] = meshMapIndex + mapSize + 1;
            triangles[shift + 4] = meshMapIndex + 1;
            triangles[shift + 5] = meshMapIndex;
        }
    }
}
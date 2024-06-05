using UnityEngine;

public class generateGrid : MonoBehaviour
{
    public GameObject instantiatingObject;

    public int WorldSizex = 10;

    public int WorldSizeZ = 10;

    public int GridOffset = 500;

    private void Start()
    {
        for(int x = -15; x < WorldSizex; x++)
        {
            for(int z = -15; z < WorldSizeZ; z++)
            {
                Vector3 pos = new Vector3(x * GridOffset, 0, z * GridOffset);
                GameObject block = Instantiate(instantiatingObject, pos, Quaternion.identity);
                block.transform.Rotate(0, 0, 0);
                block.transform.SetParent(this.transform);
            }
        }
        Vector3 position = new Vector3(7500, 0, 7500);
        this.transform.position = position;
    }
}

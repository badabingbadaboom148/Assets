using UnityEngine;

[System.Serializable]
public class GunArrays
{
    public GT2.navalGunController[] Secondaries;
    public GT2.navalGunController[] Main;
    public static GameObject controllerTargetSecondaries; // Target for Secondaries
    public static GameObject controllerTargetMain; // Target for Main
}

public class MasterGunController : MonoBehaviour
{
    public GunArrays[] navalGunControllersArray; // Array to store arrays of naval gun controllers
    public GameObject shellType;
    public bool has100mm;
    public bool has250mm;
    public bool has400mm;
    public GameObject Target;

    // SetTargets function to assign targets to naval gun controllers
    public void SetTargets(GameObject target)
    {
        if (target != null)
        {
            Target = target;
            for (int i = 0; i < navalGunControllersArray.Length; i++)
            {
                // Set the target for Secondaries
                for (int j = 0; j < navalGunControllersArray[i].Secondaries.Length; j++)
                {
                    navalGunControllersArray[i].Secondaries[j].target = target;
                    GunArrays.controllerTargetSecondaries = target;
                }

                // Set the target for Main
                for (int k = 0; k < navalGunControllersArray[i].Main.Length; k++)
                {
                    navalGunControllersArray[i].Main[k].target = target;
                    GunArrays.controllerTargetMain = target;
                }
            }
        }
        else
        {
            CeaseFire();
        }
    }

    public void CeaseFire()
    {
        Target = null;
        for (int i = 0; i < navalGunControllersArray.Length; i++)
        {
            // Set the target for Secondaries
            for (int j = 0; j < navalGunControllersArray[i].Secondaries.Length; j++)
            {
                navalGunControllersArray[i].Secondaries[j].target = null;
            }

            // Set the target for Main
            for (int k = 0; k < navalGunControllersArray[i].Main.Length; k++)
            {
                navalGunControllersArray[i].Main[k].target = null;
                Debug.Log("check 2");
            }
        }
    }
}

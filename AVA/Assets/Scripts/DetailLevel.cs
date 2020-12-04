using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum e_detailLevel
{
    SimpleDetail,
    HighDetail
}
public class DetailLevel : MonoBehaviour
{
    public GameObject vehicleMesh;
    public e_detailLevel detailLevel;

    private DetailLevelSet detailLevelSet;

    // Update is called once per frame
    void Start()
    {
        detailLevelSet = vehicleMesh.GetComponent<DetailLevelSet>();
    }

    private void OnGUI()
    {
        detailLevelSet.UpdateDetailLevel(detailLevel);
    }
}

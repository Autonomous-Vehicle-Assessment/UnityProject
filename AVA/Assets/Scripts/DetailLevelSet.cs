using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetailLevelSet : MonoBehaviour
{
    [SerializeField]
    private MeshRenderer chassisMeshSimple;
    
    [SerializeField]
    private MeshRenderer chassisMeshDetail;

    public void UpdateDetailLevel(e_detailLevel detailLevel)
    {
        switch (detailLevel)
        {
            case e_detailLevel.SimpleDetail:
                chassisMeshSimple.enabled = true;
                chassisMeshDetail.enabled = false;
                break;
            case e_detailLevel.HighDetail:
                chassisMeshSimple.enabled = false;
                chassisMeshDetail.enabled = true;
                break;
        }
    }
}

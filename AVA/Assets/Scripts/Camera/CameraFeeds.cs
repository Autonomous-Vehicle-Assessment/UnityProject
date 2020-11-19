using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraFeeds : MonoBehaviour
{
    private List<CameraFeedGenerator> cameraFeedsGenerators;
    private GameObject uiElement;
    private GameObject[] vehicles;
    private RawImage cameraFeed;
    private Text cameraFeedText;
    [Range(1,4)]
    public int vehicleSelect;

    void Awake()
    {
        vehicles = GameObject.FindGameObjectsWithTag("Vehicle");

        cameraFeedsGenerators = new List<CameraFeedGenerator>();
        int index = 0;
        
        foreach (GameObject vehicle in vehicles)
        {
            cameraFeedsGenerators.Add(vehicle.GetComponentInChildren<CameraFeedGenerator>());
            vehicle.GetComponentInChildren<CameraFeedGenerator>().id = index;
            index++;
        }

        uiElement = GameObject.FindGameObjectWithTag("UICameraFeed");
        cameraFeed = uiElement.transform.Find("Camera Image").GetComponent<RawImage>();
        cameraFeedText = uiElement.transform.Find("Information").GetComponent<Text>();
    }

    void OnGUI()
    {
        cameraFeed.texture = cameraFeedsGenerators[vehicleSelect - 1].cameraTexture;
        
        if(cameraFeedsGenerators[vehicleSelect - 1].recordFeed)
        {
            cameraFeedText.text = vehicles[vehicleSelect - 1].name + " - Recording";
        }
        else
        {
            cameraFeedText.text = vehicles[vehicleSelect - 1].name;
        }
        
    }
}

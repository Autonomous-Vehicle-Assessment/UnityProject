using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class WindowGraph : MonoBehaviour
{
    [SerializeField] private Sprite circleSprite;
    private RectTransform graphContainer;
    private int padding = 20;

    void Awake()
    {
        graphContainer = transform.Find("graphContainer").GetComponent<RectTransform>();

        //List<int> valueList = new List<int>() { 5, 25, 45, 98, 105, 113, 112, 80, 56, 45, 30, 22, 17, 15, 13, 17, 25, 7, 40, 36, 33 };
        //ShowGraph(valueList);
    }

    private GameObject CreateCircle(Vector2 anchoredPosition)
    {
        GameObject gameObject = new GameObject("circle", typeof(Image));
        gameObject.transform.SetParent(graphContainer, false);
        gameObject.GetComponent<Image>().sprite = circleSprite;
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = new Vector2(5, 5);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);

        return gameObject;
    }


    public void UpdateValue()
    {

    }
    public void ShowGraph(List<int> valueList)
    {
        float graphHeight = graphContainer.rect.height - padding * 2;
        float graphWidth = graphContainer.rect.width - padding * 2;
        float yMaximum = valueList.Max();
        float xSize = graphWidth/(valueList.Count-1);
        GameObject[] CircleObjects = new GameObject[valueList.Count];
        GameObject lastCircleGameObject = null;
        for (int i = 0; i < valueList.Count; i++)
        {
            float xPosition = i * xSize + padding;
            float yPosition = (valueList[i] / yMaximum) * graphHeight + padding;
            CircleObjects[i] = CreateCircle(new Vector2(xPosition, yPosition));
            if (lastCircleGameObject != null)
            {
                CreateDotConnection(lastCircleGameObject.GetComponent<RectTransform>().anchoredPosition, CircleObjects[i].GetComponent<RectTransform>().anchoredPosition);
            }
            lastCircleGameObject = CircleObjects[i];
        }

    }

    private void CreateDotConnection(Vector2 PositionA, Vector2 PositionB)
    {
        GameObject gameObject = new GameObject("dotConnection", typeof(Image));
        gameObject.transform.SetParent(graphContainer, false);
        gameObject.GetComponent<Image>().color = new Color(1, 1, 1, .5f);
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        Vector2 dir = (PositionB - PositionA).normalized;
        float distance = Vector2.Distance(PositionA, PositionB);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        rectTransform.sizeDelta = new Vector2(distance, 1.5f);
        rectTransform.anchoredPosition = PositionA + dir * distance * .5f;
        rectTransform.localEulerAngles = new Vector3(0, 0, GenericFunctions.GetVectorAngle(dir));
    }

    //private void CreateBoundingBox(int padding)
    //{
    //    GameObject gameObject = new GameObject("dotConnection", typeof(Image));
    //    gameObject.transform.SetParent(graphContainer, false);
    //    gameObject.GetComponent<Image>().color = new Color(1, 1, 1, .5f);
    //    RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
    //    Vector2 dir = (PositionB - PositionA).normalized;
    //    float distance = Vector2.Distance(PositionA, PositionB);
    //    rectTransform.anchorMin = new Vector2(0, 0);
    //    rectTransform.anchorMax = new Vector2(0, 0);
    //    rectTransform.sizeDelta = new Vector2(distance, 1.5f);
    //    rectTransform.anchoredPosition = PositionA + dir * distance * .5f;
    //    rectTransform.localEulerAngles = new Vector3(0, 0, GenericFunctions.GetVectorAngle(dir));
    //}
}

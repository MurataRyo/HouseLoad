using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearTask : MonoBehaviour
{
    bool nextStage;
    Vector2 next = new Vector2(-340f, 0f);
    Vector2 end = new Vector2(-340, -270f);
    RectTransform point;
    // Start is called before the first frame update
    void Awake()
    {
        GameTask.actionNum++;
        nextStage = false;
        foreach (Transform child in transform)
        {
            if (child.gameObject.tag == "Point")
            {
                point = child.gameObject.GetComponent<RectTransform>();
            }
        }
        PontPosUpdate();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void PontPosUpdate()
    {
        point.position = (nextStage ? next : end) + new Vector2(Screen.width / 2, Screen.height / 2);
    }
}

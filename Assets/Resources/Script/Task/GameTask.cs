using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class GameTask : MonoBehaviour
{
    enum Mode
    {
        main,
        pause
    }
    Mode mode;

    enum SelectNow
    {
        restart,
        title,
        back
    }
    SelectNow selectNow;

    public static int[][] mapData;
    public static List<MapObject> mapObjects;
    public static int actionNum;    //アクション数
    [SerializeField] GameObject pause;
    [SerializeField] RectTransform pointer;

    private void Awake()
    {
        mode = Mode.main;
        actionNum = 0;
        mapObjects = new List<MapObject>();


        TextAsset text = new TextAsset();
        text = Resources.Load(GetPath.NormalMap + "/001_Stage", typeof(TextAsset)) as TextAsset;

        Utility.MapLoad(text.text, out mapData, out PlayerTask.playerPos, out mapObjects);
    }

    private void Update()
    {
        switch(mode)
        {
            case Mode.main:
                if(Input.GetKeyDown(KeyCode.Escape))
                {
                    mode = Mode.pause;
                    Time.timeScale = 0f;
                    actionNum++;
                    pause.SetActive(true);
                    selectNow = 0;
                    PontPosUpdate();
                }
                break;

            case Mode.pause:
                Change();
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    actionNum--;
                    Time.timeScale = 1f;
                    pause.SetActive(false);
                    mode = Mode.main;
                }
                else if (Input.GetKeyDown(KeyCode.Return))
                {
                    actionNum--;
                    Time.timeScale = 1f;
                    pause.SetActive(false);
                    if (selectNow == 0)
                    {
                        LoadTask.NextScene(LoadTask.SceneName.Main);
                    }
                    else if ((int)selectNow == 1)
                    {
                        LoadTask.NextScene(LoadTask.SceneName.Title);
                    }
                    else
                    {
                        mode = Mode.main;
                    }
                }
                break;
        }
    }


    void PontPosUpdate()
    {
        pointer.position = ((int)selectNow * new Vector2(0f, -230f)) + new Vector2(Screen.width / 3, Screen.height / 2 + 80f);
    }

    public static bool PosInMapObject(Vector2Int position, out MapObject mObject)
    {
        mObject = null;
        for (int i = 0; i < mapObjects.Count; i++)
        {
            if (position == IntPos(mapObjects[i].pos))
            {
                mObject = mapObjects[i];
                return true;
            }
        }
        return false;
    }

    public static Vector2Int IntPos(Vector2 vec2)
    {
        return new Vector2Int(Mathf.RoundToInt(vec2.x), Mathf.RoundToInt(vec2.y));
    }

    private void Change()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if ((int)selectNow < Enum.GetValues(typeof(SelectNow)).Length - 1)
            {
                selectNow++;
            }
            else
            {
                selectNow = 0;
            }
            PontPosUpdate();
        }
        else if (Input.GetKeyDown(KeyCode.Q))
        {
            if (selectNow > 0)
            {
                selectNow--;
            }
            else
            {
                selectNow = (SelectNow)Enum.GetValues(typeof(SelectNow)).Length - 1;
            }
            PontPosUpdate();
        }
    }
}
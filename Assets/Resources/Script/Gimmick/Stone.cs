using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stone : Gimmick
{
    // Start is called before the first frame update
    void Start()
    {
        enterItem = new int[1]{ (int)Utility.ItemId.Hammer };
    }

    public override IEnumerator Trigger(int itemNum, Vector2Int position)
    {
        GameTask.actionNum++;
        DestoryObject(position);
        yield return null;
    }

    void DestoryObject(Vector2Int position)
    {
        for(int i = 0;i < GameTask.mapObjects.Count;i++)
        {
            if(GameTask.mapObjects[i].go == gameObject)
            {
                GameTask.mapObjects.RemoveAt(i);
                break;
            }
        }
        GameTask.mapData[position.x][position.y] = (int)Utility.MapId.Ground;
        GameTask.actionNum--;
        Destroy(gameObject);
    }
}

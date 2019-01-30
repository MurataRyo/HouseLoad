using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water : Gimmick
{    // Start is called before the first frame update
    void Start()
    {
        enterItem = new int[1] { (int)Utility.ItemId.Baketu };
    }
    
    public override bool EnterIf()
    {
        return PlayerTask.itemNum[(int)Utility.ItemId.Baketu] == 1;
    }

    public override IEnumerator Trigger(int itemNum, Vector2Int position)
    {
        GameTask.actionNum++;
        DestoryObject(position);
        yield return null;
    }

    void DestoryObject(Vector2Int position)
    {
        for (int i = 0; i < GameTask.mapObjects.Count; i++)
        {
            if (GameTask.mapObjects[i].go == gameObject)
            {
                GameTask.mapObjects.RemoveAt(i);
                break;
            }
        }
        PlayerTask.itemNum[(int)Utility.ItemId.Baketu]+=2;
        GameTask.mapData[position.x][position.y] = (int)Utility.MapId.Ground;
        GameObject go = Instantiate(Utility.objectId[(int)Utility.ObjectId.Ground]);
        go.transform.position = transform.position;
        GameTask.actionNum--;
        Destroy(gameObject);
    }
}

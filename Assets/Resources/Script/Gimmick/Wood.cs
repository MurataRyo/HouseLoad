using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wood : Gimmick
{
    public MapObject mapObject;
    public virtual void Start()
    {
        enterItem = new int[2] { (int)Utility.ItemId.Match, (int)Utility.ItemId.Ono };
        mapObject = null;
        for (int i = 0; i < GameTask.mapObjects.Count; i++)
        {
            if (gameObject == GameTask.mapObjects[i].go)
            {
                mapObject = GameTask.mapObjects[i];
            }
        }
    }

    public override IEnumerator Trigger(int itemNum, Vector2Int position)
    {
        GameTask.actionNum++;

        switch (itemNum)
        {
            case (int)Utility.ItemId.Ono:
                CreateObject((int)Utility.ObjectId.WoodBlock);
                GameTask.mapData[position.x][position.y] = (int)Utility.MapId.WoodBlock;
                yield break;

            case (int)Utility.ItemId.Match:
                CreateObject((int)Utility.ObjectId.Fire);
                GameTask.mapData[position.x][position.y] = (int)Utility.MapId.Fire;
                yield break;
        }
    }

    private void CreateObject(int nextObjectNum)
    {
        GameObject go = Instantiate(Utility.objectId[nextObjectNum]);
        go.transform.position = transform.position;
        mapObject.go = go;
        mapObject.objectId = nextObjectNum;
        GameTask.actionNum--;
        Destroy(gameObject);
    }
}

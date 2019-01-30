using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WoodBlock : Wood
{
    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        enterItem = new int[1] { (int)Utility.ItemId.Match };
    }

    public void Move(bool flag, Vector3 addPos, Vector2Int nextData)
    {
        StartCoroutine(MoveCor(flag,addPos,nextData));
    }

    //trueで穴に落ちる falseで穴に落ちない
    public IEnumerator MoveCor(bool flag,Vector3 addPos,Vector2Int nextData)
    {
        GameTask.actionNum++;
        yield return new WaitForSeconds(0.175f);
        Vector3 nextPos = transform.position + addPos;
        while(nextPos != transform.position)
        {
            transform.position = Vector3.MoveTowards(transform.position, nextPos, 2f * Time.deltaTime);
            yield return null;
        }

        if(!flag)
        {
            GameTask.mapData[nextData.x][nextData.y] = (int)Utility.MapId.WoodBlock;
            GameTask.actionNum--;
            yield break;
        }
        
        nextPos = transform.position + Vector3.down;
        while (nextPos != transform.position)
        {
            transform.position = Vector3.MoveTowards(transform.position, nextPos, 4f * Time.deltaTime);
            yield return null;
        }

        GameTask.mapData[nextData.x][nextData.y] = (int)Utility.MapId.Ground;
        GameTask.actionNum--;
        Destroy(this);
        yield break;
    }
}

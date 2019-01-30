using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gimmick : MonoBehaviour
{
    //対応しているアイテム
    [HideInInspector]public int[] enterItem;

    public virtual bool CheckIf(int itemNum, Vector2Int position)
    {
        for(int i = 0;i < enterItem.Length;i++)
        {
            if(itemNum == enterItem[i] && EnterIf())
            {
                StartCoroutine(Trigger(itemNum, position));
                return true;
            }
        }
        return false;
    }

    //特殊条件があれば記述
    public virtual bool EnterIf()
    {
        return true;
    }

    public virtual IEnumerator Trigger(int itemNum,Vector2Int position)
    {
        yield return null;
    }
}
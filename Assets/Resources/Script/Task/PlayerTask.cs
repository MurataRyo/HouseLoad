using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTask : MonoBehaviour
{
    private const float JUMP_SPEED = 10f;
    private const float ANGLE_SPEED = 1f;
    private const float MOVE_SPEED = 2.5f;          // 移動速度
    private Vector3 targetPosition;                 //プレイヤーが行こうとする場所
    public static Vector2Int playerPos;             //マップ上でのプレイヤーの座標
    private bool targetMoveFlag;                    //targetに移動するかどうか

    public enum NowAngle
    {
        forward,
        right,
        back,
        left
    }
    public NowAngle nowAngle = NowAngle.forward;



    //斧　ハンマー　マッチ　バケツ　の順
    //残りのアイテム数
    public static int[] itemNum = new int[4];
    List<ItemUi> itemUis;

    void Start()
    {
        targetMoveFlag = true;
        targetPosition = transform.position;
        itemUis = Utility.GetTask().GetComponent<ItemTask>().itemUis;
    }

    void Update()
    {
        PlayerMove();
        Move();
    }

    //プレイヤーが干渉する動作
    void PlayerMove()
    {
        if (transform.position != targetPosition || GameTask.actionNum != 0)
            return;

        PlayerAction();

        if (Input.GetKey(KeyCode.W) && NextPosIf())
        {
            SetTargetPosition(PlayerForward());
        }
        else if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Q))
        {
            nowAngle += Input.GetKeyDown(KeyCode.E) ? 1 : -1;
            if ((int)nowAngle == 4)
            {
                nowAngle = 0;
            }
            else if ((int)nowAngle == -1)
            {
                nowAngle = (NowAngle)3;
            }
            StartCoroutine(AngleCor());
        }
    }

    private bool ItemSet(int button, out int itemNum)
    {
        for (itemNum = 0; itemNum < itemUis.Count; itemNum++)
        {
            if (itemUis[itemNum].num == button)
            {
                return true;
            }
        }
        return false;
    }

    //アイテム使用
    private void PlayerAction()
    {
        List<KeyCode> keys = new List<KeyCode>() {
            KeyCode.Alpha1,
            KeyCode.Alpha2,
            KeyCode.Alpha3,
            KeyCode.Alpha4,
        };

        for (int i = 0; i < itemUis.Count; i++)
        {
            UseKey(keys[i]);
        }
    }


    private void UseKey(KeyCode code)
    {
        int buttonNum = (int)code - 48;

        if (buttonNum < 1 || buttonNum > 4)
            return;

        Vector2Int next = NextPos(playerPos);
        int nextObject = PosToMapId(next);
        MapObject mob = null;

        int itemId = itemUis[buttonNum - 1].itemId;
        
        if (!Input.GetKeyDown(code) || itemNum[itemId] <= 0)
            return;

        if (GameTask.PosInMapObject(next, out mob))
        {
            Debug.Log(nextObject);
            if (mob.go.GetComponent<Gimmick>() == null)
                return;

            if (mob.go.GetComponent<Gimmick>().CheckIf(itemId, next))
            {
                itemNum[itemId]--;
                itemUis[buttonNum - 1].Reload(itemNum[itemId]);
            }
        }
    }

    private Vector2Int NextPos(Vector2 now)
    {
        Vector2Int next = GameTask.IntPos(now);
        switch (nowAngle)
        {
            case NowAngle.forward:
                next += new Vector2Int(-1, 0);
                break;

            case NowAngle.right:
                next += new Vector2Int(0, 1);
                break;

            case NowAngle.left:
                next += new Vector2Int(0, -1);
                break;

            case NowAngle.back:
                next += new Vector2Int(1, 0);
                break;
        }
        return next;
    }

    private int PosToMapId(Vector2Int pos)
    {
        //範囲外はレンガを返す
        if (!PosInMap(pos))
            return (int)Utility.MapId.Wall;

        return GameTask.mapData[pos.x][pos.y];
    }

    private bool PosInMap(Vector2Int pos)
    {
        //範囲外はレンガを返す
        if (pos.x < 0 || pos.y < 0 ||
            pos.x > GameTask.mapData.Length - 1 ||
            pos.y > GameTask.mapData[pos.x].Length - 1)
            return false;

        return true;
    }

    //次が移動できるかどうか
    private bool NextPosIf()
    {
        Vector2Int next = NextPos(playerPos);

        int nextMapId = PosToMapId(next);

        MapObject mob = null;

        if (PosInMap(next) && GameTask.PosInMapObject(next, out mob))
        {
            //木を押せるとき
            if (NextMoveWoodBlock(mob))
            {
                playerPos = next;
                return true;
            }
            if (nextMapId == (int)Utility.MapId.Warp)
            {
                StartCoroutine(WarpCor(mob.go.GetComponent<Warp>()));
                return true;
            }
        }
        //前に進めるとき
        if (nextMapId == (int)Utility.MapId.Ground || nextMapId == (int)Utility.MapId.House)
        {
            if (nextMapId == (int)Utility.MapId.House)
            {
                for (int i = 0; i < GameTask.mapObjects.Count; i++)
                {
                    if (GameTask.mapObjects[i].pos == next)
                        StartCoroutine(GoalCor(GameTask.mapObjects[i].go));
                }
            }
            playerPos = next;
            return true;
        }
        return false;
    }

    #region コルーチン

    IEnumerator AngleCor()
    {
        GameTask.actionNum++;

        while (transform.eulerAngles.y != (int)nowAngle * 90f)
        {
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, Mathf.MoveTowardsAngle(transform.eulerAngles.y, (int)nowAngle * 90f, Time.deltaTime * 360f / ANGLE_SPEED), transform.eulerAngles.z);
            yield return null;
        }

        GameTask.actionNum--;
        yield break;
    }

    IEnumerator WarpCor(Warp next)
    {
        GameTask.actionNum++;

        yield return null;

        yield return new WaitWhile(() => { return targetPosition != transform.position || GameTask.actionNum > 1; });

        for (int i = 0; i < GameTask.mapObjects.Count; i++)
        {
            //対抗のワープなら
            if (GameTask.mapObjects[i].go == next.warpPos.gameObject)
            {
                //データ上の座標はあらかじめ決定しておく
                playerPos = GameTask.mapObjects[i].pos;

                //スタートにLineRendがあるかどうか
                bool flag = next.warpPos.gameObject.GetComponent<LineRenderer>();
                //lineRendを持っているほうを取得
                Warp lineInWarp = flag ? next.warpPos : next;

                Vector3[] vec3s = new Vector3[lineInWarp.GetComponent<LineRenderer>().positionCount];
                LineRenderer lineRend = lineInWarp.GetComponent<LineRenderer>();
                lineRend.GetPositions(vec3s);

                yield return MoveWarp(lineInWarp.besieData, flag);
                break;
            }
        }

        transform.position = next.warpPos.transform.position;
        targetPosition = transform.position;
        GameTask.actionNum--;
        yield break;
    }

    //positionsに座標 flagに向きを入れる※　trueなら正方向　falseなら逆方向
    IEnumerator MoveWarp(BesieData besieData, bool flag)
    {
        targetMoveFlag = false;

        yield return JumpNext(besieData, flag);

        targetMoveFlag = true;
        yield break;
    }

    //0からのスタート時
    IEnumerator JumpNext(BesieData besieData, bool flag)
    {
        float MAX_RANGE = (besieData.start - besieData.end).magnitude;
        float now = 0f;

        Vector3 start = !flag ? besieData.start : besieData.end;
        Vector3 end = flag ? besieData.start : besieData.end;

        while (end != transform.position)
        {
            now += Time.deltaTime * JUMP_SPEED;
            if (now > MAX_RANGE)
                now = MAX_RANGE;

            transform.position = Utility.Besie(start, end, besieData.center, now / MAX_RANGE);
            yield return null;
        }
    }

    IEnumerator GoalCor(GameObject house)
    {
        GameTask.actionNum++;
        for (int i = 0; i < 3; i++)
        {
            if (i != 0)
                SetTargetPosition(-house.transform.forward);
            do
            {
                yield return null;
            }
            while (transform.position != targetPosition);
        }

        GameTask.actionNum--;
        Instantiate(Resources.Load<GameObject>(GetPath.Ui + "/Canvas"));
        yield break;
    }

    #endregion

    private bool NextMoveWoodBlock(MapObject mapOb)
    {
        if (mapOb.go.GetComponent<WoodBlock>() == null)
            return false;

        Vector2Int blockNext = NextPos(playerPos);
        blockNext = NextPos(blockNext);

        if (!PosInMap(blockNext))
            return false;

        //ここまでで木ブロックが配列内であることが確定

        int nextobject = GameTask.mapData[blockNext.x][blockNext.y];

        WoodBlock woodBlock = mapOb.go.GetComponent<WoodBlock>();
        if (nextobject == (int)Utility.MapId.Ground || nextobject == (int)Utility.MapId.Hole)
        {
            Vector2Int nextPlayer = NextPos(playerPos);

            //ブロックに変更
            GameTask.mapData[nextPlayer.x][nextPlayer.y] = (int)Utility.MapId.Ground;
            if (nextobject == (int)Utility.MapId.Hole)
            {
                woodBlock.Move(true, PlayerForward(), blockNext);
            }
            else
            {
                mapOb.pos = blockNext;
                woodBlock.Move(false, PlayerForward(), blockNext);
            }
            return true;
        }
        return false;
    }

    Vector3 PlayerForward()
    {
        switch (nowAngle)
        {
            case NowAngle.forward:
                return Vector3.forward;

            case NowAngle.back:
                return Vector3.back;

            case NowAngle.right:
                return Vector3.right;

            case NowAngle.left:
                return Vector3.left;
        }

        Debug.Log("エラー　rotatezが不明確です");
        return Vector3.zero;
    }

    void SetTargetPosition(Vector3 forward)
    {
        targetPosition = transform.position + forward;
    }

    void Move()
    {
        if (!targetMoveFlag)
            return;

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, MOVE_SPEED * Time.deltaTime);
    }
}

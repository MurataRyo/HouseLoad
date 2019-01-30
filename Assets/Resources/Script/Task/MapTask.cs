using System;
using System.Collections.Generic;
using UnityEngine;

public class MapTask : MonoBehaviour
{
    //Stringをジャグ配列のデータに変換
    public static string[][] MapDataLoad(string str1, out AddGimick addGimick)
    {
        string[] str2 = str1.Split(char.Parse(":"));

        PlayerTask.itemNum = StringsToInts(str2[0].Split(char.Parse(".")));

        addGimick = new AddGimick();
        //ステージの横データに変換
        string[] str3 = str2[1].Split(char.Parse(","));
        List<string[]> str4 = new List<string[]>();

        for (int i = 0; i < str3.Length; i++)
        {
            string[] str5 = str3[i].Split(char.Parse("."));

            for (int j = 0; j < str5.Length; j++)
            {
                string[] str6 = str5[j].Split(char.Parse("_"));
                if (str6.Length >= 2)
                {
                    str5[j] = str6[0];
                    addGimick.Add(int.Parse(str6[1]));
                }
            }
            str4.Add(str5);
        }
        return str4.ToArray();
    }

    private static int[] StringsToInts(string[] str)
    {
        int[] i = new int[str.Length];
        for (int j = 0; j < i.Length; j++)
        {
            i[j] = int.Parse(str[j]);
        }

        return i;
    }

    public static void MapCreate(string[][] str, out int[][] mapData, out Vector2Int playerPos, out List<MapObject> mapObjects, AddGimick addGimick)
    {
        Vector2 vec2 = MaxPos(str);

        //左上座標の確定
        Vector3 leftUpPos = new Vector3(-vec2.x / 2, 0f, vec2.y / 2);

        mapData = DataToint(str);

        CreateStage(ref mapData, leftUpPos, out playerPos, out mapObjects, addGimick);
    }

    public static void CreateStage(ref int[][] mapData, Vector3 minPos, out Vector2Int playerPos, out List<MapObject> mapObjects, AddGimick addGimick)
    {
        //エラー回避
        playerPos = Vector2Int.zero;

        mapObjects = new List<MapObject>();

        List<Vector2Int> notCreatePos = new List<Vector2Int>();

        if (Utility.objectId == null)
            LoadObject();

        for (int j = 0; j < mapData.Length; j++)
        {
            for (int k = 0; k < mapData[j].Length; k++)
            {
                //ここで右上から左上に変換する
                Create(mapData[j][k], minPos + new Vector3(k, 0f, -j), mapObjects, new Vector2Int(j, k), addGimick, out notCreatePos);

                //プレイヤーなら
                if (mapData[j][k] == (int)Utility.MapId.Player)
                {
                    playerPos = new Vector2Int(j, k);
                    mapData[j][k] = (int)Utility.MapId.Ground;//床に変更
                }
            }
        }

        CreateWall(mapData, minPos, notCreatePos.ToArray());
    }

    //座標情報
    private class PosInfo
    {
        public int min;
        public int max;

        public PosInfo()
        {
            min = 100;
            max = -100;
        }
    }


    //場外の設置
    private static void CreateWall(int[][] mapData, Vector3 minPos, Vector2Int[] notCreatePos)
    {
        GameObject gos = new GameObject();
        gos.name = "WallParent";

        PosInfo[] posInfos = PosInfoInput(mapData);

        for (int i = 0; i < posInfos.Length; i++)
        {
            for (int j = posInfos[i].min; j <= posInfos[i].max; j++)
            {
                if (WallCreateIs(mapData, new Vector2Int(i - 1, j), notCreatePos))
                    CreateWall(gos, minPos + Vec2IntToPos(new Vector2Int(-i + 1, j)));
            }
        }
    }

    private static PosInfo[] PosInfoInput(int[][] mapData)
    {
        PosInfo[] posInfos1 = new PosInfo[mapData.Length];
        for (int i = 0; i < mapData.Length; i++)
        {
            posInfos1[i] = new PosInfo();
            for (int j = 0; j < mapData[i].Length; j++)
            {
                if (mapData[i][j] != (int)Utility.MapId.None)
                {
                    posInfos1[i].min = j;
                    break;
                }
            }
            for (int j = mapData[i].Length - 1; j >= 0; j--)
            {
                if (mapData[i][j] != (int)Utility.MapId.None)
                {
                    posInfos1[i].max = j;
                    break;
                }
            }
        }

        PosInfo[] posInfos2 = new PosInfo[posInfos1.Length + 2];

        for (int i = 0; i < posInfos2.Length; i++)
            posInfos2[i] = new PosInfo();

        for (int i = 0; i < posInfos1.Length; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (posInfos2[i + j].min > posInfos1[i].min - 1)
                    posInfos2[i + j].min = posInfos1[i].min - 1;

                if (posInfos2[i + j].max < posInfos1[i].max + 1)
                    posInfos2[i + j].max = posInfos1[i].max + 1;
            }
        }

        return posInfos2;
    }

    private static bool WallCreateIs(int[][] mapData, Vector2Int createPos, Vector2Int[] notCreatePos)
    {
        foreach (Vector2Int notPos in notCreatePos)
        {
            if (notPos == createPos)
                return false;
        }

        if (createPos.x < 0 || createPos.y < 0 || createPos.x >= mapData.Length || createPos.y >= mapData[createPos.x].Length)
            return true;

        if (mapData[createPos.x][createPos.y] == (int)Utility.MapId.None)
            return true;

        return false;
    }

    private static Vector3 Vec2IntToPos(Vector2Int vec2)
    {
        return new Vector3(vec2.y, 0f, vec2.x);
    }

    private static void CreateWall(GameObject gos, Vector3 pos)
    {
        GameObject go;
        go = CreateObject(Utility.ObjectId.Ground, pos);
        go.transform.parent = gos.transform;
        go = CreateObject(Utility.ObjectId.Wall, pos + new Vector3(0f, 1f, 0f));
        go.transform.parent = gos.transform;
    }

    //データ関連-----------------------------------------

    private static int[][] DataToint(string[][] str)
    {
        int[][] data = new int[str.Length][];
        for (int i = 0; i < str.Length; i++)
        {
            data[i] = new int[str[i].Length];
            for (int j = 0; j < str[i].Length; j++)
            {
                int ObjectId = int.Parse(str[i][j]);
                data[i][j] = ObjectId;
            }
        }
        return data;
    }

    //0,0が真ん中になるように
    private static Vector2 MaxPos(string[][] str)
    {
        int xRange = 0;
        for (int i = 0; i < str.Length; i++)
        {
            for (int j = 0; j < str[i].Length; j++)
            {
                if (xRange < str[i].Length)
                {
                    xRange = str[i].Length;
                }
            }
        }
        return new Vector2(xRange, str.Length);
    }

    public static void Create(int thisMapId, Vector3 createPos, List<MapObject> mapObjects, Vector2Int pos, AddGimick addGimick, out List<Vector2Int> notCreatePos)
    {
        notCreatePos = new List<Vector2Int>();

        if (DownBlock(thisMapId))
        {
            if (thisMapId == (int)Utility.MapId.Warp || thisMapId == (int)Utility.MapId.House)
            {
                mapObjects.Add(new MapObject(CreateObject(Utility.GetObjectId((Utility.MapId)thisMapId), createPos + Vector3.up, addGimick.number[0], thisMapId, pos, notCreatePos), thisMapId, pos));
                addGimick.number.RemoveAt(0);
            }
            else
            {
                mapObjects.Add(new MapObject(CreateObject(Utility.GetObjectId((Utility.MapId)thisMapId), createPos + Vector3.up), thisMapId, pos));
            }
            CreateObject(Utility.ObjectId.Ground, createPos);
        }
        else if (thisMapId == (int)Utility.MapId.Water)
        {
            mapObjects.Add(new MapObject(CreateObject(Utility.GetObjectId((Utility.MapId)thisMapId), createPos), thisMapId, pos));
        }
        else if (thisMapId == (int)Utility.MapId.Ground)
        {
            CreateObject(Utility.ObjectId.Ground, createPos);
        }
    }

    public static GameObject CreateObject(Utility.ObjectId objectId, Vector3 pos)
    {
        GameObject go = Instantiate(Utility.objectId[(int)objectId]);
        go.transform.position = pos;

        return go;
    }

    public static GameObject CreateObject(Utility.ObjectId objectId, Vector3 objectPos, int gimmickNum, int mapId, Vector2Int mapPos, List<Vector2Int> notCreatePos)
    {
        GameObject go = Instantiate(Utility.objectId[(int)objectId]);
        go.transform.position = objectPos;

        if (mapId == (int)Utility.MapId.Warp)
        {
            go.GetComponent<Warp>().number = gimmickNum;
        }
        else if (mapId == (int)Utility.MapId.House)
        {
            go.transform.eulerAngles = Utility.IntToAngle(gimmickNum);
            NotCreatePosHouse(go, mapPos, notCreatePos);
        }
        return go;
    }

    private static void NotCreatePosHouse(GameObject house, Vector2Int mapPos, List<Vector2Int> notCreatePos)
    {
        Vector2Int add = new Vector2Int(Mathf.RoundToInt(house.transform.forward.z), Mathf.RoundToInt(-house.transform.forward.x));
        for (int i = 1; i <= 3; i++)
        {
            for (int j = -1; j <= 2; j++)
            {
                notCreatePos.Add(mapPos + new Vector2Int(add.x * i + add.y * -j, add.y * i + add.x * j));
            }
        }
    }

    //どのブロックかの判定

    #region

    //何も置かない
    public static bool None(int i)
    {
        return InIf(i,
            new int[2] {
                (int)Utility.MapId.None,
                (int)Utility.MapId.Hole
            });
    }

    //オブジェクトの下にブロックをおくもの
    public static bool DownBlock(int i)
    {
        return InIf(i,
            new int[8] {
                (int)Utility.MapId.Player,
                (int)Utility.MapId.Wood,
                (int)Utility.MapId.WoodBlock,
                (int)Utility.MapId.Stone,
                (int)Utility.MapId.Wall,
                (int)Utility.MapId.Fire,
                (int)Utility.MapId.Warp,
                (int)Utility.MapId.House
            });
    }

    public static bool InIf(int i, int[] j)
    {
        for (int k = 0; k < j.Length; k++)
        {
            if (i == j[k])
                return true;
        }
        return false;
    }

    #endregion

    private static void LoadObject()
    {
        int size = Enum.GetValues(typeof(Utility.ObjectId)).Length;
        Utility.objectId = new GameObject[size];

        for (int i = 0; i < size; i++)
        {
            string objectName = Enum.GetName(typeof(Utility.ObjectId), i);
            Utility.objectId[i] = Resources.Load<GameObject>(GetPath.Object + "/" + objectName);
        }
    }

    #region ステージ情報
    //1番上の行にアイテム数
    //番号の間に. アイテムとステージの間に: ステージの列変更に,　を使用『重要』！！！！！

    //おの。ハンマー。マッチ。バケツの数※バケツは0か1のみ
    //ブロックはUtility参照
    // 家の後には;で向きを決める0が正面で時計回り
    //ワープには;の後に対応番号を入れる
    public const string Stage0 =
        "1.1.0.0:" +
        "2.4.1.6.3.8_3";

    public const string Stage1 =
       "3.1.0.0:" +
       "2.3.3.3.3," +
       "0.4.5.3.3.3," +
       "0.3.3.5.5.3.3," +
       "0.3.3.5.5.3.3," +
       "0.3.3.5.5.3.3.3.3," +
       "0.3.4.3.1.3.8_0";

    public const string Stage2 =
       "2.2.0.0:" +
       "0.6.6.6.6.6.6.6.0," +
       "0.6.2.3.0.2.2.6.6.6.6," +
       "0.6.2.3.2.2.6.6.2.2.6," +
       "6.6.2.6.5.6.5.7_3.2.2.6," +
       "6.1.2.2.2.2.0.6.6.6.6," +
       "6.6.2.2.0.2.2.6.0," +
       "0.6.6.6.6.6.6.6.0";

    public const string Stage3 =
       "2.2.1.1:" +
       "0.6.6.6.6.6.6.6.6.0," +
       "6.1.2.2.2.6.6.2.6.0," +
       "0.6.2.5.3.2.0.5.6.0," +
       "0.6.6.2.6.2.5.3.6.0," +
       "0.6.5.2.2.2.6.0.6.6.6.6," +
       "0.6.2.3.2.0.2.5.6.2.2.6," +
       "0.6.8.0.2.6.3.3.7_3.2.2.6," +
       "0.6.6.6.6.6.6.6.6.6.6.6";

    public const string Stage4 =
        "3.3.1.1:" +
        "6.6.6.6.6.6.6.6.6.6.0," +
        "6.1.2.6.2.2.3.0.8.6.0," +
        "6.6.2.3.0.2.5.2.2.6.0," +
        "0.6.2.5.2.2.6.2.10_1.6.0," +
        "0.6.6.6.6.6.6.6.6.6.0," +
        "0.6.10_1.5.2.6.2.3.0.6.6.6.6," +
        "0.6.2.6.3.5.2.5.5.6.2.2.6," +
        "0.6.2.3.2.3.0.3.5.7_3.2.2.6," +
        "0.6.6.6.6.6.6.6.6.6.6.6.6";

    public const string Stage5 =
        "2.2.3.1:" +
        "0.0.0.6.6.6.6.6.6.6.6.6.6.6.6.6," +
        "0.0.0.6.6.8.2.2.2.6.8.11_2.3.5.5.6," +
        "0.0.0.6.2.6.5.6.2.6.5.6.6.3.0.6," +
        "0.0.0.6.2.5.1.3.0.6.3.6.2.0.11_1.6," +
        "0.0.0.6.2.6.3.6.2.6.2.0.2.2.2.6," +
        "0.0.0.6.2.0.0.2.2.6.5.2.4.2.2.6," +
        "0.0.0.6.11_1.3.2.2.6.11_2.6.2.2.2.2.6," +
        "0.0.0.6.5.2.2.0.3.2.2.6.6.6.8.6," +
        "0.0.0.6.2.6.6.2.6.6.2.2.2.2.6.6," +
        "0.0.0.6.6.2.2.2.2.2.6.6.6.2.2.6," +
        "6.6.6.6.3.5.0.2.2.2.2.3.2.6.2.6," +
        "6.2.2.7_1.2.3.3.2.6.2.2.2.3.2.2.6," +
        "6.2.2.6.6.6.6.6.6.6.6.6.6.6.6.6," +
        "6.6.6.6";
    #endregion
}

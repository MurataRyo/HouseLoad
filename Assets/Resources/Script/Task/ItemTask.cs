using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class ItemTask : MonoBehaviour
{
    Sprite[] itemSprite;
    Sprite[] extraItem;

    public List<ItemUi> itemUis;
    GameObject canvas;

    // Start is called before the first frame update
    void Awake()
    {
        canvas = GameObject.FindGameObjectWithTag("Canvas");
        itemUis = new List<ItemUi>();

        LoadItem();

        for (int i = 0;i < PlayerTask.itemNum.Length;i++)
        {
            if(PlayerTask.itemNum[i] != 0)
            {
                //バケツ時
                if(i == (int)Utility.ItemId.Baketu)
                {
                    itemUis.Add(new Baketoitem(canvas, itemUis.Count, i, itemSprite[i],extraItem[(int)Utility.ExItemId.WaterBaketu]));
                }
                else
                {
                    itemUis.Add(new Normalitem(canvas, itemUis.Count, i, itemSprite[i]));
                }
            }
        }
    }

    private void LoadItem()
    {
        int size = Enum.GetValues(typeof(Utility.ItemId)).Length;
        itemSprite = new Sprite[size];

        for (int i = 0; i < size; i++)
        {
            string itemName = Enum.GetName(typeof(Utility.ItemId), i);
            itemSprite[i] = Resources.Load<Sprite>(GetPath.Item + "/" + itemName);
        }

        size = Enum.GetValues(typeof(Utility.ExItemId)).Length;
        extraItem = new Sprite[size];

        for (int i = 0; i < size; i++)
        {
            string itemName = Enum.GetName(typeof(Utility.ItemId), i);
            extraItem[i] = Resources.Load<Sprite>(GetPath.ExItem + "/" + itemName);
        }
    }
}


public class ItemUi : ScriptableObject
{
    Vector2 zeroPos = new Vector2(-548.2f, -423.4f);
    const float range = 213f;

    public int num;         //配列番号
    public int itemId;     //アイテムID
    public GameObject go;
    public RectTransform rect;
    public Image image;
    public ItemUi(GameObject parent, int number, int itemId, Sprite sprite)
    {
        this.itemId = itemId;
        num = number;
        go = new GameObject();
        rect = go.AddComponent<RectTransform>();
        image = go.AddComponent<Image>();
        image.sprite = sprite;
        rect.sizeDelta = new Vector2(100f, 120f);
        rect.position = zeroPos + new Vector2(number * range + Screen.width / 2, Screen.height / 2);
        go.transform.parent = parent.transform;
    }

    public virtual void Reload(int i)
    {

    }

    public virtual void End()
    {
        image.color = Color.gray;
    }
}

public class Normalitem : ItemUi
{
    GameObject textObject;
    Text text;
    public Normalitem(GameObject parent, int number, int itenNumber, Sprite sprite) : base(parent, number, itenNumber, sprite)
    {
        GameObject numberObject = Resources.Load<GameObject>(GetPath.Ui + "/Number");
        textObject = Instantiate(numberObject);
        textObject.transform.parent = go.transform;
        textObject.transform.position = go.transform.position + new Vector3(80f, -10f, 0f);
        text = textObject.GetComponent<Text>();
        text.text = PlayerTask.itemNum[itenNumber].ToString();
    }

    public override void Reload(int i)
    {
        if (i == 0)
            End();

        text.text = i.ToString();
    }
}

public class Baketoitem : ItemUi
{
    Sprite mainSprite;
    Sprite endSprite;
    public Baketoitem(GameObject parent, int number, int itenNumber, Sprite sprite, Sprite maxSprite) : base(parent, number, itenNumber, sprite)
    {
        mainSprite = sprite;
        endSprite = maxSprite;
    }

    public override void Reload(int i)
    {
        if (i == 0)
            End();

        image.sprite = i == 1 ? mainSprite : endSprite;
    }
}
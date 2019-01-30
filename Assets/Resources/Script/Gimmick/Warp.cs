using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Warp : Gimmick
{
    [HideInInspector] public int number;
    [HideInInspector] public Warp warpPos;
    [HideInInspector] public Vector3[] lineVetex;
    public BesieData besieData;

    void Start()
    {
        //ワープ先の指定
        foreach (MapObject mOb in GameTask.mapObjects)
        {
            if (mOb.go.GetComponent<Warp>() == null)
                continue;

            Warp warp = mOb.go.GetComponent<Warp>();

            if (warp.gameObject == gameObject ||
                warp.number != number)
                continue;

            warpPos = mOb.go.GetComponent<Warp>();

            if (warpPos.warpPos != null)
            {
                CreateLine(transform.position, warpPos.transform.position);
            }
        }
    }

    //ワープのパスを生成
    private void CreateLine(Vector3 start, Vector3 end)
    {
        LineRenderer line =
        gameObject.AddComponent<LineRenderer>();

        line.widthMultiplier = 0.1f;

        string Path = "Material/LineMaterial";
        line.material = Resources.Load<Material>(Path);

        line.alignment = LineAlignment.View;

        lineVetex = LinePath(start, end);

        line.positionCount = lineVetex.Length;
        line.SetPositions(lineVetex);

        Color lineColor = new Color(0.5f,0.9f,0.7f,1f);

        line.startColor = lineColor;
        line.endColor = lineColor;
    }

    private Vector3[] LinePath(Vector3 start, Vector3 end)
    {
        const float MAX_HEIGHT = 5f;
        const float ONE_RANGE = 0.05f;
        float maxRange = (start - end).magnitude;
        Vector3 now = start - (end - start).normalized * ONE_RANGE;
        Vector3 center = (start + end) / 2; //中心座標を求める  
        
        center = new Vector3(center.x, center.y + MAX_HEIGHT, center.z);    //ベジエ曲線の制御点
        besieData = new BesieData(start, end, center);

        List<Vector3> vec3s = new List<Vector3>();

        do
        {
            now = Vector3.MoveTowards(now, end, ONE_RANGE);
            float t = (start - now).magnitude / maxRange;
            vec3s.Add(Utility.Besie(start, end, center, t));
        }
        while (now != end);

        return vec3s.ToArray();
    }
}

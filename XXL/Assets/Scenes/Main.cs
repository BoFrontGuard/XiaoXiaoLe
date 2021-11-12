using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Main : MonoBehaviour
{
    public GameObject[] perfabs;//将所有的水果都收集起来
    public GameObject[] effect;
    public GameObject[,] objects;//收集所有水果的实例 用于查询
    public Text text;
    public int Score;
    public int Column;//记录行与列
    public int Row;//记录行与列
    public float size = 1;//初始值
    public float startTime = 1;//设置限定时间
    List<GameObject> cache = new List<GameObject>();//创建一个缓存
    // Start is called before the first frame update
    void Start()
    {
        //初始化二维数组的缓存
        objects = new GameObject[Column, Row];
        //随机创建水果 随机数的最大值
        int max = perfabs.Length;
        float sx = Column * -0.5f * size + size * 0.5f;
        float sy = Row * -0.5f * size + size * 0.5f;
        for (int i = 0; i < Column; i++)
        {
            for (int j = 0; j < Row; j++)
            {
                //生成一个随机数
                int index = Random.Range(0, max);
                //实例化水果对象
                GameObject go = GameObject.Instantiate(perfabs[index]);
                //将对象设置到当前脚本
                go.transform.SetParent(transform);
                go.transform.localPosition = new Vector3(i * size + sx, j * size + sy, 0);
                go.transform.localScale = new Vector3(size, size, size);
                Mark mark = go.AddComponent<Mark>();
                //记录标签的信息
                mark.Column = i;
                mark.Row = j;
                mark.Index = index;
                go.name = index.ToString();
                //创建一个碰撞对象
                go.AddComponent<BoxCollider2D>();
                objects[i, j] = go;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (startTime > 0)
        {
            startTime -= Time.deltaTime;//时间减少
        }
        if (startTime < 0)
        {
            if (Input.GetMouseButtonDown(0))
            {
                //生成射线
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                //发射射线
                RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);
                //判断击中目标 需要给目标添加信息
                if (hit.transform != null)
                {
                    lastCol = curCol;
                    lastRow = curRow;
                    Mark mark = hit.transform.GetComponent<Mark>();
                    curCol = mark.Column;
                    curRow = mark.Row;
                    //先把cache清空
                    cache.Clear();
                    //检测
                    Cheek(hit.transform.gameObject.name, mark.Column, mark.Row);
                    if (cache.Count >= 3)
                    {
                        AddScroe(cache.Count);
                        Destory();
                    }else
                    {
                        cache.Clear();
                        Swap();
                    }
                }
            }
        }
    }
    //检查行和列是否符合销毁的要求
    void Cheek(string type, int column, int row)
    {
        if (row < 0 | row >= Row || column < 0 || column >= Column)
            return;
        GameObject go = objects[column, row];
        //判断类型是否相等
        if (go.name != type)
            return;
        //防止重复添加
        if (cache.Contains(go))
            return;
        //符合要求 添加进缓存
        cache.Add(go);
        //递归 顺着这个对象找上下左右的方向
        Cheek(type, column, row - 1);
        Cheek(type, column, row + 1);
        Cheek(type, column - 1, row);
        Cheek(type, column + 1, row);
    }
    void Destory()
    {
        for (int i = 0; i < cache.Count; i++)
        {
            //计算起始位置
            float sx = Column * -0.5f * size + size * 0.5f;
            float sy = Row * -0.5f * size + size * 0.5f;
            //先查询位于哪行哪列
            Mark mark = cache[i].GetComponent<Mark>();
            //清空二维数组里面的对象
            objects[mark.Column, mark.Row] = null;
            GameObject go = GameObject.Instantiate(effect[mark.Index]);
            go.transform.SetParent(transform);
            go.transform.position = new Vector3(mark.Column * size + sx, mark.Row*size+sy,0);
            go.transform.localScale = new Vector3(size, size, size);
            GameObject.Destroy(cache[i]);
        }
        StartCoroutine(Wait(0.6f,Fall));
    }
    IEnumerator Wait(float time,System.Action action)
    {
        yield return new WaitForSeconds(time);
        action();
    }
    
    void Fall()
    {
        //计算起始位置
        float sx = Column * -0.5f * size + size * 0.5f;
        float sy = Row * -0.5f * size + size * 0.5f;
        for (int i = 0; i < Column; i++)
        {
            //临时变量
            int count = 0;
            for (int j = 0; j < Row; j++)
            {
                //判断二维数组里面的缓存是否为空
                if (objects[i, j] == null)
                {
                    //计数增加
                    count++;
                }
                else
                {
                    //计数大于零的时候需要往下掉落
                    if (count > 0)
                    {
                        Mark m = objects[i, j].GetComponent<Mark>();
                        int row = j - count;
                        m.Row = row;
                        //更新记录位置
                        objects[i, row] = objects[i, j];
                        objects[i, row].transform.DOMove(new Vector3(i * size + sx, row * size + sy,0),0.3f);
                        objects[i,j] = null;
                    }
                }
            }
        }
        Add();
    }
    void Add()
    {
        //计算起始位置
        float sx = Column * -0.5f * size + size * 0.5f;
        float sy = Row * -0.5f * size + size * 0.5f;
        for(int i = 0; i < Column; i++)
        {
            for(int j=0; j < Row; j++)
            {
                if (objects[i, j] == null)
                {
                    int max = perfabs.Length;
                    int index = Random.Range(0,max);
                    GameObject go = GameObject.Instantiate(perfabs[index]);
                    //将对象设置到当前脚本
                    go.transform.SetParent(transform);
                    //修改初始值为最顶端的位置
                    go.transform.localPosition = new Vector3(i * size + sx, (Row+1) * size + sy, 0);
                    //从最顶端的位置向下掉落
                    go.transform.DOMove(new Vector3(i * size + sx, j * size + sy, 0), 0.3f);
                    go.transform.localScale = new Vector3(size, size, size);
                    Mark mark = go.AddComponent<Mark>();
                    //记录标签的信息
                    mark.Column = i;
                    mark.Row = j;
                    mark.Index = index;
                    go.name = index.ToString();
                    //创建一个碰撞对象
                    go.AddComponent<BoxCollider2D>();
                    objects[i, j] = go;
                }
            }
        }
    }
    //交换预制体检测当前和上一帧的行和列
    int curCol, curRow;
    int lastCol, lastRow;

    void Swap()
    {
        //计算相对位置
        int ox = curCol - lastCol;
        int oy = curRow - lastRow;
        if (oy ==0)
        {
            if (ox == 1|| ox== -1)
            {
                SwapTest();
            }
        }else
        if (ox == 0)
        {
            if (oy == 1 || oy == -1)
            {
                SwapTest();
            }
        }
    }
    void SwapTest()
    {
        bool fail = true;
        GameObject a = objects[curCol, curRow];
        GameObject b = objects[lastCol, lastRow];
        //交换在缓存中的位置
        objects[curCol, curRow] = b;
        objects[lastCol, lastRow] = a;
        //交换标签信息
        var mb = b.GetComponent<Mark>();
        mb.Column = curCol;
        mb.Row = curRow;
        var ma = a.GetComponent<Mark>();
        ma.Column =lastCol;
        ma.Row = lastRow;
        int cc = curCol;
        int cr = curRow;
        int lc = lastCol;
        int lr = lastCol;
        a.transform.DOMove(b.transform.position, 0.3f);
        b.transform.DOMove(a.transform.position, 0.3f).onComplete = () =>
        {
            //清除检测缓存
            cache.Clear();
            Cheek(b.name,cc,cr);
            int c = 0;
            if (cache.Count >= 3)
            {
                c += cache.Count;
                fail = false;
                Destory();
            }
            cache.Clear();
            AddScroe(cache.Count);
            Cheek(a.name, lc, lr);
            if (cache.Count >= 3)
            {
                c += cache.Count;
                fail = false;
                Destory();
            }
            cache.Clear();
            if (fail)
            {
                objects[cc, cr] = a;
                objects[lc, lr] = b;
                a.transform.DOMove(b.transform.position, 0.3f);
                b.transform.DOMove(a.transform.position, 0.3f);
                ma.Column = cc;
                ma.Row = cr;
                mb.Column = lc;
                mb.Row = lr;

            }
        };
    }
    void AddScroe(int count)
    {
        if (count < 3)
            return;
        Score += count * 100;
        count -= 3;
        Score += count * count * 10;
        text.text = Score.ToString();
    }
}


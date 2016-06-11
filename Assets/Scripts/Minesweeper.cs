using UnityEngine;
using System.Collections;

public class Minesweeper : MonoBehaviour
{
    private Transform father;
    public GameObject prefab;

    private bool smallMap = true;

    private float startTime;
    private float nowTime;

    public enum ClickMode
    {
        Normal = 0,
        Flag,
        Question,
    }

    public enum State
    {
        RUNING = 0,
        WIN,
        LOSE,
    }

    private int w = 10;
    private int h = 13;
    private Element[,] elements;
    private bool[,] visited;
    [HideInInspector]
    public State state = State.RUNING;
    [HideInInspector]
    public ClickMode clickMode = ClickMode.Normal;

    private static Minesweeper instance;

    public static Minesweeper Instance
    {
        get
        {
            return instance;
        }
    }

    void Awake()
    {
        father = this.transform;
        instance = this;
    }

    void Start()
    {
        ReStart();
    }

    private void ReStart()
    {
        if (smallMap)
        {
            this.w = 5;
            this.h = 5;
        }
        else
        {
            this.w = 10;
            this.h = 13;
        }
        Camera.main.transform.position = new Vector3(0.5f * (this.w - 1), 0.5f * (this.h - 1), -10f);
        this.elements = new Element[this.w, this.h];
        this.visited = new bool[this.w, this.h];
        
        DoCreate();

        this.state = State.RUNING;
        startTime = Time.realtimeSinceStartup;
    }

    void OnGUI()
    {
        GUILayout.BeginVertical();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("ReStart"))
        {
            ReStart();
        }

        switch (clickMode)
        {
            case ClickMode.Normal:
                {
                    if (GUILayout.Button("Normal"))
                    {
                        clickMode = ClickMode.Flag;
                    }
                }
                break;
            case ClickMode.Flag:
                {
                    if (GUILayout.Button("Flag"))
                    {
                        clickMode = ClickMode.Question;
                    }
                }
                break;
            case ClickMode.Question:
                {
                    if (GUILayout.Button("Question"))
                    {
                        clickMode = ClickMode.Normal;
                    }
                }
                break;
        }

        smallMap = GUILayout.Toggle(smallMap, "SmallMap");
        GUILayout.EndHorizontal();

        ShowResult();

        GUILayout.EndVertical();
    }
    
    private void ShowResult()
    {
        switch (this.state)
        {
            case State.RUNING:
                {
                    GUI.color = Color.white;
                    GUILayout.Label("Runing");
                    GUI.color = Color.white;

                    nowTime = Time.realtimeSinceStartup;
                }
                break;
            case State.WIN:
                {
                    GUI.color = Color.green;
                    GUILayout.Label("Win");
                    GUI.color = Color.white;
                }
                break;
            case State.LOSE:
                {
                    GUI.color = Color.red;
                    GUILayout.Label("Lose");
                    GUI.color = Color.white;
                }
                break;
        }
        GUILayout.Label("time:" + (nowTime - startTime));
    }

    private void DestroyAllChildren(Transform tf)
    {
        if (tf == null)
        {
            return;
        }

        Transform ctf = null;
        for (int i = 0, imax = tf.childCount; i < imax; i++)
        {
            ctf = tf.GetChild(i);
            if (ctf != null && ctf.gameObject != null)
            {
                GameObject.Destroy(ctf.gameObject);
            }
        }
    }

    [ContextMenu("DoCreate")]
    private void DoCreate()
    {
        if (Application.isPlaying)
        {
            DestroyAllChildren(father);
        }

        GameObject goNew = null;
        GameObject subfather = null;
        for (int j = 0; j < this.h; j++)
        {
            subfather = new GameObject(string.Format("line_{0}", j.ToString("D2")));
            subfather.transform.parent = father;
            subfather.transform.localPosition = Vector3.zero;
            subfather.transform.localRotation = Quaternion.Euler(Vector3.zero);
            subfather.transform.localScale = Vector3.one;

            for (int i = 0; i < this.w; i++)
            {
                goNew = GameObject.Instantiate(prefab) as GameObject;
                goNew.name = prefab.name + string.Format("_{0}_{1}", i.ToString("D2"), j.ToString("D2"));
                goNew.transform.parent = subfather.transform;
                goNew.transform.localPosition = new Vector3(i * 1.0f, j * 1.0f, 0);
                goNew.transform.localRotation = Quaternion.Euler(Vector3.zero);
                goNew.transform.localScale = Vector3.one;

                elements[i, j] = goNew.GetComponent<Element>();
                if (elements[i, j] != null)
                {
                    elements[i, j].Init(i, j);
                }
            }
        }
    }

    void OnDestroy()
    {
        DestroyAllChildren(father);
    }

    public void UncoverMines()
    {
        foreach (Element elem in elements)
        {
            if (elem != null && elem.mine)
            {
                elem.LoadTexture(0);
            }
        }
    }

    public bool MineAt(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < w && y < h)
        {
            return elements[x, y].mine;
        }
        return false;
    }

    public int AdjacentMines(int x, int y)
    {
        int count = 0;
        for (int i = x - 1; i <= x + 1; i++)
        {
            for (int j = y - 1; j <= y + 1; j++)
            {
                if (MineAt(i, j))
                {
                    count++;
                }
            }
        }

        return count;
    }

    public bool IsWinFinished()
    {
        foreach (Element elem in elements)
        {
            if (elem != null && elem.IsCovered() && !elem.mine)
            {
                return false;
            }
        }
        return true;
    }

    public void ResetVisited()
    {
        for (int i = 0; i < w; i++)
        {
            for (int j = 0; j < h; j++)
            {
                visited[i, j] = false;
            }
        }
    }

    public void FFuncover(int x, int y)
    {
        if (x < 0 || y < 0 || x >= w || y >= h)
        {
            return;
        }

        if (visited[x, y] || elements[x, y].Used() == true)
        {
            return;
        }

        elements[x, y].LoadTexture(AdjacentMines(x, y));

        if (AdjacentMines(x, y) > 0)
        {
            return;
        }

        visited[x, y] = true;

        FFuncover(x - 1, y);
        FFuncover(x + 1, y);
        FFuncover(x, y - 1);
        FFuncover(x, y + 1);
    }
}

using UnityEngine;
using System.Collections;

public class Element : MonoBehaviour
{
    public Sprite[] emptyTextures;
    public Sprite mineTexture;
    public Sprite flagTexture;
    public Sprite questionTexture;
    public Sprite defaultTexture;

    [HideInInspector]
    public bool mine;
    private int x, y;

    public void Init(int x, int y)
    {
        this.x = x;
        this.y = y;
        this.mine = Random.value < 0.15f;
    }

    public void LoadTexture(int adjacentCount)
    {
        if (adjacentCount == -1)//还原
        {
            if (this.mine)
            {
                this.GetComponent<SpriteRenderer>().sprite = mineTexture;
            }
            else
            {
                this.GetComponent<SpriteRenderer>().sprite = defaultTexture;
            }
        }
        else if (adjacentCount == -2)
        {
            this.GetComponent<SpriteRenderer>().sprite = flagTexture;
        }
        else if (adjacentCount == -3)
        {
            this.GetComponent<SpriteRenderer>().sprite = questionTexture;
        }
        else if (this.mine)
        {
            this.GetComponent<SpriteRenderer>().sprite = mineTexture;
        }
        else
        {
            this.GetComponent<SpriteRenderer>().sprite = emptyTextures[adjacentCount];
        }
    }

    public bool Used()
    {
        if (IsCovered())
        {
            return false;
        }

        if (IsFlag())
        {
            return false;
        }

        if (IsQuestion())
        {
            return false;
        }

        return true;
    }

    public bool IsCovered()
    {
        return this.GetComponent<SpriteRenderer>().sprite.texture.name == "default";
    }

    public bool IsFlag()
    {
        return this.GetComponent<SpriteRenderer>().sprite.texture.name == "flag";
    }

    public bool IsQuestion()
    {
        return this.GetComponent<SpriteRenderer>().sprite.texture.name == "question";
    }

    void OnMouseUpAsButton()
    {
        if (Minesweeper.Instance.state != Minesweeper.State.RUNING || this.Used() == true)
        {
            return;
        }

        if(Minesweeper.Instance.clickMode == Minesweeper.ClickMode.Flag)
        {
            if (this.IsFlag())
            {
                this.LoadTexture(-1);
            }
            else
            {
                this.LoadTexture(-2);
            }
            return;
        }

        if (Minesweeper.Instance.clickMode == Minesweeper.ClickMode.Question)
        {
            if (this.IsQuestion())
            {
                this.LoadTexture(-1);
            }
            else
            {
                this.LoadTexture(-3);
            }
            return;
        }
        if (this.mine)
        {
            Minesweeper.Instance.state = Minesweeper.State.LOSE;
            Minesweeper.Instance.UncoverMines();
        }
        else
        {
            Minesweeper.Instance.ResetVisited();
            Minesweeper.Instance.FFuncover(x, y);

            if (Minesweeper.Instance.IsWinFinished())
            {
                Minesweeper.Instance.state = Minesweeper.State.WIN;
            }
        }
    }
}

using Godot;
using System;
using System.Collections.Generic;
using Utils;

public class GroupedPapers : MoveableItem
{
    [Export] private Texture _halfClipTexture;

    private Sprite _clip;
    private Node2D _papers;

    public override Vector2 GetSize => (_papers.GetChildCount() > 0) ? _papers.GetChild<PaperSheet>(0).GetSize : _clip.Texture.GetSize() * Scale;
    public override bool CanBeThrownAway => true;
    public override Texture GetMainTexture => (_papers.GetChildCount() > 0) ? _papers.GetChild<PaperSheet>(0).GetMainTexture : _clip.Texture;

    private const int _paperScale = 10;

    public override void _Ready()
    {
        _papers = GetChild<Node2D>(0);
        base._Ready();
    }

    public override void _Process(float delta)
    {
        base._Process(delta);
    }

    public void Constructor(PaperClip clip, PaperSheet paper)
    {
        _papers = GetChild<Node2D>(0);
        Position = paper.Position;

        _clip = clip.GetClipSprite();
        _clip.GetParent().RemoveChild(_clip);
        AddChild(_clip);
        _clip.Scale = clip.Scale;
        _clip.Position = /*paper.Position -*/ -(paper.GetSize / 2) + new Vector2(20, clip.GetSize.y / 3 + 3);
        _clip.Texture = _halfClipTexture;
        clip.QueueFree();

        AddPaper(paper);
    }

    public void AddPaper(PaperSheet paper)
    {
        paper.GetParent().RemoveChild(paper);

        _papers.AddChild(paper);
        _papers.MoveChild(paper, _papers.GetChildCount() - 1);
        paper.SetGrouped();

        paper.Position = new Vector2((GameManager.rand.Randf() - .5f) * 6, (GameManager.rand.Randf() - .5f) * 6);
    }

    public override void Pressed()
    {
        base.Pressed();
        Flip();
    }
    public override void RightClicked()
    {
        base.RightClicked();
        Flip();
    }

    private int _topPaperIdx = 0;
    private void Flip()
    {
        if (_topPaperIdx >= _papers.GetChildCount()) return;

        _papers.GetChild<PaperSheet>(_papers.GetChildCount() - 1 - _topPaperIdx).PlayFlipAnim(MoveToBack);
        _topPaperIdx++;
        //_papers.MoveChild(_papers.GetChild<PaperSheet>(0), 0);
        //_papers.GetChild<PaperSheet>(_papers.GetChildCount() - 1).Visible = true;
    }

    private void MoveToBack(PaperSheet paper)
    {
        _papers.MoveChild(paper, 0);
        _topPaperIdx--;
    }

    public PaperSheet GetFirstPaper()
    {
        return _papers.GetChild<PaperSheet>(_papers.GetChildCount() - 1 - _topPaperIdx);
    }

    public bool IsContractMet(Contract contract)
    {
        if (contract.GetNeededStamps.Count != _papers.GetChildCount()) return false;

        List<(StampShape, StampColor)> remainingPapers = new List<(StampShape, StampColor)>();
        for (int i = 0; i < _papers.GetChildCount(); i++)
        {
            if (!_papers.GetChild<PaperSheet>(i).IsStamped) return false;
            remainingPapers.Add(_papers.GetChild<PaperSheet>(i).LastStamp);
        }

        for (int i = 0; i < contract.GetNeededStamps.Count; i++)
        {
            bool found = false;
            for (int j = 0; j < remainingPapers.Count; j++)
            {
                if ((remainingPapers[j].Item1 == contract.GetNeededStamps[i].Item1) && (remainingPapers[j].Item2 == contract.GetNeededStamps[i].Item2))
                {
                    remainingPapers.RemoveAt(j);
                    found = true;
                    break;
                }
            }
            if (!found) return false;
        }
        return true;
    }
}

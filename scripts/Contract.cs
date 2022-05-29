using Godot;
using System;
using System.Collections.Generic;

public class Contract : MoveableItem
{
    [Export] private PackedScene StampMark;
    [Export] private Texture acceptTexture, declineTexture, otherTexture;

    private Node2D _graphicsParent, _marksParent;

    private NumberDisplay[] _texts;
    private Sprite _seal;

    private Sprite _mainSprite;
    private AudioStreamPlayer _errorAudio, _coinsAudio;

    public bool IsSigned => _latestColor.HasValue && (_latestColor.Value == StampColor.Red || _latestColor.Value == StampColor.Green);
    public StampColor GetStampColor => _latestColor.Value;

    private StampColor? _latestColor = null;

    public override Vector2 GetSize => _mainSprite.Texture.GetSize() * _mainSprite.Scale;
    public override bool CanBeThrownAway => true;
    public override Texture GetMainTexture => _mainSprite.Texture;


    public int CoinsValue => _coins;
    private int _coins;

    public List<(StampShape, StampColor)> GetNeededStamps => _shapes;
    private List<(StampShape, StampColor)> _shapes;

    public bool Sealed => _seal.Visible;


    public void Constructor(int coins, List<(StampShape, StampColor)> shapes)
    {
        _graphicsParent = GetChild<Node2D>(0);
        _texts = new NumberDisplay[_graphicsParent.GetChildCount() - 2];
        for (int i = 0; i < _texts.Length; i++)
            _texts[i] = _graphicsParent.GetChild<NumberDisplay>(i + 1);

        _coins = coins;
        _shapes = shapes;

        _texts[0].Text = $"{coins} coins";
        _texts[1].Text = $"{shapes.Count} pages";
        _texts[2].Text = $"stamp with";

        string s = "";
        for (int i = 0; i < shapes.Count; i++)
        {
            s += $"\\{(char)((int)shapes[i].Item2 + 'a')}{(char)((int)shapes[i].Item1 + 'a')}";
        }

        _texts[3].Text = s;
        _texts[4].Text = "paperclip";
    }

    public void Seal()
    {
        _seal.Visible = true;
    }

    public override void _Ready()
    {
        _graphicsParent = GetChild<Node2D>(0);
        _mainSprite = _graphicsParent.GetChild<Sprite>(0);

        _marksParent = GetChild<Node2D>(1);
        _errorAudio = GetChild<AudioStreamPlayer>(2);
        _coinsAudio = GetChild<AudioStreamPlayer>(3);

        _texts = new NumberDisplay[_graphicsParent.GetChildCount() - 2];
        for (int i = 0; i < _texts.Length; i++)
            _texts[i] = _graphicsParent.GetChild<NumberDisplay>(i + 1);
        _seal = _graphicsParent.GetChild<Sprite>(_graphicsParent.GetChildCount() - 1);
        _seal.Visible = false;

        base._Ready();
    }

    public override void _Process(float delta)
    {
        base._Process(delta);

        if (_disappearAnimPlaying)
        {
            if (_targetPos.DistanceTo(Position) <= 20)
            {
                Controls.Singleton.ContractDisappeared(this);
                _disappearAnimPlaying = false;
            }
        }
    }

    public void Stamp(Vector2 pos, StampColor color)
    {
        _latestColor = color;

        Texture texture = otherTexture;
        if (color == StampColor.Red) texture = declineTexture;
        if (color == StampColor.Green) texture = acceptTexture;

        SpawnMark(pos - Position + GetSize / 2, StampColorValue.GetColor(color), texture);
    }

    private void SpawnMark(Vector2 pos, Color clr, Texture texture)
    {
        Sprite mark = StampMark.Instance<Sprite>();
        _marksParent.AddChild(mark);

        mark.Visible = true;
        mark.Modulate = clr;

        Vector2 pixelPos = (pos / _mainSprite.Scale) - new Vector2(24, 7) / 2;

        mark.Material = (Material)mark.Material.Duplicate();
        ((ShaderMaterial)mark.Material).SetShaderParam("stampPos", pixelPos);
        ((ShaderMaterial)mark.Material).SetShaderParam("stampMark", texture);
    }

    private bool _disappearAnimPlaying = false;
    public void PlayDisappearAnim(Vector2 target)
    {
        _disappearAnimPlaying = true;
        Move(target, true, true);
    }

    public void PlayErrorAudio()
    {
        _errorAudio.Play();
    }
    public void PlayCoinsAudio()
    {
        _coinsAudio.Play();
    }
}

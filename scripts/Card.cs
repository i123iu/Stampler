using Godot;
using System;
using System.Collections.Generic;
using Utils;

public class Card : MoveableItem
{
    private static Dictionary<CardType, Texture> _textures;

    static Card()
    {
        _textures = new Dictionary<CardType, Texture>();
        Array arr = Enum.GetValues(typeof(CardType));
        for (int i = 0; i < arr.Length; i++)
        {
            Texture texture = ResourceLoader.Load<Texture>($"res://cards/{(CardType)arr.GetValue(i)}.png");
            _textures.Add((CardType)arr.GetValue(i), texture);
        }
    }

    public enum CardType
    {
        Pes, Kocka, 
    }

    public override Vector2 GetSize => (IsFlipped ? _frontSprite : _backSprite).Texture.GetSize() * 8;
    public override bool CanBeThrownAway => true;
    public override Texture GetMainTexture => _frontSprite.Texture;

    /// <summary> Is the front side visible? </summary>
    public bool IsFlipped { get; private set; }

    private Node2D _cardSprites;
    private Sprite _backSprite, _frontSprite;
    private Node2D _frontGraphics;

    private CardType _cardType;

    public override void _Ready()
    {
        _cardSprites = GetChild<Node2D>(0);
        _backSprite = _cardSprites.GetChild<Sprite>(0);
        _frontSprite = _cardSprites.GetChild<Sprite>(1);
        _frontSprite.Visible = false;

        _frontGraphics = GetChild<Node2D>(1);
        _frontGraphics.Visible = false;

        base._Ready();
    }

    private bool _initialized = false;
    public void SetCardType(CardType cardType)
    {
        if (_initialized) throw new Exception("Already initialized");
        _initialized = true;

        _cardType = cardType;

        _frontSprite.Texture = _textures[_cardType];
    }

    public override void _Process(float delta)
    {
        base._Process(delta);

        if (IsFlipping)
        {
            _scaleAnim.Process(delta);
            _cardSprites.Scale = new Vector2(_scaleAnim.GetCurrentState(), _cardSprites.Scale.y);
            _frontGraphics.Scale = new Vector2(Mathf.Abs(_scaleAnim.GetCurrentState()) * 8, 8);

            if (_scaleAnim.Finished)
            {
                if (_nextScaleAnim != null)
                {
                    _backSprite.Visible = !IsFlipped;
                    _frontSprite.Visible = IsFlipped;
                    _frontGraphics.Visible = IsFlipped;
                }

                _scaleAnim = _nextScaleAnim;
                _nextScaleAnim = null;
            }
        }
    }

    private bool IsFlipping => _scaleAnim != null;
    private Anim<float> _scaleAnim, _nextScaleAnim;


    public override void Pressed()
    {
        IsFlipped = !IsFlipped;
        PlayFlipAnim();
    }
    public override void RightClicked()
    {
        IsFlipped = !IsFlipped;
        PlayFlipAnim();
    }

    private void PlayFlipAnim()
    {
        _scaleAnim = new Anim<float>(new AnimPhases<float>(IsFlipped ? 1 : -1,
            new FloatAnim(AnimationPhase<float>.CurveType.Linear, Controls.Singleton.CardFlipAnimLength, 0)));
        _nextScaleAnim = new Anim<float>(new AnimPhases<float>(0,
            new FloatAnim(AnimationPhase<float>.CurveType.Linear, Controls.Singleton.CardFlipAnimLength, IsFlipped ? -1 : 1)));
    }
}

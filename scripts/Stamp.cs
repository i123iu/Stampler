using Godot;
using System;
using Utils;

public class Stamp : MoveableItem
{
    [Export] public bool IsContractStamp = false;

    [Export] public float LEGNTH_ANIM_1 = 1;
    [Export] public float AMOUNT_ANIM_1 = 1;

    [Export] public float LEGNTH_ANIM_2 = 1;
    [Export] public float AMOUNT_ANIM_2 = -1;

    [Export] public float LEGNTH_ANIM_3 = 1;
    [Export] public float AMOUNT_ANIM_3 = -1;

    private Node2D _graphics;
    private Sprite _sprite, _inkSprite;
    private AudioStreamPlayer _audio, _audioWater;
    private Sprite _shapeSprite;

    public bool HasInk => _hasInk;
    public StampColor GetStampColor => !HasInk ? throw new Exception() : _stampColor;

    private bool _hasInk = false;
    private StampColor _stampColor;

    public StampShape GetStampShape => shape;
    [Export] private StampShape shape;

    public override Vector2 GetSize => _sprite.Texture.GetSize() * Scale;
    public override bool CanBeThrownAway => false;
    public override Texture GetMainTexture => _sprite.Texture;

    public void Constructor(StampShape shape)
    {
        this.shape = shape;
        _shapeSprite.RegionRect = StampShapeValue.GetRect(shape);
    }
    public override void _Ready()
    {
        _graphics = GetChild<Node2D>(0);
        _audio = GetChild<AudioStreamPlayer>(1);
        _audioWater = GetChild<AudioStreamPlayer>(2);

        _sprite = _graphics.GetChild<Sprite>(0);
        _inkSprite = _graphics.GetChild<Sprite>(1);
        _inkSprite.Visible = false;
        if (!IsContractStamp)
        {
            _shapeSprite = _graphics.GetChild<Sprite>(2);
            Constructor(shape);
        }

        base._Ready();
    }

    public override void _Process(float delta)
    {
        _graphics.Position = new Vector2();
        base._Process(delta);

        if (_anim != null)
        {
            _anim.Process(delta);
            _graphics.Position = new Vector2(0, _anim.GetCurrentState());

            if ((_anim.GetPhaseIdx == 2) && !_hasPrinted)
            {
                _hasPrinted = true;

                Controls.Singleton.StampPrinted(this, Position + new Vector2(0, GetSize.y / 2 - 1 * Scale.y));
                _audio.Play(.13f);
            }

            if (_anim.Finished) _anim = null;
        }

        if (_audioWater.Playing && _audioWater.GetPlaybackPosition() > .16f)
            _audioWater.Stop();
    }

    public void SetInk(StampColor color)
    {
        _hasInk = true;
        _stampColor = color;
        _inkSprite.Visible = true;
        _inkSprite.Modulate = StampColorValue.GetColor(_stampColor);
        _audioWater.Play();
    }
    public void RemoveInk()
    {
        _hasInk = false;
        _inkSprite.Visible = false;
    }

    private bool _hasPrinted = false;
    private Anim<float> _anim;

    public override void RightClicked()
    {

        _anim = new Anim<float>(new AnimPhases<float>(0,
            new FloatAnim(AnimationPhase<float>.CurveType.Linear, LEGNTH_ANIM_1, AMOUNT_ANIM_1),
            new FloatAnim(AnimationPhase<float>.CurveType.Linear, LEGNTH_ANIM_2, AMOUNT_ANIM_2),
            new FloatAnim(AnimationPhase<float>.CurveType.Linear, LEGNTH_ANIM_3, AMOUNT_ANIM_3)
            ));
        _hasPrinted = false;

    }
}

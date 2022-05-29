using Godot;
using System;

public class Arrow : Node2D
{
    [Export] public bool Shown = false;
    [Export] private float Speed = 1;
    [Export] private float StartPos = 1;

    private Sprite _arrowSprite;
    private AnimatedSprite _anim;
    private Sprite _animCircle, _animCircleBack;

    private AudioStreamPlayer _acceptAudio;

    public override void _Ready()
    {
        _animCircleBack = GetChild<Sprite>(0);
        _arrowSprite = GetChild<Sprite>(1);
        _arrowSprite.Position = new Vector2(StartPos, 0);
        _anim = GetChild<AnimatedSprite>(2);
        _animCircle = GetChild<Sprite>(3);
        _acceptAudio = GetChild<AudioStreamPlayer>(4);
    }

    public override void _Process(float delta)
    {
        Position = new Vector2(GetViewportRect().Size.x - 100, GetViewportRect().Size.y - 100);

        if (_anim.IsPlaying() && _anim.Frame == 12)
        {
            _animDoneCallback();
            _anim.Stop();
            _anim.Frame = 0;
        }

        _animCircle.Visible = _animCircleBack.Visible = _anim.Visible = _anim.IsPlaying();
        _arrowSprite.Visible = !_anim.IsPlaying();


        if (Shown)
        {
            _arrowSprite.Position = new Vector2(Mathf.Clamp(_arrowSprite.Position.x - delta * Speed, 0, StartPos), 0);
        }
        else
        {
            _arrowSprite.Position = new Vector2(Mathf.Clamp(_arrowSprite.Position.x + delta * Speed, 0, StartPos), 0);
        }
    }

    private Action _animDoneCallback;
    public void StartCircleAnim(Action animDoneCallback)
    {
        _animDoneCallback = animDoneCallback;
        _anim.Play();
        _anim.Frame = 0;
        Shown = false;

        _acceptAudio.Play(.12f);
    }
}

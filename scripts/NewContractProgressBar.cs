using Godot;
using System;

public class NewContractProgressBar : Node2D
{ 
    private AnimatedSprite _anim;

    public override void _Ready()
    {
        _anim = GetChild<AnimatedSprite>(1);

        _anim.Play();

        Position = new Vector2(50, GetViewportRect().Size.y - 50);
    }

    public override void _Process(float delta)
    {
        if (!IsContractLoaded)
        {
            if (_anim.Frame == 12)
            {
                _anim.Stop();
                IsContractLoaded = true;
            }
        }
    }

    public bool IsContractLoaded { get; private set; } = false;
    public void TakeLoadedContract()
    {
        if (!IsContractLoaded) throw new Exception();

        IsContractLoaded = false;
        _anim.Play();
        _anim.Frame = 0;
    }
}

using Godot;
using System;

public class PaperClip : MoveableItem
{
    private Sprite _sprite;

    public override Vector2 GetSize => _sprite.Texture.GetSize() * Scale;
    public override bool CanBeThrownAway => true;
    public override Texture GetMainTexture => _sprite.Texture;

    public override void _Ready()
    {
        _sprite = GetChild<Sprite>(0);
        base._Ready();
    }

    protected override bool IsColorInvisible(Color clr, int x, int y)
    {
        if ((x > 2) && (x < 12) && (y > 5) && (y < 26)) return false;
        if (clr.a <= 0.01f) return true;
        return false;
    }

    public Sprite GetClipSprite()
        => _sprite;
}

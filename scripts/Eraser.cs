using Godot;
using System;

public class Eraser : MoveableItem
{
    private Sprite _sprite;

    public override Vector2 GetSize => _sprite.Texture.GetSize() * Scale;
    public override bool CanBeThrownAway => false;
    public override Texture GetMainTexture => _sprite.Texture;

    public Vector2 GetTipPos()
    {
        return Position + new Vector2(-GetSize.x / 2, GetSize.y / 2);
    }

    public override void _Ready()
    {
        _sprite = GetChild<Sprite>(0);
        base._Ready();
    }
}

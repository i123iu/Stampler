using Godot;
using System;

public class CardApproval : MoveableItem
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
}

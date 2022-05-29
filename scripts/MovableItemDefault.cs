using Godot;
using System;

public class MovableItemDefault : MoveableItem
{
    [Export] private bool canBeThrownAway = true;

    private Sprite _sprite;

    public override Vector2 GetSize => _sprite.Texture.GetSize() * Scale;
    public override bool CanBeThrownAway => canBeThrownAway;
    public override Texture GetMainTexture => _sprite.Texture;

    public override void _Ready()
    {
        _sprite = GetChild<Sprite>(0);
        base._Ready();
    }
}

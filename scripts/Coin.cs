using Godot;
using System;

public class Coin : MoveableItem
{
    private Sprite _sprite;

    public override Vector2 GetSize => _sprite.Texture.GetSize() * Scale;
    public override bool CanBeThrownAway => true;
    public override Texture GetMainTexture => _sprite.Texture;
    protected override bool Enabled => !_inserted;

    public override void _Ready()
    {
        _sprite = GetChild<Sprite>(0);
        base._Ready();
    }

    private bool _inserted = false;
    public void SetInserted()
    {
        _inserted = true;
    }
}

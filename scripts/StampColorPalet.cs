using Godot;
using System;

public class StampColorPalet : MoveableItem
{
    [Export] public StampColor stampColor;

    public StampColor GetStampColor => stampColor;

    private Sprite _sprite;

    public override Vector2 GetSize => _sprite.Texture.GetSize() * Scale;
    public override bool CanBeThrownAway => false;
    public override Texture GetMainTexture => _sprite.Texture;


    public void Constructor(StampColor stampColor)
    {
        this.stampColor = stampColor;
        _sprite.Modulate = StampColorValue.GetColor(stampColor);
    }
    public override void _Ready()
    {
        _sprite = GetChild<Sprite>(0);
        _sprite.Modulate = StampColorValue.GetColor(stampColor);

        base._Ready();
    }

    public override void _Process(float delta)
    {
        base._Process(delta);
    }
}

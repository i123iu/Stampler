using Godot;
using System;

public class BankNote : MoveableItem
{
    private int _value;
    public int GetValue => _value;

    private Sprite _sprite;
    private NumberDisplay _numberDisplay;

    public override Vector2 GetSize => _sprite.Texture.GetSize() * Scale;
    public override bool CanBeThrownAway => true;
    public override Texture GetMainTexture => _sprite.Texture;

    public override void _Ready()
    {
        _sprite = GetChild<Sprite>(0);
        _numberDisplay = GetChild<NumberDisplay>(1);

        base._Ready();
    }

    public void SetValue (int value)
    {
        _value = value;
        _numberDisplay.Text = value.ToString();
    }
}
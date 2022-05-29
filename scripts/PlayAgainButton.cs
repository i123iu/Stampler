using Godot;
using System;

public class PlayAgainButton : TextureButton
{
    [Export] public NodePath playAgainButtonPath;
    private GameOverScreen _gameOverScreen;

    public override void _Ready()
    {
        _gameOverScreen = GetNode<GameOverScreen>(playAgainButtonPath);
    }

    public override void _Pressed()
    {
        _gameOverScreen.PlayAgain();
    }
}

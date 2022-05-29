using Godot;
using System;

public class GameManager : Node
{
    public static RandomNumberGenerator rand = new RandomNumberGenerator();

    [Export] private PackedScene gameOverScreenPrefab, mainScenePrefab;

    private GameOverScreen _gameOverScreen;
    private Node _mainScene;
    private Controls _controls;

    public override void _Ready()
    {
        _gameOverScreen = gameOverScreenPrefab.Instance<GameOverScreen>();
        GetChild<CanvasLayer>(0).AddChild(_gameOverScreen);
        _gameOverScreen.SetGameManager(this);
        SpawnMainScene();
    }

    public void PlayAgain()
    {
        SpawnMainScene();
    }

    private void SpawnMainScene()
    {
        if(_mainScene != null)
        {
            _mainScene.QueueFree();
            _mainScene = null;
        }

        _mainScene = mainScenePrefab.Instance<Node>();
        AddChild(_mainScene);
        _controls = _mainScene.GetChild<Controls>(3);
        _controls.SetGameManager(this);
    }

    public void EndGame(GameStats stats)
    {
        _gameOverScreen.EndGame(stats);
    }
}

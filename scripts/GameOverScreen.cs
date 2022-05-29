using Godot;
using System;

public class GameOverScreen : Control
{
    [Export] private float AnimLength = 2f;
    [Export] private int noTexts = 6;

    private GameManager _gameManager;

    private CanvasText[] _texts;

    public void SetGameManager(GameManager gameManager)
    {
        _gameManager = gameManager;
    }
    public override void _Ready()
    {
        _texts = new CanvasText[noTexts];
        for (int i = 0; i < noTexts; i++)
            _texts[i] = GetChild(1).GetChild<CanvasText>(i);

        Visible = false;
        _isEnding = false;
        _endingAnimTime = -1;
    }

    public void EndGame(GameStats stats)
    {
        Visible = true;
        Modulate = new Color(1, 1, 1, 0);
        _isEnding = true;
        _endingAnimTime = 0;

        _texts[0].Text = $"game over";
        _texts[1].Text = $"you scored {(int)(stats.timePlayed)} points";
        _texts[2].Text = $"";
        _texts[3].Text = $"{stats.contractsFinished} contracts finished";
        _texts[4].Text = $"{stats.contractsDeclined} contracts declined";
        _texts[5].Text = $"{stats.stamps} stamps given";
        _texts[6].Text = $"{stats.paperclipsUsed} paperclips used";
    }

    public override void _Process(float delta)
    {
        if (_isEnding)
        {
            _endingAnimTime += delta;
            if (_endingAnimTime > AnimLength) _endingAnimTime = AnimLength;

            Modulate = new Color(1, 1, 1, _endingAnimTime / AnimLength);
        }
    }

    public void PlayAgain()
    {
        _gameManager.PlayAgain();

        Visible = false;
        _isEnding = false;
        _endingAnimTime = -1;
    }

    private bool _isEnding = false;
    private float _endingAnimTime = 0;
}

public class GameStats
{
    public int contractsFinished = 0, contractsDeclined = 0;
    public int stamps = 0;
    public float timePlayed = 0;
    public int paperclipsUsed = 0;
}
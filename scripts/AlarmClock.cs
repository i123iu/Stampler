using Godot;
using System;

public class AlarmClock : MoveableItem
{
    [Export] public float AnimDownAmount = 50;
    [Export] public float AnimLength = 1;

    private Node2D _top, _coin, _bot;

    private Sprite _spriteBot;
    private AudioStreamPlayer _audio1, _audio2, _audioCoin;

    private NumberDisplay _numberDisplayM, _numberDisplayS;
    private int _lastSum;

    public override Vector2 GetSize => _spriteBot.Texture.GetSize() * _spriteBot.GetParent<Node2D>().Scale;
    public override bool CanBeThrownAway => false;
    public override Texture GetMainTexture => _spriteBot.Texture;

    public override void _Ready()
    {
        _top = GetChild<Node2D>(0);
        _coin = GetChild<Node2D>(1);
        _bot = GetChild<Node2D>(2);

        _spriteBot = _bot.GetChild<Sprite>(0);
        _numberDisplayM = _bot.GetChild<NumberDisplay>(1);
        _numberDisplayS = _bot.GetChild<NumberDisplay>(2);
        _audio1 = _bot.GetChild<AudioStreamPlayer>(3);
        _audio2 = _bot.GetChild<AudioStreamPlayer>(4);
        _audioCoin = _bot.GetChild<AudioStreamPlayer>(5);

        base._Ready();
    }

    private int _lastSoundIdx = 0;
    public override void _Process(float delta)
    {
        base._Process(delta);

        string m = Controls.Singleton.TimeLeft.min.ToString();
        while (m.Length < 2) m = '0' + m;
        _numberDisplayM.Text = m;

        string s = Controls.Singleton.TimeLeft.sec.ToString();
        while (s.Length < 2) s = '0' + s;
        _numberDisplayS.Text = s;

        int currSum = Controls.Singleton.TimeLeft.min * 60 + Controls.Singleton.TimeLeft.sec;

        if (currSum != _lastSum)
        {
            if ((_lastSoundIdx++) % 2 == 0)
                _audio1.Play();
            else
                _audio2.Play();
            _lastSum = currSum;
        }

        if (_coin.GetChildCount() > 0)
        {
            ShowAnim();
            _animTime += delta;

            if (_animTime >= AnimLength)
            {
                _animTime = AnimLength;
                ShowAnim();
                _coin.GetChild<Coin>(0).QueueFree();
            }
        }
    }

    private float _animTime;
    public void InsertCoin (Coin coin)
    {
        if (_coin.GetChildCount() > 0)
            _coin.GetChild(0).QueueFree();

        coin.SetInserted();
        coin.GetParent().RemoveChild(coin);
        _coin.AddChild(coin);
        coin.Position = new Vector2();
        _animTime = 0;

        _audioCoin.Play();
    }

    private void ShowAnim()
    {
        _coin.GetChild<Node2D>(0).Position = new Vector2(0, (_animTime / AnimLength) * (AnimDownAmount));
    }
}

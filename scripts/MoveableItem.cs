using Godot;
using System;
using Utils;

public abstract class MoveableItem : Node2D
{
    public abstract Vector2 GetSize { get; }
    public abstract bool CanBeThrownAway { get; }
    public abstract Texture GetMainTexture { get; }

    private Image _texture;

    protected virtual bool Enabled => true;

    public bool IsDestroyed => _isDestroyed;

    private bool _isDestroyed = false;
    private bool _isChildItem = false;

    private MoveableItem _childItem;
    public MoveableItem ChildItem
    {
        get
        {
            return _childItem;
        }
        set
        {
            if (_childItem != null) _childItem._isChildItem = false;
            _childItem = value;
            if (_childItem != null) _childItem._isChildItem = true;
        }
    }

    public bool IsBeingMoved { get; set; }

    protected Vector2 _targetPos;
    public Vector2 TargetPos => _targetPos;

    public override void _Ready()
    {
        _targetPos = Position;
        _texture = GetMainTexture.GetData();
        _texture.Lock();
    }

    private const float gameOverAnimLen = 1;
    private float _gameOverAnimTime = -1;
    private Vector2 _gameOverScaleBefore;

    public override void _Process(float delta)
    {
        if (_gameOverAnimTime != -1)
        {
            _gameOverAnimTime += delta;
            Scale = _gameOverScaleBefore * Mathf.Clamp(1 - _gameOverAnimTime / gameOverAnimLen, 0, float.MaxValue);
            return;
        }

        if (_isDestroyed) return;
        if (_isChildItem) return;
        if (!Enabled) return;

        Vector2 diff = _targetPos - Position;
        diff *= Controls.Singleton.SmoothAmount * (_slowed ? .1f : 1f);
        Position += diff;

        if (_childItem != null)
        {
            _childItem.Position = _childItem._targetPos = Position;
        }
    }

    public void PlayGameOverAnim()
    {
        _gameOverAnimTime = 0;
        _gameOverScaleBefore = Scale;
    }

    public virtual void Pressed()
    { }
    public virtual void RightClicked()
    { }

    private bool _slowed = false;
    public void Move(Vector2 pos, bool force = false, bool slowDown = false)
    {
        _targetPos = pos;
        _slowed = slowDown;
        if (!force)
            ClampTargetPos();
    }

    public void MoveInstantly(Vector2 pos)
    {
        _targetPos = Position = pos;
    }

    private void ClampTargetPos()
    {
        Vector2 viewportSize = GetViewportRect().Size;
        if (_targetPos.x - GetSize.x / 2 < 0) _targetPos.x = GetSize.x / 2;
        if (_targetPos.x + GetSize.x / 2 >= viewportSize.x) _targetPos.x = viewportSize.x - GetSize.x / 2;

        if (_targetPos.y - GetSize.y / 2 < 0) _targetPos.y = GetSize.y / 2;
        if (_targetPos.y + GetSize.y / 2 >= viewportSize.y) _targetPos.y = viewportSize.y - GetSize.y / 2;
    }

    public bool IsInside(Vector2 pos)
    {
        if (pos.x < Position.x - GetSize.x / 2) return false;
        if (pos.y < Position.y - GetSize.y / 2) return false;

        if (pos.x > Position.x + GetSize.x / 2) return false;
        if (pos.y > Position.y + GetSize.y / 2) return false;

        Vector2 textPos = (Position - pos + GetSize / 2) / (GetSize / _texture.GetSize());
        textPos = _texture.GetSize() - textPos;

        int x = Mathf.Clamp((int)textPos.x, 0, _texture.GetWidth() - 1);
        int y = Mathf.Clamp((int)textPos.y, 0, _texture.GetHeight() - 1);

        Color clr = _texture.GetPixel(x, y);

        if (IsColorInvisible(clr, x, y))
            return false;

        return true;
    }

    protected virtual bool IsColorInvisible(Color clr, int x, int y)
        => clr.a <= 0.01f;

    public void SetDestroyed()
    {
        _isDestroyed = true;
    }
}

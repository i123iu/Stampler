using Godot;
using System;
using System.Collections.Generic;
using Utils;

public class PaperSheet : MoveableItem
{
    [Export] private Texture Font;
    private Image _fontImage;

    private Sprite _sprite;
    private Sprite _canvas;
    private AudioStreamPlayer _audio;

    private ImageTexture _canvasTexture;

    public override Vector2 GetSize => _sprite.Texture.GetSize() * Scale;
    public override bool CanBeThrownAway => true;
    public override Texture GetMainTexture => _sprite.Texture;
    protected override bool Enabled => !_grouped;

    public override void _Ready()
    {
        _sprite = GetChild<Sprite>(0);
        _canvas = GetChild<Sprite>(1);
        _audio = GetChild<AudioStreamPlayer>(2);
        base._Ready();

        _fontImage = Font.GetData();
        _fontImage.Lock();

        Image img = _sprite.Texture.GetData().Duplicate() as Image;
        img.Fill(new Color(0, 0, 0, 1));
        img.CreateFromData(img.GetWidth(), img.GetHeight(), false, Image.Format.Rgba8, TranformRGBtoRGBA(img.GetData()));
        img.Fill(new Color(0, 0, 0, 0));


        _canvasTexture = new ImageTexture();
        _canvasTexture.CreateFromImage(img, 0);
        _canvas.Texture = _canvasTexture;
    }
    public override void _Process(float delta)
    {
        base._Process(delta);

        if (_anim != null)
        {
            if (_anim.Process(delta))
            {
                if (_anim.GetPhaseIdx == 1)
                {
                    _halfFlippedCallback(this);
                }
            }
            ShowAnim();
            if (_anim.Finished) _anim = null;
        }
    }
    private void ShowAnim()
    {
        _sprite.Position = new Vector2(_anim.GetCurrentState().x, 0);
        _sprite.Scale = new Vector2(_anim.GetCurrentState().y, 1);

        _canvas.Position = new Vector2(_anim.GetCurrentState().x, 0);
        _canvas.Scale = new Vector2(_anim.GetCurrentState().y, 1);

        _canvas.Visible = (_anim.GetCurrentState().y > 0);

        _sprite.Modulate = _canvas.Visible ? new Color(1, 1, 1, 1) : new Color(.9f, .9f, .9f, 1);
    }

    public override void Pressed()
    {
        base.Pressed();
        // PlayFlipAnim();
    }

    private byte[] TranformRGBtoRGBA(byte[] bytes)
    {
        byte[] newBytes = new byte[bytes.Length * 4 / 3];

        for (int i = 0; i < bytes.Length / 3; i++)
        {
            newBytes[i + 0] = bytes[i + 0];
            newBytes[i + 1] = bytes[i + 1];
            newBytes[i + 2] = bytes[i + 2];
            newBytes[i + 3] = 0;
        }

        return newBytes;
    }

    public bool IsStamped => _lastStamp.HasValue;
    public (StampShape, StampColor) LastStamp => (!IsStamped) ? throw new Exception() : _lastStamp.Value;
    private (StampShape, StampColor)? _lastStamp;


    public void Stamp(Vector2 pos, Vector2 parentPos, StampShape shape, StampColor color)
    {
        _lastStamp = (shape, color);

        Image img = _canvasTexture.GetData();
        img.Lock();

        Vector2 textPosFrom = img.GetSize() - (parentPos - pos + GetSize / 2) / (GetSize / img.GetSize());
        int xMiddle = Mathf.Clamp((int)textPosFrom.x, 0, img.GetWidth() - 1);
        int yMiddle = Mathf.Clamp((int)textPosFrom.y, 0, img.GetHeight() - 1);

        Rect2 rect = StampShapeValue.GetRect(shape);
        for (int xi = 0; xi < 5; xi++)
            for (int yi = 0; yi < 5; yi++)
            {
                int x = xi - 2 + xMiddle;
                int y = yi - 2 + yMiddle;

                if (_fontImage.GetPixel((int)rect.Position.x + xi, yi).a >= .99f)
                {
                    if ((x < 0) || (y < 0)) continue;
                    if ((x >= img.GetWidth()) || (y >= img.GetHeight())) continue;

                    img.SetPixel(x, y, StampColorValue.GetColor(color));
                }
            }


        img.Unlock();
        _canvasTexture.SetData(img);
    }
    public void Draw(Vector2 pos, Vector2 parentPos)
    {
        if (_anim == null)
            WritePixel(pos, parentPos, new Color(0, 0, 0, 1));
    }

    public void Erase(Vector2 pos, Vector2 parentPos)
    {
        if (_anim == null)
            WritePixel(pos, parentPos, new Color(0, 0, 0, 0));
    }

    private void WritePixel(Vector2 pos, Vector2 parentPos, Color clr)
    {
        Image img = _canvasTexture.GetData();
        img.Lock();

        Vector2 textPosFrom = img.GetSize() - (parentPos - pos + GetSize / 2) / (GetSize / img.GetSize());
        int x = Mathf.Clamp((int)textPosFrom.x, 0, img.GetWidth() - 1);
        int y = Mathf.Clamp((int)textPosFrom.y, 0, img.GetHeight() - 1);

        img.SetPixel(x, y, clr);

        img.Unlock();
        _canvasTexture.SetData(img);
    }

    private bool _grouped = false;
    public void SetGrouped()
    {
        _grouped = true;
    }

    private const float len = .25f;

    private Anim<Vector3> _anim;
    private System.Action<PaperSheet> _halfFlippedCallback;
    public void PlayFlipAnim(System.Action<PaperSheet> halfFlippedCallback)
    {
        if (_anim != null)
        {
            _anim.SetFinished();
            ShowAnim();
            _anim = null;
        }

        _halfFlippedCallback = halfFlippedCallback;
        _anim = new Anim<Vector3>(new AnimPhases<Vector3>(
            new Vector3(0, 1, 0),
            new Vector3Anim(AnimationPhase<Vector3>.CurveType.Linear, 2 * len, new Vector3(-GetSize.x / Scale.x, -1, 0)),
            new Vector3Anim(AnimationPhase<Vector3>.CurveType.Linear, 2 * len, new Vector3(0, 1, 0))
            ));

        _audio.Play();

    }
}

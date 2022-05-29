using Godot;
using System;

[Tool]
public class CanvasText : Control
{
    private static Image _letters;

    private static int GetIdxInTextureFile(char ch)
    {
        if ((ch >= '0') && (ch <= '9')) return ch - '0';
        if ((ch >= 'a') && (ch <= 'z')) return ch - 'a' + 10;
        if ((ch >= 'A') && (ch <= 'Z')) return ch - 'A' + 10;
        throw new Exception();
    }
    private static Texture GetText(string text)
    {
        if (text.Length == 0) return null;

        int totalSize = 0;

        bool isSpecial = false;
        bool wasClr = false;

        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] == '\\')
            {
                isSpecial = true;
                wasClr = false;
                continue;
            }
            if (isSpecial && !wasClr)
            {
                wasClr = true;
                continue;
            }

            int currSize = 3;
            if ((text[i] == 'm') || (text[i] == 'w')) currSize = 5;
            else if (text[i] == 'n') currSize = 5;
            else if (text[i] == 'i') currSize = 2;
            else if (text[i] == ' ') currSize = 2;
            if (isSpecial)
            {
                currSize = 5;
                isSpecial = false;
                wasClr = false;
            }

            totalSize += currSize + 1;
        }

        Image img = new Image();
        img.Create(totalSize, 5, false, Image.Format.Rgba8);
        img.Lock();

        int pixelOffset = 0;
        isSpecial = false; int specialColor = -1;
        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] == '\\')
            {
                isSpecial = true;
                specialColor = -1;
                continue;
            }
            if (isSpecial && (specialColor == -1))
            {
                specialColor = text[i] - 'a';
                continue;
            }

            if (text[i] == ' ')
            {
                pixelOffset += 2;
                continue;
            }

            int currSize = 3;
            if ((text[i] == 'm') || (text[i] == 'w')) currSize = 5;
            else if (text[i] == 'n') currSize = 5;
            else if (text[i] == 'i') currSize = 2;
            if (isSpecial)
            {
                currSize = 5;
            }

            if (text[i] != ' ')
                WriteLetter(text[i], currSize, img, pixelOffset, isSpecial ? StampColorValue.GetColor((StampColor)specialColor) : new Color(0, 0, 0, 1));

            if (isSpecial)
            {
                isSpecial = false;
                specialColor = -1;
            }

            pixelOffset += currSize + 1;
        }

        img.Unlock();
        ImageTexture texture = new ImageTexture();
        texture.CreateFromImage(img, 0);
        if ((texture.Flags & 4) > 0)
            texture.Flags -= 4;
        return texture;
    }

    private static void WriteLetter(char letter, int width, Image img, int offset, Color color)
    {
        int letterOffset = 0;
        switch (width)
        {
            case 5: letterOffset = 0; break;
            case 4: letterOffset = 0; break;
            case 3: letterOffset = 1; break;
            case 2: letterOffset = 1; break;
            case 1: letterOffset = 1; break;
        }

        for (int xi = 0; xi < width; xi++)
        {
            for (int yi = 0; yi < 5; yi++)
            {
                int x = offset + xi;
                Color clr = _letters.GetPixel(GetIdxInTextureFile(letter) * 5 + xi + letterOffset, yi) * color;
                img.SetPixel(x, yi, clr);
            }
        }
    }

    [Export] private Texture _font;
    [Export] private string text;
    private TextureRect _texture;

    private string _text = "";

    public string Text { get { return _text; } set { _text = text = value; UpdateText(); } }

    public override void _Ready()
    {
        _text = text;
        _texture = GetChild<TextureRect>(0);
        _letters = _font.GetData();
        _letters.Lock();
    }

    public override void _Process(float delta)
    {
        if (_text != text)
        {
            GD.Print("CA");
            _text = text;
            UpdateText();
        }
    }

    private void UpdateText()
    {
        if (Engine.EditorHint)
        {
            _texture = GetChild<TextureRect>(0);
            _letters = _font.GetData();
            _letters.Lock();
        }
        _texture.Texture = GetText(_text);
        //_texture.RectSize = _texture.Texture.GetSize();
        //_texture.RectPosition = new Vector2(0, _texture.Texture.GetSize().y / 2);
    }
}

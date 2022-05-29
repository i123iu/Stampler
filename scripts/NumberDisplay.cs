using Godot;
using System;
using System.Collections.Generic;

[Tool]
public class NumberDisplay : Node2D
{
    [Export] public Texture digitsTexture;
    [Export] public bool AlignLeft = true;
    [Export] public Color DefaultColor = new Color(1, 1, 1, 1);

    [Export] public string Text;
    private int _lastHash = "".GetHashCode();
    //private List<Sprite> digits = new List<Sprite>(noDigits);

    public override void _Ready()
    {
        /*for (int i = 0; i < GetChildCount(); i++)
            GetChild(i).QueueFree();*/

        /*for (int i = 0; i < noDigits; i++)
        {
            Sprite sprite = digitPrefab.Instance<Sprite>();
            sprite.Visible = false;
            sprite.RegionEnabled = true;
            AddChild(sprite);
            digits.Add(sprite);
            sprite.Translate(new Vector2(i * 4 * Scale.x, 0));
        }*/
    }

    public override void _Process(float delta)
    {
        UpdateText();
    }

    private void UpdateText()
    {
        if (_lastHash == Text.GetHashCode()) return;

        _lastHash = Text.GetHashCode();

        for (int i = 0; i < GetChildCount(); i++)
            GetChild<Sprite>(i).Visible = false;

        if (Text.Length == 0) return;

        int totalSize = 0; bool isSpecial = false; bool wasClr = false;
        for (int i = 0; i < Text.Length; i++)
        {
            if (Text[i] == '\\')
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
            if ((Text[i] == 'm') || (Text[i] == 'w')) currSize = 5;
            else if (Text[i] == 'n') currSize = 4;
            else if (Text[i] == 'i') currSize = 1;
            else if (Text[i] == ' ') currSize = 2;
            if (isSpecial)
            {
                currSize = 5;
                isSpecial = false;
                wasClr = false;
            }

            totalSize += currSize + 1;
        }

        float textOffsetPixels = totalSize / 2f;// - ((IsBigger(Text[0]) ? 7 : 5) / 2f);
        if (AlignLeft) textOffsetPixels = 0;

        int digitIdx = 0, pixelOffset = 0;

        isSpecial = false; int specialColor = -1;
        for (int i = 0; i < Text.Length; i++)
        {
            if (Text[i] == '\\')
            {
                isSpecial = true;
                specialColor = -1;
                continue;
            }
            if (isSpecial && (specialColor == -1))
            {
                specialColor = Text[i] - 'a';
                continue;
            }

            if (Text[i] == ' ')
            {
                pixelOffset += 2;
                continue;
            }

            int currSize = 3;
            if ((Text[i] == 'm') || (Text[i] == 'w')) currSize = 5;
            else if (Text[i] == 'n') currSize = 4;
            else if (Text[i] == 'i') currSize = 1;
            if (isSpecial)
            {
                currSize = 5;
            }

            if (digitIdx >= GetChildCount()) break;

            GetChild<Sprite>(digitIdx).Visible = true;
            GetChild<Sprite>(digitIdx).RegionRect = isSpecial ? StampShapeValue.GetRect((StampShape)(Text[i] - 'a')) : new Rect2(5 * GetIdxInTextureFile(Text[i]), 0, 5, 5);

            GetChild<Sprite>(digitIdx).Position = new Vector2(pixelOffset - textOffsetPixels + ((currSize == 5) ? 1 : 0) + 2 + ((currSize == 1) ? -1 : 0), 0);

            GetChild<Sprite>(digitIdx).Modulate = isSpecial ? StampColorValue.GetColor((StampColor)specialColor) : DefaultColor;


            digitIdx++;
            if (isSpecial)
            {
                isSpecial = false;
                specialColor = -1;
            }

            pixelOffset += currSize + 1;
        }
    }

    private int GetIdxInTextureFile(char ch)
    {
        if ((ch >= '0') && (ch <= '9')) return ch - '0';
        if ((ch >= 'a') && (ch <= 'z')) return ch - 'a' + 10;
        if ((ch >= 'A') && (ch <= 'Z')) return ch - 'A' + 10;
        throw new Exception();
    }
}

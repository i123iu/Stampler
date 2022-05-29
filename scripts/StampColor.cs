using Godot;

public enum StampColor
{
    Green, Red, Blue, Yellow, Purple, 
}

public static class StampColorValue
{
    public static Color GetColor(StampColor stampColor)
    {
        switch (stampColor)
        {
            case StampColor.Green: return new Color(0, 1, 0, 1);
            case StampColor.Red: return new Color(1, 0, 0, 1);
            case StampColor.Blue: return new Color(0, 0, 1, 1);
            case StampColor.Yellow: return new Color(.8f, .8f, 0, 1);
            case StampColor.Purple: return new Color(0, 1, 1, 1);
            default: throw new System.Exception();
        }
    }
}

public enum StampShape
{
    Smile, Square, Heart, Arrow, Tick, 
}

public static class StampShapeValue
{
    public static Rect2 GetRect(StampShape shape)
    {
        return new Rect2(((int)shape + 36) * 5, 0, 5, 5);
    }
}
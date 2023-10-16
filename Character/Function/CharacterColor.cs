using UnityEngine;

public enum ECharacterColor
{
    NONE = -1, RED = 0, BLUE, GREEN, PINK, ORANGE, YELLOW, BLACK, WHITE, PURPLE, BROWN, CYAN, LIME, END
}


public class CharacterColor
{
    #region Variables

    private static Color[] colors = new Color[(int)ECharacterColor.END]
    {
        new Color(1f, 0f, 0f),
        new Color(0.1f, 0.1f, 1f),
        new Color(0f, 0.6f, 0f),
        new Color(1f, 0.3f, 0.9f),
        new Color(1f, 0.4f, 0f),
        new Color(1f, 0.9f, 0.1f),
        new Color(0.2f, 0.2f, 0.2f),
        new Color(0.9f, 1f, 1f),
        new Color(0.6f, 0f, 0.6f),
        new Color(0.7f, 0.2f, 0f),
        new Color(0f, 1f, 1f),
        new Color(0.7f, 1f, 0f)
    };

    #endregion Variables

    #region Methods

    public static Color GetColor(ECharacterColor color) { return colors[(int)color]; }

    public static Color Red { get { return colors[(int)ECharacterColor.RED]; } }
    public static Color Blue { get { return colors[(int)ECharacterColor.BLUE]; } }
    public static Color Green { get { return colors[(int)ECharacterColor.GREEN]; } }
    public static Color Pink { get { return colors[(int)ECharacterColor.PINK]; } }
    public static Color Orange { get { return colors[(int)ECharacterColor.ORANGE]; } }
    public static Color Yellow { get { return colors[(int)ECharacterColor.YELLOW]; } }
    public static Color Black { get { return colors[(int)ECharacterColor.BLACK]; } }
    public static Color White { get { return colors[(int)ECharacterColor.WHITE]; } }
    public static Color Purple { get { return colors[(int)ECharacterColor.PURPLE]; } }
    public static Color Brown { get { return colors[(int)ECharacterColor.BROWN]; } }
    public static Color Cyan { get { return colors[(int)ECharacterColor.CYAN]; } }
    public static Color Lime { get { return colors[(int)ECharacterColor.LIME]; } }

    #endregion Methods
}
using UnityEngine;

namespace TagAndLayer
{
    public enum LayerIndex
    {
        DEFAULT = 0,
        TRANSPARENTFX = 1,
        IGNORERAYCAST = 2,
        WATER = 4,
        UI = 5,
        MAP = 6,
        PLAYER = 7,
        INTERACTDETECT = 8,
        INTERACTABLE = 9,
        WALL = 10,
        MASK = 11,
        BEHINDMASK = 12,
        BLACK = 13
    }

    public class Layer
    {
        public const string DEFAULT = "Default";
        public const string TRANSPARENTFX = "TransparentFX";
        public const string IGNORERAYCAST = "IgnoreRaycast";
        public const string WATER = "Water";
        public const string UI = "UI";
        public const string MAP = "Map";
        public const string PLAYER = "Player";
        public const string INTERACTDETECT = "InteractDetect";
        public const string INTERACTABLE = "Interactable";
        public const string WALL = "Wall";
        public const string MASK = "Mask";
        public const string BEHINDMASK = "BehindMask";
        public const string BLAKC = "Black";

        public static int GetLayerByName(string layerName) => LayerMask.NameToLayer(layerName);
    }

    public class Tag
    {

    }
}

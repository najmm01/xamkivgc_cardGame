using UnityEngine;

[CreateAssetMenu(menuName = "Card Data")]
public class CardData : ScriptableObject
{
    public Sprite image;
    public Sprite effectImage;
    public AudioClip sfx;
    public string title;
    public string description;
    public int manaCost;
    public int damage;
    public int countInDeck;
    public bool isBooster;
    public bool isFire;
    public bool isIce;

}

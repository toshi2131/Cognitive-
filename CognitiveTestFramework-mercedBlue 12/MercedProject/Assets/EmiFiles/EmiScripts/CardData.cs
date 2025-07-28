using UnityEngine;

[CreateAssetMenu(fileName = "NewPlayingCard", menuName = "Playing Cards/Card")]
public class CardData : ScriptableObject
{
    public enum Suit { Clubs, Diamonds, Hearts, Spades }
    public enum Rank { Two, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King, Ace }

    public Suit suit;
    public Rank rank;
    public Sprite cardImage; // Visual representation of the card
    public Sprite backImage;

    public string GetCardName()
    {
        return rank.ToString() + "" + suit.ToString();
    }

}
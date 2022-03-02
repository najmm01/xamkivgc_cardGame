using UnityEngine;

[System.Serializable]
public class Hand
{
    public bool isPlayer;
    [SerializeField] private RectTransform deckTransform;
    [SerializeField] private RectTransform[] positions = new RectTransform[GameController.HandCount];
    private Card[] cards;

    public Card[] Cards => cards;

    internal bool IsReady()
    {
        return System.Array.Find(cards, card => !card.ReadyToUse) == null;
    }

    internal void AddCard(Card card, int index, GameController controller)
    {
        if (cards == null) cards = new Card[GameController.HandCount];
        cards[index] = card;

        //passing transforms instead of vector3 to account for change in position *during* lerp
        controller.AnimateCard(card, deckTransform, positions[index], isPlayer);
    }

    internal void RemoveCard(Card card)
    {
        int index = System.Array.IndexOf(cards, card);
        if (index == -1) return;

        cards[index] = null;
    }

    internal int GetAvailableSlot()
    {
        int availableIndex = -1;
        for (int i = 0; i < cards.Length; i++)
            if (cards[i] == null) availableIndex = i;

        return availableIndex;
    }
}
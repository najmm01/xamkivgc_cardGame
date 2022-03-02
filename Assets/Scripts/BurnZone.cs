using UnityEngine;

public class BurnZone : MonoBehaviour, ICardDropZone
{
    [SerializeField] private AudioClip burnSFX;
    private GameController controller;

    private void Awake()
    {
        controller = FindObjectOfType<GameController>();
    }

    public void OnEnter(Card card)
    {
        card.SetBurn(true);
    }

    public void OnExit(Card card)
    {
        card.SetBurn(false);
    }

    public void OnDrop(Card card)
    {
        SFXManager.Instance.PlaySFX(burnSFX);

        controller.DestroyAndReplaceCard(card);
        controller.ToggleTurn();
    }
}
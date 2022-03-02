using UnityEngine;

public class AttackFX : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Image icon;
    private Card card;
    private Character character;

    public void Initialize(Card cardUsed, Character characterToAttack)
    {
        card = cardUsed;
        character = characterToAttack;

        icon.sprite = card.EffectSprite;
    }

    private void TriggerDamage()
    {
        //try do damage
        character.ReceiveDamage(card);

        Destroy(gameObject);
    }
}

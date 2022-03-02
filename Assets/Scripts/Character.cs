using UnityEngine;
using UnityEngine.UI;

public class Character : MonoBehaviour, ICardDropZone
{
    private static readonly int HitAnimParam = Animator.StringToHash("Hit");

    [SerializeField] private bool isPlayer;
    [SerializeField] private AudioClip deathSFX;
    [SerializeField] private Image healthUI;
    [SerializeField] private GameObject[] manaBalls;
    [SerializeField] private GameObject glowFX;
    [SerializeField] private GameObject mirror;
    [SerializeField] private AudioClip mirrorBreakSFX;
    [SerializeField] private int maxHealth;
    [SerializeField] private int maxMana;
    [SerializeField] private int mana;
    private int health;
    private GameController controller;

    public bool IsPlayer => isPlayer;
    public bool IsIceType { get; set; }
    public bool IsFireType { get; set; }
    public int Health
    {
        get => health;
        set
        {
            health = Mathf.Clamp(value, 0, maxHealth);
            healthUI.sprite = controller.GetGreenNumberSprite(health);

            if (health == 0) TriggerDeath();
        }
    }

    public int Mana
    {
        get => mana;
        set
        {
            mana = Mathf.Clamp(value, 0, maxMana);
            for (int i = 0; i < manaBalls.Length; i++)
            {
                if (i < mana) manaBalls[i].SetActive(true);
                else manaBalls[i].SetActive(false);
            }
        }
    }

    internal bool MirrorOn
    {
        get => mirror.activeInHierarchy;
        set => mirror.SetActive(value);
    }

    public void OnEnter(Card card)
    {
        glowFX.SetActive(true);
    }

    public void OnExit(Card card)
    {
        glowFX.SetActive(false);
    }

    public void OnDrop(Card card)
    {
        glowFX.SetActive(false);

        //only player can drop cards on characters
        var player = controller.Player;
        if (card.Cost > player.Mana) return;

        //effect of card use
        bool isAttack = !(card.Damage == -1 || card.IsBooster);
        if (this.isPlayer && !isAttack)
        {
            player.Mana -= card.Cost;
            controller.UseCard(card, this);
        }
        else if (!this.isPlayer && isAttack)
        {
            player.Mana -= card.Cost;
            controller.UseCard(card, this);
        }
    }

    private void TriggerDeath()
    {
        SFXManager.Instance.PlaySFX(deathSFX);

        if (isPlayer)
        {
            controller.GameOver();
        }
        else
        {
            //destroy and replace character
            controller.UpdateScore(10);
            controller.IncrementKillCount();
            Destroy(gameObject);

            controller.CreateNewEnemy();
        }
    }

    internal void ReceiveDamage(Card card)
    {
        if (MirrorOn)
        {
            SFXManager.Instance.PlaySFX(mirrorBreakSFX);

            SFXManager.Instance.PlaySFX(card.SFX, 0.5f);
            //create fx to attack other
            var otherCharacter = isPlayer ? controller.Enemy : controller.Player;
            controller.LaunchAttack(card, otherCharacter);

            MirrorOn = false;
            return;
        }

        //receive damage
        var damage = card.Damage;
        if ((card.IsFire && IsFireType) || (card.IsIce && IsIceType)) damage /= 2;

        Health -= damage;
        GetComponent<Animator>().SetTrigger(HitAnimParam);

        if (!isPlayer)
        {
            controller.UpdateScore(damage);

            if (Health == 0) //if enemy has died, we won't toggle turn
            {
                controller.Player.Mana++;
                return;
            }

        }

        controller.ToggleTurn();
    }

    private void Awake()
    {
        controller = FindObjectOfType<GameController>();
        Health = maxHealth;
        Mana = mana;
    }
}

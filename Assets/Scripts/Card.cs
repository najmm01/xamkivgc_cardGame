using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

[RequireComponent(typeof(Animator), typeof(CanvasGroup))]
public class Card : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    private static readonly int FlipAnimParam = Animator.StringToHash("Flip");

    [SerializeField] private Image displayImage;
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI description;
    [SerializeField] private Image cost;
    [SerializeField] private Image damage;
    [SerializeField] private GameObject burnVFX;
    [SerializeField] private AudioClip cardFlipSFX;
    private List<GameObject> enteredObjects = new List<GameObject>();

    public bool ReadyToUse { get; set; }
    public Vector3 OriginalPosition { get; set; }
    public Hand AssignedHand { get; private set; }
    public Sprite EffectSprite { get; private set; }
    public bool IsBooster { get; private set; }
    public bool IsFire { get; private set; }
    public bool IsIce { get; private set; }
    public int Cost { get; private set; }
    public int Damage { get; private set; }
    public AudioClip SFX { get; private set; }
    public bool AllowDrag
    {
        get => GetComponent<CanvasGroup>().blocksRaycasts;
        set => GetComponent<CanvasGroup>().blocksRaycasts = value;
    }

    private void Awake()
    {
        AllowDrag = false;
        ReadyToUse = false;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!GameController.AllowInput || !ReadyToUse) return;

        transform.SetAsLastSibling();
        AllowDrag = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!GameController.AllowInput || !ReadyToUse) return;
        transform.position += (Vector3)eventData.delta;

        foreach (var underObject in eventData.hovered)
        {
            //if it's already in entered objects don't do anything
            if (enteredObjects.Contains(underObject)) continue;

            underObject.GetComponent<ICardDropZone>()?.OnEnter(this);
            enteredObjects.Add(underObject);
        }

        var exitedObjects = enteredObjects.FindAll(item => !eventData.hovered.Contains(item));
        foreach (var exited in exitedObjects) exited.GetComponent<ICardDropZone>()?.OnExit(this);
        enteredObjects.RemoveAll(item => !eventData.hovered.Contains(item));
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!GameController.AllowInput || !ReadyToUse) return;

        AllowDrag = true;
        transform.position = OriginalPosition;

        foreach (var entered in enteredObjects) entered.GetComponent<ICardDropZone>()?.OnDrop(this);
        enteredObjects.Clear();
    }

    internal void Initialize(CardData data, Hand hand, GameController gameController)
    {
        AssignedHand = hand;
        gameObject.name = data.title;

        displayImage.sprite = data.image;
        title.text = data.title.Trim();
        description.text = data.description.Trim();

        cost.sprite = gameController.GetGreenNumberSprite(data.manaCost);
        if (cost.sprite == null) cost.gameObject.SetActive(false);

        damage.sprite = gameController.GetRedNumberSprite(data.damage);
        if (damage.sprite == null) damage.gameObject.SetActive(false);

        IsBooster = data.isBooster;
        IsFire = data.isFire;
        IsIce = data.isIce;
        Damage = data.damage;
        Cost = data.manaCost;
        EffectSprite = data.effectImage;
        SFX = data.sfx;
    }

    internal void SetBurn(bool v)
    {
        burnVFX.SetActive(v);
    }

    internal void TriggerFlip()
    {
        SFXManager.Instance.PlaySFX(cardFlipSFX);

        GetComponent<Animator>().SetTrigger(FlipAnimParam);
    }
}

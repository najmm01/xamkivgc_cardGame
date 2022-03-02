using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameController : MonoBehaviour
{
    public const int HandCount = 3;
    private const float CardAnimDuration = 1.0f;
    private const float DeathDelay = 1.0f;
    private const float EnemyTurnDelay = 1.0f;
    private static readonly string AttackL2RState = "AttackL2R";
    private static readonly string AttackR2LState = "AttackR2L";
    public static bool AllowInput = false;

    [SerializeField] private Hand playerHand;
    [SerializeField] private Hand enemyHand;
    [SerializeField] private Card cardPrefab;
    [SerializeField] private Canvas canvas;
    [SerializeField] private ScoreData scoreData;
    [SerializeField] private CardData[] cardData;
    [SerializeField] private Sprite[] costNumbers;
    [SerializeField] private Sprite[] damageNumbers;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI turnText;
    [SerializeField] private AttackFX effectFXPrefab;
    [SerializeField] private Character enemyPrefab;
    [SerializeField] private Character player, enemy;
    [SerializeField] private GameObject enemySkipTurnIndicator;
    private Stack<CardData> playerDeck;
    private Stack<CardData> enemyDeck;
    private bool isPlayerTurn;

    public Character Player => player;
    public Character Enemy => enemy;
    private int Score
    {
        get => scoreData.score;
        set
        {
            scoreData.score = value;
            scoreText.text = $"Demon's Killed: {scoreData.killCount} | Score: {scoreData.score}";
        }
    }

    private int KillCount
    {
        get => scoreData.killCount;
        set
        {
            scoreData.killCount = value;
            scoreText.text = $"Demon's Killed: {scoreData.killCount} | Score: {scoreData.score}";
        }
    }

    private bool IsPlayerTurn
    {
        get => isPlayerTurn;
        set
        {
            isPlayerTurn = value;
            turnText.text = isPlayerTurn ? "Merlin's Turn" : "Demon's Turn";
        }
    }

    public void Quit()
    {
        SceneManager.LoadScene(0);
    }

    public void SkipTurn()
    {
        if (!IsPlayerTurn || !AllowInput) return;

        ToggleTurn();
    }

    internal void ToggleTurn()
    {
        IsPlayerTurn = !IsPlayerTurn;

        AllowInput = IsPlayerTurn;

        if (IsPlayerTurn)
        {
            player.Mana++;
        }
        else
        {
            enemy.Mana++;
            StartCoroutine(PlayEnemyTurn());
        }
    }

    public void UpdateScore(int points) => Score += points;
    public void IncrementKillCount() => KillCount += 1;

    internal void DestroyAndReplaceCard(Card card)
    {
        Destroy(card.gameObject);

        var hand = card.AssignedHand;
        hand.RemoveCard(card);

        var deck = hand.isPlayer ? playerDeck : enemyDeck;
        var newCard = CreateCardFromDeck(ref deck, hand);
        hand.AddCard(newCard, hand.GetAvailableSlot(), this);
    }

    internal void GameOver()
    {
        AllowInput = false;
        StartCoroutine(LoadGameOver());
    }

    private IEnumerator LoadGameOver()
    {
        yield return new WaitForSeconds(DeathDelay);
        SceneManager.LoadScene(2);
    }

    internal void CreateNewEnemy()
    {
        AllowInput = false;
        StartCoroutine(NewEnemy());
    }

    private IEnumerator NewEnemy()
    {
        yield return new WaitForSeconds(DeathDelay);
        enemy = Instantiate(enemyPrefab, canvas.transform);

        AllowInput = true;
    }

    internal void UseCard(Card card, Character target)
    {
        DestroyAndReplaceCard(card);

        SFXManager.Instance.PlaySFX(card.SFX);

        if (card.Damage == -1) //special case for mirror
        {
            //activate mirror
            target.MirrorOn = true;
            ToggleTurn();
            return;
        }

        if (card.IsBooster)
        {
            //increase health
            target.Health += card.Damage;
            ToggleTurn();
            return;
        }

        LaunchAttack(card, target);

        return;
    }

    internal void LaunchAttack(Card card, Character target)
    {
        AllowInput = false;
        //create fx to attack target
        var attackFX = Instantiate(effectFXPrefab, canvas.transform);
        attackFX.Initialize(card, target);
        if (target.IsPlayer) attackFX.GetComponent<Animator>().CrossFade(AttackR2LState, 0.0f);
        else attackFX.GetComponent<Animator>().CrossFade(AttackL2RState, 0.0f);
    }

    private IEnumerator PlayEnemyTurn()
    {
        var allCards = new List<Card>(enemyHand.Cards);
        Card enemyCard = null;

        while (allCards.Count != 0)
        {
            enemyCard = allCards[Random.Range(0, allCards.Count)];
            if (enemyCard.Cost <= Enemy.Mana) break;

            allCards.Remove(enemyCard);
            enemyCard = null;
        }

        yield return new WaitForSeconds(EnemyTurnDelay);

        if (enemyCard == null) //all cards have high mana cost
        {
            var burnChance = Random.Range(0, 2) == 1;
            if (burnChance)
            {
                DestroyAndReplaceCard(enemyHand.Cards[Random.Range(0, allCards.Count)]);

                while (!enemyHand.IsReady())
                {
                    yield return null;
                }

                StartCoroutine(PlayEnemyTurn());
            }
            else
            {
                enemySkipTurnIndicator.SetActive(true);

                yield return new WaitForSeconds(EnemyTurnDelay);

                enemySkipTurnIndicator.SetActive(false);
                ToggleTurn(); //skip turn
            }
        }
        else
        {
            enemyCard.TriggerFlip();

            yield return new WaitForSeconds(EnemyTurnDelay);

            Enemy.Mana -= enemyCard.Cost;
            bool isAttack = !(enemyCard.Damage == -1 || enemyCard.IsBooster);
            if (isAttack) UseCard(enemyCard, Player);
            else UseCard(enemyCard, Enemy);
        }
    }

    private void Start()
    {
        playerDeck = GetRandomDeck();
        enemyDeck = GetRandomDeck();

        DealCards(ref playerDeck, playerHand);
        DealCards(ref enemyDeck, enemyHand);

        IsPlayerTurn = true;
        Score = 0;
        KillCount = 0;
    }

    private void DealCards(ref Stack<CardData> deck, Hand hand)
    {
        Card[] cards = new Card[HandCount];
        for (int i = 0; i < HandCount; i++)
        {
            cards[i] = CreateCardFromDeck(ref deck, hand);
        }

        StartCoroutine(DealCardsRoutine(cards, hand));
    }

    //happens only at start, for both hands
    private IEnumerator DealCardsRoutine(Card[] cards, Hand hand)
    {
        for (int i = 0; i < HandCount; i++)
        {
            hand.AddCard(cards[i], i, this);
            yield return new WaitForSeconds(CardAnimDuration);
        }

        //if player hand is dealt, enable input
        if (hand.isPlayer) AllowInput = true;
    }

    private Card CreateCardFromDeck(ref Stack<CardData> deck, Hand hand)
    {
        //Q: why use "ref" here? | A: if we don't use ref...
        //changes to the contents of the array would be reflected in the caller method
        //BUT changes to the parameter itself wouldn't be
        if (deck.Count == 0) deck = GetRandomDeck();

        var cardData = deck.Pop();
        var card = Instantiate(cardPrefab, canvas.transform);
        card.Initialize(cardData, hand, this);
        card.gameObject.SetActive(false);

        return card;
    }

    private Stack<CardData> GetRandomDeck()
    {
        var orderedCards = new List<CardData>();
        foreach (var data in cardData)
        {
            for (int i = 0; i < data.countInDeck; i++)
                orderedCards.Add(data);
        }

        //randomization
        var randomDeck = new Stack<CardData>();
        for (int i = 0, len = orderedCards.Count; i < len; i++)
        {
            var randomIndex = Random.Range(0, orderedCards.Count);
            randomDeck.Push(orderedCards[randomIndex]);

            orderedCards.RemoveAt(randomIndex);
        }

        return randomDeck;
    }

    internal void AnimateCard
        (Card card, RectTransform start, RectTransform target, bool isPlayer)
    {
        card.gameObject.SetActive(true);
        StartCoroutine(AnimateCardRoutine(card, start, target, isPlayer));
    }

    private System.Collections.IEnumerator AnimateCardRoutine
        (Card card, RectTransform start, RectTransform target, bool isPlayer)
    {
        var timer = 0.0f;
        var targetScale = Vector3.one;
        var cardTransform = card.GetComponent<RectTransform>();
        while (timer < CardAnimDuration)
        {
            cardTransform.position = Vector2.Lerp(start.position, target.position, timer / CardAnimDuration);
            cardTransform.localScale = Vector3.Lerp(start.localScale, targetScale, timer / CardAnimDuration);

            timer += Time.deltaTime;
            yield return null;
        }

        cardTransform.position = target.position;
        cardTransform.localScale = targetScale;
        card.OriginalPosition = target.position;

        if (isPlayer)
        {
            card.TriggerFlip();
            card.AllowDrag = true;
        }

        card.ReadyToUse = true;
    }

    internal Sprite GetGreenNumberSprite(int number)
    {
        if (number >= costNumbers.Length || number < 0) return null;

        return costNumbers[number];
    }

    internal Sprite GetRedNumberSprite(int number)
    {
        if (number >= damageNumbers.Length || number < 0) return null;

        return damageNumbers[number];
    }
}

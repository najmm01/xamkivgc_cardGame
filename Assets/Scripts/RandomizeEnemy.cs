using UnityEngine;

public class RandomizeEnemy : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Image icon;
    [SerializeField] private Sprite iceDemon, fireDemon;
    [SerializeField] private Character enemy;

    private void Awake()
    {
        var isFire = Random.Range(0, 2) == 1;

        enemy.IsFireType = isFire;
        enemy.IsIceType = !isFire;

        icon.sprite = isFire ? fireDemon : iceDemon;
    }
}

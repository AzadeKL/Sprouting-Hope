using System.Collections;
using UnityEngine;

public class ChickenWalk : MonoBehaviour
{
    [SerializeField] private float randomizeJumpTimer = 10f;


    private SpriteRenderer sprite;

    private void Awake()
    {
        sprite = GetComponent<SpriteRenderer>();
    }

    IEnumerator Start()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(2f, randomizeJumpTimer));
            // transform.DOBlendableMoveBy(Vector3.left, 0.3f).SetLoops(2, LoopType.Yoyo);
            bool coinflip = Random.value < 0.5f;
            sprite.flipX = coinflip;
        }
    }
}

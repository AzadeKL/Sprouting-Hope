using DG.Tweening;
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
            bool coinflip = Random.value < 0.5f;
            sprite.flipX = coinflip;

            //TODO Check this part if it cleans it self or not
            DOTween.Sequence().SetDelay(2f).Append(transform.DOBlendableMoveBy(Vector3.left, 0.3f)).AppendInterval(1f).SetLoops(2, LoopType.Yoyo).Play();
            DOTween.Sequence().SetDelay(2f).Append(transform.DOBlendableMoveBy(Vector3.up / 2f, 0.15f)).SetLoops(2, LoopType.Yoyo).Play();
            DOTween.Sequence().SetDelay(4.3f).Append(transform.DOBlendableMoveBy(Vector3.up / 2f, 0.15f)).SetLoops(2, LoopType.Yoyo).Play();


        }
    }
}

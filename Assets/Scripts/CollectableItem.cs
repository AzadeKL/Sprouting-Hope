using DG.Tweening;
using UnityEngine;

public class CollectableItem : MonoBehaviour
{
    [SerializeField] private float deathTime = 10f;
    [SerializeField] private float animationTime = 0.2f;
    [SerializeField] private float waitForToRipe = 2f;
    [SerializeField] private Ease animationEase = Ease.InBounce;

    [SerializeField] private float spawnJumpMultiplier = 3f;

    private float lifeTime;

    private bool collected = false;


    private PlayerInventory player;

    private void Start()
    {
        player = GameObject.Find("Player").GetComponent<PlayerInventory>();
        transform.localScale = Vector3.zero;
        transform.DOScale(Vector3.one, 0.1f);
        transform.DOBlendableMoveBy(Random.insideUnitSphere * spawnJumpMultiplier, 1f);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (lifeTime < waitForToRipe) return;
        if (collected == true) return;

        if (collision.CompareTag("Player"))
        {
            MoveToTarget(collision.transform.position);
            collected = true;
        }
    }

    private void Update()
    {
        lifeTime += Time.deltaTime;
        if (transform.localScale == Vector3.zero && collected)
        {
            Debug.Log("collected");
            player.AddToInventory(this.tag);
            Destroy(this.gameObject);
        }
        if (lifeTime > deathTime)
        {
            Destroy(this.gameObject);
        }
    }



    private void MoveToTarget(Vector3 pos)
    {
        //TODO this moves to last player position. We need to follow player.
        transform.DOMove(pos, animationTime).SetEase(animationEase);
        transform.DOScale(Vector3.zero, animationTime).SetEase(animationEase);
    }
}

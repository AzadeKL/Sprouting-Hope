using DG.Tweening;
using System;
using UnityEngine;

using Random = UnityEngine.Random;

public class CollectableItem : MonoBehaviour
{
    [SerializeField] private float collectSpeed = 10f;
    [SerializeField] private float radiusIncreaseSpeed = 0.1f;
    [SerializeField] private float deathTime = 10f;
    [SerializeField] private float animationTime = 0.2f;
    [SerializeField] private float waitForToRipe = 2f;
    [SerializeField] private Ease animationEase = Ease.InBounce;


    [Range(0.1f, 3f)]
    [SerializeField] private float randomizerRange = 1f;

    [SerializeField] private float spawnJumpMultiplier = 3f;

    private float lifeTime;

    private bool collected = false;


    private PlayerInventory player;

    private CircleCollider2D collider2d;
    private AudioSource auidoSource;
    private void Awake()
    {
        collider2d = GetComponent<CircleCollider2D>();
        auidoSource = GetComponent<AudioSource>();
    }
    private void Start()
    {
        waitForToRipe += Random.Range(0, randomizerRange);
        deathTime += Random.Range(-randomizerRange, randomizerRange);
        collectSpeed += Random.Range(-randomizerRange, randomizerRange);

        player = GameObject.Find("Player").GetComponent<PlayerInventory>();
        transform.localScale = Vector3.zero;
        transform.DOScale(Vector3.one, 0.1f);
        transform.DOBlendableMoveBy(Random.insideUnitSphere * spawnJumpMultiplier, 0.4f);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (lifeTime < waitForToRipe) return;
        if (collected == true) return;

        if (collision.CompareTag("Player"))
        {
            MoveToTarget(collision.transform.position);
            transform.DOScale(Vector3.zero, animationTime).SetEase(animationEase);
            collider2d.radius = spawnJumpMultiplier;
            collected = true;
            auidoSource.pitch = Random.Range(0.8f, 1.2f);
            auidoSource.volume = Random.Range(0.3f, 0.8f);
            auidoSource.Play();
        }
    }

    private void Update()
    {
        if (collected == true)
        {
            MoveToTarget(player.transform.position);
        }
        else
        {
            collider2d.radius += radiusIncreaseSpeed * Time.deltaTime + Random.Range(0, randomizerRange / 10f) * Time.deltaTime;
        }
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
        transform.position = Vector3.MoveTowards(transform.position, pos, Time.deltaTime * collectSpeed);
        // transform.DOMove(pos, animationTime).SetEase(animationEase);
        // transform.DOScale(Vector3.zero, animationTime).SetEase(animationEase);
    }
}

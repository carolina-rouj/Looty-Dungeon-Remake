using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    private bool collected;
    private float baseY;

    public static void Create(Transform parent, Vector3 position)
    {
        GameObject coin = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        coin.name = "Coin";
        coin.transform.SetParent(parent, false);
        coin.transform.position = position + Vector3.up * 0.45f;
        coin.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        coin.transform.localScale = new Vector3(0.24f, 0.05f, 0.24f);
        coin.GetComponent<Renderer>().material = RuntimeMaterials.Get("coin", new Color(1f, 0.78f, 0.08f));
        Collider collider = coin.GetComponent<Collider>();
        collider.isTrigger = true;
        CoinPickup pickup = coin.AddComponent<CoinPickup>();
        pickup.baseY = coin.transform.position.y;
    }

    private void Update()
    {
        if (collected || !EnemyMovementUtility.IsGameplayActive())
        {
            return;
        }

        transform.Rotate(0f, 0f, 180f * Time.deltaTime, Space.Self);
        Vector3 p = transform.position;
        p.y = baseY + Mathf.Sin(Time.time * 4f) * 0.08f;
        transform.position = p;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (collected || !other.CompareTag("Player"))
        {
            return;
        }

        collected = true;
        DungeonGameRuntime.Instance.AddCoin(transform.position);
        Destroy(gameObject);
    }
}


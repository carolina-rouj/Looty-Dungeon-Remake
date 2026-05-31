using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    private bool collected;
    private Transform floorTile;   // tile del suelo al que va pegada
    private float baseLocalY;      // altura local de reposo (para el balanceo)

    // Crea una moneda apoyada sobre la superficie real del tile y colgada de el, de modo
    // que cae junto al suelo cuando este se desploma.
    public static void Create(Transform floorTile)
    {
        GameObject coin = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        coin.name = "Coin";

        // Superficie superior real del tile (independiente del pivote del prefab).
        float surfaceY = floorTile.position.y + 1.0f;
        Renderer tileRenderer = floorTile.GetComponentInChildren<Renderer>();
        if (tileRenderer != null) surfaceY = tileRenderer.bounds.max.y;

        coin.transform.position = new Vector3(floorTile.position.x, surfaceY + 0.18f, floorTile.position.z);
        coin.transform.SetParent(floorTile, true);   // worldPositionStays: queda sobre el tile
        coin.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        coin.transform.localScale = new Vector3(0.24f, 0.05f, 0.24f);
        coin.GetComponent<Renderer>().material = RuntimeMaterials.Get("coin", new Color(1f, 0.78f, 0.08f));
        Collider collider = coin.GetComponent<Collider>();
        collider.isTrigger = true;

        CoinPickup pickup = coin.AddComponent<CoinPickup>();
        pickup.floorTile = floorTile;
        pickup.baseLocalY = coin.transform.localPosition.y;
    }

    private void Update()
    {
        if (collected || !EnemyMovementUtility.IsGameplayActive())
        {
            return;
        }

        transform.Rotate(0f, 0f, 180f * Time.deltaTime, Space.Self);

        // Si el tile se esta cayendo (tiene Rigidbody) o ya no existe, no forzamos la
        // posicion: la moneda acompaña al tile en su caida (es hija suya).
        bool tileFalling = floorTile == null || floorTile.GetComponent<Rigidbody>() != null;
        if (tileFalling)
        {
            return;
        }

        // Balanceo suave sobre la superficie.
        Vector3 lp = transform.localPosition;
        lp.y = baseLocalY + Mathf.Sin(Time.time * 4f) * 0.05f;
        transform.localPosition = lp;
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

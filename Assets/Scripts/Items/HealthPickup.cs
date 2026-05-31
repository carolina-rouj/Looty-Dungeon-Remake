using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    private bool collected;
    private float baseY;

    public static void Create(Transform parent, Vector3 position)
    {
        GameObject root = new GameObject("Heart");
        root.transform.SetParent(parent, false);
        root.transform.position = position + Vector3.up * 0.55f;
        root.transform.localScale = Vector3.one * 0.28f;

        SphereCollider collider = root.AddComponent<SphereCollider>();
        collider.isTrigger = true;
        collider.radius = 0.9f;

        Color heartColor = new Color(1f, 0.32f, 0.4f);
        GameObject lobeLeft = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        lobeLeft.name = "LobeLeft";
        lobeLeft.transform.SetParent(root.transform, false);
        lobeLeft.transform.localPosition = new Vector3(-0.32f, 0.2f, 0f);
        lobeLeft.transform.localScale = Vector3.one * 0.62f;
        Destroy(lobeLeft.GetComponent<Collider>());
        lobeLeft.GetComponent<Renderer>().material = RuntimeMaterials.GetEmissive("heart", heartColor, 0.45f);

        GameObject lobeRight = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        lobeRight.name = "LobeRight";
        lobeRight.transform.SetParent(root.transform, false);
        lobeRight.transform.localPosition = new Vector3(0.32f, 0.2f, 0f);
        lobeRight.transform.localScale = Vector3.one * 0.62f;
        Destroy(lobeRight.GetComponent<Collider>());
        lobeRight.GetComponent<Renderer>().material = RuntimeMaterials.GetEmissive("heart", heartColor, 0.45f);

        GameObject point = GameObject.CreatePrimitive(PrimitiveType.Cube);
        point.name = "Point";
        point.transform.SetParent(root.transform, false);
        point.transform.localPosition = new Vector3(0f, -0.24f, 0f);
        point.transform.localRotation = Quaternion.Euler(0f, 45f, 0f);
        point.transform.localScale = new Vector3(0.62f, 0.62f, 0.62f);
        Destroy(point.GetComponent<Collider>());
        point.GetComponent<Renderer>().material = RuntimeMaterials.GetEmissive("heart", heartColor, 0.45f);

        HealthPickup pickup = root.AddComponent<HealthPickup>();
        pickup.baseY = root.transform.position.y;
    }

    private void Update()
    {
        if (collected || !EnemyMovementUtility.IsGameplayActive())
        {
            return;
        }

        transform.Rotate(0f, 110f * Time.deltaTime, 0f, Space.World);
        Vector3 p = transform.position;
        p.y = baseY + Mathf.Sin(Time.time * 3.4f) * 0.07f;
        transform.position = p;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (collected || !other.CompareTag("Player"))
        {
            return;
        }

        collected = true;
        if (DungeonGameRuntime.Instance != null)
        {
            DungeonGameRuntime.Instance.AddHealth(transform.position);
        }
        Destroy(gameObject);
    }
}


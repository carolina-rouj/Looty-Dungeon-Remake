using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonDoor : MonoBehaviour
{
    public bool IsOpen { get; private set; }

    private Renderer doorRenderer;
    private Collider doorCollider;

    private void Awake()
    {
        doorRenderer = GetComponent<Renderer>();
        doorCollider = GetComponent<Collider>();
        doorRenderer.material = RuntimeMaterials.Get("door_closed", new Color(0.45f, 0.16f, 0.08f));
        doorCollider.isTrigger = false;
    }

    public void Open()
    {
        IsOpen = true;
        doorRenderer.material = RuntimeMaterials.Get("door_open", new Color(1f, 0.73f, 0.08f));
        doorCollider.isTrigger = true;
        StartCoroutine(OpenAnimation());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsOpen && other.CompareTag("Player"))
        {
            DungeonGameRuntime.Instance.DoorEntered();
        }
    }

    private IEnumerator OpenAnimation()
    {
        Vector3 start = transform.localScale;
        Vector3 end = new Vector3(start.x, start.y, 0.04f);
        float elapsed = 0f;
        while (elapsed < 0.25f)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(start, end, elapsed / 0.25f);
            yield return null;
        }
    }
}


using UnityEngine;

public class CauldronEffect : MonoBehaviour
{
    [Header("Liquid")]
    public Color liquidColor = new Color(0.05f, 0.75f, 0.05f, 1f);
    public float liquidYOffset = 0.40f;
    public float liquidRadius = 0.28f;
    public float colorPulseSpeed = 1.5f;

    [Header("Wand")]
    public float wandRotationSpeed = 80f;
    public float wandOrbitRadius = 0.12f;

    private GameObject _liquid;
    private GameObject _wand;
    private Material _liquidMat;

    void Start()
    {
        _liquid = CreateLiquid();
        _wand = CreateWand();
    }

    void Update()
    {
        Vector3 pivot = transform.TransformPoint(new Vector3(0f, liquidYOffset, 0f));
        _wand.transform.RotateAround(pivot, Vector3.up, wandRotationSpeed * Time.deltaTime);

        float t = (Mathf.Sin(Time.time * colorPulseSpeed) + 1f) * 0.5f;
        Color pulsed = Color.Lerp(liquidColor, liquidColor * 1.5f, t);
        pulsed.a = 1f;
        _liquidMat.SetColor("_BaseColor", pulsed);
        _liquidMat.SetColor("_EmissionColor", pulsed * (0.25f + 0.25f * t));
    }

    GameObject CreateLiquid()
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name = "LiquidSurface";
        go.transform.SetParent(transform, false);
        go.transform.localPosition = new Vector3(0f, liquidYOffset, 0f);
        go.transform.localScale = new Vector3(liquidRadius * 2f, 0.01f, liquidRadius * 2f);
        Destroy(go.GetComponent<CapsuleCollider>());

        _liquidMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        _liquidMat.SetColor("_BaseColor", liquidColor);
        _liquidMat.EnableKeyword("_EMISSION");
        _liquidMat.SetColor("_EmissionColor", liquidColor * 0.3f);
        go.GetComponent<MeshRenderer>().sharedMaterial = _liquidMat;
        return go;
    }

    GameObject CreateWand()
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name = "StirringWand";
        go.transform.SetParent(transform, false);
        go.transform.localPosition = new Vector3(wandOrbitRadius, liquidYOffset + 0.08f, 0f);
        go.transform.localScale = new Vector3(0.07f, 0.18f, 0.07f);
        Destroy(go.GetComponent<CapsuleCollider>());

        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.SetColor("_BaseColor", new Color(0.35f, 0.18f, 0.05f));
        go.GetComponent<MeshRenderer>().sharedMaterial = mat;
        return go;
    }
}

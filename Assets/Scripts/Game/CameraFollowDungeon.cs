using UnityEngine;

public class CameraFollowDungeon : MonoBehaviour
{
    private Transform target;
    private Vector3 staticFocus;
    private bool useStaticFocus;

    // Seguimiento "leve": el foco se mezcla entre el centro de la sala y el player, y se
    // limita a los bordes de la sala para no enseñar el vacio de fuera. Da el efecto del
    // Looty Dungeon oficial (la camara sigue al heroe pero sin perder de vista la sala).
    private bool softFollow;
    private Vector3 roomCenter;
    private float followWeight = 0.55f;
    private Vector3 boundsMin;
    private Vector3 boundsMax;

    private Vector3 offset = new Vector3(3.92f, 10.18f, -5.01f);
    private float shakeUntil;
    private float shakePower;

    public void SetTarget(Transform followTarget)
    {
        target = followTarget;
        useStaticFocus = false;
        softFollow = false;
        Snap();
    }

    public void SetStaticFocus(Vector3 focus)
    {
        target = null;
        useStaticFocus = true;
        softFollow = false;
        staticFocus = focus;
        Snap();
    }

    // center = centro de la sala; min/max = limites del suelo (para acotar el desplazamiento).
    public void SetSoftFollow(Transform followTarget, Vector3 center, Vector3 min, Vector3 max, float weight = 0.55f)
    {
        target = followTarget;
        useStaticFocus = false;
        softFollow = true;
        roomCenter = center;
        boundsMin = min;
        boundsMax = max;
        followWeight = weight;
        Snap();
    }

    public void Snap()
    {
        if (TryGetFocus(out Vector3 focus))
        {
            transform.position = focus + offset;
            transform.rotation = Quaternion.Euler(58f, -38f, 0f);
        }
    }

    public void Shake(float duration, float power)
    {
        shakeUntil = Mathf.Max(shakeUntil, Time.time + duration);
        shakePower = Mathf.Max(shakePower, power);
    }

    private void LateUpdate()
    {
        if (!TryGetFocus(out Vector3 focus))
        {
            return;
        }

        Vector3 desired = focus + offset;
        if (Time.time < shakeUntil)
        {
            desired += Random.insideUnitSphere * shakePower;
        }
        else
        {
            shakePower = 0f;
        }

        transform.position = Vector3.Lerp(transform.position, desired, 6.5f * Time.deltaTime);
        transform.rotation = Quaternion.Euler(58f, -38f, 0f);
    }

    private bool TryGetFocus(out Vector3 focus)
    {
        if (softFollow && target != null)
        {
            // La camara se mueve SOLO en el eje del nivel (Z): sigue el avance del jugador
            // por la sala (que crece en una sola direccion) pero la X queda fija al centro,
            // asi no se va a izquierda/derecha. Es "seguir el nivel", no al jugador.
            float marginZ = (boundsMax.z - boundsMin.z) * 0.5f;
            focus.x = roomCenter.x;
            focus.z = Mathf.Clamp(target.position.z, roomCenter.z - marginZ, roomCenter.z + marginZ);
            focus.y = 0f;
            return true;
        }

        if (useStaticFocus)
        {
            focus = staticFocus;
            return true;
        }

        if (target != null)
        {
            focus = target.position;
            return true;
        }

        focus = Vector3.zero;
        return false;
    }
}

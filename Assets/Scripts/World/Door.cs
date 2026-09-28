using UnityEngine;
using System.Collections;

public class Door : MonoBehaviour, IInteractable
{
    [Header("Настройки двери")]
    public float openAngle = 90f;          // На сколько градусов открывается
    public float openSpeed = 3f;           // Скорость открытия
    public bool isOpen = false;

    private Quaternion closedRotation;
    private Quaternion openRotation;
    private bool isMoving = false;

    private void Start()
    {
        closedRotation = transform.localRotation;
        openRotation = closedRotation * Quaternion.Euler(0f, openAngle, 0f);
    }

    public void ToggleDoor()
    {
        if (isMoving) return;

        isOpen = !isOpen;
        StopAllCoroutines();
        StartCoroutine(RotateDoor(isOpen ? openRotation : closedRotation));
    }

    private IEnumerator RotateDoor(Quaternion targetRotation)
    {
        isMoving = true;

        while (Quaternion.Angle(transform.localRotation, targetRotation) > 0.1f)
        {
            transform.localRotation = Quaternion.Slerp(
                transform.localRotation,
                targetRotation,
                Time.deltaTime * openSpeed
            );
            yield return null;
        }

        transform.localRotation = targetRotation;
        isMoving = false;
    }
    public void Interact()
    {
        ToggleDoor();
    }

    public string GetInteractionText()
    {
        return "Открыть дверь";
    }
}
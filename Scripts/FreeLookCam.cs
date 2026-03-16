using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Unity.Cinemachine;

public class FreeLookCam : MonoBehaviour, IDragHandler
{
    [SerializeField] private Image touchArea;
    [SerializeField] private CinemachineOrbitalFollow freeLookCam;
    [SerializeField] private float sensitivity = 0.1f;

    public void OnDrag(PointerEventData eventData)
    {
        float deltaX = eventData.delta.x * sensitivity;
        float deltaY = eventData.delta.y * sensitivity;

        freeLookCam.HorizontalAxis.Value += deltaX;
        freeLookCam.VerticalAxis.Value -= deltaY;
    }
}

using UnityEngine;

public class FreezeVisualController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Renderer targetRenderer; 
    [SerializeField] private GameObject freezeObject; 
    [SerializeField] private string propertyName = "_frostamount";

    [Header("Settings")]
    [SerializeField] private float frozenValue = 0f; 
    [SerializeField] private float unfrozenValue = 1f;
    [SerializeField] private float transitionSpeed = 2f;

    private Material mat;
    private float currentValue;
    private float targetValue;
    private bool isActive = false;

    void Awake()
    {
        if (freezeObject != null)
            freezeObject.SetActive(false);

        if (targetRenderer != null)
        {
            mat = targetRenderer.material;
            currentValue = mat.GetFloat(propertyName);
        }
    }

    void Update()
    {
        if (!isActive) return;

        currentValue = Mathf.MoveTowards(currentValue, targetValue, transitionSpeed * Time.deltaTime);
        mat.SetFloat(propertyName, currentValue);

        if (Mathf.Approximately(currentValue, targetValue) && targetValue == unfrozenValue)
        {
            freezeObject.SetActive(false);
            isActive = false;
        }
    }

    public void TriggerFreeze()
    {
        if (freezeObject != null)
            freezeObject.SetActive(true);

        if (mat != null)
            currentValue = mat.GetFloat(propertyName);

        targetValue = frozenValue;
        isActive = true;
    }

    public void TriggerUnfreeze()
    {
        if (freezeObject != null)
            freezeObject.SetActive(true);

        if (mat != null)
            currentValue = mat.GetFloat(propertyName);

        targetValue = unfrozenValue;
        isActive = true;
    }
}

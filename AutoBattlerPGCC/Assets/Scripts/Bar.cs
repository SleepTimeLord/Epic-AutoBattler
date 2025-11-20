using System.Collections;
using UnityEngine;

public class Bar : MonoBehaviour
{
    [field:SerializeField]
    public int MaxValue { get; private set; }
    [field:SerializeField]
    public int Value { get; private set; }

    [SerializeField]
    private RectTransform topBar;
    [SerializeField]
    private RectTransform bottomBar;
    [SerializeField]
    private float animationSpeed = 10f;

    private float fullWidth;
    private float TargetWidth => Value * fullWidth / MaxValue;

    private Coroutine adjustBarWidthCoroutine;

    private IEnumerator AdjustBarWidth(int amount) 
    { 
        var suddenChangeBar = amount >= 0 ? bottomBar : topBar;
        var slowChangeBar = amount >= 0 ? topBar : bottomBar;
        suddenChangeBar.SetWidth(TargetWidth);
        while (Mathf.Abs(suddenChangeBar.rect.width - slowChangeBar.rect.width) > 1f) 
        {
            slowChangeBar.SetWidth(
                Mathf.Lerp(slowChangeBar.rect.width, TargetWidth, Time.deltaTime * animationSpeed));
            yield return null;
        }
        slowChangeBar.SetWidth(TargetWidth);
    }
    public void SetMax(int max)
    {
        MaxValue = max;
        Value = max;

        fullWidth = topBar.rect.width;
        topBar.SetWidth(fullWidth);
        bottomBar.SetWidth(fullWidth);
    }
    public void Change(int amount) 
    { 
        Value = Mathf.Clamp(Value + amount, 0, MaxValue);

        if (adjustBarWidthCoroutine != null) 
        { 
            StopCoroutine(adjustBarWidthCoroutine);
        }

        adjustBarWidthCoroutine = StartCoroutine(AdjustBarWidth(amount));
    }
}

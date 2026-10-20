
using System;
using System.Collections;
using UnityEngine;

public class ShopOpenCloseSign : MonoBehaviour, IInteractable
{
    [Header("Sign")]
    [SerializeField] private Transform signBoard;
    [SerializeField] private GameObject openSide;
    [SerializeField] private GameObject closedSide;
    [SerializeField] private float flipDuration = 0.4f;

    public bool IsOpen { get; private set; } = false;
    public event Action<bool> OnShopStateChanged;

    private bool isFlipping;
    private Quaternion closedRotation;
    private Quaternion openRotation;

    private void Awake()
    {
        if (signBoard == null)
            signBoard = transform;

        closedRotation = signBoard.localRotation;
        openRotation = closedRotation * Quaternion.Euler(0f, 180f, 0f);

        IsOpen = false;
        UpdateSides();
    }

    public void Interact()
    {
        if (isFlipping) return;

        IsOpen = !IsOpen;
        OnShopStateChanged?.Invoke(IsOpen);
        StartCoroutine(FlipSign());
    }

    private IEnumerator FlipSign()
    {
        isFlipping = true;

        Quaternion startRotation = signBoard.localRotation;
        Quaternion targetRotation = IsOpen ? openRotation : closedRotation;

        if (openSide != null) openSide.SetActive(false);
        if (closedSide != null) closedSide.SetActive(false);

        float elapsed = 0f;

        while (elapsed < flipDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / flipDuration);
            signBoard.localRotation =
                Quaternion.Slerp(startRotation, targetRotation, t);

            yield return null;
        }

        signBoard.localRotation = targetRotation;
        UpdateSides();
        isFlipping = false;
    }

    private void UpdateSides()
    {
        if (openSide != null) openSide.SetActive(IsOpen);
        if (closedSide != null) closedSide.SetActive(!IsOpen);
    }
}
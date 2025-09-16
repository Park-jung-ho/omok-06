using UnityEngine;
using DG.Tweening;
using System;

[RequireComponent(typeof(CanvasGroup))]
public class PanelController : MonoBehaviour
{
    [SerializeField] private RectTransform panelRectTransform;

    private CanvasGroup _backgroundCanvasGroup;
    private readonly float duration = 0.3f; // 애니메이션 시간 고정
    private readonly Ease showEase = Ease.OutBack;
    private readonly Ease hideEase = Ease.InBack;

    private void Awake()
    {
        _backgroundCanvasGroup = GetComponent<CanvasGroup>();
    }

    // 패널 열기
    public void Show()
    {
        gameObject.SetActive(true); // 무조건 켜주기

        _backgroundCanvasGroup.alpha = 0;
        panelRectTransform.localScale = Vector3.zero;

        _backgroundCanvasGroup.DOFade(1, duration).SetEase(Ease.Linear);
        panelRectTransform.DOScale(1, duration).SetEase(showEase);
    }

    // 패널 숨기기
    public void Hide(Action onHide = null)
    {
        _backgroundCanvasGroup.alpha = 1;
        panelRectTransform.localScale = Vector3.one;

        _backgroundCanvasGroup.DOFade(0, duration).SetEase(Ease.Linear);
        panelRectTransform.DOScale(0, duration).SetEase(hideEase)
            .OnComplete(() =>
            {
                onHide?.Invoke();

                // 끄기 직전에 원래 값으로 복구
                _backgroundCanvasGroup.alpha = 1;
                panelRectTransform.localScale = Vector3.one;

                gameObject.SetActive(false);
            });
    }

    // 패널 흔들기
    protected void Shake(float shakeDuration = 0.3f, float strength = 0.1f, int vibrato = 10)
    {
        panelRectTransform.DOShakeScale(shakeDuration, strength, vibrato);
    }
}

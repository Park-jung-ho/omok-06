using UnityEngine;
using DG.Tweening;
using System;

[RequireComponent(typeof(CanvasGroup))]
public class PanelController : MonoBehaviour
{
    [SerializeField] private RectTransform panelRectTransform;

    private CanvasGroup _backgroundCanvasGroup;
    private readonly float duration = 0.3f; // �ִϸ��̼� �ð� ����
    private readonly Ease showEase = Ease.OutBack;
    private readonly Ease hideEase = Ease.InBack;

    private void Awake()
    {
        _backgroundCanvasGroup = GetComponent<CanvasGroup>();
    }

    // �г� ����
    public void Show()
    {
        _backgroundCanvasGroup.alpha = 0;
        panelRectTransform.localScale = Vector3.zero;

        _backgroundCanvasGroup.DOFade(1, duration).SetEase(Ease.Linear);
        panelRectTransform.DOScale(1, duration).SetEase(showEase);
    }

    // �г� �����
    public void Hide(Action onHide = null)
    {
        _backgroundCanvasGroup.alpha = 1;
        panelRectTransform.localScale = Vector3.one;

        _backgroundCanvasGroup.DOFade(0, duration).SetEase(Ease.Linear);
        panelRectTransform.DOScale(0, duration).SetEase(hideEase)
            .OnComplete(() =>
            {
                onHide?.Invoke();
                Destroy(gameObject); // ������ �ı�
            });
    }

    // �г� ����
    protected void Shake(float shakeDuration = 0.3f, float strength = 0.1f, int vibrato = 10)
    {
        panelRectTransform.DOShakeScale(shakeDuration, strength, vibrato);
    }
}
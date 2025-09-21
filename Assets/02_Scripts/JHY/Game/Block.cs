using UnityEngine;
using UnityEngine.EventSystems;     

public class Block : MonoBehaviour
{
    [SerializeField] private Sprite wSprite;
    [SerializeField] private Sprite bSprite;
    [SerializeField] private Sprite banSprite;
    [SerializeField] private SpriteRenderer markerSpriteRenderer;
    [SerializeField] private GameObject scope;    
    private SpriteRenderer _spriteRenderer;
    private Color _defaultBlockColor;
    public bool isBanned;

    public delegate void OnBlockClicked(int index);
    private OnBlockClicked _onBlockClicked;
    public enum MarkerType { None, White, Black, Banned }

    private MarkerType currentMarkerType = MarkerType.None;
    public MarkerType CurrentMarkerType
    {
        get { return currentMarkerType; }
        set { currentMarkerType = value; }
    }
    public int _blockIndex { get; set; }

    private bool isScopeOn;
    public bool IsScopeOn
    {
        get { return isScopeOn; }
        set 
        { 
            isScopeOn = value;
            scope.SetActive(isScopeOn);
        }
    }
    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();

        _defaultBlockColor = _spriteRenderer.color;
    }
    public void ResetBlock()
    {
        markerSpriteRenderer.sprite = null;
        currentMarkerType = MarkerType.None;
        isScopeOn = false;
    }

    public void InitMarker(int blockIndex, OnBlockClicked onBlockClicked)
    {
        _blockIndex = blockIndex;
        currentMarkerType = MarkerType.None;

        SetMarker();
        SetBlockColor(_defaultBlockColor);

        _onBlockClicked = onBlockClicked;
    }

    public void SetMarker()
    {
        switch (currentMarkerType)
        {
            case MarkerType.None:
                markerSpriteRenderer.sprite = null;
                break;
            case MarkerType.White:
                markerSpriteRenderer.sprite = wSprite;
                break;
            case MarkerType.Black:
                markerSpriteRenderer.sprite = bSprite;
                break;
            case MarkerType.Banned:
                markerSpriteRenderer.sprite = banSprite;
                break;
        }
    }
    public void SetBlockColor(Color color)
    {
        _spriteRenderer.color = color;
    }

    public void onBlockClicked()
    {
        _onBlockClicked?.Invoke(_blockIndex);
    }
}

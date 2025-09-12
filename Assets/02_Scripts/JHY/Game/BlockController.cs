using UnityEngine;

public class BlockController : MonoBehaviour
{
    [SerializeField] private Block[] blocks;
    [SerializeField] private Block blockPrefab;

    public delegate void OnBlockClicked(int row, int col);
    public OnBlockClicked OnBlockClickedDelegate;

    private Vector3 firstBlockPos = new Vector3(-4.73f, 5.32f, -7f);

    private float blockSize = 0.63f;
    public float gapSize = 0.045f;

    private Block _currentFocusBlock;

    private void Awake()
    {
        blocks = new Block[Constants.BlockColumnCount * Constants.BlockColumnCount];
    }
    public Block[] GetBlocks()
    {
        return blocks;
    }

    public void InitBlocks()
    {
        float stepSize = blockSize + gapSize;

        for (int row = 0; row < Constants.BlockColumnCount; row++)
        {
            for (int col = 0; col < Constants.BlockColumnCount; col++)
            {
                int index = row * Constants.BlockColumnCount + col;
                int r = row;
                int c = col;

                float x = firstBlockPos.x + col * stepSize;
                float y = firstBlockPos.y - row * stepSize;

                Vector3 pos = new Vector3(x, y);
                Block block = Instantiate(blockPrefab, pos, Quaternion.identity, transform);

                blocks[index] = block;

                block.InitMarker(index, blockIndex =>
                {
                    OnBlockClickedDelegate?.Invoke(r, c);
                });
            }
        }
    }

    public void PlaceScope(Block.MarkerType markerType, int row, int col)
    {
        var blockIndex = row * Constants.BlockColumnCount + col;

        // 기존에 선택한 블록과 새로 선택한 블록이 동일한 경우
        if (_currentFocusBlock == blocks[blockIndex])
            return;

        // 이미 블록을 선택했었다면 기존 블록의 스코프 해제 후 새로 누른 블록 스코프 켜기
        if(_currentFocusBlock != null)
        {
            _currentFocusBlock.IsScopeOn = false;
            _currentFocusBlock.CurrentMarkerType = Block.MarkerType.None;
            _currentFocusBlock = null;
        }

        // 새로 누른 블록의 스코프 키기
        _currentFocusBlock = blocks[blockIndex];
        _currentFocusBlock.IsScopeOn = true;
        _currentFocusBlock.CurrentMarkerType = markerType;
    }

    // 착수 버튼 클릭 시 호출하기 -> SetAsNewValue 함수로 변경
    public void SetMarker()
    {
        if (_currentFocusBlock == null) 
            return;

        if(_currentFocusBlock.IsScopeOn)
        {
            _currentFocusBlock.IsScopeOn = false;
            _currentFocusBlock.SetMarker();
            _currentFocusBlock = null;
        }
    }

    public void SetBlockColor()
    {
    }
}
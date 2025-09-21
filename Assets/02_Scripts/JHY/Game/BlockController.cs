using UnityEngine;
using static Constants;

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
        blocks = new Block[BlockColumnCount * BlockColumnCount];

    }
    public Block[] GetBlocks()
    {
        return blocks;
    }
    public bool IsScopeBlock()
    {
        return _currentFocusBlock != null ;
    }

    // 현재 포커스된 블록의 row, col 반환
    public (int, int) GetFocusBlockPosition()
    {
        if (_currentFocusBlock == null)
            return (-1, -1);

        int index = _currentFocusBlock._blockIndex;

        int row = index / BlockColumnCount;
        int col = index % BlockColumnCount;

        return (row, col);
    }

    public void ResetRound()
    {
        _currentFocusBlock = null;

        for (int i = 0; i < BlockColumnCount * BlockColumnCount; i++)
        {
            blocks[i].ResetBlock();
        }
    }

    public void InitBlocks()
    {
        float stepSize = blockSize + gapSize;

        for (int row = 0; row < BlockColumnCount; row++)
        {
            for (int col = 0; col < BlockColumnCount; col++)
            {
                int index = row * BlockColumnCount + col;
                int r = row;
                int c = col;

                float x = firstBlockPos.x + col * stepSize;
                float y = firstBlockPos.y - row * stepSize;

                Vector3 pos = new Vector3(x, y);
                Block block = Instantiate(blockPrefab, pos, Quaternion.identity, transform);

                blocks[index] = block;

                block.InitMarker(index, (clickedIndex) =>
                {
                    int row = clickedIndex / Constants.BlockColumnCount;
                    int col = clickedIndex % Constants.BlockColumnCount;

                    Debug.Log($"[BLOCKCONTROLLER] 블록 클릭됨 index={clickedIndex}, row={row}, col={col}");

                    OnBlockClickedDelegate?.Invoke(row, col);
                });
            }
        }
    }

    public void PlaceScope(Block.MarkerType markerType, int row, int col)
    {
        var blockIndex = row * BlockColumnCount + col;

        // 선택한 블록에 마커가 이미 존재할 경우
        if (blocks[blockIndex].CurrentMarkerType != Block.MarkerType.None) 
            return;

        // 현재 포커스된 블록과 새로 클릭한 블록이 동일한 경우
        if (_currentFocusBlock == blocks[blockIndex])
            return;

        // 이미 블록을 선택했었다면 기존 블록의 스코프 해제
        if(_currentFocusBlock != null)
        {
            _currentFocusBlock.IsScopeOn = false;
            _currentFocusBlock.CurrentMarkerType = Block.MarkerType.None;
        }

        // 새로 누른 블록의 스코프 키기
        _currentFocusBlock = blocks[blockIndex];
        _currentFocusBlock.IsScopeOn = true;
        _currentFocusBlock.CurrentMarkerType = markerType;
    }

    // 착수 버튼 클릭 시 GameLogic 을 통해 호출될 함수
    public void SetMarker()
    {
        if (_currentFocusBlock == null)
            return;

        _currentFocusBlock.IsScopeOn = false;
        _currentFocusBlock.SetMarker();
        _currentFocusBlock = null;
    }

    public void SetBlockColor()
    {
    }

    public void PlaceStone(Block.MarkerType markerType, int row, int col)
    {
        int blockIndex = row * Constants.BlockColumnCount + col;
        var block = blocks[blockIndex];

        // 이미 돌이 있는 경우 무시
        if (block.CurrentMarkerType != Block.MarkerType.None)
            return;

        if (block.isBanned)        
            return;
        

        block.CurrentMarkerType = markerType;
        block.IsScopeOn = false;
        block.SetMarker();

        Debug.Log($"PlaceStone 실행됨 row={row}, col={col}, marker={markerType}");
    }

    public void ClearScope()
    {
        if (_currentFocusBlock != null)
        {
            _currentFocusBlock.IsScopeOn = false;
            _currentFocusBlock = null;
        }
    }
}

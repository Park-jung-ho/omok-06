using UnityEngine;

public class BlockController : MonoBehaviour
{
    [SerializeField] private GameObject boardPrefab;
    [SerializeField] private Block[] blocks;
    [SerializeField] private Block blockPrefab;

    public delegate void OnBlockClicked(int row, int col);
    public OnBlockClicked OnBlockClickedDelegate;

    private Vector3 firstBlockPos = new Vector3(-4.4f, 5f);

    private float blockSize = 0.63f;
    public float gapSize = 0.045f;

    private void Start()
    {
        blocks = new Block[Constants.BlockColumnCount * Constants.BlockColumnCount];

        InitBoard();
        InitBlocks();
    }

    public void InitBoard()
    {
        Vector3 pos = new Vector3(0f, 0.6f, 0f);
        Instantiate(boardPrefab, pos, Quaternion.identity);
    }

    public void InitBlocks()
    {
        float stepSize = blockSize + gapSize;

        for (int row = 0; row < Constants.BlockColumnCount; row++)
        {
            for (int col = 0; col < Constants.BlockColumnCount; col++)
            {
                int index = row * Constants.BlockColumnCount + col;

                float x = firstBlockPos.x + col * stepSize;
                float y = firstBlockPos.y - row * stepSize;

                Vector3 pos = new Vector3(x, y);
                Block block = Instantiate(blockPrefab, pos, Quaternion.identity, transform);


                block.InitMarker(index, blockIndex =>
                {
                    OnBlockClickedDelegate?.Invoke(row, col);
                });

                blocks[index] = block;
            }
        }
    }

    public void PlaceMaker(Block.MarkerType markerType, int row, int col)
    {
        var blockIndex = row * Constants.BlockColumnCount + col;
        blocks[blockIndex].SetMarker(markerType);
    }

    public void SetBlockColor()
    {
    }
}
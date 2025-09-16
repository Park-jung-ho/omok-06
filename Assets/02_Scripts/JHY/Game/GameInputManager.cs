using UnityEngine;

public class GameInputManager : MonoBehaviour
{
    [SerializeField] private LayerMask blockLayer;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, blockLayer))
            {
                Block block = hit.collider.GetComponent<Block>();

                if (block != null)
                {
                    block.onBlockClicked();
                }
            }
        }
    }
}

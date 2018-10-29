using UnityEngine;
using UnityEngine.Networking;

public class PlayerController : NetworkBehaviour
{
    public GameObject CameraPrefab;

    public int ID = 1;
    public WorldController WorldController;

    private Vector2 _lastGeneratedChunkPosition;
    private Vector2 _lastGeneratedChunkWorldPosition;

    public override void OnStartLocalPlayer()
    {
        Instantiate(CameraPrefab, transform);
        WorldController = GetComponent<WorldController>();
        WorldController.LocalPlayerController = this;
        CaptureLastGeneratedChunks();

        StartCoroutine(WorldController.SetInitialPlayerPosition(transform.position));
    }


    private void Update()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        Vector2 currentChunkPosition = Chunk.CalculateResidingChunk(transform.position);

        if (_lastGeneratedChunkPosition != currentChunkPosition)
        {
            Vector2 difference = _lastGeneratedChunkWorldPosition - (Vector2) transform.position;
            if (Mathf.Abs(difference.x) >= Chunk.SIZE || Mathf.Abs(difference.y) >= Chunk.SIZE)
            {
                WorldController.UpdatePlayerPosition(transform.position);
                CaptureLastGeneratedChunks();
            }
        }
    }

    private void CaptureLastGeneratedChunks()
    {
        _lastGeneratedChunkPosition = Chunk.CalculateResidingChunk(transform.position);
        _lastGeneratedChunkWorldPosition = Chunk.CalculateWorldPosition(_lastGeneratedChunkPosition) +
                                           new Vector2(Chunk.SIZE * 0.5f, Chunk.SIZE * 0.5f);
        Debug.Log("Generated Chunk: " + _lastGeneratedChunkPosition);
        Debug.Log("Generated Chunk World Position: " + _lastGeneratedChunkWorldPosition);
    }
}
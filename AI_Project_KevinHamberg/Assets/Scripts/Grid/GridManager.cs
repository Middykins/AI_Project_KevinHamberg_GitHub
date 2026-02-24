using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class Node
{
    public int X { get; }
    public int Y { get; }

    public bool Walkable { get; private set; }
    public GameObject Tile { get; }

    public float GCost { get; set; }
    public float HCost { get; set; }
    public Node Parent { get; set; }

    public float FCost => GCost + HCost;

    public Node(int x, int y, bool walkable, GameObject tile)
    {
        X = x;
        Y = y;
        Walkable = walkable;
        Tile = tile;
        ResetCosts();
    }

    public void SetWalkable(bool walkable)
    {
        Walkable = walkable;
    }

    public void ResetCosts()
    {
        GCost = float.PositiveInfinity;
        HCost = 0f;
        Parent = null;
    }
}

namespace Day02_AStar.Grid
{
    public class GridManager : MonoBehaviour
    {
        [Header("Grid Settings")]
        public int width = 10;
        public int height = 10;
        public float cellSize = 1f;

        [Header("Prefabs & Materials")]
        public GameObject tilePrefab;
        public Material walkableMaterial;
        public Material wallMaterial;

        [Header("Input")]
        [SerializeField] private Camera inputCamera;
        [SerializeField] private Transform tilesRoot;
        [SerializeField] private TextAsset level;

        private Node[,] nodes;
        private readonly Dictionary<GameObject, Node> tileToNode = new Dictionary<GameObject, Node>();
        private readonly Queue<GameObject> tilePool = new Queue<GameObject>();
        private InputAction clickAction;
        [SerializeField] private char[] levelCharacters;

        private void Awake()
        {
            if (tilesRoot == null)
            {
                tilesRoot = new GameObject("TilesRoot").transform;
                tilesRoot.parent = this.transform;
            }

            if (inputCamera == null)
            {
                inputCamera = Camera.main;
            }

            levelCharacters = level.text.Replace("\n","").Replace("\r", "").ToCharArray();

            GenerateGrid();
            InitializeWalls();
        }

        private void GenerateGrid()
        {
            ClearGrid();
            tileToNode.Clear();

            if (tilePrefab == null)
            {
                Debug.LogError($"{nameof(GridManager)}: Missing tile prefab reference.", this);
                nodes = null;
                return;
            }

            nodes = new Node[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {

                    Vector3 worldPos = new Vector3(x * cellSize, 0f, y * cellSize);
                    GameObject tileGO = AcquireTileInstance(worldPos);
                    tileGO.name = $"Tile_{x}_{y}";


                    Node node = new Node(x, y, true, tileGO);
                    nodes[x, y] = node;
                    tileToNode[tileGO] = node;


                    SetTileMaterial(node, walkableMaterial);
                }
            }
        }

        private GameObject AcquireTileInstance(Vector3 worldPos)
        {
            GameObject tileGO = tilePool.Count > 0
                ? tilePool.Dequeue()
                : Instantiate(tilePrefab, worldPos, Quaternion.identity, tilesRoot);

            tileGO.transform.SetParent(tilesRoot, false);
            tileGO.transform.SetPositionAndRotation(worldPos, Quaternion.identity);
            tileGO.SetActive(true);
            return tileGO;
        }

        private void ClearGrid()
        {
            if (nodes == null)
            {
                return;
            }

            foreach (Node node in nodes)
            {
                if (node?.Tile == null)
                {
                    continue;
                }

                node.Tile.SetActive(false);
                node.Tile.transform.SetParent(tilesRoot, false);
                tilePool.Enqueue(node.Tile);
            }
        }

        public bool IsWithinBounds(int x, int y)
        {
            return x >= 0 && x < width && y >= 0 && y < height;
        }

        public void ResetAllNodes()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    nodes[x, y].ResetCosts();
                }
            }
        }

        public bool TryGetNode(int x, int y, out Node node)
        {
            if (IsWithinBounds(x, y))
            {
                node = nodes[x, y];
                return true;
            }

            node = null;
            return false;
        }

        public Node GetNode(int x, int y)
        {
            return TryGetNode(x, y, out Node node) ? node : null;
        }

        public void SetWalkable(Node node, bool walkable)
        {
            if (node == null)
            {
                return;
            }

            node.SetWalkable(walkable);
            SetTileMaterial(node, walkable ? walkableMaterial : wallMaterial);
            SetTileScale(node, walkable ? new Vector3(1f, 0.2f, 1f) : new Vector3(1f, 2, 1f), walkable ? 0f : 0.9f);
        }

        private void SetTileMaterial(Node node, Material mat)
        {
            if (node?.Tile == null)
            {
                return;
            }

            if (mat == null)
            {
                Debug.LogWarning("Missing material reference for tile update.", this);
                return;
            }

            if (node.Tile.TryGetComponent(out MeshRenderer renderer))
            {
                renderer.material = mat;
            }
        }

        private void SetTileScale(Node node, Vector3 scale, float height)
        {
            if (node?.Tile == null)
            {
                return;
            }

            node.Tile.transform.localScale = scale;
            node.Tile.transform.position = new Vector3(node.Tile.transform.position.x, height, node.Tile.transform.position.z);
        }

        public IEnumerable<Node> GetNeighbours(Node node, bool allowDiagonals = true)
        {
            if (node == null)
            {
                yield break;
            }
            int x = node.X;
            int y = node.Y;


            Node right = GetNode(x + 1, y);
            Node left = GetNode(x - 1, y);
            Node up = GetNode(x, y + 1);
            Node down = GetNode(x, y - 1);

            if (right != null) yield return right;
            if (left != null) yield return left;
            if (up != null) yield return up;
            if (down != null) yield return down;

            if (allowDiagonals)
            {
                Node diagRightUp = GetNode(x + 1, y + 1);
                Node diagLeftUp = GetNode(x - 1, y + 1);
                Node diagRightDown = GetNode(x + 1, y - 1);
                Node diagLeftDown = GetNode(x - 1, y - 1);


                if (diagRightUp != null) yield return diagRightUp;
                if (diagLeftUp != null) yield return diagLeftUp;
                if (diagRightDown != null) yield return diagRightDown;
                if (diagLeftDown != null) yield return diagLeftDown;
            }
        }


        public Node GetNodeFromWorldPosition(Vector3 worldPos)
        {
            int x = Mathf.RoundToInt(worldPos.x / cellSize);
            int y = Mathf.RoundToInt(worldPos.z / cellSize);
            return GetNode(x, y);
        }


        public Node GetNodeFromTile(GameObject tileGO)
        {
            if (tileGO == null)
            {
                return null;
            }


            return tileToNode.TryGetValue(tileGO, out var node) ? node : null;
        }

        private void OnEnable()
        {

            clickAction = new InputAction(
                name: "Click",
                type: InputActionType.Button,
                binding: "<Mouse>/leftButton"
            );

            clickAction.performed -= OnClickPerformed;
            clickAction.performed += OnClickPerformed;
            clickAction.Enable();
        }

        private void OnDisable()
        {

            if (clickAction != null)
            {
                clickAction.performed -= OnClickPerformed;
                clickAction.Disable();
            }
        }

        private void OnDestroy()
        {
            if (clickAction == null)
            {
                return;
            }

            clickAction.Dispose();
            clickAction = null;
        }

        private void OnClickPerformed(InputAction.CallbackContext ctx)
        {
            HandleMouseClick();
        }

        private void HandleMouseClick()
        {
            if (Mouse.current == null)
            {
                return;
            }

            Camera cameraToUse = inputCamera != null ? inputCamera : Camera.main;
            if (cameraToUse == null)
            {
                Debug.LogWarning("No camera available for grid clicks.", this);
                return;
            }

            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Ray ray = cameraToUse.ScreenPointToRay(new Vector3(mousePosition.x, mousePosition.y, 0f));
            if (!Physics.Raycast(ray, out RaycastHit hitInfo))
            {
                return;
            }

            Node node = GetNodeFromTile(hitInfo.collider.gameObject);
            if (node != null)
            {
                SetWalkable(node, !node.Walkable);
            }
        }

        private void InitializeWalls()
        {
            Node node = null;
            for (int i = 0; i < 100; i++)
            {
                for(int j = 0; j < 100; j++)
                {
                    if (levelCharacters[(i*100+99-j)] == 'X')
                    {
                        node = GetNode(j, i);
                        SetWalkable(node, !node.Walkable);
                    }
                }
            }
        }

    }
}
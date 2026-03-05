using Day02_AStar.Pathfinding;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;
using Day02_AStar.Grid;

namespace Day02_AStar.Agents
{
    public class AgentMover : MonoBehaviour
    {
        public enum AgentState
        {
            Idle,
            Moving,
            ResetMovement,
            SetTarget
        }

        public Pathfinder pathfinder;
        public float moveSpeed = 2f;

        private Coroutine moveRoutine;

        private InputAction followAction;

        private float timer;

        [SerializeField] private AgentState currentState;

        [SerializeField] private GameObject ownBase;
        [SerializeField] private GameObject unitTarget;

        private void OnEnable()
        {
            currentState = AgentState.Idle;
            followAction = new InputAction(
                name: "FollowPath",
                type: InputActionType.Button,
                binding: "<Keyboard>/r"
            );

            followAction.performed += OnFollowPerformed;
            followAction.Enable();

            

            if (this.gameObject.tag == "Team 1 Worker")
            {
                ownBase = GameObject.FindGameObjectWithTag("Team 1 Base");
                unitTarget = GameObject.FindGameObjectWithTag("Team 1 Resource");
            }
            else if (this.gameObject.tag == "Team 2 Worker")
            {
                ownBase = GameObject.FindGameObjectWithTag("Team 2 Base");
                unitTarget = GameObject.FindGameObjectWithTag("Team 2 Resource");
            }
        }

        private void OnDisable()
        {
            if (followAction != null)
            {
                followAction.performed -= OnFollowPerformed;
                followAction.Disable();
            }
        }

        private void OnFollowPerformed(InputAction.CallbackContext ctx)
        {
            StartFollowPath();
        }

        private void StartFollowPath()
        {
            if (pathfinder == null)
            {
                Debug.LogWarning("AgentMover: Pathfinder reference missing.");
                return;
            }

            var grid = pathfinder.gridManager;
            if (grid == null)
            {
                Debug.LogWarning("AgentMover: GridManager reference missing.");
                return;
            }

            Node startNode = grid.GetNodeFromWorldPosition(transform.position);
            Node goalNode = grid.GetNodeFromWorldPosition(pathfinder.goalMarker.transform.position);

            if (startNode == null || goalNode == null)
            {
                Debug.LogWarning("AgentMover: Invalid start or goal node.");
                return;
            }

            var openVisual = new HashSet<Node>();
            var closedVisual = new HashSet<Node>();
            List<Node> path = pathfinder.FindPath(startNode, goalNode, openVisual, closedVisual);

            if (path == null || path.Count == 0)
            {
                Debug.Log("AgentMover: No path found.");
                return;
            }

            if (moveRoutine != null)
            {
                StopCoroutine(moveRoutine);
            }

            moveRoutine = StartCoroutine(FollowPath(path));
        }

        private IEnumerator FollowPath(List<Node> path)
        {
            int startIndex = 0;
            Node first = path[0];
            Vector3 firstWorldPos = new Vector3(first.X * pathfinder.gridManager.cellSize, 0f, first.Y * pathfinder.gridManager.cellSize);

            if (Vector3.Distance(transform.position, firstWorldPos) < 0.1f && path.Count > 1)
            {
                startIndex = 1;
            }

            for (int i = startIndex; i < path.Count; i++)
            {
                Node node = path[i];
                Vector3 targetPos = new Vector3(node.X * pathfinder.gridManager.cellSize, transform.position.y, node.Y * pathfinder.gridManager.cellSize);

                while (Vector3.Distance(transform.position, targetPos) > 0.01f)
                {
                    transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
                    yield return null; 
                }
            }

            moveRoutine = null;
        }

        private void Update()
        {
            if (moveRoutine == null || currentState == AgentState.ResetMovement)
            {
                timer += Time.deltaTime;
                currentState = AgentState.Idle;
                if (Vector3.Distance(this.transform.position, pathfinder.GetGoal().transform.position) > 2 && timer > 1.0f)
                {
                    StartFollowPath();
                    currentState = AgentState.Moving;
                    timer = 0.0f;
                }
            }

            if (Vector3.Distance(this.transform.position, pathfinder.goalMarker.transform.position) < 2f)
            {
                currentState = AgentState.SetTarget;
            }

            if(currentState == AgentState.SetTarget)
            {
                if (this.tag == "Team 1 Worker" || this.tag == "Team 2 Worker")
                {
                    if (Vector3.Distance(this.transform.position, ownBase.transform.position) < 2f)
                    {
                        pathfinder.SetTarget(unitTarget);
                        currentState = AgentState.ResetMovement;
                    }
                    else if (Vector3.Distance(this.transform.position, unitTarget.transform.position) < 2f)
                    {
                        pathfinder.SetTarget(ownBase);
                        currentState = AgentState.ResetMovement;
                    }
                }
                
            }
        }

        public AgentState GetAgentState()
        {
            return currentState;
        }

        public void SetAgentState(AgentState state)
        {
            currentState = state;
        }
    }
}
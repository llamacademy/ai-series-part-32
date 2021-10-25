using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ScenarioManager : MonoBehaviour
{
    public ObstacleAvoidanceType AvoidanceType;
    public NavMeshAgent AgentPrefab;
    public bool RandomizePriority = false;
    public float AgentSpeed = 2f;
    public float AgentRadius = 0.33f;

    [Header("Object References")]
    public GameObject Cubes;
    public NavMeshSurface Surface;

    public List<NavMeshAgent> Agents = new List<NavMeshAgent>();

    [Header("NavMesh Configurations")]
    public float AvoidancePredictionTime = 2;
    public int PathfindingIterationsPerFrame = 100;

    [Header("Circle Configuration")]
    public float CircleRadius = 25;
    [SerializeField]
    public int AgentsInCircle = 100;

    [Header("Narrow Path Configuration")]
    public int NarrowPathwayAgentsPerRegion = 25;
    public float NarrowPathwayOffset = 10;

    public float InvokeDelay = 2f;

    private void Update()
    {
        NavMesh.avoidancePredictionTime = AvoidancePredictionTime;
        NavMesh.pathfindingIterationsPerFrame = PathfindingIterationsPerFrame;
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 300, 30), "Run Circle Scenario"))
        {
            DestroyAllAgents();
            RunCircleScenario();
        }
        if (GUI.Button(new Rect(10, 50, 300, 30), "Run Narrow Pathway Scenario"))
        {
            DestroyAllAgents();
            RunNarrowPathwayScenario();
        }
        if (GUI.Button(new Rect(10, 90, 300, 30), "Run 1:1 Scenario"))
        {
            DestroyAllAgents();
            Run1On1Scenario();
        }
    }

    private void DestroyAllAgents()
    {
        Agents.ForEach(agent => Destroy(agent.gameObject));
        Agents.Clear();
    }

    private void SetAgentColors(NavMeshAgent Agent, Color Color)
    {
        Mesh mesh = Agent.GetComponent<MeshFilter>().mesh;
        Color[] colors = mesh.colors;
        for (int j = 0; j < colors.Length; j++)
        {
            colors[j] = Color;
        }
        mesh.colors = colors;
    }

    private void SetupAgent(NavMeshAgent Agent)
    {
        Agent.obstacleAvoidanceType = AvoidanceType;
        Agent.radius = AgentRadius;
        Agent.speed = AgentSpeed;
        if (RandomizePriority)
        {
            Agent.avoidancePriority = Random.Range(0, 100);
        }
        Agents.Add(Agent);
    }

    private void RunCircleScenario()
    {
        Cubes.SetActive(false);
        Surface.BuildNavMesh();
        float red = 1f;
        float green = 0f;
        float blue = 0f;
        int section = 0;

        for (int i = 0; i < AgentsInCircle; i++)
        {
            // Place in a circle
            NavMeshAgent agent = Instantiate(AgentPrefab, new Vector3(CircleRadius * Mathf.Cos(2 * Mathf.PI * i / AgentsInCircle), 0, CircleRadius * Mathf.Sin(2 * Mathf.PI * i / AgentsInCircle)), Quaternion.identity);
            SetupAgent(agent);

            // Apply Vertex Colors for Agent differentiation
            switch (section)
            {
                case 0:
                    blue += i / (float)AgentsInCircle / 6f;
                    if (blue >= 1)
                    {
                        section++;
                    }
                    break;
                case 1:
                    red -= i / (float)AgentsInCircle / 6f;
                    if (red <= 0)
                    {
                        section++;
                    }
                    break;
                case 2:
                    green += i / (float)AgentsInCircle / 6f;
                    if (green >= 1)
                    {
                        section++;
                    }
                    break;
                case 3:
                    blue -= i / (float)AgentsInCircle / 6f;
                    if (blue <= 0)
                    {
                        section++;
                    }
                    break;
                case 4:
                    red += i / (float)AgentsInCircle / 6f;
                    if (red >= 1)
                    {
                        section++;
                    }
                    break;
                case 5:
                    green -= i / (float)AgentsInCircle / 6f;
                    break;
            }

            SetAgentColors(agent, new Color(red, green, blue));
        }

        Invoke("SetCircleDestinations", InvokeDelay);
    }

    private void SetCircleDestinations()
    {
        for (int i = 0; i < Agents.Count; i++)
        {
            Agents[i].SetDestination(new Vector3(
                CircleRadius * Mathf.Cos(2 * Mathf.PI * (i + AgentsInCircle / 2) / AgentsInCircle),
                0,
                CircleRadius * Mathf.Sin(2 * Mathf.PI * (i + AgentsInCircle / 2) / AgentsInCircle)
            ));
        }
    }

    private void RunNarrowPathwayScenario()
    {
        Cubes.SetActive(true);
        Surface.BuildNavMesh();
        float squareRoot = Mathf.Ceil(Mathf.Sqrt(NarrowPathwayAgentsPerRegion));
        float red = 1;
        float green = 0;
        float blue = 0;
        for (int i = 0; i < 4; i++)
        {
            switch (i)
            {
                case 0:
                    red = 1;
                    break;
                case 1:
                    red = 0;
                    green = 1;
                    break;
                case 2:
                    blue = 1;
                    break;
                case 3:
                    green = 0;
                    break;
            }
            Color color = new Color(red, green, blue);

            int xMultiplier = i < 2 ? -1 : 1;
            int zMultiplier = i % 2 == 1 ? -1 : 1;
            for (int x = 0; x < squareRoot; x++)
            {
                for (int z = 0; z < squareRoot; z++)
                {
                    NavMeshAgent agent = Instantiate(AgentPrefab, new Vector3(
                        NarrowPathwayOffset * xMultiplier + x - squareRoot / 2f,
                        0,
                        NarrowPathwayOffset * zMultiplier + z - squareRoot / 2f
                    ), Quaternion.identity);

                    SetupAgent(agent);

                    SetAgentColors(agent, color);
                }
            }
        }

        Invoke("SetNarrowPathwayDestinations", InvokeDelay);
    }

    private void SetNarrowPathwayDestinations()
    {
        float squareRoot = Mathf.Ceil(Mathf.Sqrt(NarrowPathwayAgentsPerRegion));
        int index = 0;
        for (int i = 0; i < 4; i++)
        {
            int xMultiplier = i < 2 ? 1 : -1;
            int zMultiplier = i % 2 == 1 ? 1 : -1;
            for (int x = 0; x < squareRoot; x++)
            {
                for (int z = 0; z < squareRoot; z++)
                {
                    Agents[index].SetDestination(new Vector3(
                        NarrowPathwayOffset * xMultiplier + x - squareRoot / 2f,
                        0,
                        NarrowPathwayOffset * zMultiplier + z - squareRoot / 2f
                    ));
                    index++;
                }             
            }
        }
    }

    private void Run1On1Scenario()
    {
        Cubes.SetActive(false);
        Surface.BuildNavMesh();

        NavMeshAgent agent1 = Instantiate(AgentPrefab, new Vector3(5, 0, 0), Quaternion.identity);
        SetupAgent(agent1);

        NavMeshAgent agent2 = Instantiate(AgentPrefab, new Vector3(-5, 0, 0), Quaternion.identity);
        SetupAgent(agent2);

        SetAgentColors(agent1, Color.red);
        SetAgentColors(agent2, Color.blue);

        Invoke("Set1On1Destinations", InvokeDelay);
    }

    private void Set1On1Destinations()
    {
        Agents[0].SetDestination(Agents[1].transform.position);
        Agents[1].SetDestination(Agents[0].transform.position);
    }
}

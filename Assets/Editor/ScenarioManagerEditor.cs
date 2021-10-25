using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

[CustomEditor(typeof(ScenarioManager))]
public class ScenarioManagerEditor : Editor
{
    private GUIStyle LabelStyle;
    private Editor SurfaceEditor;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        ScenarioManager manager = (ScenarioManager)target;
        if (SurfaceEditor != null)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Nav Mesh Surface Configuration", EditorStyles.boldLabel);
            SurfaceEditor.DrawDefaultInspector();
        }
        else if (manager.Surface != null)
        {
            SurfaceEditor = CreateEditor(manager.Surface);
        }
    }

    public void OnEnable()
    {
        LabelStyle = new GUIStyle()
        {
            normal = new GUIStyleState()
            {
                textColor = Color.white
            },
            fontSize = 16
        };

        ScenarioManager manager = (ScenarioManager)target;
        if (SurfaceEditor != null && manager.Surface != null)
        {
            SurfaceEditor = CreateEditor(manager.Surface);
        }
    }

    private void OnSceneGUI()
    {
        ScenarioManager manager = (ScenarioManager)target;

        foreach (NavMeshAgent agent in manager.Agents)
        {
            Handles.Label(agent.transform.position + Vector3.up, $"{agent.avoidancePriority}", LabelStyle);
            Handles.color = agent.GetComponent<MeshFilter>().mesh.colors[0];
            Handles.ArrowHandleCap(EditorGUIUtility.GetControlID(FocusType.Passive), agent.transform.position + Vector3.up * 0.99f, agent.transform.rotation, agent.velocity.magnitude, EventType.Repaint);
            Handles.CircleHandleCap(EditorGUIUtility.GetControlID(FocusType.Passive), agent.transform.position - Vector3.up * agent.height / 2, Quaternion.Euler(90, 0, 0), agent.radius, EventType.Repaint);
            Handles.CircleHandleCap(EditorGUIUtility.GetControlID(FocusType.Passive), agent.transform.position + Vector3.up * agent.height / 2, Quaternion.Euler(90, 0, 0), agent.radius, EventType.Repaint);
            Handles.DotHandleCap(EditorGUIUtility.GetControlID(FocusType.Passive), agent.destination, Quaternion.identity, 0.25f, EventType.Repaint);
        }
    }
}

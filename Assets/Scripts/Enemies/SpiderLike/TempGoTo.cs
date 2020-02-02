using UnityEngine;
using UnityEngine.AI;

public class TempGoTo : MonoBehaviour
{
    public NavMeshAgent Agent;
    public Transform Target;

    private void Update()
    {
        Agent.SetDestination(Target.position);
    }
}

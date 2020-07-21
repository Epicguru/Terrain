using UnityEngine;
using UnityEngine.AI;

namespace Terrain.Enemies.AI
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyNavigation : MonoBehaviour
    {
        public NavMeshAgent Agent
        {
            get
            {
                if (_agent == null)
                    _agent = GetComponent<NavMeshAgent>();
                return _agent;
            }
        }
        private NavMeshAgent _agent;
        public Health Health
        {
            get
            {
                if (_health == null)
                    _health = GetComponent<Health>();
                return _health;
            }
        }
        private Health _health;

        public float Speed
        {
            get
            {
                return Agent.speed;
            }
            set
            {
                Agent.speed = value;
            }
        }

        public bool IsMoving
        {
            get
            {
                return Agent.remainingDistance > Agent.stoppingDistance - 0.2f;
            }
        }

        public Transform Target;

        public Vector3 TargetPos;

        private void Update()
        {
            if (Health.IsDead)
            {
                Agent.enabled = false;
                Agent.destination = transform.position;
                return;
            }

            if (Target != null)
                TargetPos = Target.position;

            Agent.destination = TargetPos;
        }
    }
}

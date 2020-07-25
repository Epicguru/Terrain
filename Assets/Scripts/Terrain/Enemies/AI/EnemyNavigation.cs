﻿using UnityEngine;
using UnityEngine.AI;

namespace Terrain.Enemies.AI
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyNavigation : MonoBehaviour
    {
        private NavMeshAgent Agent
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
                return Agent.enabled && Agent.remainingDistance > Agent.stoppingDistance - 0.2f;
            }
        }

        public float DistanceToTarget
        {
            get
            {
                return Agent.enabled ? Mathf.Max(0f, Agent.remainingDistance - Agent.stoppingDistance) : float.PositiveInfinity;
            }
        }

        public Vector3 Velocity
        {
            get
            {
                if (!Agent.enabled)
                    return Vector3.zero;
                return Agent.velocity;
            }
        }

        public Transform Target;

        public Vector3 TargetPos;

        private void Update()
        {
            if (Health.IsDead)
            {
                Agent.enabled = false;
                return;
            }

            if (Target != null)
            {
                TargetPos = Target.position;
                Agent.destination = TargetPos;
            }
            else
            {
                Agent.ResetPath();
            }

        }
    }
}

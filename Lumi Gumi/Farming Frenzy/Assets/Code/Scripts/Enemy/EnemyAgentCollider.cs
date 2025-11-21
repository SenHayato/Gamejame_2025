using System;
using UnityEngine;

namespace Code.Scripts.Enemy
{
    public class EnemyAgentCollider : MonoBehaviour
    {
        private EnemyAgent _agent;
        
        private void Start()
        {
            _agent = GetComponentInParent<EnemyAgent>();
        }

        private void OnMouseDown()
        {
            _agent.TrySpray();
        }

    }
}
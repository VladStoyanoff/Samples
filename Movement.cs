using UnityEngine;
using UnityEngine.AI;
using RPG.Core;
using RPG.Saving;

namespace RPG.Move 
{
    public class Movement : MonoBehaviour, IAction, ISaveable
    {

        NavMeshAgent navMeshAgent;
        [SerializeField] float maxSpeed = 6f;

        void Awake()
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
        }
        
        // Moves the unit to another unit's location

        public void MoveToTarget(GameObject target, float speedFraction) 
        {
            if (!navMeshAgent.enabled) return;
            navMeshAgent.destination = target.transform.position;
            navMeshAgent.speed = maxSpeed * Mathf.Clamp01(speedFraction);
            ActivateNavMeshAgent();
        }

        // Moves the unit to another location on the scene

        public void Move(Vector3 destination, float speedFraction)
        {
            GetComponent<ActionScheduler>().StartAction(this);
            navMeshAgent.speed = maxSpeed * Mathf.Clamp01(speedFraction);
            if (!navMeshAgent.enabled) return;
            navMeshAgent.destination = destination;
            ActivateNavMeshAgent();
        }

        public void DisableNavMeshAgent()
        {
            if (!navMeshAgent.enabled) return;
            navMeshAgent.isStopped = true;
        }

        public void ActivateNavMeshAgent()
        {
            navMeshAgent.isStopped = false;
        }

        #region Saving

        // Struct that holds information regarding the unit's positon and rotation

        [System.Serializable]
        struct MoverSaveData 
        {
            public SerializableVector3 position;
            public SerializableVector3 rotation;
        }

        // Captures the data aforementioned in the struct and creates it into serializable vector's so that the data can be saved on the disk.

        public object CaptureState()
        {
            MoverSaveData data = new MoverSaveData();
            data.position = new SerializableVector3(transform.position);
            data.rotation = new SerializableVector3(transform.eulerAngles);

            return data;
        }

        // Restores the data from disk

        public void RestoreState(object state)
        {
            MoverSaveData data = (MoverSaveData)state;
            DisableNavMeshAgent();
            transform.position = data.position.ToVector();
            transform.eulerAngles = data.rotation.ToVector();
            ActivateNavMeshAgent();
        }
        #endregion
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public enum EnemyState { Patrolling, Chasing, Searching, Waiting, Attacking }
    public EnemyState currentState;

    private NavMeshAgent _AIAgent;
    private Transform _playerTransform;
    [SerializeField] Transform[] _patrolPoints;
    private int _currentPatrolIndex;
    [SerializeField] float _visionRange = 20, _visionAngle = 120;
    private Vector3 _playerLastPosition;
    float _searchTimer, _searchWaitTime = 15, _searchRadius = 10;
    float _waitTimer;
    public float _waitTime = 5;

    void Awake() {
        _AIAgent = GetComponent<NavMeshAgent>();
        _playerTransform = GameObject.FindWithTag("Player").transform;
    }

    void Start() {
        currentState = EnemyState.Patrolling;
        SetNextPatrolPoint();
    }

    void Update() {
        switch (currentState) {
            case EnemyState.Patrolling: Patrol(); break;
            case EnemyState.Chasing: Chase(); break;
            case EnemyState.Searching: Search(); break;
            case EnemyState.Waiting: Wait(); break;
            case EnemyState.Attacking: Attack(); break;
        }
    }

    void Patrol() {
        if (OnRange()) { currentState = EnemyState.Chasing; return; }
        if (_AIAgent.remainingDistance < 0.5f) { currentState = EnemyState.Waiting; _waitTimer = 0; }
    }

    void Wait() {
        if ((_waitTimer += Time.deltaTime) >= _waitTime) { currentState = EnemyState.Patrolling; SetNextPatrolPoint(); }
    }

    void Chase() {
        if (!OnRange()) { _playerLastPosition = _playerTransform.position; currentState = EnemyState.Searching; return; }
        if (Vector3.Distance(transform.position, _playerTransform.position) < 2.0f) { currentState = EnemyState.Attacking; return; }
        _AIAgent.destination = _playerTransform.position;
    }

    void Search() {
        if (OnRange()) { currentState = EnemyState.Chasing; return; }
        _searchTimer += Time.deltaTime;
        if (_searchTimer < _searchWaitTime) {
            if (_AIAgent.remainingDistance < 0.5f) {
                if (RandomSearchPoint(_playerLastPosition, _searchRadius, out Vector3 randomPoint)) {
                    _AIAgent.destination = randomPoint;
                }
            }
        } else {
            currentState = EnemyState.Patrolling;
            _searchTimer = 0;
            SetNextPatrolPoint();
        }
    }

    void Attack() {
        Debug.Log("Enemy is attacking!");
        currentState = EnemyState.Chasing;
    }

    bool RandomSearchPoint(Vector3 center, float radius, out Vector3 point) {
        for (int i = 0; i < 5; i++) { // Intentar encontrar un punto válido hasta 5 veces
            Vector3 randomPoint = center + Random.insideUnitSphere * radius;
            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 4, NavMesh.AllAreas)) {
                point = hit.position; return true;
            }
        }
        point = center; // Si no encuentra un punto válido, usar la última posición conocida
        return false;
    }

    bool OnRange() {
        Vector3 directionToPlayer = _playerTransform.position - transform.position;
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
        float distanceToPlayer = Vector3.Distance(transform.position, _playerTransform.position);

        if (distanceToPlayer > _visionRange || angleToPlayer > _visionAngle * 0.5f) return false;
        if (Physics.Raycast(transform.position, directionToPlayer, out RaycastHit hit, distanceToPlayer) && hit.collider.CompareTag("Player")) {
            _playerLastPosition = _playerTransform.position;
            return true;
        }
        return false;
    }

    void SetNextPatrolPoint() {
        if (_patrolPoints.Length > 0) {
            _AIAgent.destination = _patrolPoints[_currentPatrolIndex].position;
            _currentPatrolIndex = (_currentPatrolIndex + 1) % _patrolPoints.Length;
        }
    }
}

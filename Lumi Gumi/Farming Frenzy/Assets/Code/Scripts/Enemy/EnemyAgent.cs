using System;
using System.Collections;
using Code.Scripts.Managers;
using Code.Scripts.Plants;
using Code.Scripts.Plants.Powers;
using Code.Scripts.Player;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;
using UnityEngine.UI;

namespace Code.Scripts.Enemy
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyAgent : MonoBehaviour
    {
        private static readonly int X = Animator.StringToHash("X");
        private static readonly int Y = Animator.StringToHash("Y");
        private static readonly int Movement = Animator.StringToHash("Movement");

        #region Editor Fields
        [SerializeField] private float _health;
        [SerializeField] private int _maxHealth;
        [SerializeField] private GameObject _healthBar;
        [SerializeField] private Slider _healthBarController;
        [SerializeField] private GameObject _healthBarVisible;
        #endregion

        #region Properties
        private Transform _plantTransform;
        private NavMeshAgent _agent;
        private Animator _agentAnimator;
        [CanBeNull] private Transform _target;
        private Transform _spawnPoint;
        private SpriteRenderer _spriteRenderer;
        private Rigidbody2D _rigidbody;
        private enum State { Hungry, Eating, Scared}
        private State _currentState;
        [CanBeNull] private Coroutine _eatingCoroutine;
        private Renderer _renderer;

        public bool CanAttack => _currentState != State.Scared && _health > 0;
        private const float Damage = 5;
        private float _timeToNextPlay;
        private float _timer;
        private AudioManager _audioManager;
        private bool playSFX;
        private bool firstUpdate;
        private GameManager _gameManager;
        private Canvas hbCanvas;

        #endregion

        #region Methods

        public void SetSpawn(Transform spawnPoint)
        {
            _spawnPoint = spawnPoint;
        }
        
        private void Start()
        {
            transform.rotation = Quaternion.Euler(-60f,0f,0f);

            _audioManager = AudioManager.Instance;
            _gameManager = GameObject.Find("[GameManager]").GetComponent<GameManager>();
            firstUpdate = false;
            _timeToNextPlay = Random.Range(2,9);
            _plantTransform = GameObject.Find("Plants").transform;
            _agent = GetComponent<NavMeshAgent>();
            _agentAnimator = GetComponent<Animator>();
            _agent.updateRotation = false;
            _agent.updateUpAxis = false;
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _currentState = State.Hungry; //when spawning in for the first time the animal is hungry
            _health = _maxHealth;
            _rigidbody = GetComponentInChildren<Rigidbody2D>();
            _renderer = GetComponent<Renderer>();

            hbCanvas = _healthBar.transform.Find("Canvas").gameObject.GetComponent<Canvas>();
            hbCanvas.transform.position = new Vector2(gameObject.transform.position.x*1000,gameObject.transform.position.y*1000 + 530);
            _healthBar.transform.localScale = new Vector2(0.001f,0.001f);
            _healthBarVisible.SetActive(false);
        }


        private static Transform GetRandomChild(Transform parent)
        {
            if (parent.childCount == 0)
            {
                return null;
            }
            var randomIndex = Random.Range(0, parent.childCount);
            return parent.GetChild(randomIndex);
        }

        private void Update()
        {

            if (!firstUpdate)
            {
                if (_gameManager._goats > 10)
                {
                    float randNum = Random.Range(1, _gameManager._goats + 1);
                    if (randNum > 10) playSFX = false;
                    else playSFX = true;
                }
                else
                {
                    playSFX = true;
                }
                firstUpdate = true;
            }
            var direction = _agent.velocity.normalized;
            _agentAnimator.SetFloat(X, direction.x);
            if (_agentAnimator.GetFloat(X) > 0)
            {
                _spriteRenderer.flipX = false;
            }
            else
            {
                _spriteRenderer.flipX = true;
            }
            _agentAnimator.SetFloat(Y, direction.y);
            _agentAnimator.SetBool(Movement, direction.magnitude > 0);
            _spriteRenderer.sortingOrder = 10000 - Mathf.CeilToInt(transform.position.y);

            if (playSFX)
            {
                _timer += Time.deltaTime;
                if (_timer > _timeToNextPlay)
                {
                    _audioManager.PlayRandomGoatNoise();
                    _timeToNextPlay = Random.Range(2, 9);
                    _timer = 0f;
                }
            }

            hbCanvas.transform.position = gameObject.transform.position + new Vector3(0, 0.85f, 0);
            _healthBar.transform.localScale = new Vector2(0.001f, 0.001f);

            lock (this)
            {
                switch (_currentState)
                {
                    case State.Hungry:
                        {
                            AcquirePlantTarget();
                            if (!_target)
                            {
                                RunAway();
                            }
                            else
                            {
                                _agent.SetDestination(_target.position);
                                _currentState = State.Eating;
                            }

                            break;
                        }
                    case State.Eating:
                        {
                            if (!_target)
                            {
                                _currentState = State.Hungry;
                            }

                            break;
                        }
                    case State.Scared when !_target:
                        RunAway(); //jika enemy harus mati hilangkan RunAway ganti ke Death
                        break;
                    case State.Scared when _agent.remainingDistance > 0.1f && _renderer.isVisible:
                        break;
                    case State.Scared:
                        //_agentAnimator.SetBool("IsAttacking", false);
                        _agent.isStopped = true;
                        Destroy(gameObject);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

            }
        }

        private void AcquirePlantTarget()
        {
            _target = GetRandomChild(_plantTransform);
        }

        private void RunAway()
        {
            if (isDeath) return;
            lock (this)
            {
                print("Running away");
                _currentState = State.Scared;
                _rigidbody.simulated = false;
                CancelEating();

                _target = _spawnPoint;

                if (_agent != null)
                {
                    _agent.SetDestination(_target!.position);
                }

                if(playSFX) _audioManager.PlaySFX("goatScared");
                PlayerController.Instance.EndContextualCursor(PlayerController.CursorState.Spray);
                _healthBarVisible.SetActive(false);
                _gameManager._goats -= 1;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="amount"></param>
        /// <returns>True if the animal is now running away</returns>
        [SerializeField] bool isDeath = false;
        private bool TakeDamage(float amount)
        {
            _health = Math.Max(0, _health - amount);
            _healthBarVisible.SetActive(true);
            _healthBarController.value = _health / _maxHealth;
            print($"Goat took {amount} damage! HP = {_health}");

            if (_health > 0)
            {
                return false;
            }
            else
            {
                isDeath = true;
                Destroy(gameObject);
            }

            RunAway();
            return true;
        }

        private void OnMouseEnter()
        {
            if (CanAttack)
            {
                PlayerController.Instance.StartContextualCursor(PlayerController.CursorState.Spray);
            }
        }

        private void OnMouseExit()
        {
            PlayerController.Instance.EndContextualCursor(PlayerController.CursorState.Spray);
        }

        private void OnMouseDown()
        {
            TrySpray();
        }

        public void TrySpray()
        {
            if(PlayerController.Instance.CurrentlyActiveCursor != PlayerController.CursorState.Spray) return;

            print($"I am goat; _health = {_health}; _state = {_currentState}; _target = {_target?.gameObject}; stopped = {_agent.isStopped}");

            if (!CanAttack) return;
            if (!PlayerController.Instance.TryPurchase(PlayerController.Instance.SprayPurchaseAmount))
            {
                GameManager.Instance.ShowFloatingText("Not enough gold");
                return;
            }

            GameManager.Instance.ShowFloatingText($"-{PlayerController.Instance.SprayPurchaseAmount}G");
            PlayerController.Instance.SprayParticles();

            _audioManager.PlaySFX("spray");
            TakeDamage(_maxHealth); //sekali hit ini
        }
        #endregion

        public void StartAttacking(Plant plant)
        {
            lock (this)
            {
                if (!CanAttack) return;
                if (_eatingCoroutine != null) return;
            
                print($"Going to start attacking {plant.PlantName}, because state = {_currentState}");

                _target = plant.transform;
                _agent.ResetPath();
                _agent.SetDestination(_target.position);
                _currentState = State.Eating;
                _eatingCoroutine = StartCoroutine(DoEating(plant));
            }
        }

        private void CancelEating()
        {
            if (isDeath) return;
            lock (this)
            {
                if (_eatingCoroutine != null)
                {
                    StopCoroutine(_eatingCoroutine);
                }

                _eatingCoroutine = null;
                if (_agent != null)
                {
                    _agent.isStopped = false;
                }
            }
        }

        private IEnumerator DoEating(Plant plant)
        {
            var spiky = plant.GetComponentInChildren<SpikyPower>();
            print($"Spike damage: {spiky?.Damage ?? 0}. Plant is {plant.PlantName}");

            // Force the agent to stop and eat current plant
            _agent.isStopped = true;
            yield return new WaitForSeconds(0.2f);
            _agent.isStopped = false;
            yield return new WaitForSeconds(2f);

            if(playSFX) _audioManager.PlaySFX("goatEating");
            
            while (true)
            {
                lock (this)
                {
                    var plantDies = !plant || plant.TakeDamage(Damage);
                    var animalRunsAway = spiky && TakeDamage(spiky.Damage);
    
                    if (plantDies)
                    {
                        _target = null;
                        _currentState = State.Hungry;
                        _eatingCoroutine = null;
                    }
    
                    if (animalRunsAway)
                    {
                        RunAway();
                        _eatingCoroutine = null;
                    }

                    if (animalRunsAway || plantDies)
                    {
                        yield break;
                    }
                }

                yield return new WaitForSeconds(2f);
            }
        }
    }
}

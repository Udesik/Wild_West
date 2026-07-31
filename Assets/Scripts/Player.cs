using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class Player : MonoBehaviour
{
    [SerializeField] private GameObject _cameraPoint;
    [SerializeField] private Animator _animator;

    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _lookSpeed;
    [SerializeField] private float _jumpForce;
    [SerializeField] private Transform _groundCheckPoint;
    [SerializeField] private float _groundRadius = 0.2f;
    [SerializeField] private LayerMask _groundLayer;
    
    private PlayerInput _playerInput;
    private Rigidbody _rigidbody;

    private Vector2 _moveDirection;
    private Vector2 _lookDirection;
    private bool _isGrounded = true;
    private float _isSprinting;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    private static readonly int JumpHash = Animator.StringToHash("Jump");
    private static readonly int HorizontalHash = Animator.StringToHash("Horizontal");
    private static readonly int VerticalHash = Animator.StringToHash("Vertical");

    private void Awake()
    {
        _playerInput = new PlayerInput();
        _rigidbody = GetComponent<Rigidbody>();

        //_playerInput.Player.Move.performed += OnMove;
        //_playerInput.Player.Look.performed += OnLook;
        //_playerInput.Player.Jump.performed += OnJump;
    }

    private void OnEnable()
    {
        _playerInput.Enable();
    }

    private void OnDisable()
    {
        _playerInput.Disable();

        Cursor.lockState = CursorLockMode.None;
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        _lookDirection = _playerInput.Player.Look.ReadValue<Vector2>();
        _moveDirection = _playerInput.Player.Move.ReadValue<Vector2>();
        _isSprinting = _playerInput.Player.Sprint.ReadValue<float>();

        _isGrounded = Physics.CheckSphere(_groundCheckPoint.position, _groundRadius, _groundLayer);
        _animator.SetBool(IsGroundedHash, _isGrounded);
        _animator.SetFloat(HorizontalHash, _moveDirection.x);
        _animator.SetFloat(VerticalHash, _moveDirection.y);

        Look();
        Move();

        _rigidbody.angularVelocity = Vector3.zero;
    }

    private void Look()
    {
        if (_lookDirection.sqrMagnitude < 0.1f)
            return;

        float scaledLookSpeed = _lookSpeed * Time.deltaTime;
        Vector3 offset = new Vector3(0f, _lookDirection.x, 0f) * scaledLookSpeed;
        Vector3 cameraOffset = new Vector3(-_lookDirection.y, 0f, 0f) * scaledLookSpeed;

        _cameraPoint.transform.Rotate(cameraOffset);
        transform.Rotate(offset);
    }

    private void Move()
    {
        if (_moveDirection.sqrMagnitude < 0.1f)
        {
            _animator.SetFloat(SpeedHash, 0f, 0.1f, Time.deltaTime);
            return;
        }

        float multiplier = _isSprinting == 1f ? 1.5f : 1f;
        float scaledMoveSpeed = _moveSpeed * Time.deltaTime * multiplier;
        Vector3 offset = new Vector3(_moveDirection.x, 0f, _moveDirection.y) * scaledMoveSpeed;
        
        transform.Translate(offset);

        if (_isSprinting == 1f)
            _animator.SetFloat(SpeedHash, 1f, 0.1f, Time.deltaTime);
        else
            _animator.SetFloat(SpeedHash, 0.5f, 0.1f, Time.deltaTime);
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if (_isGrounded)
        {
            Vector3 velocity = _rigidbody.linearVelocity;
            velocity.y = 0f;
            _rigidbody.linearVelocity = velocity;

            _rigidbody.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
            _animator.SetTrigger(JumpHash);
        }
    }

    private void OnLook(InputAction.CallbackContext context)
    {
        _lookDirection = context.ReadValue<Vector2>();
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        _moveDirection = context.ReadValue<Vector2>();
    }

    public PlayerInput GetPlayerInput()
    {
        return _playerInput;
    }
}

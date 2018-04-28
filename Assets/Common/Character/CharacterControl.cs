using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace APlusOrFail.Character
{
    using Maps;

    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D), typeof(CharacterPlayer), typeof(CharacterSprite))]
    public class CharacterControl : PropertyFieldBehavior
    {
        private static readonly int animatorInAirHash = Animator.StringToHash("inAir");
        private static readonly int animatorSquatHash = Animator.StringToHash("squat");
        private static readonly int animatorSlidingHash = Animator.StringToHash("sliding");
        private static readonly int animatorFaceRightHash = Animator.StringToHash("faceRight");
        private static readonly int animatorJumpOffsetXHash = Animator.StringToHash("jumpOffsetX");
        private static readonly int animatorJumpOffsetYHash = Animator.StringToHash("jumpOffsetY");
        private static readonly int animatorJumpHash = Animator.StringToHash("jump");
        private static readonly int animatorCurrentWheelSpeedHash = Animator.StringToHash("currentWheelSpeed");
        private static readonly int animatorGravitationalVelocityHash = Animator.StringToHash("gravitationalVelocity");


        public float maxSpeed = 1.5f;
        public float squatMaxSpeed = 2f;
        public int initialHealth;
        
        [EditorPropertyField] public int health { get; private set; }
        [EditorPropertyField] public bool won { get; private set; }
        [EditorPropertyField] public GameObject wonCause { get; private set; }

        private bool _ended;
        [EditorPropertyField] public bool ended
        {
            get
            {
                return _ended;
            }
            private set
            {
                if (_ended != value)
                {
                    _ended = value;
                    onEndedChanged?.Invoke(this, value);
                }
            }
        }
        public event EventHandler<CharacterControl, bool> onEndedChanged;

        public readonly ReadOnlyCollection<IPlayerHealthChange> healthChanges;
        private readonly List<IPlayerHealthChange> _healthChanges = new List<IPlayerHealthChange>();

        public readonly ReadOnlyCollection<IPlayerScoreChange> scoreChanges;
        private readonly List<IPlayerScoreChange> _scoreChanges = new List<IPlayerScoreChange>();
        

        private new Rigidbody2D rigidbody2D;
        private CharacterPlayer charPlayer;
        private CharacterSprite charSprite;
        private Animator charAnimator;

        public HingeJoint2D wheelJoint;
        public CircleCollider2D wheelCollider;

        private Animator _spriteAnimator;
        private Animator spriteAnimator
        {
            get
            {
                return _spriteAnimator;
            }
            set
            {
                if (_spriteAnimator != value)
                {
                    _spriteAnimator = value;
                    if (value != null)
                    {
                        value.SetBool(animatorFaceRightHash, faceRight);
                        value.SetBool(animatorInAirHash, inAir);
                        value.SetBool(animatorSquatHash, squat);
                        value.SetBool(animatorSlidingHash, sliding);
                        value.SetFloat(animatorCurrentWheelSpeedHash, targetWheelVelocity);
                        value.SetFloat(animatorGravitationalVelocityHash, gravitionalVelocity);
                    }
                }
            }
        }


        public CharacterControl()
        {
            healthChanges = new ReadOnlyCollection<IPlayerHealthChange>(_healthChanges);
            scoreChanges = new ReadOnlyCollection<IPlayerScoreChange>(_scoreChanges);
        }


        private void Start()
        {
            health = initialHealth;

            rigidbody2D = GetComponent<Rigidbody2D>();
            charPlayer = GetComponent<CharacterPlayer>();
            charSprite = GetComponent<CharacterSprite>();
            charAnimator = GetComponent<Animator>();

            spriteAnimator = charSprite.attachedSprite?.GetComponent<Animator>();
            charSprite.onAttachedSpriteChanged += OnAttachedSpriteChanged;

            UpdateEnded();
        }

        private void OnDestroy()
        {
            charSprite.onAttachedSpriteChanged -= OnAttachedSpriteChanged;
        }

        private void OnAttachedSpriteChanged(CharacterSprite charSprite, GameObject attachedSprite)
        {
            spriteAnimator = attachedSprite?.GetComponent<Animator>();
        }


        private ContactPoint2D[] contactPoints = new ContactPoint2D[4];
        private void FixedUpdate()
        {
            bool inAir = false;
            bool squat = false;
            bool sliding = false;
            bool faceRight = this.faceRight;
            float targetWheelVelocity = 0;

            bool leftInput = HasKeyPressed(Player.Action.Left);
            bool rightInput = HasKeyPressed(Player.Action.Right);
            bool jumpInput = HasKeyPressed(Player.Action.Up);
            bool squatInput = HasKeyPressed(Player.Action.Down);

            bool leftAction = leftInput && !rightInput;
            bool rightAction = !leftInput && rightInput;
            bool jumpAction = jumpInput && !squatInput;
            bool squatAction = !jumpAction && squatInput;

            int count;
            while ((count = wheelCollider.GetContacts(contactPoints)) == contactPoints.Length)
            {
                contactPoints = new ContactPoint2D[contactPoints.Length * 2];
            }

            inAir = count == 0;

            if (inAir)
            {
                if (health == 0)
                {

                }
                else if (won)
                {

                }
                else
                {
                    if (leftAction)
                    {
                        faceRight = false;
                    }
                    else if (rightAction)
                    {
                        faceRight = true;
                    }

                    if (leftAction || rightAction)
                    {
                        Vector2 flappingForce = Vector2.right * rigidbody2D.mass * Physics2D.gravity.magnitude * 0.33f * (faceRight ? 1 : -1);
                        rigidbody2D.AddForce(flappingForce);
                    }
                }
                
            }
            else // !inAir
            {
                if (health == 0)
                {

                }
                else if (won)
                {

                }
                else
                {
                    ContactPoint2D floorCp = contactPoints.Take(count)
                    .Aggregate((sld, p) => Mathf.Abs(p.point.x - wheelCollider.bounds.center.x) < Mathf.Abs(sld.point.x - wheelCollider.bounds.center.x) ? p : sld);

                    if (jumping)
                    {
                        if (preJumping)
                        {
                            ApplyPreJumpingForce(floorCp);
                        }
                    }
                    else
                    {
                        Vector2 floorTangent = floorCp.normal.Rotate90DegCW();
                        float floorAngle = Mathf.Atan2(floorTangent.y, Mathf.Abs(floorTangent.x));

                        sliding = Mathf.Abs(floorAngle) > Mathf.PI / 3;

                        if (sliding)
                        {
                            faceRight = floorAngle > 0;

                            if (jumpAction)
                            {
                                if (leftAction && floorAngle < 0 || rightAction && floorAngle > 0)
                                {
                                    Jump(floorCp, -Mathf.PI / 2);
                                }
                                else if (leftAction && floorAngle > 0)
                                {
                                    Jump(floorCp, -Mathf.PI / 4);
                                }
                                else if (rightAction && floorAngle < 0)
                                {
                                    Jump(floorCp, -Mathf.PI * 3 / 4);
                                }
                            }
                            else // sliding && && !jumpAction
                            {
                                targetWheelVelocity = maxSpeed * 0.5f * (rightAction ? 1 : (leftAction ? -1 : 0));
                                Vector2 force = -floorCp.normal * Vector3.Cross(-floorCp.normal * rigidbody2D.mass * Physics2D.gravity.magnitude, Physics2D.gravity.normalized).magnitude;
                                force *= leftAction && floorAngle < 0 || rightAction && floorAngle > 0 ? 3 : 1;
                                rigidbody2D.AddForce(force);
                            }
                        }
                        else // !sliding
                        {
                            squat = squatAction;

                            if (leftAction)
                            {
                                faceRight = false;
                            }
                            else if (rightAction)
                            {
                                faceRight = true;
                            }

                            if (leftAction || rightAction)
                            {
                                targetWheelVelocity = (squat ? squatMaxSpeed : maxSpeed) * (rightAction ? 1 : -1);
                            }

                            if (jumpAction)
                            {
                                if (leftAction)
                                {
                                    Jump(floorCp, -Mathf.PI * 5 / 12);
                                }
                                else if (rightAction)
                                {
                                    Jump(floorCp, -Mathf.PI * 7 / 12);
                                }
                                else
                                {
                                    Jump(floorCp, -Mathf.PI / 2);
                                }
                            }
                        }
                    }
                }
            }

            this.inAir = inAir;
            this.squat = squat;
            this.sliding = sliding;
            this.faceRight = faceRight;
            this.targetWheelVelocity = targetWheelVelocity;
            currentWheelSpeed = Mathf.Abs(wheelJoint.jointSpeed) * Mathf.Deg2Rad * wheelCollider.radius;
            gravitionalVelocity = Vector2.Dot(rigidbody2D.velocity, -Physics2D.gravity.normalized);
        }

        private bool HasKeyPressed(Player.Action action)
        {
            KeyCode? code = charPlayer.player?.GetKeyForAction(action);
            return code != null && Input.GetKey(code.Value);
        }


        public int ChangeHealth(IPlayerHealthChange healthChange)
        {
            if (!won)
            {
                int newHealth = Mathf.Max(health + healthChange.healthDelta, 0);
                if (newHealth != health)
                {
                    int delta = newHealth - health;
                    _healthChanges.Add(new PlayerHealthChange(healthChange.reason, delta, healthChange.cause));
                    health = newHealth;
                    UpdateEnded();
                    return delta;
                }
            }
            return 0;
        }

        public void ChangeScore(IPlayerScoreChange scoreChange)
        {
            _scoreChanges.Add(scoreChange);
        }

        public void Win(GameObject cause)
        {
            won = true;
            wonCause = cause;
            UpdateEnded();
        }

        private void UpdateEnded()
        {
            ended = health == 0 || won;
        }


        private bool _inAir;
        [EditorPropertyField(forceGet = true)]
        private bool inAir
        {
            get
            {
                return _inAir;
            }
            set
            {
                if (_inAir != value)
                {
                    _inAir = value;
                    spriteAnimator?.SetBool(animatorInAirHash, value);
                }
            }
        }

        private bool _squat;
        [EditorPropertyField(forceGet = true)]
        private bool squat
        {
            get
            {
                return _squat;
            }
            set
            {
                if (_squat != value)
                {
                    _squat = value;
                    charAnimator.SetBool(animatorSquatHash, value);
                    spriteAnimator?.SetBool(animatorSquatHash, value);
                }
            }
        }

        private bool _sliding;
        [EditorPropertyField(forceGet = true)]
        private bool sliding
        {
            get
            {
                return _sliding;
            }
            set
            {
                if (_sliding != value)
                {
                    _sliding = value;
                    spriteAnimator?.SetBool(animatorSlidingHash, value);
                }
            }
        }

        private bool _faceRight;
        [EditorPropertyField(forceGet = true)]
        private bool faceRight
        {
            get
            {
                return _faceRight;
            }
            set
            {
                if (_faceRight != value)
                {
                    _faceRight = value;
                    charAnimator.SetBool(animatorFaceRightHash, value);
                    spriteAnimator?.SetBool(animatorFaceRightHash, value);
                }
            }
        }

        [EditorPropertyField(forceGet = true)]
        private bool jumping
        {
            get { return preJumping || postJumping; }
        }

        private bool preJumping;
        private bool postJumping;

        private float _jumpingAngle = float.NaN;
        [EditorPropertyField(forceGet = true)]
        private float jumpingAngle
        {
            get
            {
                return _jumpingAngle;
            }
            set
            {
                if (_jumpingAngle != value)
                {
                    _jumpingAngle = value;
                    Vector2 offset = Quaternion.Euler(0, 0, value * Mathf.Rad2Deg) * Vector2.right;
                    charAnimator.SetFloat(animatorJumpOffsetXHash, offset.x);
                    charAnimator.SetFloat(animatorJumpOffsetYHash, offset.y);
                }
            }
        }

        private void Jump(ContactPoint2D cp, float angle)
        {
            if (!jumping)
            {
                preJumping = true;
                postJumping = false;
                ApplyPreJumpingForce(cp);
                jumpingAngle = angle;
                charAnimator.SetTrigger(animatorJumpHash);
            }
        }

        private void ApplyPreJumpingForce(ContactPoint2D cp)
        {
            if (preJumping)
            {
                Vector2 force = -cp.normal * Vector3.Cross(-cp.normal * rigidbody2D.mass * Physics2D.gravity.magnitude * 50, Physics2D.gravity.normalized).magnitude;
                rigidbody2D.AddForce(force);
            }
        }

        private void OnPreJumpAnimationEnd()
        {
            preJumping = false;
            postJumping = true;
        }

        private void OnPostJumpAnimationEnd()
        {
            preJumping = postJumping = false;
        }

        private float _targetWheelVelocity;
        [EditorPropertyField(forceGet = true)]
        private float targetWheelVelocity
        {
            get
            {
                return _targetWheelVelocity;
            }
            set
            {
                if (_targetWheelVelocity != value)
                {
                    _targetWheelVelocity = value;
                    JointMotor2D motor = wheelJoint.motor;
                    motor.motorSpeed = value / wheelCollider.radius * Mathf.Rad2Deg;
                    wheelJoint.motor = motor;
                }
            }
        }

        private float _currentWheelSpeed;
        [EditorPropertyField(forceGet = true)]
        private float currentWheelSpeed
        {
            get
            {
                return _currentWheelSpeed;
            }
            set
            {
                if (_currentWheelSpeed != value)
                {
                    _currentWheelSpeed = value;
                    spriteAnimator?.SetFloat(animatorCurrentWheelSpeedHash, value);
                }
            }
        }

        private float _gravitationalVelocity;
        [EditorPropertyField(forceGet = true)]
        private float gravitionalVelocity
        {
            get
            {
                return _gravitationalVelocity;
            }
            set
            {
                if (_gravitationalVelocity != value)
                {
                    _gravitationalVelocity = value;
                    spriteAnimator?.SetFloat(animatorGravitationalVelocityHash, value);
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using Navigation;
using Navigation.Algorithm;
using UnityEngine;
using static CelluloEnums.VisualEffect;
using static Globals;

public class CelluloEntity : MonoBehaviour
{
    public const float GoalPosThreshold = 10;
    public const float PathFindingThreshold = 50;

    //------------------------------------------------------------------------
    // Debug
    public bool logPathing = false;

    //------------------------------------------------------------------------
    // Initializable fields
    private bool _initialized;

    [NonSerialized] public bool IsCelluloVirtual;
    public float moveSpeed;
    public Color defaultColor;
    private Action<Collider2D> _onTriggerEnter2D;
    private Action<Collision2D> _onCollisionEnter2D;
    private Action<Collision2D> _onCollisionExit2D;

    [NonSerialized] public Cellulo Cellulo;

    private Transform _transform;
    private Rigidbody2D _rigidbody2D;

    //------------------------------------------------------------------------

    private Vector2 _realCelluloPosition;
    private Vector2 _lastInput;

    private DisallowMovementDirs _disallowMovementDirs = DisallowMovementDirs.None;

    private enum MovementMode
    {
        None,
        Velocity,
        Position,
        Path,
        Rails
    }

    private MovementMode _movementMode = MovementMode.None;
    private Vector2 _currentGoalPosition;

    //========================================================================

    /// <summary>
    /// Replacement for constructor must call at creation.
    /// </summary>
    /// <param name="isCelluloVirtual"></param>
    /// <param name="moveSpeed"> Either cellulo moveSpeed or rigidBody movespeed </param>
    /// <param name="onTriggerEnter2D"> Used to allow the controller to add extra
    /// behaviour upon colliding with a collider. For example: adding logic for
    /// collecting collectibles. </param>
    /// <param name="onTriggerExit2D"></param>
    /// <param name="onCollisionEnter2D"></param>
    /// <param name="onCollisionExit2D"></param>
    /// <exception cref="InvalidOperationException"></exception>
    public void Initialize(
        bool isCelluloVirtual,
        float moveSpeed,
        Color defaultColor,
        Action<Collider2D> onTriggerEnter2D = null,
        Action<Collider2D> onTriggerExit2D = null,
        Action<Collision2D> onCollisionEnter2D = null,
        Action<Collision2D> onCollisionExit2D = null)
    {
        if (!_initialized)
            _initialized = true;
        else
            throw new InvalidOperationException("Can only call initialize() once!");

        IsCelluloVirtual = isCelluloVirtual;
        this.moveSpeed = moveSpeed;
        this.defaultColor = defaultColor;
        _onTriggerEnter2D = onTriggerEnter2D;
        _onCollisionEnter2D = onCollisionEnter2D;
        _onCollisionExit2D = onCollisionExit2D;

        if (IsCelluloVirtual)
        {
            _rigidbody2D = gameObject.GetComponent<Rigidbody2D>();
        }
        else
        {
            _transform = gameObject.GetComponent<Transform>();

            Cellulo = CelluloManager.GetCellulo();

            Cellulo.setPoseChangedCallback(() =>
            {
                // Debug.Log("Cellulo " + Cellulo.macAddr + " pos - x: " + Cellulo.getX() + "  y: " + Cellulo.getY());
                _realCelluloPosition = MapCoordsToGameCoords(Cellulo.getX(), - Cellulo.getY());
            });
        }
    }

    public void SetDirectionalInput(Vector2 inputAxes)
    {
        _movementMode = MovementMode.Velocity;
        _SetDirectionalInput(inputAxes);
    }

    private void _SetDirectionalInput(Vector2 inputAxes)
    {
        if (IsCelluloVirtual)
        {
            _rigidbody2D.velocity = inputAxes * moveSpeed;
        }
        else
        {
            _lastInput = inputAxes;
        }
    }

    public void SetDirectionalInputRestricted(Vector2 inputAxes)
    {
        var restrictedInputAxes = inputAxes;
        if ((_disallowMovementDirs & DisallowMovementDirs.Right) == DisallowMovementDirs.Right && inputAxes.x > 0)
            restrictedInputAxes.x = 0;

        if ((_disallowMovementDirs & DisallowMovementDirs.Up) == DisallowMovementDirs.Up && inputAxes.y > 0)
            restrictedInputAxes.y = 0;

        if ((_disallowMovementDirs & DisallowMovementDirs.Left) == DisallowMovementDirs.Left && inputAxes.x < 0)
            restrictedInputAxes.x = 0;

        if ((_disallowMovementDirs & DisallowMovementDirs.Down) == DisallowMovementDirs.Down && inputAxes.y < 0)
            restrictedInputAxes.y = 0;

        SetDirectionalInput(restrictedInputAxes);
    }

    public void SetGoalPosition(Vector2 position)
    {
        _movementMode = MovementMode.Position;
        _SetGoalPosition(position);
    }

    private void _SetGoalPosition(Vector2 position)
    {
        _currentGoalPosition = position;

        if (!IsCelluloVirtual)
        {
            Cellulo.setGoalPosition(position[0], -position[1], moveSpeed);
        }
    }

    //------------------------------------------------------------------------
    // Path Navigation

    private List<Vector2> _path;

    public Vector2 Position
    {
        get
        {
            if (IsCelluloVirtual)
            {
                return gameObject.GetComponent<Transform>().position;
                // return _rigidbody2D.position;
            }

            return _realCelluloPosition;
        }
    }

    public float DistToGoalPos => (_currentGoalPosition - Position).magnitude;
    public Vector2 DirToGoalPos => (_currentGoalPosition - Position).normalized;

    public bool GoalPosReached => DistToGoalPos < GoalPosThreshold;

    public void SetGoalPath(List<Vector2> path)
    {
        if (logPathing)
            Debug.Log("--- Started pathing! ---");

        _movementMode = MovementMode.Path;
        _path = path;

        GotoNextNodeInPath();
    }

    private void GotoNextNodeInPath()
    {
        if (_path.Count == 0)
        {
            if (logPathing)
                Debug.Log("--- Reached End of Path! ---");

            _SetDirectionalInput(Vector2.zero);
            _movementMode = MovementMode.None;
        }
        else
        {
            var nextNode = _path[0];
            _path.RemoveAt(0);
            _SetGoalPosition(nextNode);
        }
    }

    //------------------------------------------------------------------------
    // Lighting

    private static (long r, long g, long b) ColorToCelluloRGB(Color color)
    {
        var r = (long) (color.r * 255f);
        var g = (long) (color.g * 255f);
        var b = (long) (color.b * 255f);

        return (r,g,b);
    }

    public void SetVisualEffect(CelluloEnums.VisualEffect visualEffect, Color color, long value = 0)
    {
        if (IsCelluloVirtual) return;
        var (r, g, b) = ColorToCelluloRGB(color);
        Cellulo.setVisualEffect((long) visualEffect, r, g, b, value);
    }

    public void LightsAlert()
    {
        SetVisualEffect(VisualEffectAlertAll, Color.red, 0);
    }

    public void LightsSingle(int index)
    {
        // if ()

        SetVisualEffect(VisualEffectConstSingle, defaultColor, index);
    }

    // public void LightShowScore(int score)
    // {
    //     LightSetVisualEffect(VisualEffectProgress, defaultColor, (long) (255.0f * score/ 6));
    // }

    public void LightsOff()
    {
        SetVisualEffect(VisualEffectConstAll, Color.black, 0);
    }

    public void LightsWaiting()
    {
        SetVisualEffect(VisualEffectWaiting, defaultColor, 0);
    }

    public void LightsDefault()
    {
        SetVisualEffect(VisualEffectConstAll, defaultColor, 0);
    }

    //========================================================================
    // Event Functions

    void Start()
    {
        if (!_initialized)
            throw new InvalidOperationException("Must call initialize() upon object creation");
    }

    private int _frame = 0; // Frame rate is 60fps
    private const int CelluloSendFrequency = 2; // Every X frames
    private void Update()
    {
        if (IsCelluloVirtual)
        {
            if (_movementMode == MovementMode.Position || _movementMode == MovementMode.Path)
            {
                _SetDirectionalInput(DistToGoalPos < 2 ? Vector2.zero : DirToGoalPos);
            }

        } else {
            _transform.position = _realCelluloPosition;

            if (_movementMode == MovementMode.Velocity)
            {
                //----------------------------------------------------------------
                // Periodically send new velocity to cellulo to not overload it!
                if (_frame % CelluloSendFrequency == 0)
                {
                    var velocity = _lastInput * moveSpeed;

                    // Note: In Cellulo Map y axis points downwards. In Unity it points up.
                    //       Thus we must flip the y axis.
                    Cellulo.setGoalVelocity(velocity[0], - velocity[1], 0);
                }
                //----------------------------------------------------------------
            }
        }

        if (_frame++ >= 60)
        {
            _frame = 0;
        }
    }

    private void FixedUpdate()
    {
        if (_movementMode == MovementMode.Path)
        {
            // If path is empty (has been fully covered) stop pathing.
            if (DistToGoalPos < PathFindingThreshold)
            {
                if (logPathing)
                    Debug.Log("Reached node : " + _currentGoalPosition);

                GotoNextNodeInPath();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_onTriggerEnter2D != null)
        {
            _onTriggerEnter2D(other);
        }

        Wall wall = other.GetComponent<Wall>();
        if (wall != null)
        {
            _disallowMovementDirs |= wall.disallowMovementDirs;

            if (_movementMode == MovementMode.Velocity)
            {
                SetDirectionalInput(Vector2.zero);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Wall wall = other.GetComponent<Wall>();
        if (wall != null && (_disallowMovementDirs & wall.disallowMovementDirs) == wall.disallowMovementDirs)
        {
            _disallowMovementDirs -= wall.disallowMovementDirs;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (_onCollisionEnter2D != null)
        {
            _onCollisionEnter2D(collision);
        }

        foreach (ContactPoint2D contact in collision.contacts)
        {
            Debug.DrawRay(contact.point, contact.normal, Color.magenta);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (_onCollisionExit2D != null)
        {
            _onCollisionExit2D(collision);
        }
    }
}

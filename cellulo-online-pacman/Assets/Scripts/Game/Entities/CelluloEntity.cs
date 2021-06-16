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
    ///
    /// Note: The constructor has several optional arguments of type Action which are basically
    /// methods to be called on a certain Unity trigger. For example: OnTriggerEnter2D
    /// is called when the CelluloEntity Collider2D collides with another Collider2D.
    /// See Unity documentation about Collider2D and RigidBody2D for more info.
    /// </summary>
    /// <param name="isCelluloVirtual"> If false the Cellulo will try to connect to a real
    /// Cellulo robot using Cellulo Manager. Otherwise, the CelluloEntity is just
    /// simulated within Unity.</param>
    /// <param name="moveSpeed"> Either cellulo moveSpeed or rigidBody movespeed </param>
    /// <param name="defaultColor"> Default Cellulo Lighting Color. Irrelevant for if
    /// isCelluloVirtual is true. Cellulo LEDs are set to this color when calling LightsDefault.</param>
    /// <param name="onTriggerEnter2D"> Called when OnTriggerEnter2D is called (by Unity).
    /// For example in the Pac-Man game it is used for running the logic for collecting
    /// collectibles (apples). </param>
    /// <param name="onTriggerExit2D"> Called when no longer in a collision. </param>
    /// <param name="onCollisionEnter2D"> Called when entering a collision with a RigidBody2D. </param>
    /// <param name="onCollisionExit2D"> Called when exiting a collision with a RigidBody2D. </param>
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
                // Note: In Cellulo Map y axis points downwards. In Unity it points up.
                //       Thus we must flip the y axis.

                _realCelluloPosition = MapCoordsToGameCoords(Cellulo.getX(), - Cellulo.getY());
                // Debug.Log("Cellulo " + Cellulo.macAddr + " pos - x: " + Cellulo.getX() + "  y: " + Cellulo.getY());
            });
        }
    }

    /// <summary>
    /// Set the velocity of the Cellulo to input * moveSpeed
    /// </summary>
    /// <param name="inputAxes"> 2D vector encoding the directional input to Cellulo. </param>
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

    /// <summary>
    /// Sets the velocity of the Cellulo according to input while also
    /// restricting movement according to if movement towards some direction
    /// is disabled.
    ///
    /// For instance, this is used to make sure robots cannot go through walls.
    /// </summary>
    /// <param name="inputAxes"> 2D vector encoding the directional input to Cellulo. </param>
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

    /// <summary>
    /// Tells the CelluloEntity to move towards some position on the map.
    ///
    /// Side-note: One can check if the position has been reached by reading GoalPosReached.
    /// </summary>
    /// <param name="position"></param>
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

    /// <summary>
    /// Read-only property. Returns the position of this CelluloEntity.
    /// </summary>
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

    /// <summary>
    /// Distance to Goal Position.
    /// </summary>
    public float DistToGoalPos => (_currentGoalPosition - Position).magnitude;

    /// <summary>
    /// Normalized Direction towards Goal Position.
    /// </summary>
    public Vector2 DirToGoalPos => (_currentGoalPosition - Position).normalized;

    /// <summary>
    /// True if the Goal position is closer than GoalPosThreshold.
    /// </summary>
    public bool GoalPosReached => DistToGoalPos < GoalPosThreshold;

    /// <summary>
    /// Sets the goal path for the Cellulo to follow.
    /// </summary>
    /// <param name="path"> A list of vectors defining each position in the
    /// target goal of the robot. The each node in the path is traversed in-order.</param>
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

    /// <summary>
    /// Converts a Unity Color into r, g, b longs used by Cellulo API.
    /// </summary>
    /// <param name="color"></param>
    /// <returns></returns>
    private static (long r, long g, long b) ColorToCelluloRGB(Color color)
    {
        var r = (long) (color.r * 255f);
        var g = (long) (color.g * 255f);
        var b = (long) (color.b * 255f);

        return (r,g,b);
    }

    /// <summary>
    /// Same ase Cellulo.setVisualEffect but using VisualEffect Enum and Unity Color
    /// </summary>
    /// <param name="visualEffect"></param>
    /// <param name="color"></param>
    /// <param name="value">Depends on visualEffect. See CelluloEnums for info.</param>
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
        SetVisualEffect(VisualEffectConstSingle, defaultColor, index);
    }

    // public void LightShowScore(int score)
    // {
    //     LightSetVisualEffect(VisualEffectProgress, defaultColor, (long) (255.0f * score/ 6));
    // }

    /// <summary>
    /// Turns all Cellulo lights off.
    /// </summary>
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
            // Since Unity doesn't have a SetGoalPosition. This is part of the implementation
            // that simulates that behaviour. Basically moves the RigidBody2D towards the goal
            // position until it is reached.
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
            // Otherwise go to the next node in the path.
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
            // Calls the "custom" onTrigger2D passed in Initialize.
            _onTriggerEnter2D(other);
        }

        // If in contact with a wall restrict movement towards the direction
        // that the wall restricts movement.
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
        // When no longer in contact with a wall allow movement towards
        // restricted direction again.
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
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (_onCollisionExit2D != null)
        {
            _onCollisionExit2D(collision);
        }
    }
}

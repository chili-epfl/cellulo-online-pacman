using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Script to be attached to each map. The public fields need to be bound to
/// some element in the game through Unity. These are then used by the Controllers
/// for various game logic.
/// </summary>
public class GameMap : MonoBehaviour
{
    // Position the camera should be centered
    public Transform cameraCenter;

    // Cellulo spawns. Unused for now.
    public GameObject spawns;

    // GameObject that should be the parent of all NavNodes used for navigation.
    public GameObject navNodesParent;
}

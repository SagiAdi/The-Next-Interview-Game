using UnityEngine;

// Base class for things the player can use (puzzle station, contact, job board).
// To add a new kind of thing: write one new subclass. The player and the HUD do not change.
[RequireComponent(typeof(Collider2D))]
public abstract class Interactable : MonoBehaviour
{
    // Short text for the on-screen hint, for example "Talk".
    public abstract string Prompt { get; }

    public abstract void Interact();
}

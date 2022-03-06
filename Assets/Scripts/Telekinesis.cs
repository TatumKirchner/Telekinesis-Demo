using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

public class Telekinesis : MonoBehaviour
{
    [Tooltip("The layer pickup objects are on.")]
    [SerializeField] LayerMask pickupLayer;
    [Tooltip("Pickup radius")]
    [SerializeField] float overlapSphereRadius;
    [Tooltip("The hold positions for picked up objects. Make sure there's enough for max pickup.")]
    [SerializeField] Transform[] holdPositions;
    [Tooltip("How fast the object moves towards the player when picked up.")]
    [SerializeField] float pickupObjectMovementSpeed;
    [Tooltip("The movement speed of picked up objects when being held.")]
    [SerializeField] float stationarySpeed;
    [Tooltip("The rotation to be applied when an object is held.")]
    [SerializeField] Vector3 StationaryRotation;
    [Tooltip("How quickly the held object moves in a circle when being held.")]
    [SerializeField] float movementFrequency;
    [Tooltip("How big of a circle the held object moves.")]
    [SerializeField] float movementAmplitude;
    [Tooltip("The max amount of objects the player can pickup at once. Make sure there are enough hold positions.")]
    [SerializeField] int maxPickup;
    [Tooltip("How far the player needs to be from the picked up object before it is dropped.")]
    [SerializeField] float maxHoldDistance;
    [Tooltip("How much force is added to the object when being fired.")]
    [SerializeField] float throwForce;

    Animator animator;
    int animIdPickup;
    int animIdThrow;
    int holdingObjectLayerId;
    int pickupObjectLayerId;
    PlayerInput playerInput;
    private float movementSpeed;
    Camera mainCamera;
    List<Rigidbody> pickedUpObjects;
    bool isHolding = false;
    bool throwOne = false;

    private void Awake()
    {
        playerInput = new PlayerInput();
        playerInput.Player.Enable();
        playerInput.Player.Fire.performed += Fire;
        playerInput.Player.Pickup.performed += Pickup;
        playerInput.Player.Pickup.canceled += PickupCancel;
    }

    private void Start()
    {
        mainCamera = Camera.main;
        pickedUpObjects = new List<Rigidbody>();
        animator = GetComponent<Animator>();
        animIdPickup = Animator.StringToHash("Pickup");
        animIdThrow = Animator.StringToHash("Throw");
        holdingObjectLayerId = LayerMask.NameToLayer("HoldingObject");
        pickupObjectLayerId = LayerMask.NameToLayer("Pickup");
    }

    // When the pickup button is pressed and a pickup object is within range, add it to the list.
    void Pickup(InputAction.CallbackContext context)
    {
        if (pickedUpObjects.Count <= maxPickup)
        {
            GameObject tmp = GetPickupObject();
            if (tmp != null)
            {
                if (tmp.TryGetComponent(out Rigidbody rb))
                {
                    if (pickedUpObjects.Count < maxPickup)
                    {
                        animator.SetLayerWeight(1, 1);
                        animator.SetBool(animIdPickup, true);
                        pickedUpObjects.Add(rb);
                    }
                }
            }
        }
    }

    void PickupCancel(InputAction.CallbackContext contex)
    {
        animator.SetBool(animIdPickup, false);
    }

    // Check around the player for a pickup object. If one is found return it, if not return null.
    private GameObject GetPickupObject()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, overlapSphereRadius, pickupLayer);
        if (colliders.Length > 0)
        {
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] != null)
                {
                    isHolding = true;
                    return colliders[i].gameObject;
                }
            }
            return null;
        }
        else
        {
            return null;
        }
    }

    private void FixedUpdate()
    {
        HoldObjects();
    }

    void HoldObjects()
    {
        // If our list is under the max amount, but still contains something run this.
        if (pickedUpObjects.Count <= maxPickup && pickedUpObjects.Count > 0 && isHolding)
        {
            // Create circular movement for the held objects.
            float x = Mathf.Cos(Time.time * movementFrequency) * movementAmplitude;
            float y = Mathf.Sin(Time.time * movementFrequency) * movementAmplitude;
            float z = Mathf.Cos(Time.time * movementFrequency) * movementAmplitude;

            for (int i = 0; i < pickedUpObjects.Count; i++)
            {
                if (pickedUpObjects[i] != null)
                {
                    // When the object is close to the hold position set it up to be held.
                    if (Vector3.Distance(pickedUpObjects[i].position, holdPositions[i].position) <= 1f)
                    {
                        // Freeze Y rotation makes the rotation more interesting.
                        pickedUpObjects[i].constraints = RigidbodyConstraints.FreezeRotationY;

                        // Increase the drag so when they are held they don't fly around.
                        pickedUpObjects[i].drag = 5f;
                        pickedUpObjects[i].angularDrag = 5f;

                        // Change the speed so they follow along more smoothly.
                        movementSpeed = stationarySpeed;

                        // Get the direction the objects need to move to and apply it.
                        Vector3 pos = (holdPositions[i].position + new Vector3(x, y, z) - pickedUpObjects[i].position).normalized * movementSpeed;
                        pickedUpObjects[i].AddForce(pos);

                        // Add rotation to the objects to add visual interest.
                        pickedUpObjects[i].AddTorque(StationaryRotation);
                    }
                    else
                    {
                        // Change the layer so they are not picked up by the overlap sphere.
                        pickedUpObjects[i].gameObject.layer = holdingObjectLayerId;

                        // Turn gravity off and add some drag so the objects fly to the hold position smoothly.
                        pickedUpObjects[i].useGravity = false;
                        pickedUpObjects[i].drag = 5f;
                        pickedUpObjects[i].angularDrag = 5f;

                        // Change the speed so they make it to their position quickly.
                        movementSpeed = pickupObjectMovementSpeed;

                        // Create their direction and apply it to their rigidbody's.
                        Vector3 dir = (holdPositions[i].position - pickedUpObjects[i].position).normalized * movementSpeed;
                        pickedUpObjects[i].AddForce(dir);
                    }

                    // If the objects get to far away from the player, drop them.
                    if (Vector3.Distance(pickedUpObjects[i].position, holdPositions[i].position) >= maxHoldDistance)
                    {
                        // Change their layer back to pickup so they can be picked up again.
                        pickedUpObjects[i].gameObject.layer = pickupObjectLayerId;

                        // Remove the constraints and resume simulating physics.
                        pickedUpObjects[i].constraints = RigidbodyConstraints.None;
                        pickedUpObjects[i].isKinematic = false;
                        pickedUpObjects[i].useGravity = true;

                        // Remove it from the list.
                        pickedUpObjects.RemoveAt(i);
                    }
                }
            }
        }
    }

    IEnumerator Throw()
    {
        yield return new WaitForSeconds(0.70f);
        if (throwOne)
        {
            // Fire just one object
            if (pickedUpObjects.Count > 0)
            {
                throwOne = false;
                // Create a temporary reference to the object to be fired.
                // That way we can remove it from the list so it is not being held anymore.
                Rigidbody rb = pickedUpObjects[0];
                pickedUpObjects.RemoveAt(0);

                // Set its layer back to Pickup so it can be picked up again.
                rb.gameObject.layer = pickupObjectLayerId;

                // Remove its constraints and start simulating physics again.
                rb.constraints = RigidbodyConstraints.None;
                rb.isKinematic = false;
                rb.useGravity = true;

                // Set its drag so it flies more realistically.
                rb.drag = 0.1f;
                rb.angularDrag = 0.01f;

                // Add the throw force to the object in the direction the camera is facing.
                rb.AddForce(mainCamera.transform.forward * throwForce, ForceMode.VelocityChange);

                // If it was the last object being held, reset the isHolding flag.
                if (pickedUpObjects.Count <= 0)
                    isHolding = false;
            }
        }
        else
        {
            throwOne = false;
            // Fire all held objects.
            if (pickedUpObjects.Count > 0)
            {
                // Reset the holding flag. That way the objects are no longer being held.
                isHolding = false;

                for (int i = 0; i < pickedUpObjects.Count; i++)
                {
                    // Reset the layer back to Pickup so the objects can be picked up again.
                    pickedUpObjects[i].gameObject.layer = pickupObjectLayerId;

                    // Remove the constraints and start simulating physics again.
                    pickedUpObjects[i].constraints = RigidbodyConstraints.None;
                    pickedUpObjects[i].isKinematic = false;
                    pickedUpObjects[i].useGravity = true;

                    // Set the drag so they fly more realistically
                    pickedUpObjects[i].drag = 0.1f;
                    pickedUpObjects[i].angularDrag = 0.01f;

                    // Add throw force to the objects in the direction the camera is facing.
                    pickedUpObjects[i].AddForce(mainCamera.transform.forward * throwForce, ForceMode.VelocityChange);
                }
                // Clear the list so we can repopulate it.
                pickedUpObjects.Clear();
            }
        }

        animator.SetBool(animIdThrow, false);
    }

    public void SetAnimatorWeight(int weight)
    {
        animator.SetLayerWeight(1, weight);
    }

    // Fire picked up objects depending on input.
    void Fire(InputAction.CallbackContext context)
    {
        if (pickedUpObjects.Count > 0)
        {
            if (context.interaction is PressInteraction)
            {
                throwOne = true;
            }
            else if (context.interaction is HoldInteraction)
            {
                throwOne = false;
            }
            animator.SetLayerWeight(1, 1);
            animator.SetBool(animIdThrow, true);
            StartCoroutine(Throw());
        }
    }

    bool IsAnimationPlaying(Animator anim, int layer, int state)
    {
        
        if (anim.GetCurrentAnimatorStateInfo(layer).Equals(state) && anim.GetCurrentAnimatorStateInfo(layer).normalizedTime < 1f)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, overlapSphereRadius);
    }

    private void OnDisable()
    {
        playerInput.Player.Disable();
        playerInput.Player.Fire.performed -= Fire;
        playerInput.Player.Pickup.performed -= Pickup;
        playerInput.Player.Pickup.canceled -= PickupCancel;
    }

    private void OnDestroy()
    {
        playerInput.Player.Disable();
        playerInput.Player.Fire.performed -= Fire;
        playerInput.Player.Pickup.performed -= Pickup;
        playerInput.Player.Pickup.canceled -= PickupCancel;
    }
}

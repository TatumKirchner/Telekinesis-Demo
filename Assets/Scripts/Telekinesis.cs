using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Telekinesis : MonoBehaviour
{
    [SerializeField] LayerMask pickupLayer;
    [SerializeField] float overlapSphereRadius;
    [SerializeField] Transform[] holdPositions;
    [SerializeField] float pickupObjectMovementSpeed;
    [SerializeField] float stationarySpeed;
    [SerializeField] Vector3 StationaryRotation;
    [SerializeField] float movementFrequency;
    [SerializeField] float movementAmplitude;
    [SerializeField] int maxPickup;
    private float movementSpeed;
    Camera mainCamera;
    List<Rigidbody> pickedUpObjects;
    bool isHolding = false;


    private void Start()
    {
        mainCamera = Camera.main;
        pickedUpObjects = new List<Rigidbody>();
    }

    private void Update()
    {
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (pickedUpObjects.Count <= maxPickup)
            {
                GameObject tmp = GetPickupObject();
                if (tmp != null) 
                {
                    if (tmp.TryGetComponent(out Rigidbody rb))
                    {
                        if (pickedUpObjects.Count <= maxPickup)
                        {
                            pickedUpObjects.Add(rb);
                        }
                        Debug.Log(pickedUpObjects.Count);
                        //pickupObject = rb;
                    }
                }
            }
            //else
            //{
            //    isHolding = false;
            //}
        }
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            isHolding = false;
        }
    }

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

        //if (Physics.SphereCast(raycastOrgin.position, overlapSphereRadius, transform.forward, out RaycastHit hit, maxDistance, pickupLayer))
        //{
        //    hitPosition = hit.point;
        //    Debug.Log(hit.collider.name);
        //    Collider[] colliders = Physics.OverlapSphere(hit.point, overlapSphereRadius, pickupLayer);
            
        //    for (int i = 0; i < colliders.Length; i++)
        //    {
        //        if (colliders[i] != null)
        //        {
        //            Debug.Log("Got pickup item");
        //            isHolding = true;
        //            return colliders[i].gameObject;
        //        }
        //    }
        //}
        //return null;
    }

    private void FixedUpdate()
    {
        if (pickedUpObjects.Count <= maxPickup && pickedUpObjects.Count > 0 && isHolding /*&& pickupObject != null*/)
        {
            float x = Mathf.Cos(Time.time * movementFrequency) * movementAmplitude;
            float y = Mathf.Sin(Time.time * movementFrequency) * movementAmplitude;
            float z = Mathf.Cos(Time.time * movementFrequency) * movementAmplitude;
            //holdPosition.localPosition += new Vector3(x, y, z);
            //holdPosition.localRotation *= Quaternion.Euler(1.1f, 1.1f, 1.1f);



            //if (Vector3.Distance(pickupObject.position, holdPosition.position) <= 0.1f)
            //{
            //    float x = Mathf.Cos(Time.time * movementFrequency) * movementAmplitude;
            //    float y = Mathf.Sin(Time.time * movementFrequency) * movementAmplitude;
            //    float z = Mathf.Cos(Time.time * movementFrequency) * movementAmplitude;
            //    holdPosition.localPosition += new Vector3(x, y, z);
            //    holdPosition.localRotation *= Quaternion.Euler(1.1f, 1.1f, 1.1f);
            //    pickupObject.MovePosition(holdPosition.position);
            //    pickupObject.MoveRotation(holdPosition.rotation);
            //}
            //else
            //{
            //    Vector3 moveDir = pickupObjectMovementSpeed * Time.deltaTime * (holdPosition.position - pickupObject.position).normalized;
            //    pickupObject.MovePosition(pickupObject.position + moveDir);
            //}

            for (int i = 0; i < pickedUpObjects.Count; i++)
            {
                if (pickedUpObjects[i] != null)
                {
                    if (Vector3.Distance(pickedUpObjects[i].position, holdPositions[i].position) <= 1f)
                    {
                        pickedUpObjects[i].constraints = RigidbodyConstraints.FreezeRotationY;
                        pickedUpObjects[i].drag = 5f;
                        pickedUpObjects[i].angularDrag = 5f;
                        movementSpeed = stationarySpeed;
                        Vector3 pos = (holdPositions[i].position + new Vector3(x, y, z) - pickedUpObjects[i].position).normalized * movementSpeed;
                        pickedUpObjects[i].AddForce(pos);
                        pickedUpObjects[i].AddTorque(StationaryRotation);
                    }
                    else
                    {
                        pickedUpObjects[i].gameObject.layer = LayerMask.NameToLayer("HoldingObject");
                        pickedUpObjects[i].useGravity = false;
                        pickedUpObjects[i].drag = 5f;
                        pickedUpObjects[i].angularDrag = 5f;
                        movementSpeed = pickupObjectMovementSpeed;
                        Vector3 dir = (holdPositions[i].position - pickedUpObjects[i].position).normalized * movementSpeed;
                        pickedUpObjects[i].AddForce(dir);
                    }
                }
            }


            //if (Vector3.Distance(pickupObject.position, holdPosition.position) <= 1f)
            //{
            //    pickupObject.constraints = RigidbodyConstraints.FreezeRotationY;
            //    pickupObject.drag = 5f;
            //    pickupObject.angularDrag = 5f;
            //    movementSpeed = stationarySpeed;
            //    Vector3 pos = (holdPosition.position + new Vector3(x, y, z) - pickupObject.position).normalized * movementSpeed;
            //    pickupObject.AddForce(pos);
            //    pickupObject.AddTorque(StationaryRotation);
            //}
            //else
            //{
            //    pickupObject.gameObject.layer = 0;
            //    pickupObject.useGravity = false;
            //    pickupObject.drag = 5f;
            //    pickupObject.angularDrag = 5f;
            //    movementSpeed = pickupObjectMovementSpeed;
            //    Vector3 dir = (holdPosition.position - pickupObject.position).normalized * movementSpeed;
            //    pickupObject.AddForce(dir);
            //}
        }
        else
        {
            for (int i = 0; i < pickedUpObjects.Count; i++)
            {
                pickedUpObjects[i].gameObject.layer = LayerMask.NameToLayer("Pickup");
                pickedUpObjects[i].constraints = RigidbodyConstraints.None;
                pickedUpObjects[i].isKinematic = false;
                pickedUpObjects[i].useGravity = true;
                pickedUpObjects[i].drag = 0.1f;
                pickedUpObjects[i].angularDrag = 0.01f;
                pickedUpObjects[i].AddForce(mainCamera.transform.forward * 25.0f, ForceMode.VelocityChange);
                pickedUpObjects.RemoveAt(i);
            }

            //if (pickupObject != null)
            //{
            //    pickupObject.constraints = RigidbodyConstraints.None;
            //    pickupObject.isKinematic = false;
            //    pickupObject.useGravity = true;
            //    pickupObject.drag = 0.1f;
            //    pickupObject.angularDrag = 0.01f;
            //    pickupObject.AddForce(mainCamera.transform.forward * 25.0f, ForceMode.VelocityChange);
            //    pickupObject = null;
            //}
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, overlapSphereRadius);
    }
}

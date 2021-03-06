﻿using UnityEngine;
using System.Collections.Generic;
using VRTK;
using UnityEngine.UI;

/// <summary>
/// Provides some basic functions to be used for UnityEvents via the inspector
/// </summary>
[CreateAssetMenu(menuName = "Utilities/BasicFunctions")]
public class BasicFunctions : ScriptableObject
{
    /// <summary>
    /// Toggles the active state of the given game object.
    /// </summary>
    /// <param name="gameObject">The <see cref="GameObject"/> to change</param>
    public void ToggleObject(GameObject gameObject)
    {
        if (gameObject)
        {
            gameObject.SetActive(!gameObject.activeInHierarchy);
        }
    }

    /// <summary>
    /// Destroys the given game object
    /// </summary>
    /// <param name="gameObject">The <see cref="GameObject"/> to destroy</param>
    public void DestroyObject(GameObject gameObject)
    {
        Destroy(gameObject);
    }

    /// <summary>
    /// Destroys the given game object, if it is active
    /// </summary>
    /// <param name="gameObject">The <see cref="GameObject"/> to destroy</param>
    public void DestroyObjectIfActive(GameObject gameObject)
    {
        if (gameObject && gameObject.activeInHierarchy)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Forces grabbing of the given item
    /// </summary>
    /// <param name="gameObject"></param>
    public void ForceGrabObject(VRTK_InteractableObject gameObject)
    {
        var autoGrab = FindObjectOfType<VRTK_ObjectAutoGrab>();

        if (!autoGrab)
        {
            Debug.LogError("ForceGrabObject - There is no VRTK_ObjectAutoGrab in the scene! It should be attached to one of the controllers");
            return;
        }

        autoGrab.enabled = false;
        gameObject.isGrabbable = true;
        autoGrab.objectToGrab = gameObject;
        autoGrab.enabled = true;
    }

    /// <summary>
    /// Disables teleporting around
    /// </summary>
    public void DisableTeleporting()
    {
        var leftHand = VRTK_DeviceFinder.GetControllerLeftHand();

        if (leftHand)
        {
            leftHand.GetComponent<VRTK_Pointer>().enableTeleport = false;
            leftHand.GetComponent<VRTK_Pointer>().enabled = false;
            leftHand.GetComponent<VRTK_BasePointerRenderer>().enabled = false;
        }

        leftHand = VRTK_DeviceFinder.GetControllerRightHand();

        if (leftHand)
        {
            leftHand.GetComponent<VRTK_Pointer>().enableTeleport = false;
            leftHand.GetComponent<VRTK_Pointer>().enabled = false;
            leftHand.GetComponent<VRTK_BasePointerRenderer>().enabled = false;
        }
    }

    /// <summary>
    /// Enables teleporting around
    /// </summary>
    public void EnableTeleporting()
    {
        var leftHand = VRTK_DeviceFinder.GetControllerLeftHand();

        if (leftHand)
        {
            leftHand.GetComponent<VRTK_Pointer>().enableTeleport = true;
            leftHand.GetComponent<VRTK_Pointer>().enabled = true;
            leftHand.GetComponent<VRTK_BasePointerRenderer>().enabled = true;
        }

        leftHand = VRTK_DeviceFinder.GetControllerRightHand();

        if (leftHand)
        {
            leftHand.GetComponent<VRTK_Pointer>().enableTeleport = true;
            leftHand.GetComponent<VRTK_Pointer>().enabled = true;
            leftHand.GetComponent<VRTK_BasePointerRenderer>().enabled = true;
        }
    }

    public void AlwaysShowStraightPointer()
    {
        var pointer = FindObjectOfType<VRTK_StraightPointerRenderer>();

        pointer.tracerVisibility = VRTK_BasePointerRenderer.VisibilityStates.AlwaysOn;
        pointer.cursorVisibility = VRTK_BasePointerRenderer.VisibilityStates.AlwaysOn;
        pointer.cursorScaleMultiplier = 20;
    }

    public void NormalStraightPointer()
    {
        var pointer = FindObjectOfType<VRTK_StraightPointerRenderer>();

        pointer.tracerVisibility = VRTK_BasePointerRenderer.VisibilityStates.OnWhenActive;
        pointer.cursorVisibility = VRTK_BasePointerRenderer.VisibilityStates.OnWhenActive;
        pointer.cursorScaleMultiplier = 25;
    }

    /// <summary>
    /// This will go through each child of the given object and update the tag from IncludeTeleport to Exclude teleport
    /// </summary>
    /// <param name="baseObject">The object to start to look at children of</param>
    public void MarkAllExcludeTeleport(GameObject baseObject)
    {
        Stack<Transform> transformsRemaining = new Stack<Transform>();
        transformsRemaining.Push(baseObject.transform);

        while (transformsRemaining.Count > 0)
        {
            var transform = transformsRemaining.Pop();

            foreach (Transform child in transform)
            {
                if (child == transform)
                {
                    break;
                }

                if (child.CompareTag("IncludeTeleport"))
                {
                    child.tag = "ExcludeTeleport";
                }

                transformsRemaining.Push(child);
            }
        }
    }

    public void DisableTriggerPointerCollisions()
    {
        foreach (var pointer in FindObjectsOfType<VRTK_StraightPointerRenderer>())
        {
            if (pointer.customRaycast)
            {
                pointer.customRaycast.triggerInteraction = QueryTriggerInteraction.Ignore;
            }
        }
    }

    public void InvokeClick(Button button)
    {
        button.onClick?.Invoke();
    }

    public void AllowGrab(VRTK_InteractableObject interactableObject)
    {
        interactableObject.isGrabbable = true;

        var highlighter = interactableObject.GetComponent<VRTK_InteractObjectHighlighter>();

        if (highlighter)
        {
            highlighter.enabled = true;
        }
    }

    public void AllowUse(VRTK_InteractableObject interactableObject)
    {
        interactableObject.isUsable = true;
    }


    public void AllowDropping(VRTK_InteractableObject interactableObject)
    {
        interactableObject.validDrop = VRTK_InteractableObject.ValidDropTypes.DropAnywhere;

        //if (!interactableObject.GetComponent<Rigidbody>())
        //{
        //    interactableObject.gameObject.AddComponent<Rigidbody>();
        //}
    }

    public void Log(string text)
    {
        Debug.Log(text);
    }

    public void GrowObject(GameObject gameObject)
    {
        gameObject.transform.localScale *= 1.1f;
    }

    public void AlignPlayAreaToObject(Transform transform)
    {
        var playarea = VRTK_DeviceFinder.PlayAreaTransform();

        playarea.rotation = transform.rotation;
        playarea.position = transform.position;
    }

    public void FlipPlayArea()
    {
        var playarea = VRTK_DeviceFinder.PlayAreaTransform();

        playarea.rotation = Quaternion.Euler(playarea.rotation.eulerAngles + new Vector3(0, 180, 0));
    }
}
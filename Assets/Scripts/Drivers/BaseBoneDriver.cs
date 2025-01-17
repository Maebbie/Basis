using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

public abstract class BaseBoneDriver : MonoBehaviour
{
    //figures out how to get the mouth bone and eye position
    public int ControlsLength;
    [SerializeField]
    public BasisBoneControl[] Controls;
    [SerializeField]
    public BasisBoneTrackedRole[] trackedRoles;
    public bool HasControls = false;
    public double ProvidedTime;
    public delegate void SimulationHandler();
    public event SimulationHandler OnSimulate;
    public event SimulationHandler OnPostSimulate;
    public event SimulationHandler ReadyToRead;
    /// <summary>
    /// call this after updating the bone data
    /// </summary>
    public void Simulate()
    {
        // sequence all other devices to run at the same time
        OnSimulate?.Invoke();
        //make sure to update time only after we invoke (its going to take time)
        ProvidedTime = Time.timeAsDouble;
        for (int Index = 0; Index < ControlsLength; Index++)
        {
            Controls[Index].ComputeMovement(ProvidedTime);
        }
        OnPostSimulate?.Invoke();
    }
    public void ApplyMovement()
    {
        for (int Index = 0; Index < ControlsLength; Index++)
        {
            Controls[Index].ApplyMovement();
        }
        ReadyToRead?.Invoke();
    }
#if UNITY_EDITOR
    public void OnDrawGizmos()
    {
        if (HasControls)
        {
            for (int Index = 0; Index < ControlsLength; Index++)
            {
                BasisBoneControl Control = Controls[Index];
                Control.DrawGizmos();
            }
        }
    }
#endif
    public void AddRange(BasisBoneControl[] newControls, BasisBoneTrackedRole[] newRoles)
    {
        Controls = Controls.Concat(newControls).ToArray();
        trackedRoles = trackedRoles.Concat(newRoles).ToArray();
        ControlsLength = Controls.Length;
    }
    public bool FindBone(out BasisBoneControl control, BasisBoneTrackedRole Role)
    {
        int Index = Array.IndexOf(trackedRoles, Role);

        if (Index >= 0 && Index < ControlsLength)
        {
            control = Controls[Index];
            return true;
        }

        control = null;
        return false;
    }
    public void CreateInitialArrays(Transform Parent)
    {
        trackedRoles = new BasisBoneTrackedRole[] { };
        Controls = new BasisBoneControl[] { };
        int Length = Enum.GetValues(typeof(BasisBoneTrackedRole)).Length;
        Color[] Colors = GenerateRainbowColors(Length);
        List<BasisBoneControl> newControls = new List<BasisBoneControl>();
        List<BasisBoneTrackedRole> Roles = new List<BasisBoneTrackedRole>();
        for (int Index = 0; Index < Length; Index++)
        {
            BasisBoneTrackedRole role = (BasisBoneTrackedRole)Index;
            BasisBoneControl Control = new BasisBoneControl();
            GameObject TrackedBone = new GameObject(role.ToString());
            TrackedBone.transform.parent = Parent;
            Control.BoneTransform = TrackedBone.transform;
            Control.HasBone = true;
            Control.Initialize();
            FillOutBasicInformation(Control, role.ToString(), Colors[Index]);
            newControls.Add(Control);
            Roles.Add(role);
        }
        AddRange(newControls.ToArray(), Roles.ToArray());
        HasControls = true;
    }
    public void FillOutBasicInformation(BasisBoneControl Control, string Name, Color Color)
    {
        Control.Name = Name;
        Control.Color = Color;
    }
    public Color[] GenerateRainbowColors(int RequestColorCount)
    {
        Color[] rainbowColors = new Color[RequestColorCount];

        for (int Index = 0; Index < RequestColorCount; Index++)
        {
            float hue = Mathf.Repeat(Index / (float)RequestColorCount, 1f);
            rainbowColors[Index] = Color.HSVToRGB(hue, 1f, 1f);
        }

        return rainbowColors;
    }
    public void CreateRotationalLock(BasisBoneControl addToBone, BasisBoneControl lockToBone, BasisRotationalControl.BasisClampAxis axisLock, BasisRotationalControl.BasisClampData clampData, float maxClamp, BasisRotationalControl.BasisAxisLerp axisLerp, float lerpAmount, Quaternion offset, BasisTargetController targetController, bool useAngle, float angleBeforeMove)
    {
        BasisRotationalControl rotation = new BasisRotationalControl
        {
            ClampableAxis = axisLock,
            Target = lockToBone,
            ClampSize = maxClamp,
            ClampStats = clampData,
            Lerp = axisLerp,
            LerpAmountNormal = lerpAmount,
            LerpAmountFastMovement = lerpAmount * 4,
            Offset = offset,
            TaretInterpreter = targetController,
            AngleBeforeMove = angleBeforeMove,
            UseAngle = useAngle,
            AngleBeforeSame = 1f,
            AngleBeforeSpeedup = 25f,
            ResetAfterTime = 1
        };
        addToBone.RotationControl = rotation;
    }
    public void CreatePositionalLock(BasisBoneControl Bone, BasisBoneControl Target)
    {
        BasisPositionControl Position = new BasisPositionControl
        {
            Lerp = BasisVectorLerp.Lerp,
            TaretInterpreter = BasisTargetController.TargetDirectional,
            Offset = Bone.RestingLocalSpace.BeginningPosition - Target.RestingLocalSpace.BeginningPosition,
            Target = Target,
            LerpAmount = 40
        };
        Bone.PositionControl = Position;
    }
    public static Vector3 ConvertToAvatarSpace(Animator animator, Vector3 WorldSpace, float AvatarHeightOffset, out Vector3 FloorPosition)
    {
        if (BasisHelpers.TryGetFloor(animator, out Vector3 Bottom))
        {
            FloorPosition = Bottom;
            return BasisHelpers.ConvertToLocalSpace(WorldSpace + new Vector3(0f, AvatarHeightOffset, 0f), Bottom);
        }
        else
        {
            FloorPosition = Vector3.zero;
            Debug.LogError("Missing Avatar");
            return Vector3.zero;
        }
    }
    public static Vector3 ConvertToWorldSpace(Vector3 WorldSpace, Vector3 LocalSpace)
    {
        return BasisHelpers.ConvertFromLocalSpace(LocalSpace, WorldSpace);
    }
}

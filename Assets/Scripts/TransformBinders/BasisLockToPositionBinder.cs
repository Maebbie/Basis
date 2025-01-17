using UnityEngine;
public class BasisLockToPositionBinder : MonoBehaviour
{
    public BasisLocalBoneDriver CharacterTransformDriver;
    public BasisBoneControl BoneControl;
    public BasisBoneTrackedRole Role = BasisBoneTrackedRole.Head;
    public bool hasCharacterTransformDriver = false;
    public bool HasBoneControl = false;
    public void Initialize(BasisLocalPlayer LocalPlayer)
    {
        if (LocalPlayer != null)
        {
            CharacterTransformDriver = LocalPlayer.LocalBoneDriver;
            if (CharacterTransformDriver == null)
            {
                hasCharacterTransformDriver = false;
                Debug.LogError("Missing CharacterTransformDriver");
            }
            else
            {
                hasCharacterTransformDriver = true;
                HasBoneControl = CharacterTransformDriver.FindBone(out BoneControl, Role);
            }
        }
        else
        {
            Debug.LogError("Missing LocalPlayer");
        }
        CharacterTransformDriver.ReadyToRead += Simulation;
    }
    public void OnDestroy()
    {
        if (CharacterTransformDriver != null)
        {
            CharacterTransformDriver.ReadyToRead -= Simulation;
        }
    }
    void Simulation()
    {
        if (hasCharacterTransformDriver && HasBoneControl)
        {
            transform.SetPositionAndRotation(BoneControl.BoneTransform.position, BoneControl.BoneTransform.rotation);
        }
    }
}
using Unity.Netcode;
using UnityEngine;
using GameNetcodeStuff;
using System.Collections.Generic;

namespace Ragdoll
{
	public class RagdollController : NetworkBehaviour
	{
		public static RagdollController localRagScript;
		public static Dictionary<int, RagdollController> allPlayerRagScripts;


		public static Transform[] localPlayerAnimBones;
		public static Transform[] localPlayerRagBones;
		public static Dictionary<int, Transform[]> allPlayerAnimBones;
		public static Dictionary<int, Transform[]> allPlayerRagBones;

		public SkinnedMeshRenderer ragdollModel;
		public PlayerControllerB playerScript;
		public SkinnedMeshRenderer playerModel;

		bool justActivated = true;

		public void Awake()
		{

		}

		public void Update()
		{
			if (justActivated)
			{
				justActivated = false;
				ragdollModel = GetComponent<SkinnedMeshRenderer>();
			}
		}

		public void SwapToRagdoll(bool ofLocalPlayer, int playerID)
		{
			if (!RagdollPatch.cacheRan) { P.Log("Bone cache not ran yet!!", LLV.Error); return; }
			playerScript.playerBodyAnimator.keepAnimatorStateOnDisable = true; // https://docs.unity3d.com/ScriptReference/Animator-keepAnimatorStateOnDisable.html
			playerScript.playerBodyAnimator.enabled = false;

			if (ofLocalPlayer) {
				playerModel.rootBone = localPlayerRagBones[0];
				playerModel.bones = localPlayerRagBones;
			}
		}
		public void SwapToAnimated()
		{

		}

		public static void SyncBoneTransforms(Transform[] sourceBones, Transform[] targetBones)
		{
			for (int i = 0; i < sourceBones.Length; i++)
			{
				targetBones[i].localPosition = sourceBones[i].localPosition;
				targetBones[i].localEulerAngles = sourceBones[i].localEulerAngles;
				targetBones[i].localScale = sourceBones[i].localScale; // might need to be removed
			}
		}
		// https://docs.unity3d.com/ScriptReference/Physics.SyncTransforms.html
	}
}

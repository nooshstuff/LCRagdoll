using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using GameNetcodeStuff;
using UnityEngine.Assertions;

namespace Ragdoll
{
	public static class RagdollPatch
	{
		public static GameObject ragSpinePrefab;

		internal static void Patch()
		{
			ragSpinePrefab = P.assets.LoadAsset<GameObject>("Assets/Ragdoll/spineRagdoll.prefab");
			// PlayerControllerB
			On.GameNetcodeStuff.PlayerControllerB.ConnectClientToPlayerObject += CacheBonesOnConnectPatch;
			On.GameNetcodeStuff.PlayerControllerB.Awake += PlayerControllerB_Awake;

			// DeadBodyInfo
			// DeadBodyInfo override

			// RagdollGrabbableObject

			// PlayerPhysicsRegion
		}

		#region PlayerControllerB
		private static void PlayerControllerB_Awake(On.GameNetcodeStuff.PlayerControllerB.orig_Awake orig, PlayerControllerB self)
		{
			orig(self);

			GameObject ragSpine = GameObject.Instantiate(ragSpinePrefab);
			ragSpine.GetComponent<RagdollController>().playerObjectId = (int)self.playerClientId;
			ragSpine.transform.SetParent(self.thisPlayerModel.rootBone.parent);
			P.Log("added ragSpine to player " + self.playerClientId);
			
		}

		private static void CacheBonesOnConnectPatch(On.GameNetcodeStuff.PlayerControllerB.orig_ConnectClientToPlayerObject orig, PlayerControllerB self)
		{
			orig(self);
			P.Log("caching bones for " + self.playerClientId);
			RagdollController.localRagScript = GameNetworkManager.Instance.localPlayerController.GetComponentInChildren<RagdollController>();
			RagdollController.localRagScript.playerScript = GameNetworkManager.Instance.localPlayerController;
			RagdollController.localRagScript.playerModel = GameNetworkManager.Instance.localPlayerController.thisPlayerModel;

			RagdollController.localPlayerAnimBones = GameNetworkManager.Instance.localPlayerController.thisPlayerModel.bones;
			RagdollController.localPlayerRagBones = RagdollController.localRagScript.ragdollModel.bones;

			for (int i = 0; i < StartOfRound.Instance.allPlayerScripts.Length; i++) {
				PlayerControllerB pl = StartOfRound.Instance.allPlayerScripts[i];
				RagdollController rag = pl.GetComponentInChildren<RagdollController>();
				RagdollController.allPlayerRagScripts[i] = rag;

				rag.playerScript = pl;
				rag.playerModel = pl.thisPlayerModel;

				RagdollController.allPlayerAnimBones[i] = pl.thisPlayerModel.bones;
				RagdollController.allPlayerRagBones[i] = rag.ragdollModel.bones;

				rag.animBones = pl.thisPlayerModel.bones;
				rag.ragBones = rag.ragdollModel.bones;

				pl.playerBodyAnimator.keepAnimatorStateOnDisable = true; // https://docs.unity3d.com/ScriptReference/Animator-keepAnimatorStateOnDisable.html

				rag.Initialize();
			}
		}
		#endregion
	}
}

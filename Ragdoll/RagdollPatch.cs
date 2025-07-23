using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using GameNetcodeStuff;
using UnityEngine.Assertions;
using System;

namespace Ragdoll
{
	public static class RagdollPatch
	{
		public static GameObject ragSpinePrefab;
		public static GameObject grabbablePlayerPrefab;

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

			// TEST
			On.GameNetcodeStuff.PlayerControllerB.Emote1_performed += PlayerControllerB_Emote1_performed;
			On.GameNetcodeStuff.PlayerControllerB.Emote2_performed += PlayerControllerB_Emote2_performed;
		}

		

		private static void PlayerControllerB_Emote1_performed(On.GameNetcodeStuff.PlayerControllerB.orig_Emote1_performed orig, PlayerControllerB self, UnityEngine.InputSystem.InputAction.CallbackContext context)
		{
			RagdollController.allPlayerRagScripts[(int)self.playerClientId].SwapToRagdoll();
		}

		private static void PlayerControllerB_Emote2_performed(On.GameNetcodeStuff.PlayerControllerB.orig_Emote2_performed orig, PlayerControllerB self, UnityEngine.InputSystem.InputAction.CallbackContext context)
		{
			RagdollController.allPlayerRagScripts[(int)self.playerClientId].SwapToAnimated();
		}

		#region PlayerControllerB
		private static void PlayerControllerB_Awake(On.GameNetcodeStuff.PlayerControllerB.orig_Awake orig, PlayerControllerB self)
		{
			orig(self);
			self.playerBodyAnimator.keepAnimatorStateOnDisable = true; // https://docs.unity3d.com/ScriptReference/Animator-keepAnimatorStateOnDisable.html
			GameObject ragSpine = GameObject.Instantiate(ragSpinePrefab);
			RagdollController rag = ragSpine.GetComponent<RagdollController>();
			rag.playerScript = self;
			rag.playerModel = self.thisPlayerModel;
			rag.playerObjectId = (int)self.playerClientId;
			rag.transform.SetParent(self.thisPlayerModel.rootBone.parent);
			rag.playerPhysicsBox = self.transform.Find("PlayerPhysicsBox").GetComponent<Collider>();
			
			P.Log("added ragSpine to player " + self.playerClientId);
		}

		private static void CacheBonesOnConnectPatch(On.GameNetcodeStuff.PlayerControllerB.orig_ConnectClientToPlayerObject orig, PlayerControllerB self)
		{
			orig(self);
			P.Log("caching bones for " + self.playerClientId);
			RagdollController rag = self.GetComponentInChildren<RagdollController>();
			if (self == GameNetworkManager.Instance.localPlayerController)
			{
				rag.isLocalRag = true;
				RagdollController.localRagScript = rag;
			}
			RagdollController.allPlayerRagScripts[(int)self.playerClientId] = rag;

			for (int i = 0; i < StartOfRound.Instance.allPlayerScripts.Length; i++) {
				PlayerControllerB pl = StartOfRound.Instance.allPlayerScripts[i];
				rag = RagdollController.allPlayerRagScripts[i];

				RagdollController.allPlayerAnimBones[i] = pl.thisPlayerModel.bones;
				RagdollController.allPlayerRagBones[i] = rag.ragdollModel.bones;

				rag.animBones = pl.thisPlayerModel.bones;
				rag.ragBones = rag.ragdollModel.bones;

				rag.Initialize();
			}
		}
		#endregion
	}
}

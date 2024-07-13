using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using GameNetcodeStuff;

namespace Ragdoll
{
	public static class RagdollPatch
	{
		public static bool cacheRan = false;
		public static GameObject ragSpinePrefab;

		internal static void Patch()
		{
			On.GameNetcodeStuff.PlayerControllerB.ConnectClientToPlayerObject += CacheBonesOnConnectPatch;
			On.GameNetcodeStuff.PlayerControllerB.Awake += PlayerControllerB_Awake;
		}

		private static void PlayerControllerB_Awake(On.GameNetcodeStuff.PlayerControllerB.orig_Awake orig, PlayerControllerB self)
		{
			orig(self);

			GameObject ragSpine = GameObject.Instantiate(ragSpinePrefab);
			ragSpine.transform.SetParent(self.playerModelArmsMetarig);
		}

		private static void CacheBonesOnConnectPatch(On.GameNetcodeStuff.PlayerControllerB.orig_ConnectClientToPlayerObject orig, PlayerControllerB self)
		{
			orig(self);
			RagdollController.localRagScript = GameNetworkManager.Instance.localPlayerController.GetComponentInChildren<RagdollController>();
			RagdollController.localRagScript.playerScript = GameNetworkManager.Instance.localPlayerController;
			RagdollController.localRagScript.playerModel = GameNetworkManager.Instance.localPlayerController.thisPlayerModel;

			RagdollController.localPlayerAnimBones = GameNetworkManager.Instance.localPlayerController.thisPlayerModel.bones;
			RagdollController.localPlayerRagBones = RagdollController.localRagScript.ragdollModel.bones;

			for (int i = 0; i < StartOfRound.Instance.allPlayerScripts.Length; i++) {
				RagdollController.allPlayerRagScripts[i] = StartOfRound.Instance.allPlayerScripts[i].GetComponentInChildren<RagdollController>();
				RagdollController.allPlayerRagScripts[i].playerScript = StartOfRound.Instance.allPlayerScripts[i];
				RagdollController.allPlayerRagScripts[i].playerModel = StartOfRound.Instance.allPlayerScripts[i].thisPlayerModel;
				RagdollController.allPlayerAnimBones[i] = StartOfRound.Instance.allPlayerScripts[i].thisPlayerModel.bones;
				RagdollController.allPlayerRagBones[i] = RagdollController.allPlayerRagScripts[i].ragdollModel.bones;
			}
			cacheRan = true;
		}

		// thanks
		public static void Apply(SkinnedMeshRenderer sourceSkin, SkinnedMeshRenderer targetSkin)
		{

			
			Transform[] newBones = new Transform[targetSkin.bones.Length];
			Dictionary<string, Transform> boneCache = sourceSkin.bones.ToDictionary((Transform bone) => (bone.name), (Transform bone) => bone);
			List<string> missingBones = new List<string>();
			for (int i = 0; i < targetSkin.bones.Length; i++)
			{
				if (boneCache.ContainsKey(targetSkin.bones[i].name))
				{
					newBones[i] = boneCache[targetSkin.bones[i].name];
				}
			}
			targetSkin.bones = newBones;
			
		}
	}
}

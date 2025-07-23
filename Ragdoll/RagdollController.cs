using Unity.Netcode;
using UnityEngine;
using GameNetcodeStuff;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using System;
using static UnityEngine.Rendering.DebugUI;
using Unity.Services.Authentication.Generated;
using UnityEngine.Rendering;
// 591075327
namespace Ragdoll
{
	public class RagdollController : MonoBehaviour
	{
		public static RagdollController localRagScript;
		public static Dictionary<int, RagdollController> allPlayerRagScripts = new Dictionary<int, RagdollController>();

		public static Transform[] localPlayerRagBones;
		public static Dictionary<int, Transform[]> allPlayerAnimBones = new Dictionary<int, Transform[]>();

		public static Transform[] localPlayerAnimBones;
		public static Dictionary<int, Transform[]> allPlayerRagBones = new Dictionary<int, Transform[]>();

		public bool active = false;
		public bool isLocalRag;
		public bool ragdolling = false;

		// PLAYER STUFF
		public int playerObjectId;
		public PlayerControllerB playerScript;
		public SkinnedMeshRenderer ragdollModel;
		public SkinnedMeshRenderer playerModel;
		public Collider playerPhysicsBox;

		public BodyPart[] bodyParts;
		public Transform[] animBones;
		public Transform[] ragBones;
		public Transform ragHead;

		public ScanNodeProperties scanNode;
		public GrabbablePlayer grabbable;

		public virtual void Initialize()
		{
			scanNode.headerText = $"Body of {playerScript.playerUsername}";
			ragdollModel.enabled = false;

			if (isLocalRag)
			{
				RagdollController.localPlayerAnimBones = playerScript.thisPlayerModel.bones;
				RagdollController.localPlayerRagBones = ragdollModel.bones;
			}

			if (StartOfRound.Instance != null)
			{
				for (int i = 0; i < bodyParts.Length; i++)
				{
					bodyParts[i].gameObject.tag = (playerObjectId == 0 ? "PlayerRagdoll" : $"PlayerRagdoll{playerObjectId}");
				}
			}

			/*GameObject obj = Instantiate(RagdollPatch.grabbablePlayerPrefab, StartOfRound.Instance.propsContainer);
			obj.GetComponent<NetworkObject>().Spawn();
			grabbable = obj.GetComponent<GrabbablePlayer>();
			grabbable.bodyID = playerObjectId;
			grabbable.ragdoll = this;
			grabbable.parentObject = bodyParts[5].transform;
			grabbable.grabPoint = bodyParts[5];
			grabbable.transform.SetParent(bodyParts[5].transform);
			*/

			active = true;
		}

		public virtual void FixedUpdate()
		{

		}

		public virtual void Update()
		{
		
		}

		public static float cameraLerpScale = 7.0f;
		public static float visorLerpScale = 25.0f;
		public virtual void LateUpdate() 
		{
			if (active)
			{
				ragBones[0].localScale = animBones[0].localScale;
				if (ragdolling) // or put in FixedUpdate()?
				{
					for (int i = 0; i < ragBones.Length; i++) { 
						SyncBoneTransforms(ragBones[i], animBones[i]);
					}

					playerScript.cameraContainerTransform.position = ragHead.position;
					playerScript.cameraContainerTransform.rotation = Quaternion.Slerp(playerScript.cameraContainerTransform.rotation, ragHead.rotation, 0.9f*Time.deltaTime*cameraLerpScale);

					if (isLocalRag)
					{
						float dot = Quaternion.Dot(playerScript.localVisor.rotation, playerScript.localVisorTargetPoint.rotation) + 1; // 0.0 = rotations are equal, 2.0 = rotations are opposite
						if (dot >= 0.05) { P.Log(dot); }
						//playerScript.localVisor.rotation = Quaternion.Lerp(playerScript.localVisor.rotation, playerScript.localVisorTargetPoint.rotation, 53f * Mathf.Clamp(Time.deltaTime, 0.0167f, 20f));
						playerScript.localVisor.rotation = Quaternion.Lerp(playerScript.localVisor.rotation, playerScript.localVisorTargetPoint.rotation, dot*Mathf.Clamp(Time.deltaTime, 0.0167f, 20f)*visorLerpScale);
					}
				}
				else
				{
					for (int i = 0; i < animBones.Length; i++) { SyncBoneTransforms(animBones[i], ragBones[i]); }
				}
				
			}
		}

		// see PlayerControllerB.KillPlayer() & .SpawnDeadBody() ( and .KillPlayerServerRpc() + .KillPlayerClientRpc() )
		public void SwapToRagdoll() // bool isLocalPlayer ?
		{
			if (ragdolling) { P.Log("Already ragdolling!", LLV.Message); return; }
			if (!active) { P.Log("Bone cache not ran yet or ragdollController inactive!!", LLV.Warning); return; }

			// TODO: what if you're interacting with something? or holding items?
			// TODO: should tell PlayerControllerB that we're dead or not?

			// TODO: correct properties for this?
			//playerScript.isPlayerControlled = false; // this just turns you off
			playerScript.disableLookInput = true;
			if (playerScript.inSpecialInteractAnimation && playerScript.currentTriggerInAnimationWith != null)
			{
				playerScript.currentTriggerInAnimationWith.CancelAnimationExternally();
			}
			playerScript.inSpecialInteractAnimation = true;
			playerScript.playerBodyAnimator.enabled = false;
			//playerScript.freeRotationInInteractAnimation = true;
			if (isLocalRag)
			{
				//foreach (SkinnedMeshRenderer mdl in models) { mdl.gameObject.layer = LayerMask.NameToLayer("Player"); }
				if (playerScript.currentlyHeldObjectServer != null) { playerScript.currentlyHeldObjectServer.parentObject = playerScript.serverItemHolder; }
				playerScript.playerGlobalHead.localScale = Vector3.zero;
				playerScript.thisPlayerModel.shadowCastingMode = ShadowCastingMode.On;
				playerScript.thisPlayerModelArms.enabled = false;
			}

			playerPhysicsBox.enabled = false;

			foreach (BodyPart part in bodyParts) { part.rigidbody.isKinematic = false; }
			foreach (BodyPart part in bodyParts) { part.Stop(); }

			// foreach (SkinnedMeshRenderer mdl in models) { mdl.rootBone = ragBones[0]; mdl.bones = ragBones; }

			// TODO: partial ragdoll shenanigans by switching bones to ragBones and then selectively copying from animBones

			/* TODO: Need any of these??
			playerScript.physicsParent = null;
			playerScript.overridePhysicsParent = null;
			playerScript.lastSyncedPhysicsParent = null;
			StartOfRound.Instance.CurrentPlayerPhysicsRegions.Clear();
			CancelSpecialTriggerAnimations();
			StopHoldInteractionOnTrigger();
			StartOfRound.Instance.SwitchCamera();
			isInGameOverAnimation = 1.5f;
			cursorTip.text = "";
			cursorIcon.enabled = false;
			DropAllHeldItems(spawnBody);
			DisableJetpackControlsLocally();
			*/
			// TODO: might need more work. see PlayerControllerB.LateUpdate()

			//parentedToShip = playerScript.isInElevator;
			// TODO: should i set playerScript.deadBody?

			// TODO: body velocity

			// TODO: do i activate any blood decals & MakeCorpseBloody()
			// TODO: make different death animations possible?

			// TODO: transmitting any audio?

			// TODO: server RPC?

			ragdolling = true;
		}

		public void SwapToAnimated()
		{
			if (!active) { P.Log("Bone cache not ran yet or ragdollController inactive!!", LLV.Warning); return; }

			// TODO: make player model invisible in first person again
			if (isLocalRag)
			{
				//	foreach (SkinnedMeshRenderer mdl in models) { mdl.gameObject.layer = LayerMask.NameToLayer("Default"); }
				if (playerScript.currentlyHeldObjectServer != null) { playerScript.currentlyHeldObjectServer.parentObject = playerScript.localItemHolder; }
				playerScript.playerGlobalHead.localScale = Vector3.one;
				playerScript.thisPlayerModel.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
				playerScript.thisPlayerModelArms.enabled = true;
			}

			// foreach (SkinnedMeshRenderer mdl in models) { mdl.rootBone = animBones[0]; mdl.bones = animBones; }

			foreach (BodyPart part in bodyParts) { part.Stop(); }
			foreach (BodyPart part in bodyParts) { part.rigidbody.isKinematic = true; }

			playerPhysicsBox.enabled = true;

			// TODO: player physics parent shenanigans?

			playerScript.playerBodyAnimator.enabled = true;

			playerScript.inSpecialInteractAnimation = false;
			playerScript.disableLookInput = false;

			ragdolling = false;
		}

		public static void SyncBoneTransforms(Transform sourceBone, Transform targetBone)
		{
				targetBone.localPosition = sourceBone.localPosition;
				targetBone.localRotation = sourceBone.localRotation;
				//targetBone.localScale = sourceBone.localScale; // might need to be removed
		}
		// https://docs.unity3d.com/ScriptReference/Physics.SyncTransforms.html
		public static void SyncBoneTransforms(Transform[] sourceBones, Transform[] targetBones)
		{
			for (int i = 0; i < sourceBones.Length; i++) { SyncBoneTransforms(sourceBones[i], targetBones[i]); }
		}
	}

	[System.Serializable]
	public class BodyPart : MonoBehaviour
	{
		public Transform bone;
		public Rigidbody rigidbody;
		public Collider collider;
		public CharacterJoint joint;

		public Transform? attachedTo;

		public void Attach(Transform target)
		{
			rigidbody.isKinematic = true;
			attachedTo = target;
		}
		public void Detatch(bool returnPhysics = true)
		{
			attachedTo = null;
			rigidbody.isKinematic = !returnPhysics;
			
		}
		public void Stop() { rigidbody.velocity = Vector3.zero; }
	}

	public enum PartID
	{
		Neck = 0,
		LowerArmR = 1,
		LowerArmL = 2,
		ShinR = 3,
		ShinL = 4,
		Boob = 5,
		Pelvis = 6,
		ThighR = 7,
		ThighL = 8,
		UpperArmL = 9,
		UpperArmR = 10,
	}
}

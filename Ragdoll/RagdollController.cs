using Unity.Netcode;
using UnityEngine;
using GameNetcodeStuff;
using System.Collections.Generic;

namespace Ragdoll
{
	public class RagdollController : MonoBehaviour
	{
		public static RagdollController localRagScript;
		public static Dictionary<int, RagdollController> allPlayerRagScripts = new Dictionary<int, RagdollController>();

		public static Transform[] localPlayerAnimBones;
		public static Transform[] localPlayerRagBones;
		public static Dictionary<int, Transform[]> allPlayerAnimBones = new Dictionary<int, Transform[]>();
		public static Dictionary<int, Transform[]> allPlayerRagBones = new Dictionary<int, Transform[]>();

		public bool active = false;

		public int playerObjectId;
		public PlayerControllerB playerScript;
		public SkinnedMeshRenderer ragdollModel;
		public SkinnedMeshRenderer playerModel;

		public GameObject[] parts;
		public BodyPart[] bodyParts;

		public Transform[] ragBones;
		public Transform[] animBones;

		public Transform ragHead;

		public bool ragdolling = false;

		public ScanNodeProperties scanNode;
		//public RagdollGrabbableObject grabbableObject;

		public virtual void Initialize()
		{
			scanNode.headerText = $"Body of {playerScript.playerUsername}";
			//GameObject obj = Instantiate(StartOfRound.Instance.ragdollGrabbableObjectPrefab, StartOfRound.Instance.propsContainer);
			//obj.GetComponent<NetworkObject>().Spawn();

			/*
			grabbableObject = obj.GetComponent<RagdollGrabbableObject>();
			grabbableObject.bodyID.Value = playerObjectId;
			grabbableObject.ragdoll = this;
			grabbableObject.parentObject = bodyParts[5].transform;
			grabbableObject.transform.SetParent(bodyParts[5].transform);
			grabbableObject.foundRagdollObject = true;
			*/

			if (StartOfRound.Instance != null)
			{
				for (int i = 0; i < playerScript.bodyParts.Length; i++)
				{
					bodyParts[i] = parts[i].AddComponent<BodyPart>();
					bodyParts[i].name = parts[i].name;
					if (playerObjectId == 0) { bodyParts[i].gameObject.tag = "PlayerRagdoll"; }
					else { bodyParts[i].gameObject.tag = $"PlayerRagdoll{playerObjectId}"; }
				}
			}
			active = true;
		}

		public virtual void FixedUpdate()
		{

		}

		public virtual void Update()
		{
		
		}

		public virtual void LateUpdate() 
		{
			if (active)
			{ 
				if (ragdolling) // or put in FixedUpdate()?
				{
					SyncBoneTransforms(ragBones, animBones);
					SyncBoneTransforms(ragHead, playerScript.cameraContainerTransform);
				}
				else
				{
					SyncBoneTransforms(animBones, ragBones);
				} 
			}
		}

		// see PlayerControllerB.KillPlayer() & .SpawnDeadBody() ( and .KillPlayerServerRpc() + .KillPlayerClientRpc() )
		public void SwapToRagdoll() // bool isLocalPlayer ?
		{
			if (!active) { P.Log("Bone cache not ran yet or ragdollController inactive!!", LLV.Warning); return; }

			// TODO: what if you're interacting with something? or holding items?
			// TODO: should tell PlayerControllerB that we're dead or not?

			// TODO: correct properties for this?
			playerScript.isPlayerControlled = false;
			if (playerScript.inSpecialInteractAnimation && playerScript.currentTriggerInAnimationWith != null)
			{
				playerScript.currentTriggerInAnimationWith.CancelAnimationExternally();
			}
			playerScript.inSpecialInteractAnimation = true;
			//playerScript.inAnimationWithEnemy = this; ???
			//playerScript.isInElevator = false;
			//playerScript.isInHangarShipRoom = false;
			//playerScript.freeRotationInInteractAnimation = true;

			// TODO: make player model visible in first person
			//playerScript.thisPlayerModelArms.enabled = false; // TODO: correct way to disable FP arms?

			playerScript.playerBodyAnimator.enabled = false;

			playerModel.rootBone = ragBones[0];
			playerModel.bones = ragBones;

			//SetPhysicsParent(playerScript.physicsParent); //broke it
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
			//playerScript.thisPlayerModelArms.enabled = true; // TODO: correct way to enable FP arms?

			playerModel.rootBone = animBones[0];
			playerModel.bones = animBones;

			// TODO: player physics parent shenanigans?

			playerScript.playerBodyAnimator.enabled = true;

			playerScript.inSpecialInteractAnimation = false;
			playerScript.isPlayerControlled = true;

			ragdolling = false;
		}

		public static void SyncBoneTransforms(Transform sourceBone, Transform targetBone)
		{
				targetBone.localPosition = sourceBone.localPosition;
				targetBone.localEulerAngles = sourceBone.localEulerAngles;
				targetBone.localScale = sourceBone.localScale; // might need to be removed
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

		public bool kinematic { get { return kinematic; } set { kinematic = value; if (value) { Stop(); } rigidbody.isKinematic = value; }}
		public Transform? attachedTo;

		public void Awake()
		{
			bone = GetComponent<Transform>();
			rigidbody = GetComponent<Rigidbody>();
			collider = GetComponent<Collider>();
		}

		public void Attach(Transform target)
		{
			kinematic = true;
			attachedTo = target;
		}
		public void Detatch(bool returnPhysics = true)
		{
			attachedTo = null;
			kinematic = !returnPhysics;
			
		}
		public void Stop() { rigidbody.velocity = Vector3.zero; }
	}
}

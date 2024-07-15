using Unity.Netcode;
using UnityEngine;
using GameNetcodeStuff;
using System.Collections.Generic;

namespace Ragdoll
{
	public class OldRagdollController : DeadBodyInfo
	{
		public static RagdollController localRagScript;
		public static Dictionary<int, RagdollController> allPlayerRagScripts = new Dictionary<int, RagdollController>();

		public bool active = false;

		public static Transform[] localPlayerAnimBones;
		public static Transform[] localPlayerRagBones;
		public static Dictionary<int, Transform[]> allPlayerAnimBones = new Dictionary<int, Transform[]>();
		public static Dictionary<int, Transform[]> allPlayerRagBones = new Dictionary<int, Transform[]>();

		public SkinnedMeshRenderer ragdollModel;
		public SkinnedMeshRenderer playerModel;

		public Transform[] ragBones;
		public Transform[] animBones;

		public Transform ragHead;

		public bool ragdolling = false;

		public ScanNodeProperties scanNode;
		public RagdollGrabbableObject grabbableObject;
		// GrabbableObject DeadBodyInfo.grabBodyObject;

		#region unnaccounted
		/*
		public Rigidbody[] bodyParts;

		public Rigidbody attachedLimb;
		public Transform attachedTo;

		public Rigidbody secondaryAttachedLimb;
		public Transform secondaryAttachedTo;

		public int timesOutOfBounds;
		public Vector3 spawnPosition;

		private Vector3 forceDirection;
		public float maxVelocity;
		public float speedMultiplier;
		public bool matchPositionExactly = true;
		public bool wasMatchingPosition;
		private Rigidbody previousAttachedLimb;

		public bool bodyBleedingHeavily = false; // (was true)
		private Vector3 previousBodyPosition;
		private int bloodAmount;
		private int maxBloodAmount = 30;
		public GameObject[] bodyBloodDecals;

		private bool bodyMovedThisFrame;
		private float syncBodyPositionTimer;
		private bool serverSyncedPositionWithClients;
		public bool seenByLocalPlayer;
		public AudioSource bodyAudio;
		private float velocityLastFrame;
		public Transform radarDot;
		private float timeSinceLastCollisionSFX;
		public bool parentedToShip;
		public bool detachedHead;
		public Transform detachedHeadObject;
		public Vector3 detachedHeadVelocity;
		public ParticleSystem bloodSplashParticle;
		public ParticleSystem beamUpParticle;
		public ParticleSystem beamOutParticle;
		public AudioSource playAudioOnDeath;
		public CauseOfDeath causeOfDeath;
		private float resetBodyPartsTimer;
		
		private bool bodySetToKinematic;
		public bool lerpBeforeMatchingPosition;
		private float moveToExactPositionTimer;
		public bool canBeGrabbedBackByPlayers;
		public bool isInShip;
		public bool deactivated;
		public Transform physicsParent;
		private Collider physicsParentCollider;
		public bool isParentedToPhysicsRegion;
		---------------------------------------------------------------------------

		ResetBodyPositionIfTooFarFromAttachment()
		EnableCollisionOnBodyParts()
		DetectIfSeenByLocalPlayer()
		DetectBodyMovedDistanceThreshold()
		playerScript.SyncBodyPositionWithClients();
		StopFloatingBody()

		SetPhysicsParent()
		SetBodyPartsKinematic()
		DeactivateBody()

		StopFloatingBody()
		SetRagdollPositionSafely()
		ResetRagdollPosition()

		MakeCorpseBloody()
		ChangeMesh()

		AddForceToBodyPart() - Unused(?)

		*/
		#endregion

		public void FalseStart()
		{
			scanNode.headerText = $"Body of {playerScript.playerUsername}";
			GameObject obj = Instantiate(StartOfRound.Instance.ragdollGrabbableObjectPrefab, StartOfRound.Instance.propsContainer);
			obj.GetComponent<NetworkObject>().Spawn();

			grabbableObject = obj.GetComponent<RagdollGrabbableObject>();
			grabBodyObject = grabbableObject;
			grabbableObject.bodyID.Value = playerObjectId;
			grabbableObject.ragdoll = this;
			grabbableObject.parentObject = bodyParts[5].transform;
			grabbableObject.transform.SetParent(bodyParts[5].transform);
			grabbableObject.foundRagdollObject = true;

			spawnPosition = base.transform.position;
			previousBodyPosition = Vector3.zero;
			if (StartOfRound.Instance != null)
			{
				for (int i = 0; i < playerScript.bodyParts.Length; i++)
				{
					//if (!overrideSpawnPosition) { bodyParts[i].position = playerScript.bodyParts[i].position; }
					if (playerObjectId == 0) { bodyParts[i].gameObject.tag = "PlayerRagdoll"; }
					else { bodyParts[i].gameObject.tag = $"PlayerRagdoll{playerObjectId}"; }
				}
			}
			/*
			if (detachedHead)
			{
				if (RoundManager.Instance != null && RoundManager.Instance.mapPropsContainer != null)
				{
					detachedHeadObject.SetParent(RoundManager.Instance.mapPropsContainer.transform);
				}
				detachedHeadObject.GetComponent<Rigidbody>().AddForce(detachedHeadVelocity * 350f, ForceMode.Impulse);
			}
			if (bloodSplashParticle != null)
			{
				ParticleSystem.MainModule main = bloodSplashParticle.main;
				main.customSimulationSpace = RoundManager.Instance.mapPropsContainer.transform;
			}
			*/
		}

		/*
		// THIS code might be important
		
		*/

		new void FixedUpdate()
		{
			/*
			if (!deactivated && !wasMatchingPosition && causeOfDeath == CauseOfDeath.Drowning && playerScript != null && playerScript.underwaterCollider != null && !isInShip)
			{
				FloatBodyToWaterSurface();
			}
			*/ // don't run the floating code. for now
		}

		new void Update()
		{
			/*
			if (deactivated)
			{
				isInShip = false;
				if (grabBodyObject != null && grabBodyObject.grabbable)
				{
					grabBodyObject.grabbable = false;
					grabBodyObject.grabbableToEnemies = false;
					grabBodyObject.EnablePhysics(enable: false);
					GetComponentInChildren<ScanNodeProperties>().GetComponent<Collider>().enabled = false;
				}
				return;
			}
			*/
			/*
			isInShip = parentedToShip || (grabBodyObject != null && grabBodyObject.isHeld && grabBodyObject.playerHeldBy != null && grabBodyObject.playerHeldBy.isInElevator);
			if (attachedLimb != null && attachedTo != null && matchPositionExactly)
			{
				syncBodyPositionTimer = 5f;
				ResetBodyPositionIfTooFarFromAttachment();
				resetBodyPartsTimer += Time.deltaTime;
				if (resetBodyPartsTimer >= 0.25f)
				{
					resetBodyPartsTimer = 0f;
					EnableCollisionOnBodyParts();
				}
			}
			*/
			/*
			if (GameNetworkManager.Instance == null || GameNetworkManager.Instance.localPlayerController == null) { return; }
			DetectIfSeenByLocalPlayer();
			DetectBodyMovedDistanceThreshold();
			if (bodyMovedThisFrame)
			{
				syncBodyPositionTimer = 5f;
				if (bodyBleedingHeavily && bloodAmount < maxBloodAmount)
				{
					bloodAmount++;
					playerScript.DropBlood(Vector3.down);
				}
			}
			if (attachedLimb != null && attachedTo != null)
			{
				syncBodyPositionTimer = 5f;
			}
			else if (GameNetworkManager.Instance.localPlayerController.IsOwnedByServer && !serverSyncedPositionWithClients)
			{
				if (syncBodyPositionTimer >= 0f)
				{
					syncBodyPositionTimer -= Time.deltaTime;
				}
				else
				{
					if (Physics.CheckSphere(base.transform.position, 30f, StartOfRound.Instance.playersMask))
					{
						for (int i = 0; i < StartOfRound.Instance.allPlayerScripts.Length; i++)
						{
							if (StartOfRound.Instance.allPlayerScripts[i].isPlayerControlled && !Physics.Linecast(StartOfRound.Instance.allPlayerScripts[i].gameplayCamera.transform.position, base.transform.position, StartOfRound.Instance.collidersAndRoomMaskAndDefault))
							{
								syncBodyPositionTimer = 0.3f;
								return;
							}
						}
					}
					serverSyncedPositionWithClients = true;
					playerScript.SyncBodyPositionWithClients();
				}
			}
			*/
			/*
			if (timeSinceLastCollisionSFX <= 0.5f)
			{
				timeSinceLastCollisionSFX += Time.deltaTime;
				return;
			}
			timeSinceLastCollisionSFX = 0f;
			*/
			velocityLastFrame = bodyParts[5].velocity.sqrMagnitude;
			
		}

		new void LateUpdate() 
		{
			/*
			if (deactivated)
			{
				radarDot.gameObject.SetActive(value: false);
				if (parentedToShip)
				{
					parentedToShip = false;
					base.transform.SetParent(null, worldPositionStays: true);
				}
				return;
			}
			*/
			radarDot.eulerAngles = new Vector3(0f, radarDot.eulerAngles.y, 0f);
			
			if (attachedLimb == null || attachedTo == null || attachedTo.parent == base.transform)
			{
				if (grabBodyObject != null)
				{
					grabBodyObject.grabbable = true;
				}
				moveToExactPositionTimer = 0f;
				if (wasMatchingPosition)
				{
					wasMatchingPosition = false;
					if (physicsParentCollider != null && physicsParentCollider.enabled && physicsParentCollider.ClosestPoint(bodyParts[5].position) == bodyParts[5].position)
					{
						isParentedToPhysicsRegion = true;
						base.transform.SetParent(physicsParent);
						parentedToShip = true;
						StopFloatingBody();
					}
					else if (StartOfRound.Instance.shipBounds.bounds.Contains(base.transform.position))
					{
						base.transform.SetParent(StartOfRound.Instance.elevatorTransform);
						parentedToShip = true;
						StopFloatingBody();
						isParentedToPhysicsRegion = false;
					}
					else if (isParentedToPhysicsRegion)
					{
						isParentedToPhysicsRegion = false;
						base.transform.SetParent(null, worldPositionStays: true);
					}
					previousAttachedLimb.ResetCenterOfMass();
					previousAttachedLimb.ResetInertiaTensor();
					previousAttachedLimb.freezeRotation = false;
					previousAttachedLimb.isKinematic = false;
					EnableCollisionOnBodyParts();
				}
				return;
			}
			if (grabBodyObject != null)
			{
				grabBodyObject.grabbable = canBeGrabbedBackByPlayers;
			}

			if (parentedToShip)
			{
				parentedToShip = false;
				base.transform.SetParent(null, worldPositionStays: true);
			}
			if (matchPositionExactly)
			{
				if (!lerpBeforeMatchingPosition || !(moveToExactPositionTimer < 0.3f))
				{
					if (!wasMatchingPosition)
					{
						wasMatchingPosition = true;
						Vector3 vector = base.transform.position - attachedLimb.position;
						base.transform.GetComponent<Rigidbody>().position = attachedTo.position + vector;
						previousAttachedLimb = attachedLimb;
						attachedLimb.freezeRotation = true;
						attachedLimb.isKinematic = true;
						attachedLimb.transform.position = attachedTo.position;
						attachedLimb.transform.rotation = attachedTo.rotation;
						for (int i = 0; i < bodyParts.Length; i++)
						{
							bodyParts[i].angularDrag = 1f;
							bodyParts[i].maxAngularVelocity = 2f;
							bodyParts[i].maxDepenetrationVelocity = 0.3f;
							bodyParts[i].velocity = Vector3.zero;
							bodyParts[i].angularVelocity = Vector3.zero;
							bodyParts[i].WakeUp();
						}
					}
					else
					{
						attachedLimb.position = attachedTo.position;
						attachedLimb.rotation = attachedTo.rotation;
						attachedLimb.centerOfMass = Vector3.zero;
						attachedLimb.inertiaTensorRotation = Quaternion.identity;
					}
					return;
				}
				moveToExactPositionTimer += Time.deltaTime;
				speedMultiplier = 25f;
			}
			forceDirection = Vector3.Normalize(attachedTo.position - attachedLimb.position);
			attachedLimb.AddForce(forceDirection * speedMultiplier * Mathf.Clamp(Vector3.Distance(attachedTo.position, attachedLimb.position), 0.2f, 2.5f), ForceMode.VelocityChange);
			if (attachedLimb.velocity.sqrMagnitude > maxVelocity)
			{
				attachedLimb.velocity = attachedLimb.velocity.normalized * maxVelocity;
			}
			if (!(secondaryAttachedLimb == null) && !(secondaryAttachedTo == null))
			{
				forceDirection = Vector3.Normalize(secondaryAttachedTo.position - secondaryAttachedLimb.position);
				secondaryAttachedLimb.AddForce(forceDirection * speedMultiplier * Mathf.Clamp(Vector3.Distance(secondaryAttachedTo.position, secondaryAttachedLimb.position), 0.2f, 2.5f), ForceMode.VelocityChange);
				if (secondaryAttachedLimb.velocity.sqrMagnitude > maxVelocity)
				{
					secondaryAttachedLimb.velocity = secondaryAttachedLimb.velocity.normalized * maxVelocity;
				}
			}

			if (active) {
				SyncTransforms(); // or put in FixedUpdate()?
			}
		}

		public void SyncTransforms()
		{
			if (ragdolling) {
				SyncBoneTransforms(ragBones, animBones);
				SyncBoneTransforms(ragHead, playerScript.cameraContainerTransform);
			}
			else {
				SyncBoneTransforms(animBones, ragBones);
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
			playerScript.freeRotationInInteractAnimation = true;

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

			parentedToShip = playerScript.isInElevator;
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
}

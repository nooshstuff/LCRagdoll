using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using GameNetcodeStuff;
using UnityEngine;

namespace Ragdoll
{
	public class GrabbablePlayer : GrabbableObject
	{
		public int bodyID = -1;

		public RagdollController ragdoll;

		public BodyPart grabPoint;

		public bool placed;

		public override void Start()
		{
			base.Start();
			if (HoarderBugAI.grabbableObjectsInMap != null && !HoarderBugAI.grabbableObjectsInMap.Contains(base.gameObject))
			{
				HoarderBugAI.grabbableObjectsInMap.Add(base.gameObject);
			}
			if (radarIcon != null) { Destroy(radarIcon.gameObject); }
		}

		public override void EquipItem()
		{
			base.EquipItem();
			//previousPlayerHeldBy = playerHeldBy;
			grabPoint.rigidbody.isKinematic = true;
		}

		public override void OnPlaceObject()
		{
			base.OnPlaceObject();
			grabPoint.rigidbody.isKinematic = false;
		}

		public override void OnDestroy()
		{
			base.OnDestroy();
		}

		public override void Update()
		{
			base.Update();
			if (ragdoll == null || NetworkManager.Singleton.ShutdownInProgress) { return; }
		}

		public override void GrabItemFromEnemy(EnemyAI enemy)
		{
			base.GrabItemFromEnemy(enemy);
			//heldByEnemy = true;
		}

		public override void DiscardItemFromEnemy()
		{
			base.DiscardItemFromEnemy();
			//heldByEnemy = false;
		}
	}
}

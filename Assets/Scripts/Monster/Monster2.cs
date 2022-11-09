using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Monster;
public class Monster2 : Monster.Monster {
	public Animator bowAnimator;

	public GameObject Arrow;
	public GameObject hand;
	public Transform arrowTransform;
    

	private float atkAnimationTime;
	private GameObject holdingArrow;
	[HideInInspector]
	public bool isArrowHolding;
	private bool isSpawnArrow;
	void Awake() {
		this.defaultStart();
		this.HP = 100;
	}
	void Start() {
		this.isArrowHolding = false;
		this.isSpawnArrow = false;
	}

	// Update is called once per frame
	void Update() {
		this.stateUpdate();
        Vector3.Distance(this.player.transform.position,this.gameObject.transform.position);
		if (this.isArrowHolding && !this.isSpawnArrow && this.isAtk) {
			this.holdingArrow = Instantiate(this.Arrow, this.arrowTransform.position, this.arrowTransform.rotation, this.hand.transform);
			this.holdingArrow.SetActive(true);
			this.isSpawnArrow = true;
		}
		if (!this.isArrowHolding && this.isSpawnArrow) {
			this.holdingArrow.transform.SetParent(null,true);
			var arrowScript = this.holdingArrow.GetComponent<Arrow>();
			arrowScript.isShooted = true;
			arrowScript.faceToPlayer(this.player);
			arrowScript.velocityInit();
			Destroy(this.holdingArrow, 5f);
			this.isSpawnArrow = false;
		}
		if (this.isAtk) {
			this.atkAnimationTime = this.mAnimatorState.normalizedTime % 1;
			this.bowAnimator.SetBool("isAtk", this.atkAnimationTime > 0.64);

		} else
			this.bowAnimator.SetBool("isAtk", false);

	}
}
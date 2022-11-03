using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Monster;

public class Monster1 : Monster.Monster {

    private float atkAnimationTime;
    private bool isPlayerGetDamage;
    void Start() {
        this.defaultStart();
        this.isPlayerGetDamage = false;
        this.HP = 100;
    }

    // Update is called once per frame
    void Update() {
        this.stateUpdate();

        if (this.isAtk) {
            this.atkAnimationTime = this.mAnimatorState.normalizedTime % 1;
            this.isAtkToPlayer = this.atkAnimationTime > 0.5 && this.atkAnimationTime < 0.625;
            if (this.atkAnimationTime < 0.5)
                this.isPlayerGetDamage = false;
        }
    }

    void OnTriggerStay(Collider other) {
        if (other.tag == "player" && this.isAtkToPlayer && !this.isPlayerGetDamage) {
            this.isPlayerGetDamage = true;
            // Player get damage
        }
    }
}

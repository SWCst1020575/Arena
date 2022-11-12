using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Monster;
public class Monster3 : Monster.Monster {
    public GameObject staffEffect;
    public GameObject castSkillPrefab;
    private bool isCasting = false;
    private ParticleSystem[] staffEffectPack;

    private float castSkillTime = 0f;
    private float castInterval = 1f;
    private RaycastHit findCastPostion;
    void Awake() {
        this.defaultStart();
        this.HP = 100;
    }
    // Start is called before the first frame update
    void Start() {
        this.staffEffectPack = this.staffEffect.GetComponentsInChildren<ParticleSystem>();
    }

    // Update is called once per frame
    void Update() {
        this.stateUpdate();
        if (this.isAtk && !this.isCasting) {
            this.isCasting = true;
            foreach (ParticleSystem iter in this.staffEffectPack)
                iter.Play();
        } else if (!this.isAtk && this.isCasting) {
            this.isCasting = false;
            foreach (ParticleSystem iter in this.staffEffectPack)
                iter.Stop();
        }
        if (this.isAtk) {
            if (Time.time > this.castSkillTime + this.castInterval) {
                this.castSkillTime = Time.time;
                Physics.Raycast(this.player.transform.position + Vector3.up * 50, -Vector3.up, out findCastPostion, 100.0f, LayerMask.GetMask("ground"));
                Instantiate(this.castSkillPrefab, this.findCastPostion.point, Quaternion.Euler(Vector3.zero));
            }
        }
    }
}

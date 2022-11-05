using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManControl : MonoBehaviour
{
	// animator recover speed
	public float animSpeed = 1.5f;
	// a smoothing setting for camera motion
	public float lookSmoother = 3.0f;
	// for Mecanim Modification
	public bool useCurves = true;
	public float useCurvesHeight = 0.5f;


	public float forwardSpeed = 7.0f;
	public float backwardSpeed = 2.0f;
	public float rotateSpeed = 2.0f;
	public float jumpPower = 3.0f;
	
	private CapsuleCollider col;
#pragma warning disable IDE0052 // Remove unread private members
    private Rigidbody rb;
#pragma warning restore IDE0052 // Remove unread private members

    private Vector3 velocity;
	
	private float orgColHight;
	private Vector3 orgVectColCenter;
	private Animator anim;                         
	private AnimatorStateInfo currentBaseState;

	private GameObject cameraObject;    // メインカメラへの参照

	// アニメーター各ステートへの参照
	static int idleState = Animator.StringToHash("Base Layer.Idle");
	static int locoState = Animator.StringToHash("Base Layer.Locomotion");
	static int jumpState = Animator.StringToHash("Base Layer.Jump");
	static int restState = Animator.StringToHash("Base Layer.Rest");

	// Start is called before the first frame update
	void Start()
    {
		anim = GetComponent<Animator>();
		
		col = GetComponent<CapsuleCollider>();
		rb = GetComponent<Rigidbody>();
		
		cameraObject = GameObject.FindWithTag("MainCamera");
		
		orgColHight = col.height;
		orgVectColCenter = col.center;
	}

    // Update is called once per frame
    void Update()
    {
        
    }

    // motion of man
    [System.Obsolete] // fixing BaseState
    void FixedUpdate()
	{
		float h = Input.GetAxis("Horizontal");              // 入力デバイスの水平軸をhで定義
		float v = Input.GetAxis("Vertical");                // 入力デバイスの垂直軸をvで定義
		anim.SetFloat("Speed", v);                          // Animator側で設定している"Speed"パラメタにvを渡す
		anim.SetFloat("Direction", h);                      // Animator側で設定している"Direction"パラメタにhを渡す
		anim.speed = animSpeed;                             // Animatorのモーション再生速度に animSpeedを設定する
		currentBaseState = anim.GetCurrentAnimatorStateInfo(0); // 参照用のステート変数にBase Layer (0)の現在のステートを設定する
		rb.useGravity = true;//ジャンプ中に重力を切るので、それ以外は重力の影響を受けるようにする



		// 以下、キャラクターの移動処理
		velocity = new Vector3(0, 0, v);        // 上下のキー入力からZ軸方向の移動量を取得
												// キャラクターのローカル空間での方向に変換
		velocity = transform.TransformDirection(velocity);
		//以下のvの閾値は、Mecanim側のトランジションと一緒に調整する
		if (v > 0.1)
		{
			velocity *= forwardSpeed;       // 移動速度を掛ける
		}
		else if (v < -0.1)
		{
			velocity *= backwardSpeed;  // 移動速度を掛ける
		}

		if (Input.GetButtonDown("Jump"))
		{   // スペースキーを入力したら

			//アニメーションのステートがLocomotionの最中のみジャンプできる
			if (currentBaseState.nameHash == locoState)
			{
				//ステート遷移中でなかったらジャンプできる
				if (!anim.IsInTransition(0))
				{
					rb.AddForce(Vector3.up * jumpPower, ForceMode.VelocityChange);
					anim.SetBool("Jump", true);     // Animatorにジャンプに切り替えるフラグを送る
				}
			}
		}


		// 上下のキー入力でキャラクターを移動させる
		transform.localPosition += velocity * Time.fixedDeltaTime;

		// 左右のキー入力でキャラクタをY軸で旋回させる
		transform.Rotate(0, h * rotateSpeed, 0);


		// 以下、Animatorの各ステート中での処理
		// Locomotion中
		// 現在のベースレイヤーがlocoStateの時
		if (currentBaseState.nameHash == locoState)
		{
			//カーブでコライダ調整をしている時は、念のためにリセットする
			if (useCurves)
			{
				resetCollider();
			}
		}
		// JUMP中の処理
		// 現在のベースレイヤーがjumpStateの時
		else if (currentBaseState.nameHash == jumpState)
		{
			//cameraObject.SendMessage ("setCameraPositionJumpView");	// ジャンプ中のカメラに変更
			// ステートがトランジション中でない場合
			if (!anim.IsInTransition(0))
			{

				// 以下、カーブ調整をする場合の処理
				if (useCurves)
				{
					// 以下JUMP00アニメーションについているカーブJumpHeightとGravityControl
					// JumpHeight:JUMP00でのジャンプの高さ（0〜1）
					// GravityControl:1⇒ジャンプ中（重力無効）、0⇒重力有効
					float jumpHeight = anim.GetFloat("JumpHeight");
					float gravityControl = anim.GetFloat("GravityControl");
					if (gravityControl > 0)
						rb.useGravity = false;  //ジャンプ中の重力の影響を切る

					// レイキャストをキャラクターのセンターから落とす
					Ray ray = new Ray(transform.position + Vector3.up, -Vector3.up);
					RaycastHit hitInfo = new RaycastHit();
					// 高さが useCurvesHeight 以上ある時のみ、コライダーの高さと中心をJUMP00アニメーションについているカーブで調整する
					if (Physics.Raycast(ray, out hitInfo))
					{
						if (hitInfo.distance > useCurvesHeight)
						{
							col.height = orgColHight - jumpHeight;          // 調整されたコライダーの高さ
							float adjCenterY = orgVectColCenter.y + jumpHeight;
							col.center = new Vector3(0, adjCenterY, 0); // 調整されたコライダーのセンター
						}
						else
						{
							// 閾値よりも低い時には初期値に戻す（念のため）					
							resetCollider();
						}
					}
				}
				// Jump bool値をリセットする（ループしないようにする）				
				anim.SetBool("Jump", false);
			}
		}
		// IDLE中の処理
		// 現在のベースレイヤーがidleStateの時
		else if (currentBaseState.nameHash == idleState)
		{
			//カーブでコライダ調整をしている時は、念のためにリセットする
			if (useCurves)
			{
				resetCollider();
			}
			// スペースキーを入力したらRest状態になる
			if (Input.GetButtonDown("Jump"))
			{
				anim.SetBool("Rest", true);
			}
		}
		// REST中の処理
		// 現在のベースレイヤーがrestStateの時
		else if (currentBaseState.nameHash == restState)
		{
			//cameraObject.SendMessage("setCameraPositionFrontView");		// カメラを正面に切り替える
			// ステートが遷移中でない場合、Rest bool値をリセットする（ループしないようにする）
			if (!anim.IsInTransition(0))
			{
				anim.SetBool("Rest", false);
			}
		}
	}

	void resetCollider()
	{
		// コンポーネントのHeight、Centerの初期値を戻す
		col.height = orgColHight;
		col.center = orgVectColCenter;
	}
}

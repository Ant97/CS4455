﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CanvasGroup))]

public class RentAGuardAI : MonoBehaviour {

	public bool isFrozen = false;
    public float freezeTimer = 0f;
    private Vector3 preFrozenVel;
    // private Rigidbody rb;

	public UnityEngine.AI.NavMeshAgent agent;
	private GameObject player;
	private RaycastHit hit;
	private Vector3 distVec;

	public float visionDist = 10;
	public float visionAngle = 30;

	private float timer = 0;
	private bool walkingBackDestSet = false;
	private Vector3 initPos;
	private Quaternion initRot;

	private bool gameOver = false;
	private bool knockedOut = true;

    private Animator anim;
    private Animator playerAnim;
    CanvasGroup canvasGroup;
    //GameObject GameOverMenu;

    public enum AIState
	{
		InitState,
		Stationary,
		Chase,
		LoseTarget,
		WalkBack
	};
	public AIState currState;

    private void Awake()
    {
        canvasGroup = GameObject.Find("GameOverCanvas").GetComponent<CanvasGroup>();
        HideGameOverMenu();
        //GameOverMenu = GameObject.Find("GameOverMenu");
        //GameOverMenu.SetActive(false);
    }

    // Use this for initialization
    void Start () {
		// rb = GetComponent<Rigidbody>();
		agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        //player = GameObject.FindGameObjectWithTag("Player");
        initPos = transform.position;
		initRot = transform.rotation;

        player = GameObject.FindWithTag("Player");
        playerAnim = player.GetComponent<Animator>();
        anim = this.gameObject.GetComponent<Animator>();
    }
	
	// Update is called once per frame
	void Update () {
		if (gameOver || knockedOut) {
			return;
		}
		/*if (Input.GetKeyDown(KeyCode.F)) {
			Freeze();
		}
		if (isFrozen) {
			freezeTimer += Time.deltaTime;
			if (freezeTimer > 5) {
				agent.Resume();
				agent.velocity = preFrozenVel;
				// rb.constraints = RigidbodyConstraints.None;
				freezeTimer = 0;
				isFrozen = false;
			} else {
				return;
			}
		}*/

		if (CheckForPlayer()) {
            anim.SetBool("isRun", true);
            currState = AIState.Chase;
			timer = 0; //used to check time since player was last spotted, along with time since chase stopped (no need to have two timers)
		} else {
			timer += Time.deltaTime;
		}

		switch (currState) {
			case AIState.InitState:
				initPos.y = transform.position.y;
				currState = AIState.Stationary;
				break;

			case AIState.Stationary:
				//TODO: loop through idle animation
				break;

			case AIState.Chase:
				agent.SetDestination(player.transform.position); //change "Player" name as appropriate
				if (agent.remainingDistance < 1f && !agent.pathPending) {
                    //TODO: attack
                    //print("there will eventually have been an attack here");
                    //Time.timeScale = 0f;
                    //GameObject.Find("PauseCanvas").GetComponent<PauseMenu>().ShowPauseMenu();
                    //gameOver = true;
                    anim.SetBool("isAttack", true);
                    playerAnim.SetBool("isDeath", true);
                    //print("there will eventually have been an attack here");
                    StartCoroutine(ShowGameOverMenu());
                }
				if (timer >= 1) {
					currState = AIState.LoseTarget;
					timer = 0;
				}
				break;

			case AIState.LoseTarget:
				agent.Stop();
				agent.ResetPath();
				//TODO: loop through confused animation
				if (timer >= 3) {
					walkingBackDestSet = false;
					currState = AIState.WalkBack;
				}
				break;

			case AIState.WalkBack:
				if (transform.position != initPos) {
					if (!walkingBackDestSet) {
						agent.Stop();
						agent.ResetPath();
						agent.SetDestination(initPos);
						walkingBackDestSet = true;
					} else {
						print(initPos);
					}
				} else if (transform.rotation != initRot) {
					transform.rotation = Quaternion.Lerp(transform.rotation, initRot, Time.deltaTime * 3);
				} else {
					agent.Stop();
					agent.ResetPath();
					walkingBackDestSet = false;
					currState = AIState.Stationary;
				}
				break;

			default:
				break;
		}
	}

	/*public void Freeze() {
		preFrozenVel = agent.velocity;
		isFrozen = true;
		agent.Stop();
		agent.velocity = Vector3.zero;
		// rb.constraints = RigidbodyConstraints.FreezeAll;
	}*/

	bool CheckForPlayer() {
		distVec = player.transform.position - transform.position;
		return (distVec.magnitude < visionDist
			&& Vector3.Angle(transform.TransformDirection(Vector3.forward), distVec) < visionAngle
			&& Physics.Raycast(transform.position, distVec, out hit, visionDist) && hit.collider.gameObject.tag == "Player");
	}

    IEnumerator ShowGameOverMenu()
    {
        yield return new WaitForSeconds(3f);

        //GameOverMenu.SetActive(true);

        //GameStart.prev = SceneManager.GetActiveScene().name;
        //SceneManager.LoadScene("GameOverScene");
        //SceneManager.LoadScene("GameOverScene");

        Cursor.lockState = CursorLockMode.None;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;
        Time.timeScale = 0f;
    }

    public void HideGameOverMenu()
    {
        Cursor.lockState = CursorLockMode.Locked;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0f;
        Time.timeScale = 1f;
    }

    private void OnCollisionEnter(Collision collision)
    {
    	if (collision.collider.tag == "Cube" || (collision.collider.name == "End" && collision.collider.GetComponent<Animator>().GetBool("Out") == true)) {
			knockedOut = true;
    	}
    }
}

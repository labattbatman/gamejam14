﻿using UnityEngine;
using System.Collections;


public class NonPhysicsPlayerController : MonoBehaviour
{
	// movement config
	public float gravity = -25f;
	public float runSpeed = 8f;
	public float groundDamping = 20f; // how fast do we change direction? higher means faster
	public float inAirDamping = 5f;
	public float jumpHeight = 3f;

	[HideInInspector]
	private float normalizedHorizontalSpeed = 0;

	private CharacterController2D _controller;
	private Animator _animator;
	private RaycastHit2D _lastControllerColliderHit;
	private Vector3 _velocity;

	public string PNumber;

	public GameObject TotemPart1;
	public GameObject TotemPart2;
	public GameObject TotemPart3;
	public GameObject TotemPart4;
	public GameObject TotemPart5;
	public GameObject TotemPart6;


	private int nbRoche, nbBois,nbOs,nbMetal,nbPlume,nbLiane;
	private bool putRoche, putBois, putOs, putMetal, putPlume, putLiane;
	private int nbParts = 6;
	
	private int []partPosition = new int[6];
	
	private int partNumber = 6;
	
	private bool stunned = false;
	private float stunnedTimer = 0.0f;

	private float speedTimer = 0.0f;
	private float baseSpeed;
	
	private float jumpTimer = 0.0f;
	private float baseJumpHeight;

	public bool IsStunned(){
		return stunned;
	}

	void Awake()
	{
		_animator = GetComponent<Animator>();
		_controller = GetComponent<CharacterController2D>();

		// listen to some events for illustration purposes
		_controller.onControllerCollidedEvent += onControllerCollider;
		_controller.onTriggerEnterEvent += onTriggerEnterEvent;
		_controller.onTriggerExitEvent += onTriggerExitEvent;
	}


	#region Event Listeners

	void onControllerCollider( RaycastHit2D hit )
	{
		// bail out on plain old ground hits cause they arent very interesting
		if( hit.normal.y == 1f )
			return;

		// logs any collider hits if uncommented. it gets noisy so it is commented out for the demo
		//Debug.Log( "flags: " + _controller.collisionState + ", hit.normal: " + hit.normal );
	}


	void onTriggerEnterEvent( Collider2D col )
	{
		if (col.gameObject.tag == "Totem")
			checkDroppingItemOnTotem ();
		Debug.Log( "onTriggerEnterEvent: " + col.gameObject.name );
	}


	void onTriggerExitEvent( Collider2D col )
	{
		Debug.Log( "onTriggerExitEvent: " + col.gameObject.name );
	}

	#endregion


	// the Update loop contains a very simple example of moving the character around and controlling the animation
	void Update()
	{

		if(stunned)
			stunnedTimer -= Time.deltaTime;
		
		if(stunnedTimer <= 0.0f){
			_animator.SetTrigger("NotStunned");
			stunned = false;
		}

		if(!stunned)
			HandleMovement();

		VerifySpeedTimer();
		VerifyJumpTimer();
	}

	void VerifyJumpTimer()
	{
		if(jumpTimer > 0.0f){
			jumpTimer -= Time.deltaTime;
			if(jumpTimer <= 0)
			{
				jumpHeight = baseJumpHeight;
				jumpTimer = 0.0f;
			}
			
		}
	}

	void HandleMovement()
	{
		if (Input.GetAxis("LeftTrigger_"+ PNumber) > 0.5)
		{
			gameObject.GetComponent<PlayerInventoryPowerUp>().ConsumePowerUps();
		}
		if(stunned)
			stunnedTimer -= Time.deltaTime;
		
		if(stunnedTimer <= 0.0f){
			_animator.SetTrigger("NotStunned");
			stunned = false;
		}
		
		// grab our current _velocity to use as a base for all calculations
		_velocity = _controller.velocity;
		
		if( _controller.isGrounded )
			_velocity.y = 0;
		
		if( Input.GetAxisRaw ("LeftAnalogX_"+PNumber)  > 0.5 )
		{
			normalizedHorizontalSpeed = 1;
			if( transform.localScale.x < 0f )
				transform.localScale = new Vector3( -transform.localScale.x, transform.localScale.y, transform.localScale.z );


		}
		else if( Input.GetAxisRaw ("LeftAnalogX_"+PNumber) < -0.05 )
		{
			normalizedHorizontalSpeed = -1;
			if( transform.localScale.x > 0f )
				transform.localScale = new Vector3( -transform.localScale.x, transform.localScale.y, transform.localScale.z );
		}
		else
		{
			normalizedHorizontalSpeed = 0;

		}
		
		
		// we can only jump whilst grounded
		if( _controller.isGrounded && Input.GetButtonDown("Abutton_"+PNumber) )
		{
			_velocity.y = Mathf.Sqrt( 2f * jumpHeight * -gravity );
			_animator.SetTrigger("Jump");
		}
		
		
		// apply horizontal speed smoothing it
		var smoothedMovementFactor = _controller.isGrounded ? groundDamping : inAirDamping; // how fast do we change direction?
		_velocity.x = Mathf.Lerp( _velocity.x, normalizedHorizontalSpeed * runSpeed, Time.deltaTime * smoothedMovementFactor );
		
		// apply gravity before moving
		_velocity.y += gravity * Time.deltaTime;

		_animator.SetFloat("Speed", Mathf.Abs (_velocity.x));

		_controller.move( _velocity * Time.deltaTime );

	}
	
	public void ApplyBonusJump(float bonus, float time)
	{
		jumpHeight *= bonus;
		SetJumpTimer(time);
	}
	
	void SetJumpTimer(float time)
	{
		jumpTimer = time;
	}

	void VerifySpeedTimer()
	{
		if(speedTimer > 0.0f){
			speedTimer -= Time.deltaTime;
			if(speedTimer <= 0)
			{
				runSpeed = baseSpeed;
				speedTimer = 0.0f;
			}
			
		}
	}
	
	public void ApplyBonusSpeed(float bonus, float time)
	{
		runSpeed *= bonus;
		SetSpeedTimer(time);
	}
	
	void SetSpeedTimer(float time)
	{
		speedTimer = time;
	}

	void checkDroppingItemOnTotem()
	{
		if(Input.GetButtonDown("Xbutton_"+PNumber))
	   	{
			gameObject.GetComponent<PlayerInventoryMaterials>().UseItem();
			
			//if(player got all items to add a part)
			if((nbRoche==0)||(putRoche))
			{
				if((nbBois==0)||(putBois))
				{
					if((nbOs==0)||(putOs))
					{
						if((nbMetal==0)||(putMetal))
						{
							if((nbPlume==0)||(putPlume))
							{
								if(nbLiane!=0)
								{	
									if(!putLiane)
									{
										Instantiate(TotemPart1, SetPosition(false), Quaternion.identity);
										putLiane = true;
										removeOne("liane");
										nbParts--;
									}
								}
							}
							else
							{	
								if(!putPlume)
								{	
									Instantiate(TotemPart2, SetPosition(true), Quaternion.identity);
									putPlume = true;
									removeOne("plume");
									nbParts--;
								}
							}
						}
						else
						{	
							if(!putMetal)
							{
								Instantiate(TotemPart3, SetPosition(false), Quaternion.identity);
								putMetal = true;
								removeOne("metal");
								nbParts--;
							}
						}
					}
					else
					{	
						if(!putOs)
						{
							Instantiate(TotemPart4, SetPosition(false), Quaternion.identity);
							putOs = true;
							removeOne("os");
							nbParts--;
						}
					}
				}
				else
				{	
					if(!putBois)
					{
						Instantiate(TotemPart5, SetPosition(false), Quaternion.identity);
						putBois = true;
						removeOne("bois");
						nbParts--;
					}
				}
			}
			else
			{	
				if(!putRoche)
				{
					Instantiate(TotemPart6, SetPosition(false), Quaternion.identity);
					putRoche = true;
					removeOne("roche");
					nbParts--;
				}
			}
			
			if((putRoche)&&(putBois)&&(putOs)&&(putMetal)&&(putPlume)&&(putLiane))
			{
				
				GameObject part;
				part = (GameObject)Instantiate(TotemPart6, GetPosition ("Roche"), Quaternion.identity);
				part.gameObject.transform.localScale = FlipTotem(part.gameObject.transform.localScale);
				part = (GameObject)Instantiate(TotemPart5, GetPosition ("Bois"), Quaternion.identity);
				part.gameObject.transform.localScale = FlipTotem(part.gameObject.transform.localScale);
				part = (GameObject)Instantiate(TotemPart4, GetPosition ("Os"), Quaternion.identity);
				part.gameObject.transform.localScale = FlipTotem(part.gameObject.transform.localScale);
				part = (GameObject)Instantiate(TotemPart3, GetPosition ("Metal"), Quaternion.identity);
				part.gameObject.transform.localScale = FlipTotem(part.gameObject.transform.localScale);
				part = (GameObject)Instantiate(TotemPart2, GetPosition ("Plume"), Quaternion.identity);
				part.gameObject.transform.localScale = FlipTotem(part.gameObject.transform.localScale);
				part = (GameObject)Instantiate(TotemPart1, GetPosition ("Liane"), Quaternion.identity);
				part.gameObject.transform.localScale = FlipTotem(part.gameObject.transform.localScale);
			}
		}
	}

	 Vector3 FlipTotem(Vector3 currentScale){

		Vector3 scale = currentScale;
		scale.x *= -1;
		return scale;
	}

	public void GetItems(int roche, int bois, int os, int metal, int plume, int liane)
	{
		nbRoche = roche;
		nbBois = bois;
		nbOs = os;
		nbMetal = metal;
		nbPlume = plume;
		nbLiane = liane;
	}
	
	void removeOne(string objet)
	{
		switch(objet)
		{
		case "roche": nbRoche--; gameObject.GetComponent<PlayerInventoryMaterials>().setRoche(nbRoche);break;
		case "bois":nbBois--; gameObject.GetComponent<PlayerInventoryMaterials>().setRoche(nbBois);break;
		case "os":nbMetal--; gameObject.GetComponent<PlayerInventoryMaterials>().setOs(nbOs);break;
		case "metal":nbMetal--; gameObject.GetComponent<PlayerInventoryMaterials>().setRoche(nbMetal);break;
		case "plume":nbPlume--; gameObject.GetComponent<PlayerInventoryMaterials>().setRoche(nbPlume);break;
		case "liane":nbLiane--; gameObject.GetComponent<PlayerInventoryMaterials>().setRoche(nbLiane);break;
		}
	}
	
	Vector2 SetPosition(bool isItTheBird)
	{
		switch (nbParts) 
		{
		case 1 : 
			if(isItTheBird)
				return new Vector2(TotemPart2.transform.position.x,TotemPart1.transform.position.y); 
			else 
				return TotemPart1.transform.position; break;
		case 2 : 
			if(isItTheBird)
				return TotemPart2.transform.position;
			else 
				return new Vector2(TotemPart1.transform.position.x,TotemPart2.transform.position.y); break;
		case 3 : 
			if(isItTheBird)
				return new Vector2(TotemPart2.transform.position.x,TotemPart3.transform.position.y); 
			else 
				return TotemPart3.transform.position; break;
		case 4 : 
			if(isItTheBird)
				return new Vector2(TotemPart2.transform.position.x,TotemPart4.transform.position.y); 
			else 
				return TotemPart4.transform.position; break;
		case 5 :  
			if(isItTheBird)
				return new Vector2(TotemPart2.transform.position.x,TotemPart5.transform.position.y); 
			else 
				return TotemPart5.transform.position; break;
		case 6 :  
			if(isItTheBird)
				return new Vector2(TotemPart2.transform.position.x,TotemPart6.transform.position.y); 
			else 
				return TotemPart6.transform.position; break;
		default: return new Vector2(5,5); break;
		}
	}
	
	Vector2 GetPosition(string tag)
	{
		switch(tag)
		{
		case "Roche": return new Vector2(-GameObject.FindWithTag("Roche").transform.position.x,GameObject.FindWithTag("Roche").transform.position.y); break;
		case "Bois": return new Vector2(-GameObject.FindWithTag("Bois").transform.position.x,GameObject.FindWithTag("Bois").transform.position.y); break;
		case "Os": return new Vector2(-GameObject.FindWithTag("Os").transform.position.x,GameObject.FindWithTag("Os").transform.position.y);break;
		case "Metal": return new Vector2(-GameObject.FindWithTag("Metal").transform.position.x,GameObject.FindWithTag("Metal").transform.position.y);break;
		case "Plume": return new Vector2(-GameObject.FindWithTag("Plume").transform.position.x,GameObject.FindWithTag("Plume").transform.position.y);break;
		case "Liane": return new Vector2(-GameObject.FindWithTag("Liane").transform.position.x,GameObject.FindWithTag("Liane").transform.position.y);break;
		default : return new Vector2(5,5);break;
		}
	}
	
}

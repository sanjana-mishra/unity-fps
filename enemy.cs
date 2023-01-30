using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class enemy : MonoBehaviour

{
    Animator animator;
    public float animation_speed;
    public GameObject[] targets;
    int i = 0;
    public GameObject player;
    public GameObject bulletHole;
    public GameObject bulletWound;
    public GameObject muzzleFlash;
    public GameObject shotSound;
    public GameObject end, start;
    public GameObject gun;
    public Vector3 player_direction;
    public bool isDead;
    public bool isDetect;
    public float maxHealth = 100.0f;
    public float health;
    float gunShotTime = 0.1f;
    float dist_player;
    float angle_player;


    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();

        isDetect = false;
        animation_speed = 0.1f;

    }

    // Update is called once per frame
    void Update()
    {
        player_direction = (player.transform.position - transform.position).normalized;
        
        isDetect = DetectPlayer();

        if(!isDead)
        {
            if(isDetect)
            {
                animator.SetBool("walk", false);

                if(dist_player <= 10)
                {
                    animator.SetBool("run", false);
                    transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(player.transform.position - transform.position), Time.deltaTime*2.0f);
                    Shoot();
                } 
                else if (dist_player > 10)
                {
                    animator.SetBool("fire", false);  
                    animator.SetBool("run", true);
                    transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(player.transform.position - transform.position), Time.deltaTime*2.0f);
                }
            }
            else
            {
                animator.SetBool("run", false);
                animator.SetBool("fire", false);        
                animator.SetBool("walk", true);  
                FollowTargets();
            }
        }
        else
        {
            animator.SetBool("run", false);
            animator.SetBool("fire", false);        
            animator.SetBool("walk", false);              
        }
        
    }

    void FollowTargets()
    {
        int num_targets = targets.Length;
        Vector3 pos = new Vector3(targets[i].transform.position.x, transform.position.y, targets[i].transform.position.z);
        float dist = Vector3.Distance(pos, transform.position);
        transform.LookAt(pos);
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(targets[i].transform.position - transform.position), Time.deltaTime);
                
        if (dist < 1.0) {
            i = (i + 1) % num_targets;
        }

    }

    public void Being_shot(float damage)
    {
    
        if (!isDead)
        {
            health = health-damage;
            Debug.Log(health);
            if (health <= 0)
            {
                isDead = true;
                Destroy(gameObject.GetComponent<CharacterController>());
                animator.SetTrigger("dead");

                //separate gun
                gun.AddComponent<Rigidbody>();
                BoxCollider gunCollider = gun.AddComponent<BoxCollider>();
            }
        }
    }
    
    public bool DetectPlayer()
    {        
        dist_player = Vector3.Distance(transform.position, player.transform.position);
        angle_player = Vector3.Angle(transform.forward, player_direction);
        if (angle_player <= 40 && dist_player <= 15)
        {            
            return true;
        }
        else
        {
            return false;
        }
    }

    void FollowPlayer()
    {
        Vector3 playerPos = new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z);
        transform.LookAt(playerPos);
        animator.SetFloat("animation_speed", animation_speed);
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(playerPos - transform.position), Time.deltaTime*2.0f);
    }

    void Shoot()
    {
        if (gunShotTime >= 0.0f)
        {
            gunShotTime = gunShotTime - Time.deltaTime;
        }

        if (gunShotTime <= 0)
        {
            animator.SetBool("fire", true);
            Tuple<bool, RaycastHit> shot = shotDetection();
            addEffects(shot.Item1, shot.Item2);
            gunShotTime = 0.5f;
        }

    }

    Tuple<bool, RaycastHit> shotDetection()
    {
        RaycastHit rayHit;
        int e = UnityEngine.Random.Range(0,101);
        bool checkShot;
        if(e <= 20)
        {            
            checkShot = Physics.Raycast(end.transform.position, player.transform.Find("swat:Hips").position-end.transform.position, out rayHit, 100.0f);
        }
        else
        {
            Vector3 strayShot = new Vector3(UnityEngine.Random.Range(0, 0.5f), UnityEngine.Random.Range(0, 0.5f), UnityEngine.Random.Range(0, 0.5f));
            checkShot = Physics.Raycast(end.transform.position, ((player.transform.Find("swat:Hips").position-end.transform.position) + strayShot).normalized, out rayHit, 100.0f);
        }
        Debug.Log(rayHit.collider.tag);
        return Tuple.Create(checkShot, rayHit);
    }

    void addEffects (bool checkShot, RaycastHit rayHit)
    {
        //muzzle flash
        GameObject muzzleFlashObject = Instantiate(muzzleFlash, end.transform.position, end.transform.rotation);
        muzzleFlashObject.GetComponent<ParticleSystem>().Play();
        Destroy(muzzleFlashObject, 0.1f);

        //gunshot sound
        Destroy((GameObject) Instantiate(shotSound, transform.position, transform.rotation), 1.0f);

        //bullet shots
        if(checkShot)
        {

            if(rayHit.collider.tag == "environment")
            {
                GameObject bulletHoleObject = Instantiate(bulletHole, rayHit.point + rayHit.collider.transform.up*0.01f, rayHit.collider.transform.rotation);
                Destroy(bulletHoleObject, 2.0f);
            }
            else if(rayHit.collider.tag == "Player")
            {
                GameObject bulletHoleObject = Instantiate(bulletWound, rayHit.point + rayHit.collider.transform.up*0.01f, rayHit.collider.transform.rotation);
                Destroy(bulletHoleObject, 2.0f);
                player.GetComponent<Gun>().Being_shot(player.GetComponent<Gun>().maxHealth * 0.2f);
            }
            else
            {
                GameObject bulletHoleObject = Instantiate(bulletHole, rayHit.point + rayHit.collider.transform.up*0.01f, rayHit.collider.transform.rotation);
                Destroy(bulletHoleObject, 2.0f);                
            }
        }
    }

}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;


public class Gun : MonoBehaviour {

    public bool gameState = true;

    public GameObject end, start; // The gun start and end point
    public GameObject gun;
    public Animator animator;
    
    public GameObject spine;
    public GameObject handMag;
    public GameObject gunMag;

    float gunShotTime = 0.1f;
    float gunReloadTime = 1.0f;
    Quaternion previousRotation;
    public float maxHealth = 100.0f;
    public float health;
    public bool isDead;
    public Vector3 player_direction;
    public GameObject bulletWound;

    public Text magBullets;
    public Text remainingBullets;
    public Text healthShow;
    // public Text remaininghealthShow;


    int magBulletsVal = 30;
    int remainingBulletsVal = 90;
    int magSize = 30;
    public GameObject headMesh;
    public static bool leftHanded { get; private set; }

    public GameObject bulletHole;
    public GameObject muzzleFlash;
    public GameObject shotSound;

    // Use this for initialization
    void Start() {
        //headMesh.GetComponent<SkinnedMeshRenderer>().enabled = false; // Hiding player character head to avoid bugs :)
    }

    // Update is called once per frame
    void Update() {
        
        // Cool down times
        if (gunShotTime >= 0.0f)
        {
            gunShotTime -= Time.deltaTime;
        }
        if (gunReloadTime >= 0.0f)
        {
            gunReloadTime -= Time.deltaTime;
        }


        if ((Input.GetMouseButtonDown(0) || Input.GetMouseButton(0)) && gunShotTime <= 0 && gunReloadTime <= 0.0f && magBulletsVal > 0 && !isDead)
        { 
            Tuple <bool, RaycastHit> shotD = shotDetection(); // Should be completed

            addEffects(shotD.Item1, shotD.Item2 ); // Should be completed

            animator.SetBool("fire", true);
            gunShotTime = 0.5f;
            
            // Instantiating the muzzle prefab and shot sound
            
            magBulletsVal = magBulletsVal - 1;
            if (magBulletsVal <= 0 && remainingBulletsVal > 0)
            {
                animator.SetBool("reloadAfterFire", true);
                gunReloadTime = 2.5f;
                Invoke("reloaded", 2.5f);
            }
        }
        else
        {
            animator.SetBool("fire", false);
        }

        if ((Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.R)) && gunReloadTime <= 0.0f && gunShotTime <= 0.1f && remainingBulletsVal > 0 && magBulletsVal < magSize && !isDead )
        {
            animator.SetBool("reload", true);
            gunReloadTime = 2.5f;
            Invoke("reloaded", 2.0f);
        }
        else
        {
            animator.SetBool("reload", false);
        }
        updateText();

    }
 

    public void Being_shot(float damage) // getting hit from enemy
    {
        if (!isDead)
        {
            health = health-damage;
            Debug.Log(health);
            if (health <= 0)
            {
                health = 0;            
                isDead = true;
                Destroy(gameObject.GetComponent<CharacterController>());
                animator.SetBool("dead", true);
                GameOver();
            }
        }
    }

    public void ReloadEvent(int eventNumber) // appearing and disappearing the handMag and gunMag
    {
        if(eventNumber==1)
        {
            handMag.GetComponent<SkinnedMeshRenderer>().enabled =true;
            gunMag.GetComponent<SkinnedMeshRenderer>().enabled =false;
        }
        if(eventNumber==2)
        {
            handMag.GetComponent<SkinnedMeshRenderer>().enabled =false;
            gunMag.GetComponent<SkinnedMeshRenderer>().enabled =true;
        }
        
    }

    public void GameOver()
    {
        gameState = false;
        Invoke("RestartGame", 10.0f);

    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

    }

    void reloaded()
    {
        int newMagBulletsVal = Mathf.Min(remainingBulletsVal + magBulletsVal, magSize);
        int addedBullets = newMagBulletsVal - magBulletsVal;
        magBulletsVal = newMagBulletsVal;
        remainingBulletsVal = Mathf.Max(0, remainingBulletsVal - addedBullets);
        animator.SetBool("reloadAfterFire", false);
    }

    void updateText()
    {
        magBullets.text = magBulletsVal.ToString() ;
        remainingBullets.text = remainingBulletsVal.ToString();

        healthShow.text = health.ToString();
    }

    Tuple <bool, RaycastHit> shotDetection() // Detecting the object which player shot 
    {
        //bullet holes
        RaycastHit rayHit;
        int layerMask =1<<8;
        layerMask =~layerMask;

        bool checkShot = Physics.Raycast(end.transform.position, (end.transform.position - start.transform.position).normalized, out rayHit, 100.0f, layerMask);
       
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
            else if(rayHit.collider.tag == "enemy")
            {
                GameObject bulletHoleObject = Instantiate(bulletWound, rayHit.point + rayHit.collider.transform.up*0.01f, rayHit.collider.transform.rotation);
                Destroy(bulletHoleObject, 2.0f);
                rayHit.collider.GetComponentInParent<enemy>().Being_shot(rayHit.collider.GetComponentInParent<enemy>().maxHealth * 0.2f);
            }
            else
            {
                GameObject bulletHoleObject = Instantiate(bulletHole, rayHit.point + rayHit.collider.transform.up*0.01f, rayHit.collider.transform.rotation);
                Destroy(bulletHoleObject, 2.0f);                
            }
        }
    }

    public void RefillAmmo()
    {
        remainingBulletsVal = 90;
        magBulletsVal = magSize;
    }
    
}
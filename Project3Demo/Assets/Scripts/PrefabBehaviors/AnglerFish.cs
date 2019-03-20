﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnglerFish : MonoBehaviour {

    public bool playerDetected;
    public bool collidedWithPlayer;

    [SerializeField] float chaseCooldown;
    
    [SerializeField] float moveTime;

    [SerializeField] float stunTime;

    [SerializeField] float windTime;

    [SerializeField] float chaseTime;
    
    //These are different from lampreywaypoints in which they are not tagged
    [SerializeField] List<GameObject> patrolPoint;

    Vector3 currentPos;

    bool movingFinished = true;
    bool chaseFinished = true;
    bool stunned;
    bool winding;

    GameObject diver;

    GameObject anglerLight;

    int pointIndex = 0;

    void Start(){
        currentPos = transform.position;
        diver = GameObject.FindGameObjectWithTag("Diver");
        anglerLight = transform.Find("Spot Light").gameObject;
    }
    
    void Update(){
        if(stunned || winding){
            return;
        }
        if(collidedWithPlayer){
            ResetCoroutines();
            collidedWithPlayer = false;
            winding = true;
            StartCoroutine(WindBack());
        }
        else if(playerDetected){
            //Chase after player
            StartCoroutine(ChaseTimer());
            if(Vector3.Distance(diver.transform.position + (diver.transform.up * 2), this.transform.position) <= .3){
                collidedWithPlayer = true;
            }
            if(chaseFinished){
                ResetCoroutines();
                chaseFinished = false;
                StartCoroutine(ChaseAtPlayer());
            }
        }
        else if(movingFinished){
            //Pursue the waypoints
            ResetCoroutines();
            movingFinished = false;
            StartCoroutine(MoveBetweenPoints());
        }
    }

    void ResetCoroutines(){
        StopAllCoroutines();
        chaseFinished = true;
        movingFinished = true;
    }

    IEnumerator MoveBetweenPoints(){
        currentPos = transform.position;
        for(int i = pointIndex; i < patrolPoint.Count; i++){
            pointIndex = i;
            float elapsedTime = 0.0f;
            while(elapsedTime < moveTime){
                if(playerDetected){
                    yield return null;
                }
                transform.LookAt(patrolPoint[i].transform);
                transform.position = Vector3.Lerp(currentPos, patrolPoint[i].transform.position, (elapsedTime / moveTime));
                elapsedTime += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
            currentPos = transform.position;
        }
        pointIndex = 0;
        movingFinished = true;
    }

    IEnumerator WindBack(){
        currentPos = transform.position;
        Vector3 diverPos = diver.transform.position + (diver.transform.up * 1.8f);
        float elapsedTime = 0.0f;
        while(elapsedTime < windTime){
            transform.position = Vector3.Lerp(currentPos, diverPos - (transform.forward * 5), (elapsedTime / windTime));
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        currentPos = transform.position;
        winding = false;
    }

    IEnumerator ChaseAtPlayer(){
        currentPos = transform.position;
        float elapsedTime = 0.0f;
        while(elapsedTime < chaseTime){
            if(!playerDetected){
                yield return null;
            }
            //From the unity docs. Changes rotation of y and nothing else
            Vector3 relativePos = diver.transform.position - transform.position;
            
            Quaternion rotation = Quaternion.LookRotation(relativePos, Vector3.up);
            transform.rotation = new Quaternion(transform.rotation.x, rotation.y, transform.rotation.z, transform.rotation.w);

            transform.position = Vector3.Lerp(currentPos, diver.transform.position + (diver.transform.up * 2), (elapsedTime / chaseTime));
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        currentPos = transform.position;
        chaseFinished = true;
    }

    IEnumerator ChaseTimer(){
        yield return new WaitForSeconds(chaseCooldown);
        playerDetected = false;
    }

    void OnCollisionEnter(Collision col){
        if(col.gameObject.tag == "Flare"){
            stunned = true;
            StopAllCoroutines();
            StartCoroutine(Stun());
        }
    }

    IEnumerator Stun(){
        anglerLight.SetActive(false);
        yield return new WaitForSeconds(stunTime);
        anglerLight.SetActive(true);
        stunned = false;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshTrail : MonoBehaviour
{
    public float activeTime = 2f;               //잔상 효과 지속 시간
    public MovementInput moveScript;            //캐릭터의 움직임을 제어하는 스크립트
    public float speedBoost = 6;                //잔상 효과 사용시 속도 증가량
    public Animator animator;                   //캐릭터의 애니메이션을 제어하는 컴포넌트
    public float animSpeedBoost = 1.5f;         //잔상 효과 사용 시 애니메이션 속도 증가량

    [Header("Mesh Releted")]                    //메시(3D 모델) 관련 설정
    public float meshRefreshRate = 0.1f;        //잔상이 생성되는 시간 간격
    public float meshDestroyDelay = 3.0f;       //생성된 잔상이 사라지는 데 걸리는 시간
    public Transform positionToSpawn;           //잔상이 생성될 위치

    [Header("Shader Releted")]                  //셰이더 관련 설정
    public Material mat;                        //잔상에 적용될 재질
    public string shaderVarRef;                 //셰이더에서 사용할 변수 이름 (Alpha)
    public float shaderVarRate = 0.1f;          //셰이더 효과의 변화 속도
    public float shaderVarRefreshRate = 0.05f;  //셰이더 효과가 업데이트 되는 시간 간격

    private SkinnedMeshRenderer[] skinnedRenderer;          //캐릭터의 3D 모델을 렌더링 하는 컴포넌트
    private bool isTrailActive;                             //현재 잔상 효과가 활성화되어 있는지 확인하는 변수

    private float normalSpeed;                              //원래 이동속도를 저장하는 변수
    private float normalAnimSpeed;                          //원래 애니메이션 속도를 저장하는 변수

    IEnumerator AnimateMaterIalFloat(Material m, float valueGoal, float rate, float refreshRate)
    {
        float valueToAnimate = m.GetFloat(shaderVarRef);

        while (valueToAnimate > valueGoal)
        {
            valueToAnimate -= rate;
            m.SetFloat(shaderVarRef, valueToAnimate);
            yield return new WaitForSeconds(refreshRate);
        }
    }

    IEnumerator ActivateTrail(float timeActivated)
    {
        normalSpeed = moveScript.movementSpeed;
        moveScript.movementSpeed = speedBoost;

        normalAnimSpeed = animator.GetFloat("animSpeed");
        animator.SetFloat("animSpeed", animSpeedBoost);

        while (timeActivated > 0)
        {
            timeActivated -= meshRefreshRate;

            if(skinnedRenderer == null)
                skinnedRenderer = positionToSpawn.GetComponentsInChildren<SkinnedMeshRenderer>();

            for(int i = 0; i < skinnedRenderer.Length; i++)
            {
                GameObject gObj = new GameObject();
                gObj.transform.SetPositionAndRotation(positionToSpawn.position, positionToSpawn.rotation);

                MeshRenderer mr = gObj.AddComponent<MeshRenderer>();
                MeshFilter mf = gObj.AddComponent<MeshFilter>();

                Mesh m = new Mesh();
                skinnedRenderer[i].BakeMesh(m);
                mf.mesh = m;
                mr.material = mat;

                StartCoroutine(AnimateMaterIalFloat(mr.material, 0, shaderVarRate, shaderVarRefreshRate));

                Destroy(gObj, meshDestroyDelay);
            }
            yield return new WaitForSeconds(meshRefreshRate);
        }
        moveScript.movementSpeed = normalSpeed;
        animator.SetFloat("animSpeed", normalAnimSpeed);
        isTrailActive = false;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space) && !isTrailActive)
        {
            isTrailActive = true;
            StartCoroutine(ActivateTrail(activeTime));
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MoveUpDown : MonoBehaviour
{
    public float distance;
    public float upSizeValue;
    public bool isUseMoveUp = false;

    void Start()
    {
        // 현재 Y축 위치를 기준으로 움직이게 하기 위해 상대적인 이동을 설정합니다.
        float startY = transform.position.y; // 현재 Y축 위치 저장
        float moveDuration = 0.5f; // 한 방향으로 이동하는 시간 (0.5초)

        // 크기 변화 설정 (크기가 1에서 1.2로 커졌다가 1로 돌아옴)
        Vector3 originalScale = transform.localScale; // 원래 크기
        Vector3 scaledUp = originalScale * upSizeValue; // 20% 더 커진 크기

        // Sequence를 사용하여 애니메이션을 동시에 실행하도록 설정합니다.
        Sequence sequence = DOTween.Sequence(); // Sequence 생성
 
        // Y축 이동 애니메이션 추가
        if(isUseMoveUp) {
            sequence.Append(transform.DOMoveY(startY + distance, moveDuration));
        }

        // 크기 변화 애니메이션 추가 (Y축 이동과 동시에 실행)
        sequence.Join(transform.DOScale(scaledUp, moveDuration));

        // 반복 설정: 무한 루프, Yoyo 방식으로 위 아래 반복
        sequence.SetLoops(-1, LoopType.Yoyo);
    }
}

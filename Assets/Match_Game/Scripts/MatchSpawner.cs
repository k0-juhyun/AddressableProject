using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class MatchSpawner : MonoBehaviour
{
    [SerializeField] private	GameObject		applePrefab;
	[SerializeField] private	GameObject		resultPanel;
	[SerializeField] private	Transform		appleParent;
	[SerializeField] private	ParticleSystem[] particles;
	[SerializeField] private	ParticleSystem attackParticle;
	[SerializeField] private RectTransform  targetRectTransform;
	[SerializeField] private Text scoreText;
	[SerializeField] private Text comboText;
	[SerializeField] private Text finalScoreText;
	private int	width = 12, height = 7; 
	private readonly int	spacing = 15;
	private string gameSettingString = ""; // 현재 맵 세팅 정보 
	private int lastScore = 0;
	[NonSerialized] public	List<Coin>	appleList = new List<Coin>();

    public void Start()
    {
        SpawnApples(7, 12);
    }

	/// <summary>
    /// 처음 맵 세팅 
    /// </summary>
    public void SpawnApples(int height, int width)
	{
		resultPanel.SetActive(false);

		this.height = height;
		this.width = width;
		//Debug.Log("SpawnApples height : " + height + " width : " + width);

		// 기존에 남아있는 데이터 있으면 삭제 후 생성
		if(appleList.Count > 0){
			foreach(Coin item in appleList){
				Destroy(item.gameObject);
			}
			appleList.Clear();
		}

		Vector2 size = applePrefab.GetComponent<RectTransform>().sizeDelta;
		size += new Vector2(spacing, spacing);

		int sum = 0;
		for ( int y = 0; y < height; ++ y )
		{
			for ( int x = 0; x < width; ++ x )
			{
				GameObject clone = Instantiate(applePrefab, appleParent);
				RectTransform rect = clone.GetComponent<RectTransform>();

				float px = (-width * 0.5f + 0.5f + x) * size.x;
				float py = (height * 0.5f - 0.5f - y) * size.y;
				rect.anchoredPosition = new Vector2(px, py);

				Coin apple = clone.GetComponent<Coin>();
				apple.Number = UnityEngine.Random.Range(1, 10);

				if ( y == height - 1 && x == width - 1 )
				{
					apple.Number = 10 - (sum % 10);
				}

				sum += apple.Number;

				appleList.Add(apple);
			}
		}

		//if(gameSettingString.Length == appleList.Count){
			// for ( int x = 0; x < appleList.Count; x++ )
			// {
			// 	appleList[x].Number = gameSettingString[x] - '0';
			// }	
		//}
		//Debug.Log($"AppleSpawner::SpawnApples() : {sum}");
	}

	public void ReSetting(string gameSettingString)
	{
		//Debug.Log("ReSetting : " + gameSettingString);
		this.gameSettingString = gameSettingString;
	}

	/// <summary>
    /// 누군가 공격함
    /// </summary>
	public void PlayAttackParticle(){
		if(attackParticle.isPlaying){
			attackParticle.Stop();
			attackParticle.transform.localPosition = Vector3.zero;
		}

		float posX = UnityEngine.Random.Range(1f, 4f);
		int dirX = UnityEngine.Random.Range(0, 2);
		if(dirX == 0){
			attackParticle.transform.localPosition = new Vector3(posX * -1, 0, 0);
		}else{
			attackParticle.transform.localPosition = new Vector3(posX, 0, 0);
		}
		attackParticle.Play();
		DOVirtual.DelayedCall(5, delegate () {
			attackParticle.Stop();
			attackParticle.transform.localPosition = Vector3.zero;
        });
	}

	public void DestroyApple(Coin removeItem)
	{
		appleList.Remove(removeItem);
		for(int i=0; i<particles.Length; i++){
			if(particles[i].isPlaying == false){
				Vector3 screenPosition = Camera.main.WorldToScreenPoint(removeItem.gameObject.transform.position);
				Vector2 localPoint;
				RectTransformUtility.ScreenPointToLocalPointInRectangle(targetRectTransform, screenPosition, Camera.main, out localPoint);
				particles[i].gameObject.transform.localPosition = new Vector3(localPoint.x, localPoint.y, 0);
				particles[i].Play();
				//SFXManager.Instance.PlaySFX(KindOfSFX.CardPop);
				break;
			}
		}
		Destroy(removeItem.gameObject);
	}
	
	/// <summary>
    /// 점수랑 콤보 글자 반영 
    /// </summary>
	public void ChangeText(int score, int combo){
		scoreText.text = score.ToString() + "점";
		comboText.text = combo.ToString() + "번";
		if(combo != 0){
			AnimateTextScale(comboText);
		}
		if(lastScore != score){
			AnimateTextScale(scoreText);
			lastScore = score;
		}
	}

	public void GameOver(int lastScore){
		finalScoreText.text = lastScore.ToString();
		resultPanel.SetActive(true);
		//SFXManager.Instance.PlaySFX(KindOfSFX.SpillCoins);
	}

	private void AnimateTextScale(Text text)
    {
        // 텍스트 크기 애니메이션 (커지고 작아짐)
		text.transform.localScale = Vector3.one * 1.4f;
        text.transform.DOScale(1, 0.5f).OnComplete(() =>{
            text.transform.localScale = Vector3.one;
        });
    }
}

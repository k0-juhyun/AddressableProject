using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class MouseDragController : MonoBehaviour
{
    //[SerializeField] private Match_Game gameController;
	[SerializeField] private MatchSpawner appleSpawner;
	[SerializeField] private RectTransform rectTransform;
	[SerializeField] private Canvas canvas; 
	private	readonly int ANSWER = 10;
	private	int	sum = 0;
	private	Vector2	start = Vector2.zero;
	private	Vector2	end = Vector2.zero;
	private bool isObstructDis = false;
	private	List<Coin>	selectedAppleList = new List<Coin>();
    private int totalScore = 0;
    private int totalCombo = 0;

    public void Start()
    {
        Setting();
    }

    public void Setting(){
        totalScore = 0;
        totalCombo = 0;
    }

    private void Update()
	{
		//if ( gameController.IsGameStart == false ) return;

		if ( Input.GetMouseButtonDown(0) )
		{
			Vector2 worldPosition = Input.mousePosition;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(
				rectTransform.GetComponent<RectTransform>(), 
				worldPosition, 
				canvas.worldCamera, 
				out start
			);
		}

		if ( Input.GetMouseButton(0) )
		{
			Vector2 worldPosition = Input.mousePosition;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(
				rectTransform.GetComponent<RectTransform>(), 
				worldPosition, 
				canvas.worldCamera,  
				out end
			);
			SelectApples();
		}

		if ( Input.GetMouseButtonUp(0) )
		{
			if ( sum == ANSWER ) // 합이 10이라면
			{
				int score = 0;
				foreach ( Coin apple in selectedAppleList )
				{
					score ++;
					appleSpawner.DestroyApple(apple);
				}
                totalScore += score;
                totalCombo += 1;
                appleSpawner.ChangeText(totalScore, totalCombo);
			}
			else
			{
				foreach ( Coin apple in selectedAppleList )
				{
					apple.isClick = false;
					apple.OnDeselected();
				}
				if(sum != 0){ // 사과를 하나라도 선택했을 경우
                    totalCombo = 0;
                    appleSpawner.ChangeText(totalScore, totalCombo);
				}
			}
			selectedAppleList.Clear();
			sum = 0;
			start = end = Vector2.zero;
		}

        if (Input.GetKeyDown(KeyCode.Q))
        {
            appleSpawner.GameOver(totalScore);
        }
	}

	/// <summary>
    /// 선택된 사과 찾기 
    /// </summary>
	private void SelectApples()
	{
		foreach ( Coin apple in appleSpawner.appleList )
		{
			float distance = Vector2.Distance(end, apple.Position);
			if ( distance < 40)
			{
				float distanceToLastTile = 0;
				if(selectedAppleList.Count > 0){
					distanceToLastTile = Vector2.Distance(selectedAppleList[selectedAppleList.Count - 1].Position, apple.Position);
					//Debug.Log("SelectApples : distanceToLastTile : " + distanceToLastTile);
				}
				if(apple.isClick == false && (distanceToLastTile < 200 || isObstructDis == false)){
					apple.isClick = true;
					apple.OnSelected();
					selectedAppleList.Add(apple);
					sum += apple.Number;
					//SFXManager.Instance.PlaySFX(KindOfSFX.UIClick);
				}
			}
		}
	}
}